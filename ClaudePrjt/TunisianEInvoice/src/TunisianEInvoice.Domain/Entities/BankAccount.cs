namespace TunisianEInvoice.Domain.Entities
{
    public class BankAccount
    {
        public string FunctionCode { get; set; }
        public string AccountNumber { get; set; }
        public string OwnerIdentifier { get; set; }
        public string BankCode { get; set; }
        public string BranchIdentifier { get; set; }
        public string InstitutionName { get; set; }
    }
}
