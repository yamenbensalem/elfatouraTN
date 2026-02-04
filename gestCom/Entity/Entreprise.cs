using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using T4C_Commercial_Project.DAL;
using System.Globalization;
using System.Data.Odbc;
using System.Collections;

namespace T4C_Commercial_Project.Entity
{
    public class Entreprise
    {
        public static string Separateur = CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator;
        
        //les attributs:
        public string codeentreprise;
        public string matriculeFiscale_entreprise;
        public string raisonSociale_entreprise;
        public string nomCommercial_entreprise;
        public string typePersonne_entreprise;
        public string comptebancaire_entreprise;
        public string registrecommerce_entreprise; // c'est l'identifiant de l'entreprise.
        public double capitalSocial_entreprise; // la société de type personne morale admet un capital social.
        public string description_entreprise;
        public string adresse_entreprise;
        public string codepostal_entreprise;
        public string ville_entreprise;
        public string pays_entreprise;
        public string telfixe1_entreprise;
        public string telfixe2_entreprise;
        public string telmobile_entreprise;
        public string fax_entreprise;
        public string email_entreprise;
        public string site_entreprise;
        public string matricule_entreprise;
        public string codeTVA_entreprise;
        public string codecatego_entreprise;
        public string numetab_entreprise;
        public int assujittieTVA_entreprise;
        public int assujittieFodec_entreprise;        
        public int exonore_entreprise;
        public string codedouane_entreprise;
        public int code_devise;
        public string logo_entreprise;

        private string[] FuctionByClient =
              new string[] { "Entreprise", "Clientss", "Fournisseurs", "Stock", "DevisForm","factureAvoir", "BonsLivraison", "BonsSortie", "FacturesClient", 
                            "CommandesVente", "FacturesLibres", "BonsRetour", "BonsReception", "FacturesFournisseur", "DemandesPrix",
                            "CommandesAchat", "ReglementsFactureClient", "ReglementsFournisseur", "RetenueSources ", "Acomptes", 
                            "ApurementAcompte", "ApurementAvoirs", "EtatStock ", "EtatFournisseur", "Etats", "JournalVentess", 
                            "Balance", "Connect", "Accueiluser", "CreerSession", "Activation" };
        
        private string[] ListOfClient =
               new string[] { DAL.VariablesGlobales.ClientEnterpriseNameForSahbiNouira, DAL.VariablesGlobales.ClientEnterpriseNameForMariemDeCommerce };
              
        private Hashtable mListOfFunctionByClient;

        private ArrayList listOfFunction = new ArrayList();

        // Les méthodes:
        public Entreprise(string matriculeFiscale_entreprise, string raisonSociale_entreprise, string _nomCommercial_entreprise,
                            string _typePersonne_entreprise, string _comptebancaire_entreprise,
                            string _registrecommerce_entreprise, double _capitalSocial_entreprise, string _description_entreprise, 
                            string _adresse_entreprise, string _ville__entreprise, string _codepostal_entreprise, string _pays_entreprise,
                            string _telfixe1_entreprise, string _telfixe2_entreprise, string _telmobile_entreprise,
                            string _fax_entreprise, string _email_entreprise, string _site_entreprise, string _matricule_entreprise,
                            string _codeTVA_entreprise, string _codecatego_entreprise, string _numetab_entreprise,
                            int _assjtva_entreprise, int _assjFodec_entreprise, int _exonore_entreprise, string _codedouane_entreprise,
                            int _code_devise, string _logo_entreprise)
        {
            this.matriculeFiscale_entreprise = matriculeFiscale_entreprise;
            this.raisonSociale_entreprise = raisonSociale_entreprise;
            this.nomCommercial_entreprise = _nomCommercial_entreprise;
            this.typePersonne_entreprise = _typePersonne_entreprise;  
            this.comptebancaire_entreprise = _comptebancaire_entreprise;
            this.registrecommerce_entreprise = _registrecommerce_entreprise;
            this.capitalSocial_entreprise = _capitalSocial_entreprise;
            this.description_entreprise = _description_entreprise;
            this.adresse_entreprise = _adresse_entreprise;
            this.ville_entreprise = _ville__entreprise;
            this.codepostal_entreprise = _codepostal_entreprise;
            this.pays_entreprise = _pays_entreprise;
            this.telfixe1_entreprise = _telfixe1_entreprise;
            this.telfixe2_entreprise = _telfixe2_entreprise;
            this.telmobile_entreprise = _telmobile_entreprise;
            this.fax_entreprise = _fax_entreprise;
            this.email_entreprise = _email_entreprise;
            this.site_entreprise = _site_entreprise;            
            this.matricule_entreprise = _matricule_entreprise;
            this.codeTVA_entreprise = _codeTVA_entreprise; 
            this.codecatego_entreprise = _codecatego_entreprise; 
            this.numetab_entreprise = _numetab_entreprise;            
            this.assujittieTVA_entreprise = _assjtva_entreprise;
            this.assujittieFodec_entreprise = _assjFodec_entreprise;
            this.exonore_entreprise = _exonore_entreprise;
            this.codedouane_entreprise = _codedouane_entreprise;
            this.code_devise = _code_devise;
            this.logo_entreprise = _logo_entreprise;
            inializeFunctionnality();
        }

