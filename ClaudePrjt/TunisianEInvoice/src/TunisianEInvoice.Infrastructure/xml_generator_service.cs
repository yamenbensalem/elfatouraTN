// TunisianEInvoice.Application/Interfaces/IXmlGeneratorService.cs
using TunisianEInvoice.Domain.Entities;

namespace TunisianEInvoice.Application.Interfaces
{
    public interface IXmlGeneratorService
    {
        string GenerateXmlWithoutSignature(Invoice invoice);
        string GenerateXmlWithSignature(Invoice invoice, byte[] certificate, string certificatePassword);
    }
}

// TunisianEInvoice.Infrastructure/Services/XmlGeneratorService.cs
using System;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using TunisianEInvoice.Application.Interfaces;
using TunisianEInvoice.Domain.Entities;

namespace TunisianEInvoice.Infrastructure.Services
{
    public class XmlGeneratorService : IXmlGeneratorService
    {
        private readonly ISignatureService _signatureService;

        public XmlGeneratorService(ISignatureService signatureService)
        {
            _signatureService = signatureService;
        }

        public string GenerateXmlWithoutSignature(Invoice invoice)
        {
            XNamespace ns = "";
            
            var teif = new XElement("TEIF",
                new XAttribute("version", invoice.Version),
                new XAttribute("controlingAgency", invoice.ControllingAgency),
                
                CreateInvoiceHeader(invoice.Header),
                CreateInvoiceBody(invoice.Body),
                CreateRefTtnVal(invoice.TtnValidation)
            );

            var doc = new XDocument(
                new XDeclaration("1.0", "UTF-8", null),
                teif
            );

            var settings = new XmlWriterSettings
            {
                Encoding = Encoding.UTF8,
                Indent = true,
                IndentChars = "  ",
                OmitXmlDeclaration = false
            };

            using var stringWriter = new Utf8StringWriter();
            using var xmlWriter = XmlWriter.Create(stringWriter, settings);
            doc.Save(xmlWriter);
            return stringWriter.ToString();
        }

        public string GenerateXmlWithSignature(Invoice invoice, byte[] certificate, string certificatePassword)
        {
            var xmlWithoutSig = GenerateXmlWithoutSignature(invoice);
            var xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(xmlWithoutSig);

            // Add signature elements
            _signatureService.SignXml(xmlDoc, certificate, certificatePassword);

            return xmlDoc.OuterXml;
        }

        private XElement CreateInvoiceHeader(InvoiceHeader header)
        {
            return new XElement("InvoiceHeader",
                new XElement("MessageSenderIdentifier",
                    new XAttribute("type", header.SenderIdentifier.Type),
                    header.SenderIdentifier.Value),
                new XElement("MessageRecieverIdentifier",
                    new XAttribute("type", header.ReceiverIdentifier.Type),
                    header.ReceiverIdentifier.Value)
            );
        }

        private XElement CreateInvoiceBody(InvoiceBody body)
        {
            return new XElement("InvoiceBody",
                CreateBgm(body.DocumentInfo),
                CreateDtm(body.DateInfo),
                CreatePartnerSection(body.Partners),
                CreatePytSection(body.PaymentSections),
                CreateFtx(body.FreeTexts),
                CreateSpecialConditions(body.SpecialConditions),
                CreateLinSection(body.LineItems),
                CreateInvoiceMoa(body.Amounts),
                CreateInvoiceTax(body.Taxes)
            );
        }

        private XElement CreateBgm(DocumentInfo docInfo)
        {
            return new XElement("Bgm",
                new XElement("DocumentIdentifier", docInfo.DocumentIdentifier),
                new XElement("DocumentType",
                    new XAttribute("code", docInfo.DocumentTypeCode),
                    docInfo.DocumentTypeName)
            );
        }

        private XElement CreateDtm(DateInfo dateInfo)
        {
            return new XElement("Dtm",
                new XElement("DateText",
                    new XAttribute("format", "ddMMyy"),
                    new XAttribute("functionCode", "I-31"),
                    dateInfo.InvoiceDate.ToString("ddMMyy")),
                dateInfo.PeriodFrom != null && dateInfo.PeriodTo != null
                    ? new XElement("DateText",
                        new XAttribute("format", "ddMMyy-ddMMyy"),
                        new XAttribute("functionCode", "I-36"),
                        $"{dateInfo.PeriodFrom}-{dateInfo.PeriodTo}")
                    : null,
                dateInfo.DueDate.HasValue
                    ? new XElement("DateText",
                        new XAttribute("format", "ddMMyy"),
                        new XAttribute("functionCode", "I-32"),
                        dateInfo.DueDate.Value.ToString("ddMMyy"))
                    : null
            );
        }

        private XElement CreatePartnerSection(System.Collections.Generic.List<Partner> partners)
        {
            return new XElement("PartnerSection",
                partners.Select(p => CreatePartnerDetails(p))
            );
        }

