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
    public partial class ReglementFournisseur
    {
        public static string Separateur = CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator;
        //les attributs d'un Reglement:              
        public int code_reglement;
        public string  code_fournisseur;
        public double montant_reglement;
        public string date_reglement;
        public string date_echeance_reglement;
        public string mode_reglement;
        public string description_reglement;
        public string banque_reglement;
        public string adresse_reglement;
        public string num_facture_fournisseur;

        // constructeur

        //les constructeurs:
        public ReglementFournisseur(int _codeR)
        {
            this.code_reglement = _codeR;
        }

        public ReglementFournisseur()
        {

        }

        public ReglementFournisseur(int _codeR, string  _codeFourni, double _montantR,
                            String _dateR, string _echeanceR, String _modeR,
                            String _idmodeR, string _banque, string _adresse, string _num_facture_fournisseur)
        {
            this.code_reglement = _codeR;
            this.code_fournisseur = _codeFourni;
            this.montant_reglement = _montantR;
            this.date_reglement = _dateR;
            this.date_echeance_reglement = _echeanceR;
            this.mode_reglement = _modeR;
            this.description_reglement = _idmodeR;
            this.banque_reglement = _banque;
            this.adresse_reglement = _adresse;
            this.num_facture_fournisseur = _num_facture_fournisseur;

        }


        /************************************************************************************/
        /************************************************************************************/

        /************************************************************************************/
        /************************************************************************************/
        //les methodes:
        public Boolean ajouterReglementFournisseur()
        {
            string CommandText = "insert into  " + DAL.DataBaseTableName.TableReglementFactureFournisseur +
                    "  values (" + this.code_reglement +
                    ", " + this.code_fournisseur +
                    ", " + this.montant_reglement.ToString().ToString().Replace(',', '.') +
                    ", '" + this.date_reglement +
                    "', '" + this.date_echeance_reglement +
                    "', '" + this.mode_reglement +
                    "', '" + this.description_reglement.ToString().Replace("'", "''") +
                    "', '" + this.banque_reglement.ToString().Replace("'", "''") +
                   "', '" + this.adresse_reglement.ToString().Replace("'", "''") +
                     "', '" + this.num_facture_fournisseur + "');";
            return DataBaseConnexion.addOrUpdateElementInDataBase(CommandText, Program.SelectGlobalMessages.ImpAddReglement);


        }


        /************************************************************************************/
        /************************************************************************************/
        public Boolean modifierReglementfacture()
        {
            string CommandText = "update " + DAL.DataBaseTableName.TableReglementFactureFournisseur +
                    " set code_fournisseur = " + this.code_fournisseur +
                    " , montant_reglement = " + this.montant_reglement.ToString().ToString().Replace(',', '.') +
                    " , date_reglement= '" + this.date_reglement + "' " +
                    " , date_echeance_reglement = '" + this.date_echeance_reglement + "' " +
                    " , mode_reglement ='" + this.mode_reglement + "' " +
                    ", description_reglement = '" + this.description_reglement.ToString().Replace("'", "''") + "'" +
                     ", banque_reglement = '" + this.banque_reglement.ToString().Replace("'", "''") + "'" +
                     ", adresse_reglement = '" + this.adresse_reglement.ToString().Replace("'", "''") + "'" +
                       ", num_facture_fournisseur = '" + this.num_facture_fournisseur + "'" +
                    " where code_reglement=" + this.code_reglement + ";";
            return DataBaseConnexion.addOrUpdateElementInDataBase(CommandText, Program.SelectGlobalMessages.ImpUpdateReglement);

        }
        /************************************************************************************/
        /************************************************************************************/
        public Boolean supprimerReglementFournisseur()
        {
            string CommandText = "delete from " + DAL.DataBaseTableName.TableReglementFactureFournisseur +
                    " where code_reglement=" + this.code_reglement + ";";

            return DataBaseConnexion.addOrUpdateElementInDataBase(CommandText, Program.SelectGlobalMessages.ImpDeleteReglement);

        }


        /************************************************************************************/
        /************************************************************************************/
        // lancer une requete qui retourne le BL avec le _codeBonLivraison
        public ReglementFournisseur getReglementfacture(int _codeReg)
        {
            ReglementFournisseur reglementFacture = null;
            if (DAL.DataBaseConnexion.getRowsCount(DAL.DataBaseTableName.TableReglementFactureFournisseur, "code_reglement") != 0)
            {
                OdbcConnection connection = DAL.DataBaseConnexion.getConnection();
                try
                {
                    OdbcCommand cmd = connection.CreateCommand();

                    cmd.CommandText = "select * from  " + DAL.DataBaseTableName.TableReglementFactureFournisseur +
                        " where code_reglement=" + _codeReg;
                    OdbcDataReader Reader = cmd.ExecuteReader();
                    if (Reader.Read())
                    {
                        reglementFacture = new ReglementFournisseur(Reader.GetInt32(0),
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

                    }
                    else
                        throw new Exception();

                    Reader.Close();


                }
                catch (OdbcException e)
                {
                    MessageBox.Show(e.Message, Program.SelectGlobalMessages.SelectReglement,
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return reglementFacture;
                }

            }
            return reglementFacture;
        }

        /************************************************************************************/
        /************************************************************************************/



        /************************************************************************************/
        /************************************************************************************/

        /************************************************************************************/
        /************************************************************************************/
        //recupérer tous les ligneBonLivraison pour un BonLivraison dont son code est passé en paramétre
        public ArrayList getAllReglementFournisseur(string  _codeFournisseur)
        {
            ArrayList tab_reglement = new ArrayList();
            ReglementFournisseur Reglement = null;
          
                OdbcConnection connection = DAL.DataBaseConnexion.getConnection();
                try
                {
                    OdbcCommand cmd = connection.CreateCommand();
                    cmd.CommandText = "select * from  " + DAL.DataBaseTableName.TableReglementFactureFournisseur +
                                      " where codefournisseur_reglement='" + _codeFournisseur + "'";
                    OdbcDataReader Reader = cmd.ExecuteReader();
                    while (Reader.Read())
                    {
                        Reglement = new ReglementFournisseur(Reader.GetInt32(0),
                                                             Reader.GetString(1),
                                                             Reader.GetDouble(2),
                                                             Reader.GetString(3),
                                                             Reader.GetString(4),
                                                             Reader.GetString(5),
                                                             Reader.GetString(6),
                                                             Reader.GetString(7),
                                                             Reader.GetString(8),
                                                             Reader.GetString(9));                                                                      
                        tab_reglement.Add(Reglement);
                    }
                    Reader.Close();

                  
                }
                catch (OdbcException e)
                {
                    MessageBox.Show(e.Message, Program.SelectGlobalMessages.SelectReglement,
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

                if (tab_reglement.Count == 0)
                    return null;
                else
                    return tab_reglement;
        }


        /************************************************************************************/
        /************************************************************************************/
        // elle renvoie le numero maximal des numeros des bons livraisons dans la base:
        public int getMaxNumReglementFournisseur()
        {
            int maxNumReglement = 0;
            if (DAL.DataBaseConnexion.getRowsCount(DAL.DataBaseTableName.TableReglementFactureFournisseur, "code_reglement") != 0)
            {
                OdbcConnection connection = DAL.DataBaseConnexion.getConnection();
                try
                {
                    OdbcCommand cmd = connection.CreateCommand();

                    cmd.CommandText = "select max(code_reglement) from " +
                        DAL.DataBaseTableName.TableReglementFactureFournisseur;


                    OdbcDataReader reader = cmd.ExecuteReader();
                    if (reader.Read())
                    {
                        maxNumReglement = reader.GetInt32(0);
                    }
                    else
                    { throw new Exception(); }

                    reader.Close();
                    return (maxNumReglement);
                }
                catch (OdbcException e)
                {
                    MessageBox.Show(e.Message, Program.SelectGlobalMessages.SelectReglement,
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return maxNumReglement;
                }
                /*catch (Exception)
                {
                    MessageBox.Show(Program.SelectGlobalMessages.ImpSelectReglement,
                         Program.SelectGlobalMessages.SelectReglement, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return maxNumReglement;
                }*/


            }

            return maxNumReglement;
        }


        /************************************************************************************/
        /************************************************************************************/






    }

}
