// TunisianEInvoice.API/Program.cs
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using System;
using System.IO;
using System.Reflection;
using TunisianEInvoice.Application.Interfaces;
using TunisianEInvoice.Application.Services;
using TunisianEInvoice.Application.Mappings;
using TunisianEInvoice.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers()
    .AddNewtonsoftJson(options =>
    {
        options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
        options.SerializerSettings.NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore;
    });

// Configure AutoMapper
builder.Services.AddAutoMapper(typeof(InvoiceMappingProfile));

// Register application services
builder.Services.AddScoped<IInvoiceService, InvoiceService>();
builder.Services.AddScoped<IXmlGeneratorService, XmlGeneratorService>();
builder.Services.AddScoped<IXmlValidationService, XmlValidationService>();
builder.Services.AddScoped<IPdfGeneratorService, PdfGeneratorService>();
builder.Services.AddScoped<ISignatureService, SignatureService>();

// Configure Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Tunisian E-Invoice API",
        Version = "v1",
        Description = "API for generating Tunisian electronic invoices (TEIF format) with XML validation and PDF generation",
        Contact = new OpenApiContact
        {
            Name = "Tunisian E-Invoice Support",
            Email = "support@einvoice.tn"
        }
    });

    // Include XML comments
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }
});

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Tunisian E-Invoice API v1");
        c.RoutePrefix = string.Empty; // Serve Swagger UI at root
    });
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthorization();
app.MapControllers();

app.Run();

// TunisianEInvoice.API/appsettings.json
/*
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "InvoiceSettings": {
    "XsdSchemaPath": "Resources/Schemas",
    "CertificatePath": "Certificates",
    "DefaultCurrency": "TND",
    "DefaultCountry": "TN",
    "CompanyCapital": 2000000,
    "StampDutyAmount": 0.500
  },
  "TtnSettings": {
    "ApiUrl": "https://www.tradenet.com.tn",
    "ValidationEndpoint": "/validation",
    "SignaturePolicy": "urn:2.16.788.1.2.1"
  }
}
*/

// TunisianEInvoice.Application/Mappings/InvoiceMappingProfile.cs
using AutoMapper;
using System.Linq;
using TunisianEInvoice.Application.DTOs;
using TunisianEInvoice.Domain.Entities;

namespace TunisianEInvoice.Application.Mappings
{
    public class InvoiceMappingProfile : Profile
    {
        public InvoiceMappingProfile()
        {
            // Invoice Request DTO to Domain Entity
            CreateMap<InvoiceRequestDto, Invoice>()
                .ForMember(dest => dest.Header, opt => opt.MapFrom(src => new InvoiceHeader
                {
                    SenderIdentifier = new PartnerIdentifier
                    {
                        Type = src.Sender.IdentifierType,
                        Value = src.Sender.Identifier
                    },
                    ReceiverIdentifier = new PartnerIdentifier
                    {
                        Type = src.Receiver.IdentifierType,
                        Value = src.Receiver.Identifier
                    }
                }))
                .ForMember(dest => dest.Body, opt => opt.MapFrom(src => new InvoiceBody
                {
                    DocumentInfo = new DocumentInfo
                    {
                        DocumentIdentifier = src.DocumentIdentifier,
                        DocumentTypeCode = src.DocumentType,
                        DocumentTypeName = GetDocumentTypeName(src.DocumentType)
                    },
                    DateInfo = new DateInfo
                    {
                        InvoiceDate = src.InvoiceDate,
                        DueDate = src.DueDate,
                        PeriodFrom = src.PeriodFrom,
                        PeriodTo = src.PeriodTo
                    },
                    Partners = MapPartners(src.Sender, src.Receiver),
                    PaymentSections = src.PaymentSections.Select(p => new PaymentSection
                    {
                        TermsTypeCode = p.PaymentTermsTypeCode,
                        TermsDescription = p.PaymentTermsDescription,
                        BankAccount = p.BankAccount != null ? new BankAccount
                        {
                            FunctionCode = p.BankAccount.FunctionCode,
                            AccountNumber = p.BankAccount.AccountNumber,
                            OwnerIdentifier = p.BankAccount.OwnerIdentifier,
                            BankCode = p.BankAccount.BankCode,
                            BranchIdentifier = p.BankAccount.BranchIdentifier,
                            InstitutionName = p.BankAccount.InstitutionName
                        } : null
                    }).ToList(),
                    FreeTexts = src.FreeText != null ? new System.Collections.Generic.List<string> { src.FreeText } : new System.Collections.Generic.List<string>(),
                    SpecialConditions = src.SpecialConditions ?? new System.Collections.Generic.List<string>(),
                    LineItems = src.LineItems.Select(l => new LineItem
                    {
                        ItemIdentifier = l.ItemIdentifier,
                        ItemCode = l.ItemCode,
                        ItemDescription = l.ItemDescription,
                        Language = l.Language ?? "fr",
                        Quantity = l.Quantity,
                        MeasurementUnit = l.MeasurementUnit ?? "UNIT",
                        Tax = new TaxInfo
                        {
                            TaxTypeCode = l.TaxType ?? "I-1602",
                            TaxTypeName = GetTaxTypeName(l.TaxType ?? "I-1602"),
                            TaxRate = l.TaxRate
                        },
                        Amounts = new LineAmounts
                        {
                            UnitPriceExcludingTax = l.UnitPriceExcludingTax,
                            TotalExcludingTax = l.TotalExcludingTax
                        }
                    }).ToList()
                }));

            // Partner DTO to Domain
            CreateMap<PartnerDto, Partner>()
                .ForMember(dest => dest.Identifier, opt => opt.MapFrom(src => new PartnerIdentifier
                {
                    Type = src.IdentifierType,
                    Value = src.Identifier
                }))
                .ForMember(dest => dest.Address, opt => opt.MapFrom(src => src.Address))
                .ForMember(dest => dest.Contacts, opt => opt.MapFrom(src => src.Contacts));

            CreateMap<AddressDto, Address>()
                .ForMember(dest => dest.CountryCode, opt => opt.MapFrom(src => src.Country ?? "TN"))
                .ForMember(dest => dest.Language, opt => opt.MapFrom(src => src.Language ?? "fr"));

            CreateMap<ContactDto, Contact>()
                .ForMember(dest => dest.CommunicationType, opt => opt.MapFrom(src => src.Type))
                .ForMember(dest => dest.CommunicationAddress, opt => opt.MapFrom(src => src.Value));
        }

