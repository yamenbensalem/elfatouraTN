# elfatouraTN

Backend (ASP.NET Core 9.0)
1. Entity Framework Core Persistence

EInvoiceDbContext.cs - DbContext with SQL Server
Client.cs - Company/sender entity with TTN config
InvoiceRecord.cs - Invoice persistence entity
Repository interfaces and implementations for CRUD operations
2. QR Code Generation (QRCoder)

QrCodeService.cs - El Fatoora format: MF|NumFac|Date|TotalTTC
3. PDF Generation (QuestPDF)

PdfGeneratorService.cs - Professional invoice layout with embedded QR codes
4. TTN WebService Integration

TtnService.cs - SOAP client for invoice submission/validation
Frontend (Angular 20)
5. Angular Frontend (einvoice-frontend/)

Invoice list, form, and detail components
Client list and form components
Services for API communication
Routing with lazy loading
Custom CSS utility classes

To Run the Applications
Backend:

'''
cd ClaudePrjt\TunisianEInvoice\src\TunisianEInvoice.API
dotnet run
# API runs on http://localhost:5230
'''

Frontend:
'''
cd ClaudePrjt\einvoice-frontend
npm start
# Opens on http://localhost:4200
'''