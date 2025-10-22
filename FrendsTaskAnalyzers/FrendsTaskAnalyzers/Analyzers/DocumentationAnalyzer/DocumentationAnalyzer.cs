using System.Collections.Generic;
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

    private static readonly ImmutableArray<string> RequiredTagsForPublic = ["summary"];
    private static readonly ImmutableArray<string> RequiredTagsForProperties = ["example"];

    private static readonly ImmutableDictionary<string, string> UnsupportedTagsWithAttributes =
        new Dictionary<string, string> { ["see"] = "cref", ["seealso"] = "cref", ["cref"] = "<ANY>" }
            .ToImmutableDictionary();

    protected override void RegisterActions(CompilationStartAnalysisContext context)
    {
        if (!AssignTaskMethods(context)) return;
        context.RegisterSyntaxNodeAction(AnalyzeClassDocumentation, SyntaxKind.ClassDeclaration);
        context.RegisterSyntaxNodeAction(AnalyzeMemberDocumentation, SyntaxKind.MethodDeclaration);
        context.RegisterSyntaxNodeAction(AnalyzeMemberDocumentation, SyntaxKind.PropertyDeclaration);
    }

    private static void AnalyzeClassDocumentation(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is not ClassDeclarationSyntax classSyntax) return;
        var symbol = context.SemanticModel.GetDeclaredSymbol(classSyntax, context.CancellationToken);
        if (symbol is null || symbol.DeclaredAccessibility != Accessibility.Public) return;
        var xml = symbol.GetDocumentationCommentXml(cancellationToken: context.CancellationToken);
        ValidateXml(context, symbol, xml, checkForDocumentationLink: false);
    }

    private void AnalyzeMemberDocumentation(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is not MemberDeclarationSyntax memberSyntax) return;
        var symbol = context.SemanticModel.GetDeclaredSymbol(memberSyntax, context.CancellationToken);
        if (symbol is null || symbol.DeclaredAccessibility != Accessibility.Public) return;
        var isTaskMethod = TaskMethods.Any(t => symbol.ToDisplayString().StartsWith(t.Path));
        var xml = symbol.GetDocumentationCommentXml(cancellationToken: context.CancellationToken);
        ValidateXml(context, symbol, xml, checkForDocumentationLink: isTaskMethod);
    }

    private static void ValidateXml(SyntaxNodeAnalysisContext context, ISymbol symbol, string? xml,
        bool checkForDocumentationLink)
    {
        try
        {
            // Missing XML docs → required tags missing
            if (string.IsNullOrWhiteSpace(xml))
            {
                if (symbol.Kind == SymbolKind.Property)
                {
                    foreach (var tag in RequiredTagsForProperties)
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

                foreach (var tag in RequiredTagsForPublic)
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
            if (symbol.Kind == SymbolKind.Property)
            {
                foreach (var tag in RequiredTagsForProperties)
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
            }

            foreach (var tag in RequiredTagsForPublic)
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
            foreach (var tagWithAttribute in UnsupportedTagsWithAttributes)
            {
                bool foundUnsupportedTag =
                    tagWithAttribute.Value == "<ANY>" && xDoc.Descendants(tagWithAttribute.Key).Any() ||
                    tagWithAttribute.Value != "<ANY>" && xDoc.Descendants(tagWithAttribute.Key)
                        .Attributes(tagWithAttribute.Value).Any();
                if (foundUnsupportedTag)
                {
                    context.ReportDiagnostic(
                        Diagnostic.Create(
                            DocumentationRules.UnsupportedTagsUsed,
                            symbol.Locations.First(),
                            tagWithAttribute.Key,
                            tagWithAttribute.Value
                        )
                    );
                }
            }

            // [Documentation] check only on main class
            var hasDocLink = xDoc.Descendants().Any(e => e.Value.Contains("[Documentation]"));
            if (checkForDocumentationLink && !hasDocLink)
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
