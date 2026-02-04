using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Data;using T4C_Commercial_Project.DAL;
using System.Collections;
using System.Globalization;
using System.Data.Odbc;

namespace T4C_Commercial_Project.Entity
{
    public partial class BonReception
    {
        public static string Separateur = CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator;

        //les attributs :       
        public string code_bonreception;
        public string codefournisseur_bonreception;
        public String date_bonreception;
        public String datereception_bonreception;
        public String adresse_bonreception;
        public String statut_bonreception;
        public String isFacture_bonreception;
        public String notes_bonreception;
        public double apayer_bonreception;
        public string numfact;
        // les constructeurs:
        public BonReception(string _code_bonreception)
        {
            code_bonreception = _code_bonreception;
        }

        public BonReception()
        {

        }

        public BonReception(string _codeBR, string _codefournisseur, String _dateBR, String _dateReception,
                            String _adresseReception, String _statutBR, String _isFacture, String _notesBR, double _apayerBR, string numfact)
        {
            this.code_bonreception = _codeBR;
            this.codefournisseur_bonreception = _codefournisseur;
            this.date_bonreception = _dateBR;
            this.datereception_bonreception = _dateReception;
            this.adresse_bonreception = _adresseReception;
            this.statut_bonreception = _statutBR;
            this.isFacture_bonreception = _isFacture;
            this.notes_bonreception = _notesBR;
            this.apayer_bonreception = _apayerBR;
            this.numfact = numfact;
        }

        // les méthodes:        
        public Boolean ajouterBonReception()
        {
            string CommandText = "insert into "+ DAL.DataBaseTableName.TableBonReception +
                    " values ( '" + code_bonreception + "' "+
                    " ,  '"  + codefournisseur_bonreception +"'"+
                    " , '"  + date_bonreception + "' " +
                    " , '"  + datereception_bonreception + "' " +
                    " , '"  + adresse_bonreception.ToString().Replace("'", "''") + "' " +
                    " , '"  + statut_bonreception + "' " +
                    " , '"  + isFacture_bonreception + "' " +
                    " , '"  + notes_bonreception.ToString().Replace("'", "''") + "' " +
                    " ,  "  + apayer_bonreception.ToString().Replace(',', '.') +
                    " , '" + numfact.ToString().Replace("'", "''") + "' " + ")";
                return DataBaseConnexion.addOrUpdateElementInDataBase(CommandText, Program.SelectGlobalMessages.ImpAddBonReception);
        }

        public Boolean modifierBonReception()
        {
           string CommandText = "update " + DAL.DataBaseTableName.TableBonReception +
                    " Set codefournisseur_bonreception = '" + this.codefournisseur_bonreception +"'"+
                    " , date_bonreception = '" + this.date_bonreception + "' " +
                    " , datereception_bonreception = '" + this.datereception_bonreception + "' " +
                    " , adresse_bonreception = '" + this.adresse_bonreception.ToString().Replace("'", "''") + "' " +
                    " , statut_bonreception = '" + this.statut_bonreception + "' " +
                    " , isFacture_bonreception = '" + this.isFacture_bonreception + "' " +
                    " , notes_bonreception = '" + this.notes_bonreception.ToString().Replace("'", "''") + "' " +
                    " , apayer_bonreception = " + this.apayer_bonreception.ToString().Replace(',', '.') +
                     " , numfacture = '" + this.numfact.ToString().Replace("'", "''") + "' " +
                    " Where code_bonreception = '" + this.code_bonreception + "' " + ";";

                return DataBaseConnexion.addOrUpdateElementInDataBase(CommandText, Program.SelectGlobalMessages.ImpUpdateBonReception);
        }

        // supprimer un Bon et toutes ses lignes:
        public static Boolean supprimerBonReception(string _code_bonreception)
        {
            if (LigneBonReception.supprimerAllLigneBonReception(_code_bonreception) == true)
            {
                String CommandText = "delete from " + DAL.DataBaseTableName.TableBonReception +
                        " Where code_bonreception = '" + _code_bonreception + "' " + ";";
                return DataBaseConnexion.addOrUpdateElementInDataBase(CommandText, Program.SelectGlobalMessages.ImpDeleteBonReception);
            }
            else
                return false;       
        }

