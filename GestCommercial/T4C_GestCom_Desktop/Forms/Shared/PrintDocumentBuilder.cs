using System.Diagnostics;
using System.Net;
using System.Text;

namespace T4C_GestCom_Desktop.Forms.Shared;

/// <summary>
/// Generic content model for a printable commercial document (facture/avoir/devis/commande/bon) —
/// one shape covers all of them, the way the web app's PrintDocHeader + print-table/print-totals-table
/// CSS classes are shared across every Components/Pages/Print/*.razor page.
/// </summary>
public sealed record PrintDocumentModel(
    string DocType,
    string Numero,
    DateTime Date,
    string? Etat,
    string PartyLabel,
    string PartyName,
    IReadOnlyList<string> PartyDetails,
    IReadOnlyList<(string Label, string Value)> HeaderRight,
    IReadOnlyList<string> ColumnHeaders,
    IReadOnlyList<IReadOnlyList<string>> Rows,
    IReadOnlyList<(string Label, string Value, bool Emphasize)> Totals,
    string? Note,
    (string[] Headers, IReadOnlyList<string[]> Rows)? Reglements,
    string? EntrepriseFooter);

/// <summary>
/// Renders a PrintDocumentModel to a self-contained HTML file and opens it in the default browser,
/// where the user prints or saves as PDF via the browser's own Ctrl+P — the same mechanism the web
/// app uses (window.print() on its /print/* pages). Avoids building a pixel-perfect GDI+ layout for
/// every one of the nine document types.
/// </summary>
public static class PrintDocumentBuilder
{
    public static void PreviewInBrowser(PrintDocumentModel model)
    {
        var html = BuildHtml(model);
        var path = Path.Combine(Path.GetTempPath(), $"T4C_{model.DocType.Replace(' ', '_')}_{model.Numero}_{Guid.NewGuid():N}.html");
        File.WriteAllText(path, html, Encoding.UTF8);
        Process.Start(new ProcessStartInfo(path) { UseShellExecute = true });
    }

    /// <summary>Internal (not private) so tests can verify the generated markup without touching the filesystem or launching a browser.</summary>
    internal static string BuildHtml(PrintDocumentModel m)
    {
        var sb = new StringBuilder();
        sb.Append("""
            <!DOCTYPE html>
            <html lang="fr"><head><meta charset="utf-8">
            <title>PAGE_TITLE</title>
            <style>
                body { font-family: Segoe UI, Arial, sans-serif; font-size: 13px; color: #212529; margin: 24px; }
                .toolbar { margin-bottom: 16px; }
                .toolbar button { padding: 6px 14px; font-size: 13px; }
                .header { display: flex; justify-content: space-between; border-bottom: 2px solid #333; padding-bottom: 10px; margin-bottom: 14px; }
                .header h1 { font-size: 18px; margin: 0 0 4px; }
                .header .meta { text-align: right; font-size: 12px; color: #555; }
                .parties { display: flex; justify-content: space-between; margin-bottom: 16px; }
                .party-label { font-size: 11px; text-transform: uppercase; color: #777; margin-bottom: 2px; }
                .party-name { font-weight: 600; font-size: 14px; }
                table.lines { width: 100%; border-collapse: collapse; margin-bottom: 14px; }
                table.lines th, table.lines td { border: 1px solid #ccc; padding: 5px 8px; font-size: 12px; }
                table.lines th { background: #f2f2f2; text-align: left; }
                .text-end { text-align: right; }
                .text-center { text-align: center; }
                .totals-wrap { display: flex; justify-content: flex-end; }
                table.totals { border-collapse: collapse; min-width: 280px; }
                table.totals td { padding: 3px 8px; }
                table.totals tr.net td { font-weight: 700; border-top: 2px solid #333; font-size: 14px; }
                .note { margin-top: 12px; font-size: 12px; }
                .footer { margin-top: 24px; padding-top: 8px; border-top: 1px solid #ccc; font-size: 11px; color: #777; }
                @media print { .toolbar { display: none; } }
            </style></head><body>
            <div class="toolbar">
                <button onclick="window.print()">Imprimer / PDF</button>
            </div>
            """);
        sb.Replace("PAGE_TITLE", Html($"{m.DocType} {m.Numero}"));

        sb.Append($"""<div class="header"><div><h1>{Html(m.DocType)}</h1><div>N° {Html(m.Numero)}</div></div>""");
        sb.Append($"""<div class="meta">Date : {m.Date:dd/MM/yyyy}""");
        if (!string.IsNullOrWhiteSpace(m.Etat)) sb.Append($"<br/>État : {Html(m.Etat)}");
        sb.Append("</div></div>");

        sb.Append($"""<div class="parties"><div><div class="party-label">{Html(m.PartyLabel)}</div><div class="party-name">{Html(m.PartyName)}</div>""");
        foreach (var line in m.PartyDetails)
            sb.Append($"<div>{Html(line)}</div>");
        sb.Append("</div>");

        if (m.HeaderRight.Count > 0)
        {
            sb.Append("""<div class="text-end">""");
            foreach (var (label, value) in m.HeaderRight)
                sb.Append($"""<div class="party-label">{Html(label)}</div><div>{Html(value)}</div>""");
            sb.Append("</div>");
        }
        sb.Append("</div>");

        sb.Append("""<table class="lines"><thead><tr>""");
        foreach (var h in m.ColumnHeaders)
            sb.Append($"<th>{Html(h)}</th>");
        sb.Append("</tr></thead><tbody>");
        foreach (var row in m.Rows)
        {
            sb.Append("<tr>");
            foreach (var cell in row)
                sb.Append($"<td>{Html(cell)}</td>");
            sb.Append("</tr>");
        }
        sb.Append("</tbody></table>");

        sb.Append("""<div class="totals-wrap"><table class="totals">""");
        foreach (var (label, value, emphasize) in m.Totals)
            sb.Append($"""<tr{(emphasize ? " class=\"net\"" : "")}><td>{Html(label)}</td><td class="text-end">{Html(value)}</td></tr>""");
        sb.Append("</table></div>");

        if (!string.IsNullOrWhiteSpace(m.Note))
            sb.Append($"""<div class="note"><strong>Note :</strong> {Html(m.Note)}</div>""");

        if (m.Reglements is { Rows.Count: > 0 } reglements)
        {
            sb.Append("""<div style="margin-top:16px"><div style="font-weight:600;margin-bottom:4px">Règlements</div><table class="lines"><thead><tr>""");
            foreach (var h in reglements.Headers)
                sb.Append($"<th>{Html(h)}</th>");
            sb.Append("</tr></thead><tbody>");
            foreach (var row in reglements.Rows)
            {
                sb.Append("<tr>");
                foreach (var cell in row)
                    sb.Append($"<td>{Html(cell)}</td>");
                sb.Append("</tr>");
            }
            sb.Append("</tbody></table></div>");
        }

        sb.Append($"""<div class="footer">{Html(m.EntrepriseFooter ?? string.Empty)}<br/>Imprimé le {DateTime.Now:dd/MM/yyyy à HH:mm}</div>""");
        sb.Append("</body></html>");

        return sb.ToString();
    }

    private static string Html(string? s) => WebUtility.HtmlEncode(s ?? string.Empty);
}
