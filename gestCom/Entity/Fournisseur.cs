using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Windows.Forms;
using System.Data;
using T4C_Commercial_Project.DAL;
using System.Collections;
using System.Globalization;
using System.Data.Odbc;

namespace T4C_Commercial_Project.Entity
{
    public class Fournisseur
    {
        // les attributs:
        public static string Separateur = CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator;
        public String code_fournisseur;
        public String matriculefiscale_fournisseur;
        public String nom_fournisseur;
        public String typepersonne_fournisseur;
        public String typeentreprise_fournisseur;
        public String rib_fournisseur;
        public String adresse_fournisseur;
        public String codepostal_fournisseur;
        public String ville_fournisseur;
        public String pays_fournisseur;
        public String tel_fournisseur;
        public String telmobile_fournisseur;
        public String fax_fournisseur;
        public String email_fournisseur;
        public String site_fournisseur;
        public String etat_fournisseur;
        public int    nbtransactions_fournisseur;
        public String note_fournisseur;
        public String etranger_fournisseur;
        public String exonore_fournisseur;
        public Double maxcredit_fournisseur;
        public int code_devise;

        //constructeur par défaut:
        public Fournisseur() { }

        //constructeur avec 1 seul paramètres( code_Fournisseur):
        public Fournisseur(string _code_Fournisseur)
        {
            code_fournisseur = _code_Fournisseur;
        }

        //constructeur avec paramètres:
        public Fournisseur(string _code_Fournisseur, String _matricule_fiscale_Fournisseur, String _nom_Fournisseur,
            String _typepersonne_fournisseur, String _typeentreprise_fournisseur, String _rib_Fournisseur,
            String _adresse_Fournisseur, String _codePostal_Fournisseur, String _ville_Fournisseur, String _pays_Fournisseur,
            String _tel_Fournisseur, String _telmobile_Fournisseur, String _email_Fournisseur, String _siteInternet_Fournisseur,
            String _fax_Fournisseur, String _etat_Fournisseur, int _nb_transactions_Fournisseur, String _note_Fournisseur,
            String _etranger_fournisseur, String _exonore_fournisseur, Double _maxcredit_fournisseur, int _code_devise)
        {
            code_fournisseur = _code_Fournisseur;
            matriculefiscale_fournisseur = _matricule_fiscale_Fournisseur;
            nom_fournisseur = _nom_Fournisseur;
            typepersonne_fournisseur = _typepersonne_fournisseur;
            typeentreprise_fournisseur = _typeentreprise_fournisseur;
            rib_fournisseur = _rib_Fournisseur;
            adresse_fournisseur = _adresse_Fournisseur;
            codepostal_fournisseur = _codePostal_Fournisseur;
            ville_fournisseur = _ville_Fournisseur;
            pays_fournisseur = _pays_Fournisseur;
            tel_fournisseur = _tel_Fournisseur;
            telmobile_fournisseur = _telmobile_Fournisseur;
            fax_fournisseur = _fax_Fournisseur;
            email_fournisseur = _email_Fournisseur;
            site_fournisseur = _siteInternet_Fournisseur;
            etat_fournisseur = _etat_Fournisseur;
            nbtransactions_fournisseur = _nb_transactions_Fournisseur;
            note_fournisseur = _note_Fournisseur;
            etranger_fournisseur = _etranger_fournisseur;
            exonore_fournisseur = _exonore_fournisseur;
            maxcredit_fournisseur = _maxcredit_fournisseur;
            code_devise = _code_devise;
        }

        // les méthodes:

