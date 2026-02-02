using System.Collections.Generic;

namespace TunisianEInvoice.Application.DTOs
{
    public class PartnerDto
    {
        public string IdentifierType { get; set; } // "I-01" (Matricule Fiscal)
        public string Identifier { get; set; } // "0736202XAM000"
        public string Name { get; set; }
        public AddressDto Address { get; set; }
        public List<ContactDto> Contacts { get; set; }
        
        // Pour le destinataire
        public string AccountMode { get; set; } // "SMTP"
        public string AccountRank { get; set; } // "P"
        public string Profile { get; set; } // "Salle Publique"
        public string ClientCode { get; set; } // "41115530"
        public string RegistrationNumber { get; set; } // "B154702000"
        public string LegalForm { get; set; } // "SA"
    }
}
