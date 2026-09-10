namespace Web_GestCom;

/// <summary>
/// Cache-busting token for static assets (app.css, bundled Blazor styles, vendor CSS) referenced
/// with a "?v=" query string. Computed once per process start, so every deploy/restart forces
/// browsers to fetch fresh copies instead of silently reusing whatever they cached before the
/// deploy — a stale app.css masked a shipped fix for returning visitors until this was added.
/// </summary>
internal static class AssetVersion
{
    public static readonly string Value = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
}