        private XElement CreatePartnerDetails(Partner partner)
        {
            return new XElement("PartnerDetails",
                new XAttribute("functionCode", partner.FunctionCode),
                new XElement("Nad",
                    new XElement("PartnerIdentifier",
                        new XAttribute("type", partner.Identifier.Type),
                        partner.Identifier.Value),
                    new XElement("PartnerName",
                        new XAttribute("nameType", partner.NameType ?? "Qualification"),
                        partner.Name),
                    new XElement("PartnerAdresses",
                        new XAttribute("lang", partner.Address.Language ?? "fr"),
                        new XElement("AdressDescription", partner.Address.Description ?? ""),
                        new XElement("Street", partner.Address.Street ?? ""),
                        new XElement("CityName", partner.Address.City ?? ""),
                        new XElement("PostalCode", partner.Address.PostalCode ?? ""),
                        new XElement("Country",
                            new XAttribute("codeList", "ISO_3166-1"),
                            partner.Address.CountryCode ?? "TN")
                    )
                ),
                partner.References?.Select(r => new XElement("RffSection",
                    new XElement("Reference",
                        new XAttribute("refID", r.RefId),
                        r.Value)
                )),
                partner.Contacts?.Select(c => CreateCtaSection(c))
            );
        }

        private XElement CreateCtaSection(Contact contact)
        {
            return new XElement("CtaSection",
                new XElement("Contact",
                    new XAttribute("functionCode", contact.FunctionCode ?? "I-94"),
                    new XElement("ContactIdentifier", contact.Identifier ?? contact.Name),
                    new XElement("ContactName", contact.Name)
                ),
                !string.IsNullOrEmpty(contact.CommunicationType)
                    ? new XElement("Communication",
                        new XElement("ComMeansType", contact.CommunicationType),
                        new XElement("ComAdress", contact.CommunicationAddress))
                    : null
            );
        }

        private XElement CreatePytSection(System.Collections.Generic.List<PaymentSection> payments)
        {
            if (payments == null || !payments.Any())
                return null;

            return new XElement("PytSection",
                payments.Select(p => new XElement("PytSectionDetails",
                    new XElement("Pyt",
                        new XElement("PaymentTearmsTypeCode", p.TermsTypeCode),
                        new XElement("PaymentTearmsDescription", p.TermsDescription)
                    ),
                    p.BankAccount != null
                        ? new XElement("PytFii",
                            new XAttribute("functionCode", p.BankAccount.FunctionCode),
                            new XElement("AccountHolder",
                                new XElement("AccountNumber", p.BankAccount.AccountNumber),
                                new XElement("OwnerIdentifier", p.BankAccount.OwnerIdentifier)
                            ),
                            new XElement("InstitutionIdentification",
                                new XAttribute("nameCode", p.BankAccount.BankCode),
                                new XElement("BranchIdentifier", p.BankAccount.BranchIdentifier),
                                new XElement("InstitutionName", p.BankAccount.InstitutionName)
                            )
                        )
                        : null
                ))
            );
        }

        private XElement CreateFtx(System.Collections.Generic.List<string> freeTexts)
        {
            if (freeTexts == null || !freeTexts.Any())
                return null;

            return new XElement("Ftx",
                new XElement("FreeTextDetail",
                    new XAttribute("subjectCode", "I-41"),
                    freeTexts.Select(ft => new XElement("FreeTexts", ft))
                )
            );
        }

        private XElement CreateSpecialConditions(System.Collections.Generic.List<string> conditions)
        {
            if (conditions == null || !conditions.Any())
                return null;

            return new XElement("SpecialConditions",
                conditions.Select(c => new XElement("SpecialCondition", c))
            );
        }

        private XElement CreateLinSection(System.Collections.Generic.List<LineItem> items)
        {
            return new XElement("LinSection",
                items.Select(item => new XElement("Lin",
                    new XElement("ItemIdentifier", item.ItemIdentifier),
                    new XElement("LinImd",
                        new XAttribute("lang", item.Language ?? "fr"),
                        new XElement("ItemCode", item.ItemCode),
                        new XElement("ItemDescription", item.ItemDescription)
                    ),
                    new XElement("LinQty",
                        new XElement("Quantity",
                            new XAttribute("measurementUnit", item.MeasurementUnit),
                            item.Quantity.ToString(CultureInfo.InvariantCulture))
                    ),
                    new XElement("LinTax",
                        new XElement("TaxTypeName",
                            new XAttribute("code", item.Tax.TaxTypeCode),
                            item.Tax.TaxTypeName),
                        new XElement("TaxDetails",
                            new XElement("TaxRate", item.Tax.TaxRate.ToString(CultureInfo.InvariantCulture))
                        )
                    ),
                    new XElement("LinMoa",
                        new XElement("MoaDetails",
                            new XElement("Moa",
                                new XAttribute("amountTypeCode", "I-183"),
                                new XAttribute("currencyCodeList", "ISO_4217"),
                                new XElement("Amount",
                                    new XAttribute("currencyIdentifier", "TND"),
                                    item.Amounts.UnitPriceExcludingTax.ToString(CultureInfo.InvariantCulture))
                            )
                        ),
                        new XElement("MoaDetails",
                            new XElement("Moa",
                                new XAttribute("amountTypeCode", "I-171"),
                                new XAttribute("currencyCodeList", "ISO_4217"),
                                new XElement("Amount",
                                    new XAttribute("currencyIdentifier", "TND"),
                                    item.Amounts.TotalExcludingTax.ToString(CultureInfo.InvariantCulture))
                            )
                        )
                    )
                ))
            );
        }

