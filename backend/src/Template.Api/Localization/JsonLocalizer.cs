using System.Collections.Concurrent;
using System.Text.Json;

namespace VoiceFlowStudio.Api.Localization;

/// <summary>
/// Constitution XI: localization files loaded and cached in memory at startup.
/// Supports Arabic (ar) and English (en); selection driven by Accept-Language.
/// </summary>
public interface ILocalizer
{
    string this[string key] { get; }
    string Translate(string key, string? culture = null);
}

public sealed class JsonLocalizer : ILocalizer
{
    private const string Default = "en";
    private static readonly string[] Supported = { "en", "ar" };

    private readonly IReadOnlyDictionary<string, IReadOnlyDictionary<string, string>> _bundles;
    private readonly IHttpContextAccessor _ctx;

    public JsonLocalizer(IWebHostEnvironment env, IHttpContextAccessor ctx)
    {
        _ctx = ctx;
        var dir = Path.Combine(env.ContentRootPath, "Resources");
        var bundles = new ConcurrentDictionary<string, IReadOnlyDictionary<string, string>>();
        foreach (var lang in Supported)
        {
            var path = Path.Combine(dir, $"{lang}.json");
            if (!File.Exists(path)) continue;
            using var stream = File.OpenRead(path);
            var dict = JsonSerializer.Deserialize<Dictionary<string, string>>(stream)
                       ?? new Dictionary<string, string>();
            bundles[lang] = dict;
        }
        _bundles = bundles;
    }

    public string this[string key] => Translate(key, null);

    public string Translate(string key, string? culture = null)
    {
        var lang = ResolveCulture(culture);
        if (_bundles.TryGetValue(lang, out var bundle) && bundle.TryGetValue(key, out var value))
            return value;
        if (_bundles.TryGetValue(Default, out var fallback) && fallback.TryGetValue(key, out var fb))
            return fb;
        return key;
    }

    private string ResolveCulture(string? explicitCulture)
    {
        if (!string.IsNullOrWhiteSpace(explicitCulture) && Supported.Contains(explicitCulture))
            return explicitCulture;
        var header = _ctx.HttpContext?.Request.Headers.AcceptLanguage.ToString();
        if (string.IsNullOrEmpty(header)) return Default;
        foreach (var part in header.Split(','))
        {
            var lang = part.Split(';')[0].Trim().Split('-')[0].ToLowerInvariant();
            if (Supported.Contains(lang)) return lang;
        }
        return Default;
    }
}
