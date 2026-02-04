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
    public class CommandeVente
    {
        public static string Separateur = CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator;
        
        // les attributs:
        public int code_commandevente;
        public int codeclient_commandevente;        
        public string date_commandevente;
        public string datelivraison_commandevente;
        public string statut_commandevente; //Commande Livrée/non Livrée : DAL.VariablesGlobales.EntityLivre/CommandeVenteLivreNonLivree.
        public double apayer_commandevente; 
        public string modeexpedition_commandevente;
        public string modepayement_commandevente;
        public string notes_commandevente;
       
        // les constructeurs
        public CommandeVente(int _codeCommande, int _codeClient, string _dateCommande, string _dateLivraison, string _statut, double _apayer, 
                            string _modeexpedition, string _modepayement, string _note )
        {
            code_commandevente = _codeCommande;
            codeclient_commandevente = _codeClient;            
            date_commandevente = _dateCommande;
            datelivraison_commandevente = _dateLivraison;
            statut_commandevente = _statut;
            apayer_commandevente = _apayer;
            modeexpedition_commandevente = _modeexpedition;
            modepayement_commandevente = _modepayement;
            notes_commandevente = _note;
        }

        public CommandeVente(int v_code)
        {
            code_commandevente = v_code;
        }

        public CommandeVente()
        {

        }
       
        // les methodes:
        public Boolean ajouterCommandeVente()
        {
            string CommandText = "insert into " + DAL.DataBaseTableName.TableCommandeVente +
                    " values(" + 
                    this.code_commandevente +
                    ",  " + this.codeclient_commandevente +                    
                    ", '" + this.date_commandevente + "'" +
                    ", '" + this.datelivraison_commandevente + "'" +
                    ", '" + this.statut_commandevente + "'" +                    
                    ",  " + this.apayer_commandevente.ToString().ToString().Replace(',', '.') +
                    ", '" + this.modeexpedition_commandevente.ToString().Replace("'", "''") + "'" +
                    ", '" + this.modepayement_commandevente.ToString().Replace("'", "''") + "'" +
                    ", '" + this.notes_commandevente.ToString().Replace("'", "''") + "'" +
                    ");";
            return DataBaseConnexion.addOrUpdateElementInDataBase(CommandText, Program.SelectGlobalMessages.ImpAddCommandeVente);
        }
        
        public Boolean modifierCommandeVente() 
        {
            string CommandText = "update " + DAL.DataBaseTableName.TableCommandeVente +
                      " set " +
                     " codeClient_commandeVente = " + this.codeclient_commandevente +                     
                     ", date_commandeVente = '" + this.date_commandevente + "'" +
                     ", datelivraison_commandevente = '" + this.datelivraison_commandevente + "'" +
                     ", statut_commandeVente = '" + this.statut_commandevente + "'" +
                     ", apayer_commandevente = " + apayer_commandevente.ToString().ToString().Replace(',', '.') +
                     ", modeexpedition_commandevente = '" + modeexpedition_commandevente.ToString().Replace("'", "''") + "'" +
                     ", modepayement_commandevente = '" + modepayement_commandevente.ToString().Replace("'", "''") + "'" +
                     ", notes_commandevente = '" + this.notes_commandevente.ToString().Replace("'", "''") + "'" +
                     "  where code_commandeVente = " + this.code_commandevente;
            return DataBaseConnexion.addOrUpdateElementInDataBase(CommandText, Program.SelectGlobalMessages.ImpUpdateCommandeVente);
        }

        public static bool supprimerUneCommandeVente(int _codeCommandeVente)
        {
            // suppression de toutes les lignes de la commande en question :
            LigneCommandeVente.SupprimerAllLigneFromCMD(_codeCommandeVente);
            // suppression de la commande en question :
            string CommandText2 = "delete from " + DAL.DataBaseTableName.TableCommandeVente +
                                  " where code_commandevente = " + _codeCommandeVente ;
            return DataBaseConnexion.addOrUpdateElementInDataBase(CommandText2, Program.SelectGlobalMessages.ImpDeleteCommandeVente);
        }

        public static CommandeVente getCommandeVente(int _codeCommandeVente)
        {
            CommandeVente commandeVente = null;

            OdbcConnection connection = DAL.DataBaseConnexion.getConnection();
            try
            {

                OdbcCommand cmd = connection.CreateCommand();

                cmd.CommandText = "select * from " + DAL.DataBaseTableName.TableCommandeVente +
                                  " where code_commandevente = " + _codeCommandeVente;
                OdbcDataReader Reader = cmd.ExecuteReader();
                if (Reader.Read())
                {
                    commandeVente = new CommandeVente(Reader.GetInt32(0), Reader.GetInt32(1), Reader.GetString(2), Reader.GetString(3), 
                                    Reader.GetString(4), Reader.GetDouble(5), Reader.GetString(6), Reader.GetString(7), Reader.GetString(8));
                }
                Reader.Close();
            }
            catch (OdbcException e)
            {
                MessageBox.Show(e.Message, Program.SelectGlobalMessages.SelectCommandeVente,
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception)
            {
                MessageBox.Show(Program.SelectGlobalMessages.ImpSelectCommandeVente,
                  Program.SelectGlobalMessages.SelectCommandeVente, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            return commandeVente;
        }

        public static Boolean updateStatut(int _codeCommandeVente, int _newStatut)
        {
            string CommandText = "update  " + DAL.DataBaseTableName.TableCommandeVente + 
                " set statut_commandevente = '" + _newStatut +"'" +
                " where code_commandevente=" + _codeCommandeVente;
            return DataBaseConnexion.addOrUpdateElementInDataBase(CommandText, Program.SelectGlobalMessages.ImpUpdateCommandeVente);
        }
        
        public static bool isExistedCommandeVente(int _codeCommandeVente)
        {
            bool exist = false ;

            OdbcConnection connection = DAL.DataBaseConnexion.getConnection();
            try
            {
                OdbcCommand cmd = connection.CreateCommand();
                cmd.CommandText = "select * from " + DAL.DataBaseTableName.TableCommandeVente +
                                   " where code_commandevente=" + _codeCommandeVente;
                OdbcDataReader reader = cmd.ExecuteReader();
                if (reader.Read() == true)
                {
                    exist = true ;
                }
                reader.Close();
            }

            catch (OdbcException e)
            {
                MessageBox.Show(e.Message, Program.SelectGlobalMessages.SelectCommandeVente,
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            return exist;
        }

        public static ArrayList getAllCommandeVente()
        {
            ArrayList listOfCommandesVente = null;
            CommandeVente commandeVente = null;            
            OdbcConnection connection = DAL.DataBaseConnexion.getConnection();
            try
            {                
                OdbcCommand cmd = connection.CreateCommand();
                cmd.CommandText = "select * from   " + DAL.DataBaseTableName.TableCommandeVente + 
                                  " order by Format(date_commandevente, 'mm/dd/yyyy')   ";
                OdbcDataReader Reader = cmd.ExecuteReader();
                if (Reader.HasRows == true)
                {
                    listOfCommandesVente = new ArrayList();
                    while (Reader.Read())
                    {
                        commandeVente = new CommandeVente(Reader.GetInt32(0), Reader.GetInt32(1), Reader.GetString(2), Reader.GetString(3),
                                        Reader.GetString(4), Reader.GetDouble(5), Reader.GetString(6), Reader.GetString(7), Reader.GetString(8));
                        listOfCommandesVente.Add(commandeVente);
                    }                    
                }
                Reader.Close();
            }
            catch (OdbcException e)
            {
                MessageBox.Show(e.Message, Program.SelectGlobalMessages.SelectCommandeVente,
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception)
            {
                MessageBox.Show(Program.SelectGlobalMessages.ImpSelectCommandeVente,
                     Program.SelectGlobalMessages.SelectCommandeVente, MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            return listOfCommandesVente;
        }

        public ArrayList getAllCommandeVenteByClient(int _codeClient)
        {
            ArrayList listOfCommandesVente = new ArrayList();
            CommandeVente commandeVente = null;            
            OdbcConnection connection = DAL.DataBaseConnexion.getConnection();
            try
            {
                OdbcCommand cmd = connection.CreateCommand();

                cmd.CommandText = "select * from   " + DAL.DataBaseTableName.TableCommandeVente + 
                                  " Where codeclient_commandevente=" + _codeClient +
                                  " order by Format(date_commandevente, 'mm/dd/yyyy')  ";
                OdbcDataReader Reader = cmd.ExecuteReader();
                while (Reader.Read())
                {
                    commandeVente = new CommandeVente(Reader.GetInt32(0), Reader.GetInt32(1), Reader.GetString(2), Reader.GetString(3),
                                     Reader.GetString(4), Reader.GetDouble(5), Reader.GetString(6), Reader.GetString(7), Reader.GetString(8));

                    listOfCommandesVente.Add(commandeVente);
                }
                Reader.Close();
            }
            catch (OdbcException e)
            {
                MessageBox.Show(e.Message, Program.SelectGlobalMessages.SelectCommandeVente,
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception)
            {
                MessageBox.Show(Program.SelectGlobalMessages.ImpSelectCommandeVente,
                     Program.SelectGlobalMessages.SelectCommandeVente, MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            return listOfCommandesVente;
        }
        
        public ArrayList getAllCommandeVenteByStatut(String _statut) // Statut = Livree/Nonlivrée 
        {
            ArrayList listOfCommandesVente = new ArrayList();
            CommandeVente commandeVente = null;            
            OdbcConnection connection = DAL.DataBaseConnexion.getConnection();
            try
            {
                OdbcCommand cmd = connection.CreateCommand();
                cmd.CommandText = " select * from   " + DAL.DataBaseTableName.TableCommandeVente +
                                  " where statut_commandevente = '" + _statut + "'" +
                                  " order by Format(date_commandevente, 'mm/dd/yyyy')";
                OdbcDataReader Reader = cmd.ExecuteReader();
                while (Reader.Read())
                {
                    commandeVente = new CommandeVente(Reader.GetInt32(0), Reader.GetInt32(1), Reader.GetString(2), Reader.GetString(3),
                                     Reader.GetString(4), Reader.GetDouble(5), Reader.GetString(6), Reader.GetString(7), Reader.GetString(8));
                    listOfCommandesVente.Add(commandeVente);                }
                Reader.Close();
            }
            catch (OdbcException e)
            {
                MessageBox.Show(e.Message, Program.SelectGlobalMessages.SelectCommandeVente,
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception)
            {
                MessageBox.Show(Program.SelectGlobalMessages.ImpSelectCommandeVente,
                     Program.SelectGlobalMessages.SelectCommandeVente, MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            return listOfCommandesVente;
        }
    }
}