        private XElement CreateInvoiceMoa(InvoiceAmounts amounts)
        {
            return new XElement("InvoiceMoa",
                new XElement("AmountDetails",
                    new XElement("Moa",
                        new XAttribute("amountTypeCode", "I-179"),
                        new XAttribute("currencyCodeList", "ISO_4217"),
                        new XElement("Amount",
                            new XAttribute("currencyIdentifier", "TND"),
                            amounts.Capital.ToString(CultureInfo.InvariantCulture))
                    )
                ),
                new XElement("AmountDetails",
                    new XElement("Moa",
                        new XAttribute("amountTypeCode", "I-180"),
                        new XAttribute("currencyCodeList", "ISO_4217"),
                        new XElement("Amount",
                            new XAttribute("currencyIdentifier", "TND"),
                            amounts.TotalIncludingTax.ToString(CultureInfo.InvariantCulture)),
                        new XElement("AmountDescription",
                            new XAttribute("lang", "fr"),
                            amounts.AmountInWords)
                    )
                ),
                new XElement("AmountDetails",
                    new XElement("Moa",
                        new XAttribute("amountTypeCode", "I-176"),
                        new XAttribute("currencyCodeList", "ISO_4217"),
                        new XElement("Amount",
                            new XAttribute("currencyIdentifier", "TND"),
                            amounts.TotalExcludingTax.ToString(CultureInfo.InvariantCulture))
                    )
                ),
                new XElement("AmountDetails",
                    new XElement("Moa",
                        new XAttribute("amountTypeCode", "I-182"),
                        new XAttribute("currencyCodeList", "ISO_4217"),
                        new XElement("Amount",
                            new XAttribute("currencyIdentifier", "TND"),
                            amounts.TotalTaxableBase.ToString(CultureInfo.InvariantCulture))
                    )
                ),
                new XElement("AmountDetails",
                    new XElement("Moa",
                        new XAttribute("amountTypeCode", "I-181"),
                        new XAttribute("currencyCodeList", "ISO_4217"),
                        new XElement("Amount",
                            new XAttribute("currencyIdentifier", "TND"),
                            amounts.TotalTaxAmount.ToString(CultureInfo.InvariantCulture))
                    )
                )
            );
        }

        private XElement CreateInvoiceTax(System.Collections.Generic.List<TaxDetails> taxes)
        {
            return new XElement("InvoiceTax",
                taxes.Select(tax => new XElement("InvoiceTaxDetails",
                    new XElement("Tax",
                        new XElement("TaxTypeName",
                            new XAttribute("code", tax.TaxTypeCode),
                            tax.TaxTypeName),
                        new XElement("TaxDetails",
                            new XElement("TaxRate", tax.TaxRate.ToString(CultureInfo.InvariantCulture))
                        )
                    ),
                    new XElement("AmountDetails",
                        new XElement("Moa",
                            new XAttribute("amountTypeCode", "I-177"),
                            new XAttribute("currencyCodeList", "ISO_4217"),
                            new XElement("Amount",
                                new XAttribute("currencyIdentifier", "TND"),
                                tax.TaxableBase.ToString(CultureInfo.InvariantCulture))
                        )
                    ),
                    new XElement("AmountDetails",
                        new XElement("Moa",
                            new XAttribute("amountTypeCode", "I-178"),
                            new XAttribute("currencyCodeList", "ISO_4217"),
                            new XElement("Amount",
                                new XAttribute("currencyIdentifier", "TND"),
                                tax.TaxAmount.ToString(CultureInfo.InvariantCulture))
                        )
                    )
                ))
            );
        }

        private XElement CreateRefTtnVal(RefTtnVal ttnVal)
        {
            if (ttnVal == null)
                return null;

            return new XElement("RefTtnVal",
                new XElement("ReferenceTTN",
                    new XAttribute("refID", "I-88"),
                    ttnVal.TtnReference),
                new XElement("ReferenceCEV", ttnVal.QrCodeBase64),
                new XElement("ReferenceDate",
                    new XElement("DateText",
                        new XAttribute("format", "ddMMyyHHmm"),
                        new XAttribute("functionCode", "I-37"),
                        ttnVal.ValidationDate.ToString("ddMMyyHHmm"))
                )
            );
        }
    }

    // Helper class for UTF-8 encoding
    public class Utf8StringWriter : System.IO.StringWriter
    {
        public override Encoding Encoding => Encoding.UTF8;
    }
}
