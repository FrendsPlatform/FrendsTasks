using System.Linq;
using System.Text.Json.Nodes;

namespace FrendsTaskAnalyzers.Tests;

public static class Helpers
{
    public const string TaskMetadataFileName = "FrendsTaskMetadata.json";

    public static string CreateMetadataJson(params string[] taskMethods)
    {
        var tasks = taskMethods
            .Select(t => new JsonObject { ["TaskMethod"] = t })
            .ToArray<JsonNode?>();
        return new JsonObject { ["Tasks"] = new JsonArray(tasks) }.ToJsonString();
    }
}
