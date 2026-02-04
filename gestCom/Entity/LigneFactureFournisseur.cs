using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Data;
using T4C_Commercial_Project.DAL;
using System.Globalization;
using System.Collections;
using System.Data.Odbc;

namespace T4C_Commercial_Project.Entity
{
    public class LigneFactureFournisseur
    {
        public static string Separateur = CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator;


        public int    numero_lignefacturefournisseur;
        public int    numero_facturefournisseur;
        public string codeproduit_lignefacturefournisseur;
        public string designationproduit_lignefacturefournisseur;
        public double fodecproduit_lignefacturefournisseur;
        public double quantite_lignefacturefournisseur;
        public double prixunitaire_lignefacturefournisseur;
        public double montantHT_lignefacturefournisseur;
        public double tvaproduit_lignefacturefournisseur;
        public double remise_lignefacturefournisseur;
        public int    codeBonReception_lignefacturefournisseur;

        public LigneFactureFournisseur(int _numligne, int _numeroFacture, string _codeProduit, string _designation,double _fodec, double _quantite,
                                       double _prixunitaire, double _montantHT, double _tvaProduit, double _remise,int _codebonreception)
        {
            this.numero_lignefacturefournisseur = _numligne;
            this.numero_facturefournisseur = _numeroFacture;
            this.codeproduit_lignefacturefournisseur = _codeProduit;
            this.designationproduit_lignefacturefournisseur = _designation;
            this.fodecproduit_lignefacturefournisseur = _fodec;
            this.quantite_lignefacturefournisseur = _quantite;
            this.prixunitaire_lignefacturefournisseur = _prixunitaire;
            this.montantHT_lignefacturefournisseur = _montantHT;
            this.tvaproduit_lignefacturefournisseur = _tvaProduit;
            this.remise_lignefacturefournisseur = _remise;
            this.codeBonReception_lignefacturefournisseur = _codebonreception;
        }
        //*************************************************************************
        //************************************************************************
        public LigneFactureFournisseur(int   _numeroFacture)
        {
            this.numero_facturefournisseur = _numeroFacture;
        }
        public LigneFactureFournisseur()
        {

        }
        //*************************************************************************
        //************************************************************************
        // ajouterDevisClientFormSuivantBL un currentProduit à la devisClient Fournisseur courante:
        public Boolean AjouterLigneFacture()
        {
           string CommandText = "INSERT INTO "+  DAL.DataBaseTableName.TableLigneFactureFournisseur+ 
               " VALUES(" +
                this.numero_lignefacturefournisseur +","+
                this.numero_facturefournisseur + ",'" +
                this.codeproduit_lignefacturefournisseur.ToString().ToString() + "','" +
                this.designationproduit_lignefacturefournisseur.ToString().ToString() + "'," +
                this.fodecproduit_lignefacturefournisseur + "," +
                this.quantite_lignefacturefournisseur.ToString().ToString().Replace(',', '.') + "," +
                this.prixunitaire_lignefacturefournisseur.ToString().ToString().Replace(',', '.') + "," +
                this.montantHT_lignefacturefournisseur.ToString().ToString().Replace(',', '.') + "," +
                this.tvaproduit_lignefacturefournisseur.ToString().ToString().Replace(',', '.') + "," +
                this.remise_lignefacturefournisseur.ToString().ToString().Replace(',', '.') + "," +
                this.codeBonReception_lignefacturefournisseur +
                    ");";   
            return DataBaseConnexion.addOrUpdateElementInDataBase(CommandText, Program.SelectGlobalMessages.ImpAddLigneFactureFournisseur);
        }
        //*************************************************************************
        //************************************************************************
        //modifier une ligne dans la devisClient Fournisseur courante:
       public Boolean ModifierLigneFacture()
       {
            string CommandText = "update " + DAL.DataBaseTableName.TableLigneFactureFournisseur +
                      "  set " +
                      "  quantite_lignefacturefournisseur = " + this.quantite_lignefacturefournisseur.ToString().ToString().Replace(',', '.') +
                      ", prixunitaire_lignefacturefournisseur = " + this.prixunitaire_lignefacturefournisseur.ToString().ToString().Replace(',', '.') +
                      ", montantHT_lignefacturefournisseur = " + this.montantHT_lignefacturefournisseur.ToString().ToString().Replace(',', '.') +
                      ", tvaproduit_lignefacturefournisseur = " + this.tvaproduit_lignefacturefournisseur.ToString().ToString().Replace(',', '.') +
                      ", remise_lignefacturefournisseur = " + this.remise_lignefacturefournisseur.ToString().ToString().Replace(',', '.') +
                      ", designationproduit_lignefacturefournisseur='" + this.designationproduit_lignefacturefournisseur.ToString().Replace("'", "''") + "'" +
                      "  where numero_facturefournisseur = " + this.numero_facturefournisseur +
                      "  and num_lignefacturefournisseur = " + this.numero_lignefacturefournisseur +
                      "  and codeproduit_lignefacturefournisseur = '" + this.codeproduit_lignefacturefournisseur + "';";
            return DataBaseConnexion.addOrUpdateElementInDataBase(CommandText, Program.SelectGlobalMessages.ImpUpdateLigneFactureFournisseur);
        }
        //*************************************************************************
        //************************************************************************
        //supprimerSelectedFournisseur la ligne courante de la devisClient Fournisseur courante
        public Boolean SupprimerLigneFacture()
        {
           string CommandText = "delete from " + DAL.DataBaseTableName.TableLigneFactureFournisseur +
                " where numero_facturefournisseur = " + this.numero_facturefournisseur +
                " and num_lignefacturefournisseur = " + this.numero_lignefacturefournisseur +
                " and codeproduit_lignefacturefournisseur = '" + this.codeproduit_lignefacturefournisseur + "';";
            return DataBaseConnexion.addOrUpdateElementInDataBase(CommandText, Program.SelectGlobalMessages.ImpDeleteLigneFactureFournisseur);
        }
        //*************************************************************************
        //************************************************************************
        //supprime toutes les lignes factures de la devisClient Fournisseur courante:
        public  Boolean SupprimerAllLigneFromFacture()
        {
            string CommandText = "delete from lignefacturefournisseur " +
                                 "where numero_facturefournisseur =   " + this.numero_facturefournisseur;
            return DataBaseConnexion.addOrUpdateElementInDataBase(CommandText, Program.SelectGlobalMessages.ImpDeleteLigneFactureFournisseur);
        }

