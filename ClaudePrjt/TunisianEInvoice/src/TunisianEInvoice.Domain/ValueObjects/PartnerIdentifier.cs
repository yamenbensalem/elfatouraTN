using System.Text.RegularExpressions;

namespace TunisianEInvoice.Domain.Entities
{
    public class PartnerIdentifier
    {
        public string Type { get; set; } // I-01, I-02, I-03, I-04
        public string Value { get; set; }

        public bool IsValid()
        {
            return Type switch
            {
                "I-01" => IsValidMatriculeFiscale(Value),
                "I-02" => IsValidCIN(Value),
                "I-03" => IsValidCarteSejour(Value),
                _ => !string.IsNullOrEmpty(Value)
            };
        }

        private bool IsValidMatriculeFiscale(string value)
        {
            if (string.IsNullOrEmpty(value) || value.Length != 13)
                return false;
            
            // Format: 7 chiffres + 1 lettre + 1 lettre + 1 lettre + 3 zéros
            return Regex.IsMatch(
                value, 
                @"^[0-9]{7}[ABCDEFGHJKLMNPQRSTVWXYZ][ABDNP][CMNP][0]{3}$"
            );
        }

        private bool IsValidCIN(string value)
        {
            return !string.IsNullOrEmpty(value) && 
                   value.Length == 8 && 
                   Regex.IsMatch(value, @"^[0-9]{8}$");
        }

        private bool IsValidCarteSejour(string value)
        {
            return !string.IsNullOrEmpty(value) && 
                   value.Length == 9 && 
                   Regex.IsMatch(value, @"^[0-9]{9}$");
        }
    }
}
