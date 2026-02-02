namespace TunisianEInvoice.Application.DTOs
{
    public class BankAccountDto
    {
        public string AccountNumber { get; set; }
        public string OwnerIdentifier { get; set; }
        public string BankCode { get; set; }
        public string BranchIdentifier { get; set; }
        public string InstitutionName { get; set; }
        public string FunctionCode { get; set; } // "I-141"
    }
}
