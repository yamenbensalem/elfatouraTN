using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using System.Data;
using T4C_Commercial_Project.Entity;
using T4C_Commercial_Project.DAL;
using System.Collections;
using System.Globalization;
using System.Data.Odbc;

namespace T4C_Commercial_Project.Entity
{
    public class DevisClient
    {
        public static string Separateur = CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator;

        //les attributs:
        public string numero_devis;
        public string codeclient_devis;
        public string date_devis;
        public double remise_devis;
        public double montantHT_devis;
        public double apayer_devis;
        public string statut_devis;
        public string validite_devis;

        //les constructeurs:
        public DevisClient(string v_numeroDevis, string v_codeClient, string v_date, double v_remise, double v_montantHT,
            double v_netApayer, string v_statut, string _validite)
        {
            numero_devis = v_numeroDevis;
            codeclient_devis = v_codeClient;            
            date_devis = v_date;
            remise_devis = v_remise;
            montantHT_devis = v_montantHT;
            apayer_devis = v_netApayer;
            statut_devis = v_statut;
            validite_devis = _validite;
        }

        public DevisClient(string v_numeroDevis)
        {
            numero_devis = v_numeroDevis;
        }

        public DevisClient()
        {

        }

        //les methodes:
        public Boolean ajouterDevis()
        {
            string CommandText = "insert into " + DAL.DataBaseTableName.TableDevisClient +
                     " values(" +
                     " '" + this.numero_devis + "'," +
                     " '" + this.codeclient_devis + "', " +                     
                     " '" + this.date_devis + "' ," +
                            this.remise_devis.ToString().ToString().Replace(',', '.') + " , " +
                            this.montantHT_devis.ToString().ToString().Replace(',', '.') + "," +
                            this.apayer_devis.ToString().ToString().Replace(',', '.') + " , " +
                     " '" + this.statut_devis + "', " +
                     " '" + this.validite_devis.ToString().Replace("'", "''") + "');";
            return DataBaseConnexion.addOrUpdateElementInDataBase(CommandText, Program.SelectGlobalMessages.ImpAddDevis);
        }
        public Boolean modifierDevis()
        {
            string CommandText = "update " + DAL.DataBaseTableName.TableDevisClient +
                    " set codeclient_devis = '" + this.codeclient_devis + "' " +                   
                    ", date_devis='" + this.date_devis + "'" +
                    ", remise_devis=" + this.remise_devis.ToString().ToString().Replace(',', '.') +
                    ", montantHT_devis=" + this.montantHT_devis.ToString().ToString().Replace(',', '.') +
                    ", apayer_devis=" + this.apayer_devis.ToString().ToString().Replace(',', '.') +
                    ", statut_devis='" + this.statut_devis + "' " +
                    ", validite_devis='" + this.validite_devis.ToString().Replace("'", "''") + "' " +
                     " where numero_devis = '" + numero_devis+ "'";
            return DataBaseConnexion.addOrUpdateElementInDataBase(CommandText, Program.SelectGlobalMessages.ImpUpdateDevis);
        }
        public static Boolean supprimerDevis(string _numeroDevis)
        {
            Boolean executed = false;
            LigneDevisClient ligneDevisClient = new LigneDevisClient();
            if (LigneDevisClient.supprimerAllLigneDevis(_numeroDevis) == true)
            {
                string CommandText = "delete from " + DAL.DataBaseTableName.TableDevisClient +
                                    " where numero_devis = '" + _numeroDevis + "' ";
                executed = DataBaseConnexion.addOrUpdateElementInDataBase(CommandText, Program.SelectGlobalMessages.ImpDeleteDevis);
            }
            return executed;
        }
                
        public static DevisClient getDevis(string _numeroDevis)
        {
            DevisClient devisClient = null;

            if (DataBaseConnexion.getMaxNumberOfStringColumn(DAL.DataBaseTableName.TableDevisClient, "numero_devis") != 0)
            {
                OdbcConnection connection = DAL.DataBaseConnexion.getConnection();
                try
                {
                    OdbcCommand cmd = connection.CreateCommand();


                    cmd.CommandText = "select * from " +  DAL.DataBaseTableName.TableDevisClient +
                        " where numero_devis = '" + _numeroDevis + "' ";

                    OdbcDataReader Reader = cmd.ExecuteReader();

                    if (Reader.Read())
                    {
                        devisClient = new DevisClient(Reader.GetString(0), Reader.GetString(1),
                                                        Reader.GetString(2), Reader.GetDouble(3),
                                                        Reader.GetDouble(4), Reader.GetDouble(5), 
                                                        Reader.GetString(6), Reader.GetString(7) 
                                                        );                    
                    }

                    Reader.Close();

                }
                catch (OdbcException e)
                {
                    MessageBox.Show(e.Message, Program.SelectGlobalMessages.SelectBonLivraison,
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                catch (Exception)
                {
                    MessageBox.Show(Program.SelectGlobalMessages.ImpSelectDevis,
                         Program.SelectGlobalMessages.SelectDevis, MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            return devisClient;
        }

        public Boolean updateStatutDevis(string _newStatus)
        {
            string CommandText = "update " + DAL.DataBaseTableName.TableDevisClient +" set statut_devis = '" + _newStatus +
                 "'  where numero_devis = '" + this.numero_devis + "';";

            return DataBaseConnexion.addOrUpdateElementInDataBase(CommandText, Program.SelectGlobalMessages.ImpUpdateDevis);
        }

        public static Boolean supprimerAllDevis()
        {
            Boolean executed = false;
            LigneDevisClient ligneDevisClient = new LigneDevisClient();
            if (LigneDevisClient.supprimerAllLigneDevis() == true)
            {
                string CommandText = "delete from " + DAL.DataBaseTableName.TableDevisClient;
                executed = DataBaseConnexion.addOrUpdateElementInDataBase(CommandText, Program.SelectGlobalMessages.ImpDeleteDevis);
            }
            return executed;
        }
    }
}
