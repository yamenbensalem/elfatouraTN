using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Windows.Forms;
using System.Data;
using T4C_Commercial_Project.DAL;
using System.Globalization;
using System.Data.Odbc;
using System.Collections;

namespace T4C_Commercial_Project.Entity
{
    class LigneCommandeAchat
    {
        public static string Separateur = CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator;
        
        // les attributs
        public int numero_lignecommandeachat;
        public string code_commandeachat;
        public string codeproduit_lignecommandeachat;
        public string designationproduit_lignecommandeachat;
        public double quantite_lignecommandeachat;
        public double prixunitaire_lignecommandeachat;
        public double montantHT_lignecommandeachat;
        public double tvaproduit_lignecommandeachat;
        public double remise_lignecommandeachat;        
        
        // Les constructeurs :
        public LigneCommandeAchat(string _codeCommandeAchat, string _codeProduit)
        {
            this.code_commandeachat = _codeCommandeAchat;
            this.codeproduit_lignecommandeachat = _codeProduit;
        }

        public LigneCommandeAchat(int numeroLigne, string _codeCommandeAchat, string _codeProduit, 
                                string _designationProduit, double _quantite,
                                double _prixunitaire, double _montantHT, double _tvaproduit, double _remise)
        {
            numero_lignecommandeachat = numeroLigne;
            code_commandeachat = _codeCommandeAchat;
            codeproduit_lignecommandeachat = _codeProduit;
            designationproduit_lignecommandeachat = _designationProduit;
            quantite_lignecommandeachat = _quantite;
            prixunitaire_lignecommandeachat = _prixunitaire;
            montantHT_lignecommandeachat = _montantHT;
            tvaproduit_lignecommandeachat = _tvaproduit;
            remise_lignecommandeachat = _remise;            
        }

        public LigneCommandeAchat()
        {

        }

        public bool ajouterLigneCommandeAchat()
        {
            string CommandText = "insert into " + DAL.DataBaseTableName.TableLigneCommandeAchat + "  values(" +
                            numero_lignecommandeachat + "," +
                      "'" + code_commandeachat + "'," +
                      "'" + codeproduit_lignecommandeachat + "'," +
                      "'" + designationproduit_lignecommandeachat.ToString().Replace("'", "''") + "'," +
                            quantite_lignecommandeachat.ToString().ToString().Replace(',', '.') + "," +
                            prixunitaire_lignecommandeachat.ToString().ToString().Replace(',', '.') + "," +
                            montantHT_lignecommandeachat.ToString().ToString().Replace(',', '.') + "," +
                            tvaproduit_lignecommandeachat.ToString().ToString().Replace(',', '.') + "," +
                            remise_lignecommandeachat.ToString().ToString().Replace(',', '.') +                       
                            ");";
            return DataBaseConnexion.addOrUpdateElementInDataBase(CommandText, Program.SelectGlobalMessages.ImpAddLigneCommandeAchat);
        }

        public bool modifierLigneCommandeAchat()
        {
            string CommandText = "update " + DAL.DataBaseTableName.TableLigneCommandeAchat + " set " +
                    " designation_prod = '" + this.designationproduit_lignecommandeachat.ToString().Replace("'", "''") + "'," +       
                    " quantite_lignecommandeachat = " + this.quantite_lignecommandeachat.ToString().ToString().Replace(',', '.') + "," +
                    " prixunitaire_lignecommandeachat = " + this.prixunitaire_lignecommandeachat.ToString().ToString().Replace(',', '.') + "," +
                    " montantHT_lignecommandeachat = " + this.montantHT_lignecommandeachat.ToString().ToString().Replace(',', '.') + "," +
                    " tvaproduit_lignecommandeachat = " + this.tvaproduit_lignecommandeachat.ToString().ToString().Replace(',', '.') + "," +
                    " remise_lignecommandeachat = " + this.remise_lignecommandeachat.ToString().ToString().Replace(',', '.') + 
                    " WHERE code_commandeachat = '" + this.code_commandeachat + "'" +
                    " AND codeproduit_lignecommandeachat = '" + this.codeproduit_lignecommandeachat + "';";
            return DataBaseConnexion.addOrUpdateElementInDataBase(CommandText, Program.SelectGlobalMessages.ImpUpdateLigneCommandeAchat);
        }

        public static bool supprimerLigneCommandeAchat(string _codeCommandeAchat, string _codeProduit)
        {
            string CommandText = "delete from " + DAL.DataBaseTableName.TableLigneCommandeAchat +
                       " WHERE code_commandeachat = '" + _codeCommandeAchat + "'" +
                       " AND codeproduit_lignecommandeachat= '" + _codeProduit + "' ";
            return DataBaseConnexion.addOrUpdateElementInDataBase(CommandText, Program.SelectGlobalMessages.ImpDeleteLigneCommandeAchat);
        }

