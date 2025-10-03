using System.Collections.Immutable;
using System.Linq;
using System.Xml.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace FrendsTaskAnalyzers.Analyzers.DocumentationAnalyzer;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class DocumentationAnalyzer : BaseAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
    [
        DocumentationRules.DocumentationLinkMissing,
        DocumentationRules.UnsupportedTagsUsed,
        DocumentationRules.RequiredTagsMissing,
        DocumentationRules.DocumentationInvalid
    ];

    private static readonly string[] RequiredTags = ["summary", "example"];
    private static readonly string[] UnsupportedTags = ["cref"];

    protected override void RegisterActions(CompilationStartAnalysisContext context)
    {
        if (!AssignTaskMethods(context)) return;
        context.RegisterSyntaxNodeAction(AnalyzeClassDocumentation, SyntaxKind.ClassDeclaration);
        context.RegisterSyntaxNodeAction(AnalyzeMemberDocumentation, SyntaxKind.MethodDeclaration);
        context.RegisterSyntaxNodeAction(AnalyzeMemberDocumentation, SyntaxKind.PropertyDeclaration);
    }

    private void AnalyzeClassDocumentation(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is not ClassDeclarationSyntax classSyntax) return;
        var symbol = context.SemanticModel.GetDeclaredSymbol(classSyntax, context.CancellationToken);
        if (symbol is null || symbol.DeclaredAccessibility != Accessibility.Public) return;
        var isTaskClass = TaskMethods!.Any(t => t.Path.StartsWith(symbol.ToDisplayString()));
        var xml = symbol.GetDocumentationCommentXml(cancellationToken: context.CancellationToken);
        ValidateXml(context, symbol, xml, checkForDocumentationLink: isTaskClass);
    }

    private void AnalyzeMemberDocumentation(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is not MemberDeclarationSyntax memberSyntax) return;
        var symbol = context.SemanticModel.GetDeclaredSymbol(memberSyntax, context.CancellationToken);
        if (symbol is null || symbol.DeclaredAccessibility != Accessibility.Public) return;
        var xml = symbol.GetDocumentationCommentXml(cancellationToken: context.CancellationToken);
        ValidateXml(context, symbol, xml, checkForDocumentationLink: false);
    }

    private static void ValidateXml(SyntaxNodeAnalysisContext context, ISymbol symbol, string? xml,
        bool checkForDocumentationLink)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(xml))
            {
                // Missing XML docs → required tags missing
                foreach (var tag in RequiredTags)
                {
                    context.ReportDiagnostic(
                        Diagnostic.Create(
                            DocumentationRules.RequiredTagsMissing,
                            symbol.Locations.First(),
                            tag
                        )
                    );
                }

                return;
            }

            var xDoc = XDocument.Parse(xml);

            // Required tags
            foreach (var tag in RequiredTags)
            {
                if (xDoc.Descendants(tag).FirstOrDefault() is null)
                {
                    context.ReportDiagnostic(
                        Diagnostic.Create(
                            DocumentationRules.RequiredTagsMissing,
                            symbol.Locations.First(),
                            tag
                        )
                    );
                }
            }

            // Unsupported tags
            foreach (var tag in UnsupportedTags)
            {
                if (xDoc.Descendants(tag).Any())
                {
                    context.ReportDiagnostic(
                        Diagnostic.Create(
                            DocumentationRules.UnsupportedTagsUsed,
                            symbol.Locations.First(),
                            tag
                        )
                    );
                }
            }

            // [Documentation] check only on main class
            if (checkForDocumentationLink && !xml!.Contains("[Documentation]"))
            {
                context.ReportDiagnostic(
                    Diagnostic.Create(
                        DocumentationRules.DocumentationLinkMissing,
                        symbol.Locations.First()
                    )
                );
            }
        }
        catch
        {
            context.ReportDiagnostic(
                Diagnostic.Create(
                    DocumentationRules.DocumentationInvalid,
                    symbol.Locations.First()
                )
            );
        }
    }
}