        private static System.Collections.Generic.List<Partner> MapPartners(PartnerDto sender, PartnerDto receiver)
        {
            var partners = new System.Collections.Generic.List<Partner>();

            // Sender (I-62)
            partners.Add(new Partner
            {
                FunctionCode = "I-62",
                Identifier = new PartnerIdentifier
                {
                    Type = sender.IdentifierType,
                    Value = sender.Identifier
                },
                Name = sender.Name,
                NameType = "Qualification",
                Address = new Address
                {
                    Description = sender.Address?.Description ?? "",
                    Street = sender.Address?.Street ?? "",
                    City = sender.Address?.City ?? "",
                    PostalCode = sender.Address?.PostalCode ?? "",
                    CountryCode = sender.Address?.Country ?? "TN",
                    Language = sender.Address?.Language ?? "fr"
                },
                References = new System.Collections.Generic.List<Reference>
                {
                    new Reference { RefId = "I-815", Value = sender.RegistrationNumber ?? "" },
                    new Reference { RefId = "I-816", Value = sender.LegalForm ?? "" }
                },
                Contacts = sender.Contacts?.Select(c => new Contact
                {
                    FunctionCode = "I-94",
                    Identifier = c.Identifier,
                    Name = c.Name,
                    CommunicationType = c.Type,
                    CommunicationAddress = c.Value
                }).ToList() ?? new System.Collections.Generic.List<Contact>()
            });

            // Receiver (I-64)
            partners.Add(new Partner
            {
                FunctionCode = "I-64",
                Identifier = new PartnerIdentifier
                {
                    Type = receiver.IdentifierType,
                    Value = receiver.Identifier
                },
                Name = receiver.Name,
                NameType = "Qualification",
                Address = new Address
                {
                    Description = receiver.Address?.Description ?? "",
                    Street = receiver.Address?.Street ?? "",
                    City = receiver.Address?.City ?? "",
                    PostalCode = receiver.Address?.PostalCode ?? "",
                    CountryCode = receiver.Address?.Country ?? "TN",
                    Language = receiver.Address?.Language ?? "fr"
                },
                References = new System.Collections.Generic.List<Reference>
                {
                    new Reference { RefId = "I-81", Value = receiver.Identifier?.Substring(0, 9) ?? "" },
                    new Reference { RefId = "I-811", Value = receiver.AccountMode ?? "" },
                    new Reference { RefId = "I-813", Value = receiver.Profile ?? "" },
                    new Reference { RefId = "I-812", Value = receiver.AccountRank ?? "" },
                    new Reference { RefId = "I-814", Value = receiver.ClientCode ?? "" }
                },
                Contacts = receiver.Contacts?.Select(c => new Contact
                {
                    FunctionCode = "I-94",
                    Identifier = c.Identifier,
                    Name = c.Name,
                    CommunicationType = c.Type,
                    CommunicationAddress = c.Value
                }).ToList() ?? new System.Collections.Generic.List<Contact>()
            });

            return partners;
        }

        private static string GetDocumentTypeName(string code)
        {
            return code switch
            {
                "I-11" => "Facture",
                "I-12" => "Facture d'avoir",
                "I-13" => "Facture de régularisation",
                "I-14" => "Facture récapitulative",
                "I-15" => "Facture proforma",
                "I-16" => "Note de débit",
                _ => "Facture"
            };
        }

        private static string GetTaxTypeName(string code)
        {
            return code switch
            {
                "I-1601" => "droit de timbre",
                "I-1602" => "TVA",
                "I-1603" => "FODEC",
                _ => "TVA"
            };
        }
    }
}
