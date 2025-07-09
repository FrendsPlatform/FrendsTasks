namespace FrendsTaskAnalyzers.ParametersAnalyzer;

public class ExpectedParameter
{
    public string Type { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public bool Required { get; set; }
    public bool IsProperty { get; set; } = true;
}