        public Boolean ajoutFournisseur()
        {
            string CommandText = "insert into  " + DAL.DataBaseTableName.TableFournisseur +
                 "  values ( '" + this.code_fournisseur + "' " +
                 " , '" + this.matriculefiscale_fournisseur + "' " +
                 " , '" + this.nom_fournisseur.ToString().Replace("'", "''") + "' " +
                 " , '" + this.typepersonne_fournisseur.ToString().Replace("'", "''") + "' " +
                 " , '" + this.typeentreprise_fournisseur.ToString().Replace("'", "''") + "' " +
                 " , '" + this.rib_fournisseur + "' " +
                 " , '" + this.adresse_fournisseur.ToString().Replace("'", "''") + "' " +
                 " , '" + this.codepostal_fournisseur + "' " +
                 " , '" + this.ville_fournisseur.ToString().Replace("'", "''") + "' " +
                 " , '" + this.pays_fournisseur + "' " +
                 " , '" + this.tel_fournisseur + "' " +
                 " , '" + this.telmobile_fournisseur + "' " +
                 " , '" + this.fax_fournisseur + "' " +
                 " , '" + this.email_fournisseur + "' " +
                 " , '" + this.site_fournisseur + "' " +
                 " , '" + this.etat_fournisseur + "' " +
                 " ,  " + this.nbtransactions_fournisseur +
                 " , '" + this.note_fournisseur.ToString().Replace("'", "''") + "' " +
                 " , '" + this.etranger_fournisseur + "' " +
                 " , '" + this.exonore_fournisseur + "' " +
                 " ,  " + this.maxcredit_fournisseur +
                 " ,  " + this.code_devise + ")";
            return DataBaseConnexion.addOrUpdateElementInDataBase(CommandText, Program.SelectGlobalMessages.ImpUpdateFournisseur);
        }

        public Boolean modifierFournisseur()
        {
            string CommandText = "update " + DAL.DataBaseTableName.TableFournisseur + " set " +
                   " matriculefiscale_fournisseur = '" + this.matriculefiscale_fournisseur + "' " +
                   " , nom_fournisseur = '" + this.nom_fournisseur.ToString().Replace("'", "''") + "' " +
                   " , typepersonne_fournisseur = '" + this.typepersonne_fournisseur.ToString().Replace("'", "''") + "' " +
                   " , typeentreprise_fournisseur = '" + this.typeentreprise_fournisseur.ToString().Replace("'", "''") + "' " +
                   " , rib_fournisseur = '" + this.rib_fournisseur + "' " +
                   " , adresse_fournisseur = '" + this.adresse_fournisseur.ToString().Replace("'", "''") + "' " +
                   " , codePostal_fournisseur =  '" + this.codepostal_fournisseur + "' " +
                   " , ville_fournisseur = '" + this.ville_fournisseur.ToString().Replace("'", "''") + "' " +
                   " , pays_fournisseur = '" + this.pays_fournisseur + "' " +
                   " , tel_fournisseur = '" + this.tel_fournisseur + "' " +
                   " , telmobile_fournisseur = '" + this.telmobile_fournisseur + "' " +
                   " , email_fournisseur = '" + this.email_fournisseur + "' " +
                    " , site_fournisseur = '" + this.site_fournisseur + "' " +
                   " , fax_fournisseur = '" + this.fax_fournisseur + "' " +
                   " , etat_fournisseur = '" + this.etat_fournisseur + "' " +
                //" , nbtransactions_fournisseur = " + this.nbtransactions_fournisseur +
                   " , note_fournisseur = '" + this.note_fournisseur.ToString().Replace("'", "''") + "' " +
                   " , etranger_fournisseur = '" + this.etranger_fournisseur + "' " +
                   " , exonore_fournisseur = '" + this.exonore_fournisseur + "' " +
                   " , maxcredit_fournisseur = " + this.maxcredit_fournisseur +
                   " , code_devise = " + this.code_devise +
                   "  where code_fournisseur = '" + this.code_fournisseur + "' ";
            return DataBaseConnexion.addOrUpdateElementInDataBase(CommandText, Program.SelectGlobalMessages.ImpUpdateFournisseur);
        }

        public static Boolean supprimerFournisseur(string _code_fournisseur)
        {
            string CommandText = "DELETE FROM " + DAL.DataBaseTableName.TableFournisseur +
                " WHERE code_fournisseur ='" + _code_fournisseur + "';";
            return DataBaseConnexion.addOrUpdateElementInDataBase(CommandText, Program.SelectGlobalMessages.ImpDeleteFournisseur);
        }

        public static Boolean supprimerAllFournisseur()
        {
            string CommandText = "DELETE FROM " + DAL.DataBaseTableName.TableFournisseur +
                 " WHERE code_fournisseur <> '1';";
            return DataBaseConnexion.addOrUpdateElementInDataBase(CommandText, Program.SelectGlobalMessages.ImpDeleteFournisseur);
        }