        public static bool supprimerToutesLignesCommandeAchat(string _codeCommandeAchat)
        {
            string CommandText = "delete from  " + DAL.DataBaseTableName.TableLigneCommandeAchat +
                                 " where code_commandeachat = '" + _codeCommandeAchat + "';";
            return DataBaseConnexion.addOrUpdateElementInDataBase(CommandText, Program.SelectGlobalMessages.ImpDeleteLigneCommandeAchat);
        }

        public static LigneCommandeAchat getOneLigneCmdAchat(string _codeCommandeAchat, string _codeProduit)
        {
            LigneCommandeAchat ligneCmdAchat = null;
            OdbcConnection connection = DAL.DataBaseConnexion.getConnection();
            try
            {
                OdbcCommand cmd = connection.CreateCommand();
                cmd.CommandText = "select * from   " + DAL.DataBaseTableName.TableLigneCommandeAchat +
                                    " where code_commandeachat = '" + _codeCommandeAchat + "'" +
                                    " and codeproduit_lignecommandeachat = '" + _codeProduit + "';";
                OdbcDataReader Reader = cmd.ExecuteReader();
                if (Reader.Read())
                {
                    ligneCmdAchat = new LigneCommandeAchat(Reader.GetInt32(0), Reader.GetString(1), Reader.GetString(2), 
                                    Reader.GetString(3), Reader.GetDouble(4), Reader.GetDouble(5), Reader.GetDouble(6), 
                                    Reader.GetDouble(7), Reader.GetDouble(8));
                }
                Reader.Close();
            }
            catch (OdbcException e)
            {
                MessageBox.Show(e.Message, Program.SelectGlobalMessages.SelectCommandeAchat,
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch
            {
                MessageBox.Show(Program.SelectGlobalMessages.ImpSelectLigneCommandeAchat,
                     Program.SelectGlobalMessages.SelectCommandeAchat, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            return ligneCmdAchat;
        }
                
        public bool isExistedLigneCommandeAchat(string _code_CmdAchat, string _codeproduit_ligneCmdAchat, ref bool isException)
        {
            bool isExist=false;
            isException = false;
            OdbcConnection connection = DAL.DataBaseConnexion.getConnection();
            try
            {
                OdbcCommand cmd = connection.CreateCommand();
                cmd.CommandText = "select * from " + DAL.DataBaseTableName.TableLigneCommandeAchat +
                        " where code_commandeachat = '" + _code_CmdAchat + "'" + 
                        " and codeproduit_lignecommandeachat ='" + _codeproduit_ligneCmdAchat + "';";
                OdbcDataReader Reader = cmd.ExecuteReader();
                if (Reader.Read())
                {
                    isExist = true;

                }                
                Reader.Close();
                
            }
            catch (OdbcException e)
            {
                MessageBox.Show(e.Message, Program.SelectGlobalMessages.SelectCommandeAchat,
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                isException = true;
            }
            catch
            {
                MessageBox.Show(Program.SelectGlobalMessages.ImpSelectLigneCommandeAchat,
                     Program.SelectGlobalMessages.SelectCommandeAchat, MessageBoxButtons.OK, MessageBoxIcon.Error);
                isException = true;
            }
            return isExist;
        }

        public static Boolean SupprimerToutesLignesCommandeAchat(string _code_commandeAchat)
        {
            string CommandText1 = "delete from  " + DAL.DataBaseTableName.TableLigneCommandeAchat +
                                  " where code_commandeachat = '" + _code_commandeAchat + "'";
            return DataBaseConnexion.addOrUpdateElementInDataBase(CommandText1, Program.SelectGlobalMessages.ImpDeleteCommandeAchat);
        }

        public static ArrayList getToutesLignesCommandeAchat(string _code_commandeachat)
        {
            ArrayList tab_lignesCmdAchat = new ArrayList();
            LigneCommandeAchat ligneCmdAchat = null;
            OdbcConnection connection = DAL.DataBaseConnexion.getConnection();
            try
            {
                OdbcCommand cmd = connection.CreateCommand();
                cmd.CommandText = "select * from  " + DAL.DataBaseTableName.TableLigneCommandeAchat +
                                  " where code_commandeachat = '" + _code_commandeachat + "'";
                OdbcDataReader Reader = cmd.ExecuteReader();
                while (Reader.Read())
                {
                    ligneCmdAchat = new LigneCommandeAchat(Reader.GetInt32(0), Reader.GetString(1), 
                                    Reader.GetString(2), Reader.GetString(3),  Reader.GetDouble(4),
                                    Reader.GetDouble(5), Reader.GetDouble(6), Reader.GetDouble(7), Reader.GetDouble(8));                                                
                    tab_lignesCmdAchat.Add(ligneCmdAchat);
                }
                Reader.Close();
            }
            catch (OdbcException e)
            {
                MessageBox.Show(e.Message, Program.SelectGlobalMessages.SelectCommandeAchat,
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            if (tab_lignesCmdAchat.Count == 0)
                return null;
            else
                return tab_lignesCmdAchat;
        }
    }

}
