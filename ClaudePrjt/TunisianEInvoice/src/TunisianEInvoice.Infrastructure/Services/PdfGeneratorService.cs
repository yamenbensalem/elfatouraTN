using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using TunisianEInvoice.Application.Interfaces;
using TunisianEInvoice.Domain.Entities;

namespace TunisianEInvoice.Infrastructure.Services
{
    /// <summary>
    /// PDF Generator Service for creating Tunisian e-invoice PDFs using QuestPDF.
    /// Generates professional invoices compliant with El Fatoora specifications.
    /// </summary>
    public class PdfGeneratorService : IPdfGeneratorService
    {
        private readonly IQrCodeService _qrCodeService;

        public PdfGeneratorService(IQrCodeService qrCodeService)
        {
            _qrCodeService = qrCodeService;
            
            // Required for QuestPDF community license
            QuestPDF.Settings.License = LicenseType.Community;
        }

        public async Task<byte[]> GeneratePdfAsync(Invoice invoice)
        {
            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(40);
                    page.DefaultTextStyle(x => x.FontSize(10));

                    page.Header().Element(c => ComposeHeader(c, invoice));
                    page.Content().Element(c => ComposeContent(c, invoice));
                    page.Footer().Element(c => ComposeFooter(c, invoice));
                });
            });

            var pdfBytes = document.GeneratePdf();
            return await Task.FromResult(pdfBytes);
        }

        private void ComposeHeader(IContainer container, Invoice invoice)
        {
            var sender = invoice.Body?.Partners?.Find(p => p.FunctionCode == "I-62");
            var docInfo = invoice.Body?.DocumentInfo;

            container.Column(column =>
            {
                column.Spacing(10);

                // Company header
                column.Item().Row(row =>
                {
                    row.RelativeItem(2).Column(col =>
                    {
                        if (sender != null)
                        {
                            col.Item().Text(sender.Name ?? "")
                                .FontSize(16).Bold().FontColor(Colors.Blue.Darken2);
                            
                            if (!string.IsNullOrEmpty(sender.NameType))
                                col.Item().Text(sender.NameType);
                            
                            col.Item().Text($"MF: {sender.Identifier?.Value ?? invoice.Header?.SenderIdentifier?.Value}")
                                .FontSize(9);
                            
                            if (sender.Address != null)
                            {
                                col.Item().Text(sender.Address.Street ?? "");
                                col.Item().Text($"{sender.Address.PostalCode ?? ""} {sender.Address.City ?? ""}");
                            }
                            
                            // Get contact info from Contacts list
                            var phoneContact = sender.Contacts?.Find(c => c.CommunicationType == "TE");
                            var emailContact = sender.Contacts?.Find(c => c.CommunicationType == "EM");
                            
                            if (phoneContact != null)
                                col.Item().Text($"Tél: {phoneContact.CommunicationAddress}");
                            if (emailContact != null)
                                col.Item().Text($"Email: {emailContact.CommunicationAddress}");
                        }
                    });

                    row.RelativeItem(1).Column(col =>
                    {
                        col.Item().AlignRight().Text("FACTURE")
                            .FontSize(24).Bold().FontColor(Colors.Blue.Darken2);
                        
                        col.Item().AlignRight().Text(docInfo?.DocumentTypeName ?? "Facture")
                            .FontSize(11);
                    });
                });

                // Invoice info box
                column.Item().PaddingTop(10).Row(row =>
                {
                    row.RelativeItem().Background(Colors.Grey.Lighten3).Padding(10).Column(col =>
                    {
                        col.Item().Row(r =>
                        {
                            r.RelativeItem().Text("N° Facture:").SemiBold();
                            r.RelativeItem().Text(docInfo?.DocumentIdentifier ?? "");
                        });
                        col.Item().Row(r =>
                        {
                            r.RelativeItem().Text("Date:").SemiBold();
                            r.RelativeItem().Text(invoice.Body?.DateInfo?.InvoiceDate.ToString("dd/MM/yyyy") ?? "");
                        });
                        if (invoice.Body?.DateInfo != null && !string.IsNullOrEmpty(invoice.Body.DateInfo.PeriodFrom))
                        {
                            col.Item().Row(r =>
                            {
                                r.RelativeItem().Text("Période:").SemiBold();
                                r.RelativeItem().Text($"{invoice.Body.DateInfo.PeriodFrom} - {invoice.Body.DateInfo.PeriodTo}");
                            });
                        }
                    });

                    row.ConstantItem(20);

                    // Generate QR Code
                    var senderMf = sender?.Identifier?.Value ?? invoice.Header?.SenderIdentifier?.Value ?? "";
                    var invoiceNum = docInfo?.DocumentIdentifier ?? "";
                    var invoiceDate = invoice.Body?.DateInfo?.InvoiceDate ?? DateTime.Now;
                    var totalTtc = invoice.Body?.Amounts?.TotalIncludingTax ?? 0;

                    var qrContent = _qrCodeService.BuildElFatooraQrContent(senderMf, invoiceNum, invoiceDate, totalTtc);
                    var qrBytes = _qrCodeService.GenerateQrCode(qrContent, 5);

                    row.ConstantItem(80).Image(qrBytes);
                });

                column.Item().PaddingTop(5).LineHorizontal(1).LineColor(Colors.Blue.Darken2);
            });
        }

        private void ComposeContent(IContainer container, Invoice invoice)
        {
            var receiver = invoice.Body?.Partners?.Find(p => p.FunctionCode == "I-64");

            container.PaddingVertical(10).Column(column =>
            {
                column.Spacing(10);

                // Client information
                column.Item().Background(Colors.Blue.Lighten5).Padding(10).Column(col =>
                {
                    col.Item().Text("CLIENT").Bold().FontColor(Colors.Blue.Darken2);
                    col.Item().PaddingTop(5);
                    
                    if (receiver != null)
                    {
                        col.Item().Text(receiver.Name ?? "")
                            .FontSize(12).SemiBold();
                        col.Item().Text($"MF: {receiver.Identifier?.Value}");
                        
                        if (receiver.Address != null)
                        {
                            col.Item().Text(receiver.Address.Street ?? "");
                            col.Item().Text($"{receiver.Address.PostalCode ?? ""} {receiver.Address.City ?? ""}");
                        }
                    }
                });

                // Line items table
                column.Item().PaddingTop(10).Element(c => ComposeLineItems(c, invoice));

                // Totals
                column.Item().PaddingTop(10).Element(c => ComposeTotals(c, invoice));

                // Amount in words
                if (invoice.Body?.Amounts?.AmountInWords != null)
                {
                    column.Item().PaddingTop(10).Background(Colors.Grey.Lighten4).Padding(8).Row(row =>
                    {
                        row.AutoItem().Text("Arrêté la présente facture à la somme de: ").SemiBold();
                        row.RelativeItem().Text(invoice.Body.Amounts.AmountInWords);
                    });
                }
            });
        }

        private void ComposeLineItems(IContainer container, Invoice invoice)
        {
            var lineItems = invoice.Body?.LineItems ?? new List<LineItem>();

            container.Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.ConstantColumn(30);   // #
                    columns.ConstantColumn(60);   // Code
                    columns.RelativeColumn(3);    // Description
                    columns.ConstantColumn(50);   // Qté
                    columns.ConstantColumn(70);   // PU HT
                    columns.ConstantColumn(50);   // TVA
                    columns.ConstantColumn(80);   // Total HT
                });

                // Header
                table.Header(header =>
                {
                    header.Cell().Background(Colors.Blue.Darken2).Padding(5)
                        .Text("#").FontColor(Colors.White).Bold();
                    header.Cell().Background(Colors.Blue.Darken2).Padding(5)
                        .Text("Code").FontColor(Colors.White).Bold();
                    header.Cell().Background(Colors.Blue.Darken2).Padding(5)
                        .Text("Désignation").FontColor(Colors.White).Bold();
                    header.Cell().Background(Colors.Blue.Darken2).Padding(5)
                        .AlignRight().Text("Qté").FontColor(Colors.White).Bold();
                    header.Cell().Background(Colors.Blue.Darken2).Padding(5)
                        .AlignRight().Text("PU HT").FontColor(Colors.White).Bold();
                    header.Cell().Background(Colors.Blue.Darken2).Padding(5)
                        .AlignRight().Text("TVA").FontColor(Colors.White).Bold();
                    header.Cell().Background(Colors.Blue.Darken2).Padding(5)
                        .AlignRight().Text("Total HT").FontColor(Colors.White).Bold();
                });

                // Rows
                int lineNumber = 1;
                foreach (var item in lineItems)
                {
                    var bgColor = lineNumber % 2 == 0 ? Colors.Grey.Lighten4 : Colors.White;

                    table.Cell().Background(bgColor).Padding(5).Text(lineNumber.ToString());
                    table.Cell().Background(bgColor).Padding(5).Text(item.ItemCode ?? "");
                    table.Cell().Background(bgColor).Padding(5).Text(item.ItemDescription ?? "");
                    table.Cell().Background(bgColor).Padding(5).AlignRight()
                        .Text($"{item.Quantity:N3}");
                    table.Cell().Background(bgColor).Padding(5).AlignRight()
                        .Text($"{item.Amounts?.UnitPriceExcludingTax:N3}");
                    table.Cell().Background(bgColor).Padding(5).AlignRight()
                        .Text($"{item.Tax?.TaxRate ?? 0:N0}%");
                    table.Cell().Background(bgColor).Padding(5).AlignRight()
                        .Text($"{item.Amounts?.TotalExcludingTax:N3}");

                    lineNumber++;
                }
            });
        }

        private void ComposeTotals(IContainer container, Invoice invoice)
        {
            var amounts = invoice.Body?.Amounts;
            var taxes = invoice.Body?.Taxes ?? new List<TaxDetails>();

            container.Row(row =>
            {
                row.RelativeItem(2); // Spacer

                row.RelativeItem(1).Border(1).BorderColor(Colors.Grey.Medium).Column(col =>
                {
                    // Tax breakdown
                    foreach (var tax in taxes)
                    {
                        col.Item().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Row(r =>
                        {
                            r.RelativeItem().Text($"{tax.TaxTypeCode} ({tax.TaxRate:N0}%)");
                            r.AutoItem().AlignRight().Text($"{tax.TaxAmount:N3} TND");
                        });
                    }

                    // Subtotals
                    col.Item().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Row(r =>
                    {
                        r.RelativeItem().Text("Total HT").SemiBold();
                        r.AutoItem().AlignRight().Text($"{amounts?.TotalExcludingTax:N3} TND").SemiBold();
                    });

                    col.Item().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Row(r =>
                    {
                        r.RelativeItem().Text("Total TVA");
                        r.AutoItem().AlignRight().Text($"{amounts?.TotalTaxAmount:N3} TND");
                    });

                    if (amounts?.StampDuty != null && amounts.StampDuty > 0)
                    {
                        col.Item().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Row(r =>
                        {
                            r.RelativeItem().Text("Droit de timbre");
                            r.AutoItem().AlignRight().Text($"{amounts.StampDuty:N3} TND");
                        });
                    }

                    // Grand total
                    col.Item().Background(Colors.Blue.Darken2).Padding(8).Row(r =>
                    {
                        r.RelativeItem().Text("TOTAL TTC").Bold().FontColor(Colors.White);
                        r.AutoItem().AlignRight().Text($"{amounts?.TotalIncludingTax:N3} TND")
                            .Bold().FontColor(Colors.White).FontSize(12);
                    });
                });
            });
        }

        private void ComposeFooter(IContainer container, Invoice invoice)
        {
            container.Column(column =>
            {
                column.Item().LineHorizontal(1).LineColor(Colors.Grey.Medium);
                
                column.Item().PaddingTop(5).Row(row =>
                {
                    row.RelativeItem().AlignLeft().Text(text =>
                    {
                        text.DefaultTextStyle(x => x.FontSize(8).FontColor(Colors.Grey.Darken1));
                        text.Span("Document généré électroniquement - ");
                        text.Span("Facture conforme au format TEIF 1.8.8").Italic();
                    });

                    row.RelativeItem().AlignRight().Text(text =>
                    {
                        text.DefaultTextStyle(x => x.FontSize(8).FontColor(Colors.Grey.Darken1));
                        text.Span("Page ");
                        text.CurrentPageNumber();
                        text.Span(" / ");
                        text.TotalPages();
                    });
                });
            });
        }
    }
}
