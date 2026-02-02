using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TunisianEInvoice.Domain.Entities;

namespace TunisianEInvoice.Infrastructure.Persistence;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<EInvoiceDbContext>();
        
        // Ensure database is created
        await context.Database.EnsureCreatedAsync();
        
        // Seed clients if none exist
        if (!await context.Clients.AnyAsync())
        {
            var clients = GetSampleClients();
            await context.Clients.AddRangeAsync(clients);
            await context.SaveChangesAsync();
        }
        
        // Seed invoices if none exist
        if (!await context.Invoices.AnyAsync())
        {
            var clients = await context.Clients.ToListAsync();
            if (clients.Count >= 2)
            {
                var invoices = GetSampleInvoices(clients);
                await context.Invoices.AddRangeAsync(invoices);
                await context.SaveChangesAsync();
            }
        }
    }
    
    private static List<Client> GetSampleClients()
    {
        return new List<Client>
        {
            new Client
            {
                Id = Guid.NewGuid(),
                MatriculeFiscal = "1234567/A/B/M/000",
                Name = "Société ABC SARL",
                LegalForm = "SARL",
                RegistrationNumber = "B123456789",
                Capital = 100000.000m,
                AddressDescription = "Zone Industrielle",
                Street = "Rue de l'Industrie, N° 25",
                City = "Tunis",
                PostalCode = "1000",
                CountryCode = "TN",
                Phone = "+216 71 123 456",
                Fax = "+216 71 123 457",
                Email = "contact@abc-sarl.tn",
                Website = "www.abc-sarl.tn",
                BankCode = "07",
                BankAccountNumber = "07123456789012345678",
                BankName = "Banque Nationale Agricole",
                TtnAccountMode = "TEST",
                TtnAccountRank = "1",
                TtnProfile = "STANDARD",
                TtnClientCode = "ABC001",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new Client
            {
                Id = Guid.NewGuid(),
                MatriculeFiscal = "7654321/C/D/P/000",
                Name = "Entreprise XYZ SA",
                LegalForm = "SA",
                RegistrationNumber = "A987654321",
                Capital = 500000.000m,
                AddressDescription = "Centre Urbain Nord",
                Street = "Avenue Habib Bourguiba, Immeuble Le Cristal",
                City = "Ariana",
                PostalCode = "2080",
                CountryCode = "TN",
                Phone = "+216 70 987 654",
                Email = "info@xyz-sa.tn",
                BankCode = "10",
                BankAccountNumber = "10987654321098765432",
                BankName = "Société Tunisienne de Banque",
                TtnAccountMode = "TEST",
                TtnAccountRank = "1",
                TtnProfile = "STANDARD",
                TtnClientCode = "XYZ001",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new Client
            {
                Id = Guid.NewGuid(),
                MatriculeFiscal = "9876543/E/F/M/000",
                Name = "Commerce Plus SUARL",
                LegalForm = "SUARL",
                RegistrationNumber = "C456789123",
                Capital = 50000.000m,
                Street = "Avenue de la Liberté, N° 100",
                City = "Sfax",
                PostalCode = "3000",
                CountryCode = "TN",
                Phone = "+216 74 456 789",
                Email = "ventes@commerce-plus.tn",
                BankCode = "03",
                BankAccountNumber = "03456789012345678901",
                BankName = "Banque de Tunisie",
                TtnAccountMode = "TEST",
                TtnAccountRank = "1",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new Client
            {
                Id = Guid.NewGuid(),
                MatriculeFiscal = "1111111/A/A/M/000",
                Name = "Tech Innovation SNC",
                LegalForm = "SNC",
                RegistrationNumber = "D111222333",
                Capital = 25000.000m,
                Street = "Technopole El Ghazala",
                City = "Ariana",
                PostalCode = "2088",
                CountryCode = "TN",
                Phone = "+216 70 111 222",
                Email = "hello@tech-innovation.tn",
                Website = "www.tech-innovation.tn",
                TtnAccountMode = "TEST",
                TtnAccountRank = "1",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new Client
            {
                Id = Guid.NewGuid(),
                MatriculeFiscal = "2222222/B/C/P/000",
                Name = "Mohamed Ben Ali",
                LegalForm = "PP",
                Street = "Rue Ibn Khaldoun, N° 50",
                City = "Sousse",
                PostalCode = "4000",
                CountryCode = "TN",
                Phone = "+216 73 222 333",
                Email = "m.benali@email.tn",
                TtnAccountMode = "TEST",
                TtnAccountRank = "1",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            }
        };
    }
    
    private static List<InvoiceRecord> GetSampleInvoices(List<Client> clients)
    {
        var invoices = new List<InvoiceRecord>();
        var random = new Random(42); // Fixed seed for reproducible data
        
        var sender = clients.First();
        var receivers = clients.Skip(1).ToList();
        
        // Create 10 sample invoices
        for (int i = 1; i <= 10; i++)
        {
            var receiver = receivers[random.Next(receivers.Count)];
            var invoiceDate = DateTime.UtcNow.AddDays(-random.Next(1, 60));
            var lineCount = random.Next(1, 5);
            var invoiceId = Guid.NewGuid();
            
            var invoice = new InvoiceRecord
            {
                Id = invoiceId,
                SenderId = sender.Id,
                ReceiverId = receiver.Id,
                DocumentIdentifier = $"FAC-2026-{i:D4}",
                InvoiceNumber = i,
                DocumentTypeCode = "I-11",
                DocumentTypeName = "Facture",
                InvoiceDate = invoiceDate,
                DueDate = invoiceDate.AddDays(30),
                Status = (InvoiceStatus)random.Next(0, 6),
                CreatedAt = invoiceDate,
                UpdatedAt = DateTime.UtcNow
            };
            
            // Generate line items
            decimal totalHT = 0;
            decimal totalTVA = 0;
            
            var lines = new List<InvoiceLineRecord>();
            var products = GetSampleProducts();
            
            for (int j = 1; j <= lineCount; j++)
            {
                var product = products[random.Next(products.Count)];
                var qty = random.Next(1, 10);
                var lineTotal = product.Price * qty;
                var lineTax = lineTotal * (product.TaxRate / 100);
                
                lines.Add(new InvoiceLineRecord
                {
                    Id = Guid.NewGuid(),
                    InvoiceId = invoiceId,
                    LineNumber = j,
                    ItemCode = product.Code,
                    ItemDescription = product.Description,
                    Quantity = qty,
                    MeasurementUnit = product.Unit,
                    UnitPriceExcludingTax = product.Price,
                    TotalExcludingTax = lineTotal,
                    TaxRate = product.TaxRate,
                    TaxAmount = lineTax
                });
                
                totalHT += lineTotal;
                totalTVA += lineTax;
            }
            
            // Add stamp duty for invoices > 1000 TND
            decimal stampDuty = totalHT > 1000 ? 1.000m : 0.600m;
            
            invoice.TotalExcludingTax = totalHT;
            invoice.TotalTaxAmount = totalTVA;
            invoice.StampDuty = stampDuty;
            invoice.TotalIncludingTax = totalHT + totalTVA + stampDuty;
            invoice.LineItems = lines;
            
            // Add tax summary
            var taxSummary = lines
                .GroupBy(l => l.TaxRate)
                .Select(g => new InvoiceTaxRecord
                {
                    Id = Guid.NewGuid(),
                    InvoiceId = invoiceId,
                    TaxTypeCode = "V",
                    TaxTypeName = "TVA",
                    TaxRate = g.Key,
                    TaxableBase = g.Sum(l => l.TotalExcludingTax),
                    TaxAmount = g.Sum(l => l.TaxAmount)
                })
                .ToList();
            
            invoice.Taxes = taxSummary;
            
            invoices.Add(invoice);
        }
        
        return invoices;
    }
    
    private static List<(string Code, string Description, decimal Price, decimal TaxRate, string Unit)> GetSampleProducts()
    {
        return new List<(string, string, decimal, decimal, string)>
        {
            ("PROD001", "Ordinateur portable HP ProBook 450 G8", 2500.000m, 19, "UNIT"),
            ("PROD002", "Écran LED 24 pouces Samsung", 450.000m, 19, "UNIT"),
            ("PROD003", "Clavier sans fil Logitech K380", 89.000m, 19, "UNIT"),
            ("PROD004", "Souris optique Microsoft", 45.000m, 19, "UNIT"),
            ("PROD005", "Câble HDMI 2m", 25.000m, 19, "UNIT"),
            ("SERV001", "Service maintenance informatique", 150.000m, 19, "HOUR"),
            ("SERV002", "Formation bureautique (journée)", 350.000m, 19, "DAY"),
            ("SERV003", "Développement logiciel personnalisé", 500.000m, 19, "HOUR"),
            ("CONS001", "Cartouche d'encre HP 305 Noir", 65.000m, 19, "UNIT"),
            ("CONS002", "Ramette papier A4 (500 feuilles)", 18.000m, 7, "PACK"),
            ("CONS003", "Classeur à levier A4", 8.500m, 7, "UNIT"),
            ("FOOD001", "Café premium (1kg)", 45.000m, 7, "KG"),
            ("FOOD002", "Eau minérale (pack 6x1.5L)", 4.500m, 7, "PACK")
        };
    }
}
