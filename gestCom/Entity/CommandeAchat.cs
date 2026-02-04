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
    public class CommandeAchat
    {
        public static string Separateur = CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator;
        
        // Les attributs :
        public string code_commandeachat;
        public string codefournisseur_commandeachat;
        public string date_commandeachat;
        public string dateReception_commandeachat;
        public string statut_commandeachat;
        public double apayer_commandeachat;
        public string modeexpedition_commandeachat;
        public string modepayement_commandeachat;
        public string notes_commandeachat;
                  
        // Les constructeurs :
        public CommandeAchat(string _code, string _codeFournisseur, string _date, string _dateReception, string _statut,
                            double _apayer, string _modeexpedition, string _modepayement, string _notes )
        {
            this.code_commandeachat = _code;
            this.codefournisseur_commandeachat = _codeFournisseur;
            this.date_commandeachat = _date;
            this.dateReception_commandeachat = _dateReception;
            this.statut_commandeachat = _statut;
            this.apayer_commandeachat = _apayer;
            this.modeexpedition_commandeachat = _modeexpedition;
            this.modepayement_commandeachat = _modepayement;
            this.notes_commandeachat = _notes;
        }

        public CommandeAchat(string _code)
        {
            this.code_commandeachat = _code;
        }

        public CommandeAchat()
        {

        }
              
        // Les methodes:
        public Boolean ajouterCommandeAchat()
        {
           string CommandText = "insert into " +  DAL.DataBaseTableName.TableCommandeAchat + " values(" +
                    "'" + this.code_commandeachat + "," +
                    "'" + this.codefournisseur_commandeachat + "," +
                    "'" + this.date_commandeachat + "'," +
                    "'" + this.dateReception_commandeachat + "'," +
                    "'" + this.statut_commandeachat + "'," +
                    this.apayer_commandeachat.ToString().ToString().Replace(',', '.') + "," +
                    "'" + this.modeexpedition_commandeachat.ToString().Replace("'", "''") + "'," +
                    "'" + this.modepayement_commandeachat.ToString().Replace("'", "''") + "'," +
                    "'" + this.notes_commandeachat.ToString().Replace("'", "''") +  "'" +                 
                    ")";
             return  DataBaseConnexion.addOrUpdateElementInDataBase(CommandText,Program.SelectGlobalMessages.ImpAddCommandeAchat);
        
        }
        
        public Boolean modifierCommandeAchat()
        {
            string CommandText = "update " + DAL.DataBaseTableName.TableCommandeAchat + " set " +
                       " codefournisseur_commandeachat = '" + this.codefournisseur_commandeachat + "'," +
                       " date_commandeachat = '" + this.date_commandeachat + "'," +
                       " dateliv_commandeachat = '" + this.dateReception_commandeachat + "'," +
                       " statut_commandeachat = '" + this.statut_commandeachat + "'," +
                       " apayer_commandeachat = " + apayer_commandeachat.ToString().ToString().Replace(',', '.') + "," +
                       " modeexpedition_commandeachat = '" + this.modeexpedition_commandeachat.ToString().Replace("'", "''") + "'," +
                       " modepayement_commandeachat = '" + this.modepayement_commandeachat.ToString().Replace("'", "''") + "'," +
                       " notes_commandeachat = '" + this.notes_commandeachat.ToString().Replace("'", "''") + "'" +
                       " where code_commandeachat = '" + this.code_commandeachat + "'";

                return DataBaseConnexion.addOrUpdateElementInDataBase(CommandText, Program.SelectGlobalMessages.ImpUpdateCommandeAchat);
        }

        public static Boolean supprimerCommandeAchat(string _code_commandeachat)
        {            
            LigneCommandeAchat.SupprimerToutesLignesCommandeAchat(_code_commandeachat);
            string CommandText = "delete from  " + DAL.DataBaseTableName.TableCommandeAchat + 
                                 " where  code_commandeachat = '" + _code_commandeachat + "'";
                return DataBaseConnexion.addOrUpdateElementInDataBase(CommandText, Program.SelectGlobalMessages.ImpDeleteCommandeAchat);
        }

        public static CommandeAchat getCommandeAchat(string _code_commandeachat)
        {
            CommandeAchat v_CommandeAchat = null;                     
            OdbcConnection connection = DAL.DataBaseConnexion.getConnection();           
            try
            {
                OdbcCommand cmd = connection.CreateCommand();
                cmd.CommandText = "select * from " +  DAL.DataBaseTableName.TableCommandeAchat +
                                  " where code_commandeachat = '" + _code_commandeachat + "'";
                OdbcDataReader Reader = cmd.ExecuteReader();
                if (Reader.Read())
                {
                    v_CommandeAchat = new CommandeAchat( Reader.GetString(0),Reader.GetString(1), Reader.GetString(2),
                                                        Reader.GetString(3), Reader.GetString(4), Reader.GetDouble(6),
                                                        Reader.GetString(7), Reader.GetString(8), Reader.GetString(9));
                }
                Reader.Close();
            }
            catch (OdbcException e)
            {
                MessageBox.Show(e.Message,  Program.SelectGlobalMessages.SelectCommandeAchat,
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception)
            {
                MessageBox.Show( Program.SelectGlobalMessages.ImpSelectCommandeAchat,
                  Program.SelectGlobalMessages.SelectCommandeAchat, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            return v_CommandeAchat;
        }

        public static ArrayList getToutesCommandesAchat()
        {
             
            ArrayList tab_CommandeAchat = new ArrayList();
            CommandeAchat v_CommandeAchat = null;
            OdbcConnection connection = DataBaseConnexion.getConnection();
            try
            {
                OdbcCommand cmd = connection.CreateCommand();

                cmd.CommandText = " select * from   " + DAL.DataBaseTableName.TableCommandeAchat + 
                                  " order by Format(date_commandeachat, 'mm/dd/yyyy')  ";
                OdbcDataReader Reader = cmd.ExecuteReader();
                while (Reader.Read())
                {
                    v_CommandeAchat = new CommandeAchat(Reader.GetString(0), Reader.GetString(1),
                                    Reader.GetString(2), Reader.GetString(3),
                                    Reader.GetString(4), Reader.GetDouble(6), Reader.GetString(7),
                                    Reader.GetString(8), Reader.GetString(9));
                    tab_CommandeAchat.Add(v_CommandeAchat);
                }
                Reader.Close();
            }
            catch (OdbcException e)
            {
                MessageBox.Show(e.Message, Program.SelectGlobalMessages.SelectCommandeAchat,
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            return tab_CommandeAchat;
        }

        public ArrayList getCommandeAchatByFournisseur(string _codeFournisseur)
        {            
            ArrayList tab_CommandeAchat = new ArrayList();
            CommandeAchat v_CommandeAchat = null;
            OdbcConnection connection = DataBaseConnexion.getConnection();
            try
            {
                OdbcCommand cmd = connection.CreateCommand();
                cmd.CommandText = " select * from  " +  DAL.DataBaseTableName.TableCommandeAchat +
                                  " where codefournisseur_commandeachat = '" + _codeFournisseur + "'" +
                                  " order by Format(date_commandeachat, 'mm/dd/yyyy') ";               
                OdbcDataReader Reader = cmd.ExecuteReader();
                while (Reader.Read())
                {
                    v_CommandeAchat = new CommandeAchat(Reader.GetString(0), Reader.GetString(1), Reader.GetString(2), 
                                      Reader.GetString(3), Reader.GetString(4), Reader.GetDouble(6), Reader.GetString(7),
                                      Reader.GetString(8), Reader.GetString(9));

                    tab_CommandeAchat.Add(v_CommandeAchat);
                }

                Reader.Close();

            }
            catch (OdbcException e)
            {
                MessageBox.Show(e.Message, Program.SelectGlobalMessages.SelectCommandeAchat,
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            return tab_CommandeAchat;

        }

        public Boolean updateStatutCommandeAchat(int _code_CommandeAchat, String _newStatut)
        {
            string CommandText = " update " + DAL.DataBaseTableName.TableCommandeAchat +
                                 " set statut_commandeachat = '" + _newStatut + "' " +
                                 " where code_commandeachat = '" + _code_CommandeAchat + "'";
            return DataBaseConnexion.addOrUpdateElementInDataBase(CommandText, Program.SelectGlobalMessages.ImpUpdateBonLivraison);
        }
    }
}
