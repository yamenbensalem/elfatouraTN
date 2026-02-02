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

5. elfatouraTN API (TunisianEInvoice.API/)
Controllers for Clients and Invoices
GET /api/clients - List all clients
GET /api/clients/{id} - Get client by ID
GET /api/clients/by-mf/{matriculeFiscal} - Get by MF
POST /api/clients - Create client
PUT /api/clients/{id} - Update client
DELETE /api/clients/{id} - Delete client
GET /api/invoice - List invoices (paged)
GET /api/invoice/{id} - Get invoice by ID
POST /api/invoice/generate - Generate invoice

# Running the Applications
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