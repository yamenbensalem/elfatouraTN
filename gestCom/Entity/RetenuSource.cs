using System;
using System.Collections.Generic;

namespace T4C_Commercial_Project.Entity
{  
    public class LignesRetenuSource : IEquatable<LignesRetenuSource>
    { 
        public int CodeDetailRetenuSource { get; set; }
        public int CodeRetenuSource { get; set; }
        public string CodeFactureDevisRetenuSource { get; set; }
        public string DescriptionRetenuSource { get; set; }
        public string MontantBrut { get; set; }
        public string Retenu { get; set; }
        public string NumchequeRetenuSource { get; set; }
        public string AgenceRetenuSource { get; set; }

        public bool Equals(LignesRetenuSource other)
        {
            return CodeDetailRetenuSource == other.CodeDetailRetenuSource;
        }
    }

    public class RetenuSource : IEquatable<RetenuSource>
    {
        public int CodeRetenuSource { get; set; } = -1;

        public string DescriptionRetenuSource { get; set; } = "Test Description retenu";
        public DateTime EcheanceRetenuSource { get; set; } = DateTime.Today;
        public DateTime CreationRetenuSource { get; set; } = DateTime.Today;
        public DateTime MisAjourRetenuSource { get; set; } = DateTime.Today;
        public int StatutDuRetenuSource { get; set; } = -1;
        public Fournisseur CodePayeur { get; set; } = new Fournisseur() { adresse_fournisseur = "Test", matriculefiscale_fournisseur = "Test Matr" };
        public Client CodeBeneficaire { get; set; } = new Client() { adresse_client = "Test", matriculefiscale_client = "Test Matr" };

        public List<LignesRetenuSource> DetailRetenuSources { get; set; }
        public int TypeRetenuASource { get; internal set; } = -1;
        public int TauxRetenuSource { get; internal set; } = -1;

        public bool Equals(RetenuSource other)
        {
            return EcheanceRetenuSource == other.EcheanceRetenuSource 
            && StatutDuRetenuSource == other.StatutDuRetenuSource 
                ;
        }
    }
}
