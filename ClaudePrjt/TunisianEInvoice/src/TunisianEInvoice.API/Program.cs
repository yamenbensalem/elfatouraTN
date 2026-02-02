using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
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
using TunisianEInvoice.Infrastructure.Persistence;
using TunisianEInvoice.Infrastructure.Persistence.Repositories;
using TunisianEInvoice.Infrastructure.ExternalServices;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers()
    .AddNewtonsoftJson(options =>
    {
        options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
        options.SerializerSettings.NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore;
    });

// Configure Entity Framework with SQL Server
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
    ?? "Server=(localdb)\\mssqllocaldb;Database=TunisianEInvoice;Trusted_Connection=True;MultipleActiveResultSets=true";
builder.Services.AddDbContext<EInvoiceDbContext>(options =>
    options.UseSqlServer(connectionString));

// Configure AutoMapper
builder.Services.AddAutoMapper(typeof(InvoiceMappingProfile));

// Register repositories
builder.Services.AddScoped<IClientRepository, ClientRepository>();
builder.Services.AddScoped<IInvoiceRepository, InvoiceRepository>();

// Register application services
builder.Services.AddScoped<IInvoiceService, InvoiceService>();
builder.Services.AddScoped<IXmlGeneratorService, XmlGeneratorService>();
builder.Services.AddScoped<IXmlValidationService, XmlValidationService>();
builder.Services.AddScoped<IQrCodeService, QrCodeService>();
builder.Services.AddScoped<IPdfGeneratorService, PdfGeneratorService>();
builder.Services.AddScoped<ISignatureService, SignatureService>();

// Register TTN service with HttpClient
builder.Services.AddHttpClient<ITtnService, TtnService>();

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

// Seed the database with mock data in development
if (app.Environment.IsDevelopment())
{
    await DatabaseSeeder.SeedAsync(app.Services);
}

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