        public static ArrayList getAllLignesFactureFournisseur(int _numFacture)
        {
            ArrayList listOfLignesFactureClient = null;
            LigneFactureFournisseur ligneFACT = null;
            OdbcConnection connection = DAL.DataBaseConnexion.getConnection();
            try
            {
                OdbcCommand cmd = connection.CreateCommand();
                cmd.CommandText = "select * from " + DAL.DataBaseTableName.TableLigneFactureFournisseur +
                                  " where numero_facturefournisseur="+ _numFacture;
                OdbcDataReader Reader = cmd.ExecuteReader();
                if (Reader.HasRows == true)
                {
                    listOfLignesFactureClient = new ArrayList();
                    while (Reader.Read())
                    {
                        ligneFACT = new LigneFactureFournisseur(Reader.GetInt32(0), Reader.GetInt32(1), Reader.GetString(2), Reader.GetString(3),
                                                           Reader.GetDouble(4), Reader.GetDouble(5), Reader.GetDouble(6), Reader.GetDouble(7),
                                                           Reader.GetDouble(8), Reader.GetDouble(9), Reader.GetInt32(10));
                        listOfLignesFactureClient.Add(ligneFACT);
                    }

                    Reader.Close();
                }
            }
            catch
            {
                MessageBox.Show(Program.SelectGlobalMessages.ImpSelectLigneFactureClient,
                                Program.SelectGlobalMessages.SelectBonReception, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            return listOfLignesFactureClient;
        }
    }
}