        public static BonReception getBonReception(string _codeBonReception)
        {
            BonReception currentBonReception = null;
            OdbcConnection connection = DAL.DataBaseConnexion.getConnection();
            try
            {
                OdbcCommand cmd = connection.CreateCommand();

                //MessageBox.Show("_codeBonReception : " + _codeBonReception);

                cmd.CommandText = "select * from  " + DAL.DataBaseTableName.TableBonReception +
                    " where code_bonreception = '" + _codeBonReception + "' ";

                OdbcDataReader Reader = cmd.ExecuteReader();

                if (Reader.Read())
                {
                    currentBonReception = new BonReception(Reader.GetString(0), Reader.GetString(1), Reader.GetString(2), 
                        Reader.GetString(3),Reader.GetString(4), Reader.GetString(5), Reader.GetString(6), 
                        Reader.GetString(7), Reader.GetDouble(8), Reader.GetString(9));
                }
                Reader.Close();               
            }
            catch (OdbcException e)
            {
                MessageBox.Show(e.Message,  Program.SelectGlobalMessages.SelectBonReception,
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                
            }
            catch (Exception)
            {
                MessageBox.Show(Program.SelectGlobalMessages.ImpSelectBonReception,
                     Program.SelectGlobalMessages.SelectBonReception, MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            return currentBonReception;
        }
                
        public static Boolean isBonReceptionFacture(string _codebr)
        {
            Boolean isFacture =  false;
            OdbcConnection connection = DAL.DataBaseConnexion.getConnection();
            try
            {
                OdbcCommand cmd = connection.CreateCommand();

                cmd.CommandText = "select isFacture_bonreception from " +
                                    DAL.DataBaseTableName.TableBonReception +
                                    " where code_bonreception = '" + _codebr + "' ";
                OdbcDataReader reader = cmd.ExecuteReader();
                if (reader.Read() == true)
                {
                    if(reader.GetString(0) == DAL.VariablesGlobales.EntityFacture)
                        isFacture = true;
                }
                reader.Close();
            }
            catch (OdbcException e)
            {
                MessageBox.Show(e.Message, Program.SelectGlobalMessages.SelectBonReception,
                    MessageBoxButtons.OK, MessageBoxIcon.Error);                 
            }
            catch (Exception)
            {
                MessageBox.Show(Program.SelectGlobalMessages.ImpSelectBonReception,
                     Program.SelectGlobalMessages.DeleteBonReception, MessageBoxButtons.OK,
                    MessageBoxIcon.Error);                
            }

            return isFacture;
        }

        public static Boolean setBonReceptionAsFacture(string  _codeBr, string _newEtatFacturation)
        {
            string CommandText = "update " + DAL.DataBaseTableName.TableBonReception +
                     " set isFacture_bonreception = '" + _newEtatFacturation + "' " +
                     " Where code_bonreception = '" + _codeBr + "' " + ";";
            return DataBaseConnexion.addOrUpdateElementInDataBase(CommandText, Program.SelectGlobalMessages.ImpUpdateBonReception);
        }

        public static ArrayList getALLBonsReception()
        {
            ArrayList tab_lignesBR = new ArrayList();
            BonReception currentBonReception = null;            
            OdbcConnection connection = DAL.DataBaseConnexion.getConnection();
            try
            {
                OdbcCommand cmd = connection.CreateCommand();
                cmd.CommandText = "select * from " + DAL.DataBaseTableName.TableBonReception + 
                                " order by Format(datereception_bonreception, 'mm/dd/yyyy')  ";
                OdbcDataReader Reader = cmd.ExecuteReader();
                while (Reader.Read())
                {
                    currentBonReception = new BonReception(Reader.GetString(0), Reader.GetString(1), Reader.GetString(2), Reader.GetString(3),
                        Reader.GetString(4), Reader.GetString(5), Reader.GetString(6), Reader.GetString(7), Reader.GetDouble(8), Reader.GetString(9));
                    tab_lignesBR.Add(currentBonReception);
                }
                Reader.Close();
            }
            catch (OdbcException e)
            {
                MessageBox.Show(e.Message, Program.SelectGlobalMessages.SelectBonReception,
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception)
            {
                MessageBox.Show(Program.SelectGlobalMessages.ImpSelectBonReception,
                     Program.SelectGlobalMessages.DeleteBonReception, MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }

            return tab_lignesBR;
        }

        public static ArrayList getALLBonsReceptionFactures()
        {
            ArrayList tab_lignesBR = new ArrayList();
            BonReception currentBonReception = null;            
            OdbcConnection connection = DAL.DataBaseConnexion.getConnection();
            try
            {
                OdbcCommand cmd = connection.CreateCommand();
                cmd.CommandText = "select * from "  + DAL.DataBaseTableName.TableBonReception +
                                 " WHERE isFacture_bonreception = '" + DAL.VariablesGlobales.EntityFacture + "' ";
                OdbcDataReader Reader = cmd.ExecuteReader();
                while (Reader.Read())
                {
                    currentBonReception = new BonReception(Reader.GetString(0), Reader.GetString(1), Reader.GetString(2), Reader.GetString(3),
                        Reader.GetString(4), Reader.GetString(5), Reader.GetString(6), Reader.GetString(7), Reader.GetDouble(8), Reader.GetString(9));

                    tab_lignesBR.Add(currentBonReception);                    
                }
                Reader.Close();
            }
            catch (OdbcException e)
            {
                MessageBox.Show(e.Message, Program.SelectGlobalMessages.SelectBonReception,
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception)
            {
                MessageBox.Show(Program.SelectGlobalMessages.ImpSelectBonReception,
                     Program.SelectGlobalMessages.DeleteBonReception, MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }

            return tab_lignesBR;
        }

        public static ArrayList getALLBonsReceptionNonFactures()
        {
            ArrayList tab_lignesBR = new ArrayList();
            BonReception currentBonReception = null;
            OdbcConnection connection = DAL.DataBaseConnexion.getConnection();
            try
            {
                OdbcCommand cmd = connection.CreateCommand();
                cmd.CommandText = "select * from " + DAL.DataBaseTableName.TableBonReception +
                                 " WHERE isFacture_bonreception = '" + DAL.VariablesGlobales.EntityNonFacture + "' ";
                OdbcDataReader Reader = cmd.ExecuteReader();
                while (Reader.Read())
                {
                    currentBonReception = new BonReception(Reader.GetString(0), Reader.GetString(1), Reader.GetString(2), Reader.GetString(3),
                        Reader.GetString(4), Reader.GetString(5), Reader.GetString(6), Reader.GetString(7), Reader.GetDouble(8), Reader.GetString(9));
                    
                    tab_lignesBR.Add(currentBonReception);
                }
                Reader.Close();
            }
            catch (OdbcException e)
            {
                MessageBox.Show(e.Message, Program.SelectGlobalMessages.SelectBonReception,
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception)
            {
                MessageBox.Show(Program.SelectGlobalMessages.ImpSelectBonReception,
                     Program.SelectGlobalMessages.DeleteBonReception, MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }

            return tab_lignesBR;
        }
        
        public static ArrayList getALLBonsReceptionByFournisseur(string  _codeFournisseur)
        {
            ArrayList tab_lignesBR = new ArrayList();
            BonReception currentBonReception = null;
            OdbcConnection connection = DAL.DataBaseConnexion.getConnection();
            try
            {
                OdbcCommand cmd = connection.CreateCommand();
                cmd.CommandText = "select * from " + DAL.DataBaseTableName.TableBonReception +
                                  " WHERE codefournisseur_bonreception = '" + _codeFournisseur + "';";
                OdbcDataReader Reader = cmd.ExecuteReader();
                while (Reader.Read())
                {
                    currentBonReception = new BonReception(Reader.GetString(0), Reader.GetString(1), Reader.GetString(2), Reader.GetString(3),
                        Reader.GetString(4), Reader.GetString(5), Reader.GetString(6), Reader.GetString(7), Reader.GetDouble(8), Reader.GetString(9));

                    tab_lignesBR.Add(currentBonReception);
                }
                Reader.Close();
            }
            catch (OdbcException e)
            {
                MessageBox.Show(e.Message, Program.SelectGlobalMessages.SelectBonReception,
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception)
            {
                MessageBox.Show(Program.SelectGlobalMessages.ImpSelectBonReception,
                     Program.SelectGlobalMessages.DeleteBonReception, MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }

            return tab_lignesBR;
        }
        
        public static ArrayList getALLBonsReceptionNonFacturesByFournisseur(string  _codeFournisseur)
        {
            ArrayList tab_lignesBR = new ArrayList();
            BonReception currentBonReception = null;
            OdbcConnection connection = DAL.DataBaseConnexion.getConnection();
            try
            {
                OdbcCommand cmd = connection.CreateCommand();
                cmd.CommandText = "select * from " + DAL.DataBaseTableName.TableBonReception +
                                  " where  isFacture_bonreception = '" + DAL.VariablesGlobales.EntityNonFacture + "' " +
                                  " and  codefournisseur_bonreception = ' " + _codeFournisseur + "'"+
                                  " order by Format(date_bonreception, 'mm/dd/yyyy')";
                OdbcDataReader Reader = cmd.ExecuteReader();
                while (Reader.Read())
                {
                    currentBonReception = new BonReception(Reader.GetString(0), Reader.GetString(1), Reader.GetString(2), Reader.GetString(3),
                      Reader.GetString(4), Reader.GetString(5), Reader.GetString(6), Reader.GetString(7), Reader.GetDouble(8), Reader.GetString(9));
                    tab_lignesBR.Add(currentBonReception);
                }
                Reader.Close();
            }
            catch (OdbcException e)
            {
                MessageBox.Show(e.Message, Program.SelectGlobalMessages.SelectBonReception,
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            catch (Exception)
            {
               MessageBox.Show( Program.SelectGlobalMessages.ImpSelectBonReception,
                                Program.SelectGlobalMessages.DeleteBonReception, MessageBoxButtons.OK,
                                MessageBoxIcon.Error);
            }
            return tab_lignesBR;
        }
        
        public static ArrayList getListOfCodeBonsReceptionByFactureFournisseur(string _codefact)
        {
            ArrayList tab_lignesBR = new ArrayList();
            // BonLivraison unBL = null;
            string codeBR;
            OdbcConnection connection = DAL.DataBaseConnexion.getConnection();

            try
            {
                OdbcCommand cmd = connection.CreateCommand();
                cmd.CommandText = " Select distinct codeBonReception_lignefacturefournisseur from " + 
                                                                DAL.DataBaseTableName.TableLigneFactureFournisseur +
                                  " Where numero_facturefournisseur = '" + _codefact  + "'" +
                                  " And codeBonReception_lignefacturefournisseur is not NULL " +
                                  " OR codeBonReception_lignefacturefournisseur <> '0'  Order by codeBonReception_lignefacturefournisseur";
                OdbcDataReader Reader = cmd.ExecuteReader();
                while (Reader.Read())
                {
                    codeBR = Reader.GetString(0);
                    if (codeBR != "0")
                    {
                        tab_lignesBR.Add(codeBR);
                    }
                }
                Reader.Close();
                if (tab_lignesBR.Count == 0)
                    return null;
                else
                    return tab_lignesBR;
            }
            catch (OdbcException e)
            {
                MessageBox.Show(e.Message, Program.SelectGlobalMessages.SelectLigneBonLivraison,
                    MessageBoxButtons.OK, MessageBoxIcon.Error);

                return null;
            }
        }

        public static Boolean supprimerALLBonReception()
        {
            if (LigneBonReception.supprimerAllLigneBonReception() == true)
            {
                String CommandText = "delete from " + DAL.DataBaseTableName.TableBonReception;
                return DataBaseConnexion.addOrUpdateElementInDataBase(CommandText, Program.SelectGlobalMessages.ImpDeleteBonReception);
            }
            else
                return false;
        }
    }
}