        public Entreprise()
        {
            inializeFunctionnality();
        }

        public Boolean ajouterEntreprise()
        {
            string CommandText = "insert into " + DAL.DataBaseTableName.TableEntreprise + " values ( " +
               " '" + this.matriculeFiscale_entreprise + "' " +
               "  , '" + this.raisonSociale_entreprise.ToString().Replace("'", "''") + "' " +
               " , '" + this.nomCommercial_entreprise + "'" +
               " , '" + this.typePersonne_entreprise + "'" +
               " , '" + this.comptebancaire_entreprise + "' " +
               " , '" + this.registrecommerce_entreprise + "' " +
               " ,  " + this.capitalSocial_entreprise +
               " , '" + this.description_entreprise.ToString().Replace("'", "''") + "' " +
               " , '" + this.adresse_entreprise.ToString().Replace("'", "''") + "' " +
               " , '" + this.ville_entreprise.ToString().Replace("'", "''") + "' " +
               " , '" + this.codepostal_entreprise.ToString().Replace("'", "''") + "' " +
               " , '" + this.pays_entreprise.ToString().Replace("'", "''") + "' " +
               " , '" + this.telfixe1_entreprise + "' " +
               " , '" + this.telfixe2_entreprise + "' " +
               " , '" + this.telmobile_entreprise + "' " +
               " , '" + this.fax_entreprise + "' " +
               " , '" + this.email_entreprise + "'" +
               " , '" + this.site_entreprise + "' " +
               " , '" + this.matricule_entreprise + "' " +
               " , '" + this.codeTVA_entreprise + "' " +
               " , '" + this.codecatego_entreprise + "' " +
               " , '" + this.numetab_entreprise + "' " +
               " ,  " + this.assujittieTVA_entreprise +
               " ,  " + this.assujittieFodec_entreprise +
               " ,  " + this.exonore_entreprise +
               " , '" + this.codedouane_entreprise + "'" +
               ", " + this.code_devise +
               ", '" + this.logo_entreprise + "'" +
               " ) ;";

            return DataBaseConnexion.addOrUpdateElementInDataBase(CommandText, Program.SelectGlobalMessages.ImpAddEntreprise);
        }      

        public void inializeFunctionnality()
        {
            mListOfFunctionByClient = new Hashtable();
            for (int i = 0; i < ListOfClient.Count(); i++)
            {
                if (DataBaseConnexion.mClientEnterprise == ListOfClient[i])
                    mListOfFunctionByClient.Add(DataBaseConnexion.mClientEnterprise, FuctionByClient);
            }
        }

        public bool IsAuthorizedFunction(string _menuItem)
        {
            if ((mListOfFunctionByClient != null) && (mListOfFunctionByClient.Contains(DataBaseConnexion.mClientEnterprise)))
            {
                string[] vFunctionByClient = (string[])mListOfFunctionByClient[DataBaseConnexion.mClientEnterprise];
                for (int i = 0; i < vFunctionByClient.Count(); i++)
                {
                    if (_menuItem == vFunctionByClient[i])
                        return true;
                }

            }
            return false;

        }

