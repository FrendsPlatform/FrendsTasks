namespace FrendsTaskAnalyzers.Models;

public class TaskMetadata(string vendor, string system, string action)
{
    public string Vendor { get; } = vendor;

    public string System { get; } = system;

    public string Action { get; } = action;
}
