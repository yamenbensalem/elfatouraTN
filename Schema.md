┌─────────────────────┐
│    Enterprises      │
├─────────────────────┤
│ Id (PK)             │
│ Name                │
│ MatriculeFiscal     │
│ SubscriptionPlan    │
│ IsActive            │
└─────────────────────┘
         │
         │ 1:N
         ▼
┌─────────────────────┐     ┌─────────────────────┐
│      Users          │     │      Products       │
├─────────────────────┤     ├─────────────────────┤
│ Id (PK)             │     │ Id (PK)             │
│ EnterpriseId (FK)   │     │ EnterpriseId (FK)   │
│ Email               │     │ Code, Name          │
│ Role                │     │ TaxRate             │
└─────────────────────┘     └─────────────────────┘
         │                           │
         │                           │ 1:N (optional)
         ▼                           ▼
┌─────────────────────┐     ┌─────────────────────┐
│      Clients        │────▶│   InvoiceRecords    │
├─────────────────────┤     ├─────────────────────┤
│ Id (PK)             │     │ Id (PK)             │
│ EnterpriseId (FK)   │     │ EnterpriseId (FK)   │
│ MatriculeFiscal     │     │ SenderId, ReceiverId│
│ Name, Address       │     │ Status, Totals      │
└─────────────────────┘     └─────────────────────┘