using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace AIMD.ReportApp.Models;

public sealed class GenerateRequestPayload
{
    [JsonPropertyName("exam")]
    public string Exam { get; set; } = "CT ABDOMEN PELVIS";

    [JsonPropertyName("dictation")]
    public string Dictation { get; set; } = string.Empty;

    [JsonPropertyName("indication")]
    public string? Indication { get; set; }

    [JsonPropertyName("comparison")]
    public string? Comparison { get; set; }
}

public sealed class GenerateResponsePayload
{
    [JsonPropertyName("exam")]
    public string Exam { get; set; } = string.Empty;

    [JsonPropertyName("findings")]
    public Dictionary<string, string> Findings { get; set; } = new();

    [JsonPropertyName("impression")]
    public List<string> Impression { get; set; } = new();

    [JsonPropertyName("unmatched")]
    public List<string> Unmatched { get; set; } = new();

    [JsonPropertyName("timings_ms")]
    public Dictionary<string, double> Timings { get; set; } = new();

    [JsonPropertyName("metadata")]
    public Dictionary<string, string> Metadata { get; set; } = new();
}
