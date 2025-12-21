using System.IO;
using System.Reflection;
using System.Text.Json;
using AlgerianChequeFiller.Models;

namespace AlgerianChequeFiller.Services;

/// <summary>
/// Loads and saves cheque templates.
/// </summary>
public class TemplateStore
{
    private readonly string _appDataPath;
    private readonly JsonSerializerOptions _jsonOptions;

    public TemplateStore()
    {
        _appDataPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "AlgerianChequeFiller");
        
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        };

        Directory.CreateDirectory(_appDataPath);
    }

    /// <summary>
    /// Load the default template from embedded resources.
    /// </summary>
    public ChequeTemplate LoadDefaultTemplate()
    {
        try
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = "AlgerianChequeFiller.Resources.default_template.json";

            using var stream = assembly.GetManifestResourceStream(resourceName);
            if (stream != null)
            {
                using var reader = new StreamReader(stream);
                var json = reader.ReadToEnd();
                var template = JsonSerializer.Deserialize<ChequeTemplate>(json, _jsonOptions);
                return template ?? CreateFallbackTemplate();
            }
        }
        catch
        {
            // Fall through to fallback
        }

        return CreateFallbackTemplate();
    }

    /// <summary>
    /// Load template from user's app data.
    /// </summary>
    public ChequeTemplate? LoadTemplate(string templateId)
    {
        try
        {
            var path = Path.Combine(_appDataPath, $"{templateId}.json");
            if (File.Exists(path))
            {
                var json = File.ReadAllText(path);
                return JsonSerializer.Deserialize<ChequeTemplate>(json, _jsonOptions);
            }
        }
        catch
        {
            // Return null on error
        }
        return null;
    }

    /// <summary>
    /// Save template to user's app data.
    /// </summary>
    public void SaveTemplate(ChequeTemplate template)
    {
        var path = Path.Combine(_appDataPath, $"{template.Id}.json");
        var json = JsonSerializer.Serialize(template, _jsonOptions);
        File.WriteAllText(path, json);
    }

    /// <summary>
    /// Get list of saved template IDs.
    /// </summary>
    public List<string> GetSavedTemplateIds()
    {
        var ids = new List<string>();
        try
        {
            foreach (var file in Directory.GetFiles(_appDataPath, "*.json"))
            {
                ids.Add(Path.GetFileNameWithoutExtension(file));
            }
        }
        catch { }
        return ids;
    }

    /// <summary>
    /// Check if disclaimer has been accepted.
    /// </summary>
    public bool IsDisclaimerAccepted()
    {
        var path = Path.Combine(_appDataPath, "disclaimer_accepted");
        return File.Exists(path);
    }

    /// <summary>
    /// Mark disclaimer as accepted.
    /// </summary>
    public void AcceptDisclaimer()
    {
        var path = Path.Combine(_appDataPath, "disclaimer_accepted");
        File.WriteAllText(path, DateTime.Now.ToString("O"));
    }

    private ChequeTemplate CreateFallbackTemplate()
    {
        return new ChequeTemplate
        {
            Id = "default",
            TemplateName = "Algerian Cheque – Default",
            ChequeSizeMm = new SizeMm(160, 80),
            GlobalOffsetMm = new PointMm(0, 0),
            Fields = new Dictionary<string, FieldRect>
            {
                ["AmountNumeric"] = new FieldRect(140, 8, 32, 8) { Alignment = "Right" },
                ["AmountWordsL1"] = new FieldRect(50, 20, 120, 7) { Alignment = "Left" },
                ["Beneficiary"] = new FieldRect(38, 30, 130, 8) { Alignment = "Left" },
                ["AmountWordsL2"] = new FieldRect(10, 38, 80, 7) { Alignment = "Left" },
                ["Place"] = new FieldRect(100, 50, 35, 7) { Alignment = "Left" },
                ["Date"] = new FieldRect(140, 50, 32, 7) { Alignment = "Left" }
            },
            Fonts = new FontSettings
            {
                LatinFamily = "Arial",
                ArabicFamily = "Traditional Arabic",
                MinSize = 8,
                MaxSize = 12
            }
        };
    }
}