        public Boolean modifierEntreprise()
        {
            string CommandText = "Update " + DAL.DataBaseTableName.TableEntreprise + " Set " +
              "   matriculefiscale_entreprise = '" + this.matriculeFiscale_entreprise + "'" +
              " , raisonsociale_entreprise = '" + this.raisonSociale_entreprise.ToString().Replace("'", "''") + "' " +
              " , nomCommercial_entreprise = '" + this.nomCommercial_entreprise + "'" +
              " , typePersonne_entreprise = '" + this.typePersonne_entreprise + "'" +
              " , comptebancaire_entreprise = '" + this.comptebancaire_entreprise + "' " +
              " , capitalSocial_entreprise = " + this.capitalSocial_entreprise +
              " , description_entreprise = '" + this.description_entreprise.ToString().Replace("'", "''") + "' " +
              " , adresse_entreprise = '" + this.adresse_entreprise.ToString().Replace("'", "''") + "' " +
              " , ville_entreprise = '" + this.ville_entreprise.ToString().Replace("'", "''") + "' " +
              " , codepostal_entreprise = '" + this.codepostal_entreprise.ToString().Replace("'", "''") + "' " +
              " , pays_entreprise = '" + this.pays_entreprise.ToString().Replace("'", "''") + "' " +
              " , telfixe1_entreprise = '" + this.telfixe1_entreprise + "' " +
              " , telfixe2_entreprise = '" + this.telfixe2_entreprise + "' " +
              " , telmobile_entreprise = '" + this.telmobile_entreprise + "' " +
              " , fax_entreprise = '" + this.fax_entreprise + "' " +
              " , email_entreprise = '" + this.email_entreprise + "'" +
              " , site_entreprise ='" + this.site_entreprise + "' " +
              " , matricule_entreprise = '" + this.matricule_entreprise + "' " +
              " , codeTVA_entreprise = '" + this.codeTVA_entreprise + "' " +
              " , codecatego_entreprise = '" + this.codecatego_entreprise + "' " +
              " , numetab_entreprise = '" + this.numetab_entreprise + "'" +
              " , assjtva_entreprise = " + this.assujittieTVA_entreprise +
              " , assjFodec_entreprise = " + this.assujittieFodec_entreprise +
              " , exonore_entreprise = " + this.exonore_entreprise +
              " , codedouane_entreprise = '" + this.codedouane_entreprise + "'" +
              " , code_devise = " + this.code_devise +
              " , logo_entreprise = '" + this.logo_entreprise + "'" +
              " WHERE registrecommerce_entreprise = '" + this.registrecommerce_entreprise + "' "; 
            return DataBaseConnexion.addOrUpdateElementInDataBase(CommandText, Program.SelectGlobalMessages.ImpUpdateEntreprise);
        }

        public static Boolean supprimerEntreprise(String _registrecommerce_entreprise)
        {
            string CommandText = "DELETE FROM " + DataBaseTableName.TableEntreprise +
                    " WHERE registrecommerce_entreprise = '" + _registrecommerce_entreprise + "' ";
            return DataBaseConnexion.addOrUpdateElementInDataBase(CommandText, Program.SelectGlobalMessages.ImpDeleteClient);
        }

