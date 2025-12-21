using System.Text.Json.Serialization;

namespace AlgerianChequeFiller.Models;

/// <summary>
/// Cheque template with field positions and settings.
/// </summary>
public class ChequeTemplate
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    [JsonPropertyName("templateName")]
    public string TemplateName { get; set; } = "Default";

    [JsonPropertyName("chequeSizeMm")]
    public SizeMm ChequeSizeMm { get; set; } = new(160, 80);

    [JsonPropertyName("globalOffsetMm")]
    public PointMm GlobalOffsetMm { get; set; } = new(0, 0);

    [JsonPropertyName("fields")]
    public Dictionary<string, FieldRect> Fields { get; set; } = new();

    [JsonPropertyName("fonts")]
    public FontSettings Fonts { get; set; } = new();

    /// <summary>
    /// Get a field by its ID.
    /// </summary>
    public FieldRect? GetField(FieldId fieldId)
    {
        return Fields.TryGetValue(fieldId.ToString(), out var field) ? field : null;
    }
}

/// <summary>
/// Size in millimeters.
/// </summary>
public record SizeMm
{
    [JsonPropertyName("width")]
    public double Width { get; init; }

    [JsonPropertyName("height")]
    public double Height { get; init; }

    public SizeMm() { }
    public SizeMm(double width, double height)
    {
        Width = width;
        Height = height;
    }
}

/// <summary>
/// Point/offset in millimeters.
/// </summary>
public record PointMm
{
    [JsonPropertyName("x")]
    public double X { get; init; }

    [JsonPropertyName("y")]
    public double Y { get; init; }

    public PointMm() { }
    public PointMm(double x, double y)
    {
        X = x;
        Y = y;
    }
}

/// <summary>
/// Font settings for the template.
/// </summary>
public class FontSettings
{
    [JsonPropertyName("latinFamily")]
    public string LatinFamily { get; set; } = "Arial";

    [JsonPropertyName("arabicFamily")]
    public string ArabicFamily { get; set; } = "Traditional Arabic";

    [JsonPropertyName("minSize")]
    public double MinSize { get; set; } = 8;

    [JsonPropertyName("maxSize")]
    public double MaxSize { get; set; } = 12;
}