        public static Fournisseur getFournisseur(string _code_fournisseur)
        {
            Fournisseur fournisseur = null;
            try
            {

                if (DataBaseConnexion.getItemsCountOfEntity(DAL.DataBaseTableName.TableFournisseur) > 0)
                {
                    OdbcConnection connection = DAL.DataBaseConnexion.getConnection();
                    //try
                    //{
                    OdbcCommand cmd = connection.CreateCommand();
                    cmd.CommandText = "select * from  " + DAL.DataBaseTableName.TableFournisseur +
                    " where code_fournisseur = '" + _code_fournisseur + "'";

                    OdbcDataReader Reader = cmd.ExecuteReader();
                    if (Reader.Read())
                    {
                        fournisseur = new Fournisseur(Reader.GetString(0), Reader.GetString(1), Reader.GetString(2),
                        Reader.GetString(3), Reader.GetString(4), Reader.GetString(5), Reader.GetString(6),
                        Reader.GetString(7), Reader.GetString(8), Reader.GetString(9), Reader.GetString(10),
                        Reader.GetString(11), Reader.GetString(12), Reader.GetString(13), Reader.GetString(14),
                        Reader.GetString(15), Reader.GetInt32(16), Reader.GetString(17), Reader.GetString(18),
                        Reader.GetString(19), Reader.GetDouble(20), Reader.GetInt32(21));
                    }
                    Reader.Close();

                }
            }
            catch (OdbcException e)
            {
                MessageBox.Show(e.Message, Program.SelectGlobalMessages.SelectFournisseur,
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            //catch
            //{
            //    MessageBox.Show(Program.SelectGlobalMessages.ImpSelectFournisseur,
            //     Program.SelectGlobalMessages.SelectFournisseur, MessageBoxButtons.OK, MessageBoxIcon.Error);
            //}
            //  }
            return fournisseur;
        }

        public static Fournisseur getFournisseurByMatriculeFiscale(string _matriculefiscale_fournisseur)
        {
            Fournisseur fournisseur = null;
            try
            {

                if (DataBaseConnexion.getItemsCountOfEntity(DAL.DataBaseTableName.TableFournisseur) > 0)
                {
                    OdbcConnection connection = DAL.DataBaseConnexion.getConnection();
                    //try
                    //{
                    OdbcCommand cmd = connection.CreateCommand();
                    cmd.CommandText = "select * from  " + DAL.DataBaseTableName.TableFournisseur +
                    " where matriculefiscale_fournisseur = '" + _matriculefiscale_fournisseur + "'";

                    OdbcDataReader Reader = cmd.ExecuteReader();
                    if (Reader.Read())
                    {
                        fournisseur = new Fournisseur(Reader.GetString(0), Reader.GetString(1), Reader.GetString(2),
                        Reader.GetString(3), Reader.GetString(4), Reader.GetString(5), Reader.GetString(6),
                        Reader.GetString(7), Reader.GetString(8), Reader.GetString(9), Reader.GetString(10),
                        Reader.GetString(11), Reader.GetString(12), Reader.GetString(13), Reader.GetString(14),
                        Reader.GetString(15), Reader.GetInt32(16), Reader.GetString(17), Reader.GetString(18),
                        Reader.GetString(19), Reader.GetDouble(20), Reader.GetInt32(21));
                    }
                    Reader.Close();

                }
            }
            catch (OdbcException e)
            {
                MessageBox.Show(e.Message, Program.SelectGlobalMessages.SelectFournisseur,
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            //catch
            //{
            //    MessageBox.Show(Program.SelectGlobalMessages.ImpSelectFournisseur,
            //     Program.SelectGlobalMessages.SelectFournisseur, MessageBoxButtons.OK, MessageBoxIcon.Error);
            //}
            //  }
            return fournisseur;
        }

        public static Boolean IncrementerNombreTransactions(string _code_fournisseur)
        {
            string CommandText = "update " + DAL.DataBaseTableName.TableFournisseur +
                    " set nbtransactions_fournisseur = nbtransactions_fournisseur  + 1 " +
                    " where code_fournisseur = '" + _code_fournisseur + "'";
            return DataBaseConnexion.addOrUpdateElementInDataBase(CommandText, Program.SelectGlobalMessages.ImpUpdateFournisseur);
        }

        public static Boolean updateEtat(string _code_fournisseur, string _etat)
        {
            string CommandText = "Update " + DAL.DataBaseTableName.TableFournisseur +
                      " Set etat_fournisseur ='" + _etat + "' " +
                      " Where code_fournisseur ='" + _code_fournisseur + "'";
            return DataBaseConnexion.addOrUpdateElementInDataBase(CommandText, Program.SelectGlobalMessages.ImpUpdateFournisseur);
        }

        public static string getCodeFournisseur(string _nameFournisseur)
        {
            string codeFournisseur = "";
            if (DataBaseConnexion.getRowsCount(DAL.DataBaseTableName.TableFournisseur, "code_fournisseur") != 0)
            {
                OdbcConnection connection = DataBaseConnexion.getConnection();
                try
                {
                    OdbcCommand cmd = connection.CreateCommand();
                    cmd.CommandText = "select code_fournisseur from " + DataBaseTableName.TableFournisseur +
                                      " where nom_fournisseur = '" + _nameFournisseur + "'";
                    OdbcDataReader Reader = cmd.ExecuteReader();
                    if (Reader.Read())
                    {
                        codeFournisseur = Reader.GetString(0);
                    }
                    Reader.Close();
                }
                catch (OdbcException e)
                {
                    MessageBox.Show(e.Message, Program.SelectGlobalMessages.SelectFournisseur,
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            return codeFournisseur;
        }
        public ArrayList getAllReglementssFournisseur(int _code_Fournisseur)
        {
            ArrayList listeReg = new ArrayList();
            ReglementFournisseur RegFourni = new ReglementFournisseur();
            OdbcConnection connection = DAL.DataBaseConnexion.getConnection();

            try
            {
                OdbcCommand cmd = connection.CreateCommand();
                cmd.CommandText = "select  *  from  " + DAL.DataBaseTableName.TableReglementFactureFournisseur +
                     " where code_fournisseur='" + _code_Fournisseur + "' ";
                // " where code_fournisseur=" + _code_Fournisseur + " AND   date_echeance_reglement  > '" + _NewDate + "'";
                // ORDER BY date_echeance_reglement DESC code_reglement


                OdbcDataReader Reader = cmd.ExecuteReader();


                while (Reader.Read())
                {
                    RegFourni = new ReglementFournisseur(Reader.GetInt32(0),
                            Reader.GetString(1),
                            Reader.GetDouble(2),
                            Reader.GetString(3),
                            Reader.GetString(4),
                            Reader.GetString(5),
                            Reader.GetString(6),
                            Reader.GetString(7),
                            Reader.GetString(8),
                             Reader.GetString(9)
                            );
                    listeReg.Add(RegFourni);
                }
                Reader.Close();

            }
            catch (OdbcException e)
            {
                MessageBox.Show(e.Message, Program.SelectGlobalMessages.SelectReglement,
                    MessageBoxButtons.OK, MessageBoxIcon.Error);

            }
            catch
            {
                MessageBox.Show(Program.SelectGlobalMessages.ImpSelectReglement,
                      Program.SelectGlobalMessages.SelectReglement, MessageBoxButtons.OK, MessageBoxIcon.Error);

            }
            if (listeReg.Count == 0)
                return null;
            else

                return listeReg;

        }
        public ArrayList getBRForFournisseur(string _code_Fournisseur)
        {
            ArrayList listeBrs = new ArrayList();
            BonReception BR = new BonReception();
            OdbcConnection connection = DAL.DataBaseConnexion.getConnection();

            //try  
            //{
            OdbcCommand cmd = connection.CreateCommand();
            string nn = "Non Facturé";
            cmd.CommandText = "select * from  " + DAL.DataBaseTableName.TableBonReception +
                              " where isFacture_bonreception ='" + nn +
                              "' and codefournisseur_bonreception= '" + _code_Fournisseur + "';";
            OdbcDataReader Reader = cmd.ExecuteReader();
            while (Reader.Read())
            {
                BR = new BonReception(Reader.GetString(0), Reader.GetString(1), Reader.GetString(2), Reader.GetString(3),
                        Reader.GetString(4), Reader.GetString(5), Reader.GetString(6), Reader.GetString(7), 
                        Reader.GetInt32(8), Reader.GetString(9));
                listeBrs.Add(BR);
            }
            Reader.Close();
            //}
            //catch (OdbcException e)
            //{
            //    MessageBox.Show(e.Message, Program.SelectGlobalMessages.SelectBonReception,
            //        MessageBoxButtons.OK, MessageBoxIcon.Error);
            //}
            //catch
            //{
            //    MessageBox.Show(Program.SelectGlobalMessages.ImpSelectClient,
            //          Program.SelectGlobalMessages.SelectClient, MessageBoxButtons.OK, MessageBoxIcon.Error);
            //}
            if (listeBrs.Count == 0)
                return null;
            else
                return listeBrs;
        }
    }
}