        public static Boolean supprimerAllEntreprise()
        {
            string CommandText = "DELETE FROM " + DataBaseTableName.TableEntreprise;
            return DataBaseConnexion.addOrUpdateElementInDataBase(CommandText, Program.SelectGlobalMessages.ImpDeleteClient);
        }
        public static Entreprise getFirstEntreprise()
        {
            Entreprise entreprise = null;

            OdbcConnection connection = DataBaseConnexion.getConnection();
            try
            {
                OdbcCommand cmd = connection.CreateCommand();
                cmd.CommandText = "Select * from  " + DAL.DataBaseTableName.TableEntreprise ;
                OdbcDataReader Reader = cmd.ExecuteReader();
                
                if (Reader.Read())
                {
                    entreprise = new Entreprise(Reader.GetString(0), Reader.GetString(1), Reader.GetString(2), Reader.GetString(3),
                    Reader.GetString(4), Reader.GetString(5), Reader.GetDouble(6), Reader.GetString(7), Reader.GetString(8),
                    Reader.GetString(9), Reader.GetString(10), Reader.GetString(11), Reader.GetString(12),
                    Reader.GetString(13), Reader.GetString(14), Reader.GetString(15), Reader.GetString(16), Reader.GetString(17),
                    Reader.GetString(18), Reader.GetString(19), Reader.GetString(20), Reader.GetString(21),
                    Reader.GetInt32(22), Reader.GetInt32(23), Reader.GetInt32(24), Reader.GetString(25), Reader.GetInt32(26),
                    Reader.GetString(27));
                }
                Reader.Close();
            }
            catch (OdbcException e)
            {
                MessageBox.Show(e.Message, Program.SelectGlobalMessages.SelectEntreprise,
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            //catch (Exception e)
            //{
            //    MessageBox.Show(Program.SelectGlobalMessages.ImpSelectEntreprise, Program.SelectGlobalMessages.SelectEntreprise,
            //         MessageBoxButtons.OK, MessageBoxIcon.Error);
            //}

            return entreprise;
        }

        public static Entreprise getEntreprise(String _registrecommerce_entreprise)
        {
            Entreprise entreprise = null;

            OdbcConnection connection = DataBaseConnexion.getConnection();
            try
            {
                OdbcCommand cmd = connection.CreateCommand();
                cmd.CommandText = "Select * from  " + DAL.DataBaseTableName.TableEntreprise +
                                    " Where registrecommerce_entreprise = '" + _registrecommerce_entreprise + "' ";
                OdbcDataReader Reader = cmd.ExecuteReader();

                if (Reader.Read())
                {
                    entreprise = new Entreprise(Reader.GetString(0), Reader.GetString(1), Reader.GetString(2), Reader.GetString(3),
                    Reader.GetString(4), Reader.GetString(5), Reader.GetDouble(6), Reader.GetString(7), Reader.GetString(8),
                    Reader.GetString(9), Reader.GetString(10), Reader.GetString(11), Reader.GetString(12),
                    Reader.GetString(13), Reader.GetString(14), Reader.GetString(15), Reader.GetString(16), Reader.GetString(17),
                    Reader.GetString(18), Reader.GetString(19), Reader.GetString(20), Reader.GetString(21),
                    Reader.GetInt32(22), Reader.GetInt32(23), Reader.GetInt32(24), Reader.GetString(25), Reader.GetInt32(26),
                    Reader.GetString(27));
                }
                Reader.Close();
            }
            catch (OdbcException e)
            {
                MessageBox.Show(e.Message, Program.SelectGlobalMessages.SelectEntreprise,
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            //catch (Exception e)
            //{
            //    MessageBox.Show(Program.SelectGlobalMessages.ImpSelectEntreprise, Program.SelectGlobalMessages.SelectEntreprise,
            //         MessageBoxButtons.OK, MessageBoxIcon.Error);
            //}

            return entreprise;
        }

        override
        public String ToString()
        {
            string chaine = "";

            chaine += "matriculeFiscale_entreprise = " + this.matriculeFiscale_entreprise + "\n";
            chaine += "raisonSociale_entreprise = " + this.raisonSociale_entreprise + "\n";
            chaine += "nomCommercial_entreprise  = " + this.nomCommercial_entreprise + "\n";
            chaine += "typePersonne_entreprise  = " + this.typePersonne_entreprise + "\n";
            chaine += "comptebancaire_entreprise = "+ this.comptebancaire_entreprise + "\n";
            chaine += "capitalSocial_entreprise  = " + this.capitalSocial_entreprise + "\n";
            chaine += "description_entreprise = "+ this.description_entreprise + "\n";
            chaine += "adresse_entreprise = " + this.adresse_entreprise + "\n";
            chaine += "telfixe1_entreprise = " + this.telfixe1_entreprise + "\n";
            chaine += "telfixe2_entreprise  = " + this.telfixe2_entreprise + "\n";
            chaine += "telmobile_entreprise  = " + this.telmobile_entreprise + "\n";
            chaine += "fax_entreprise  = " + this.fax_entreprise + "\n";
            chaine += "email_entreprise  = " + this.email_entreprise + "\n";
            chaine += "site_entreprise  = " + this.site_entreprise + "\n";
            chaine += "registrecommerce_entreprise  = " + this.registrecommerce_entreprise + "\n";
            chaine += "matricule_entreprise  = " + this.matricule_entreprise + "\n";
            chaine += "codeTVA_entreprise  = " + this.codeTVA_entreprise + "\n";
            chaine += "codecatego_entreprise  = " + this.codecatego_entreprise + "\n";
            chaine += "numetab_entreprise  = " + this.numetab_entreprise + "\n";
            chaine += "assjFodec_entreprise  = " + this.assujittieFodec_entreprise + "\n";
            chaine += "assjtva_entreprise  = " + this.assujittieTVA_entreprise + "\n";
            chaine += "codedouane_entreprise  = " + this.codedouane_entreprise + "\n";
            chaine += "code_devise  = " + this.code_devise;
            chaine += "logo_entreprise  = " + this.logo_entreprise; 

            return chaine;
        }
        
        public Boolean isExonoreTVA()
        {
            if(this.exonore_entreprise==1) return true; else return false;
        }

    }
}