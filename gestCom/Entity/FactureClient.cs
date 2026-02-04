using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Data;
using T4C_Commercial_Project.DAL;
using System.Collections;
using T4C_Commercial_Project.Entity;
using System.Globalization;
using System.Data.Odbc;
using T4C_Commercial_Project.Forms;

namespace T4C_Commercial_Project.Entity
{
    public class FactureClient
    {
        public static string Separateur = CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator;
        public string numero_factureclient;
        public string codeclient_factureclient;
        public string date_factureclient;
        public double remise_factureclient;
        public double montantHT_factureclient;
        public double apayer_factureclient;
        public string origine_factureclient;
        public string statut_factureclient;
        public double timbre_factureclient;
        public string modepayement_factureclient;        
        public int avoir_factureclient;
        public Double montantApresRAS_factureclient;
        public string notes_factureclient;
        public Double montantRestant_factureclient;
        
        // Les constructeurs :
        public FactureClient(string _numero, string _codeClient, string _date, double _remise,
                            double _montantHT, double _apayerfacture, string _originefacture, string _statut,
                            double _timbre, string _modePayement, int _avoir, Double _montantApresRAS, string _notes,
                            double _montantRestant)
        {
            this.numero_factureclient = _numero;
            this.codeclient_factureclient = _codeClient;
            this.date_factureclient = _date;
            this.remise_factureclient = _remise;
            this.montantHT_factureclient = _montantHT;
            this.apayer_factureclient = _apayerfacture;
            this.origine_factureclient = _originefacture;
            this.statut_factureclient = _statut;
            this.timbre_factureclient = _timbre;
            this.modepayement_factureclient = _modePayement;
            this.avoir_factureclient = _avoir;
            this.montantApresRAS_factureclient = _montantApresRAS;
            this.notes_factureclient = _notes;
            this.montantRestant_factureclient = _montantRestant;
        }

        public FactureClient(string _numero_Facture)
        {
            this.numero_factureclient = _numero_Facture;
        }

        public FactureClient()
        {

        }

        public Boolean ajouterFactureClient()
        {
            OdbcConnection connection = DAL.DataBaseConnexion.getConnection();
            string CommandText = "insert into " + DAL.DataBaseTableName.TableFactureClient +
                    "  values ( " +
                   "'"+ this.numero_factureclient + "'," +
                    "'" + this.codeclient_factureclient + "','" +
                    this.date_factureclient + "'," +
                    this.remise_factureclient.ToString("0.##0").Replace(',', '.') + "," +
                    this.montantHT_factureclient.ToString("0.##0").Replace(',', '.') + "," +
                    this.apayer_factureclient.ToString("0.##0").Replace(',', '.') + ",'" +
                    this.origine_factureclient + "','" +
                    this.statut_factureclient + "'," +
                    this.timbre_factureclient.ToString().ToString().Replace(',', '.') + ", '" +
                    this.modepayement_factureclient + "'," +
                    this.avoir_factureclient + "," +
                    this.montantApresRAS_factureclient.ToString("0.##0").Replace(',', '.') + ",'" +
                    this.notes_factureclient.ToString().Replace("'", "''") + "'," +
                    this.montantRestant_factureclient.ToString().ToString().Replace(',', '.') +
                    ");";

            return DataBaseConnexion.addOrUpdateElementInDataBase(CommandText, Program.SelectGlobalMessages.ImpAddFactureClient);            
        }
        public static Boolean updateMontantRestant(string _numeroFactureClient, Double _newsMTrestant)
        {
            string CommandText = "update "  + DAL.DataBaseTableName.TableFactureClient + 
                                    " set montantRestant_factureclient =" + _newsMTrestant.ToString().ToString().Replace(',', '.') +
                                    " where numero_factureclient = '" + _numeroFactureClient + "' ";
            return DataBaseConnexion.addOrUpdateElementInDataBase(CommandText, Program.SelectGlobalMessages.ImpUpdateFactureClient);
        }

        public static Boolean updateStatutFacture(string _numeroFactureClient, string _newstat)
        {
            string CommandText = "update " + DAL.DataBaseTableName.TableFactureClient + 
                                 " set statut_factureclient = '" + _newstat + "' " +
                                 " where numero_factureclient = '" + _numeroFactureClient +"' ";
            return DataBaseConnexion.addOrUpdateElementInDataBase(CommandText, Program.SelectGlobalMessages.ImpUpdateFactureClient);
        }

        public Boolean modifierFactureClient()
        {
            string CommandText = "update " + DAL.DataBaseTableName.TableFactureClient +
                       "   set " +
                       "   codeclient_factureclient = '" + this.codeclient_factureclient + "'" +
                       " , date_factureclient ='" + this.date_factureclient + "'" +
                       " , remise_factureclient= " + this.remise_factureclient.ToString().ToString().Replace(',', '.') +
                       " , montantHT_factureclient= " + this.montantHT_factureclient.ToString().ToString().Replace(',', '.') +
                       " , apayer_factureclient = " + this.apayer_factureclient.ToString().ToString().Replace(',', '.') +
                       " , origine_factureclient = '" + this.origine_factureclient + "'" +
                       " , statut_factureclient ='" + this.statut_factureclient + "'" +
                       " , timbre_factureclient = " + this.timbre_factureclient.ToString().ToString().Replace(',', '.') +
                       " , modepayement_factureclient ='" + this.modepayement_factureclient + "'" +
                       " , avoir_factureclient =" + this.avoir_factureclient +
                       " , montantApresRAS_factureclient = " + montantApresRAS_factureclient.ToString().ToString().Replace(',', '.') +
                       " , notes_factureclient='" + this.notes_factureclient.ToString().Replace("'", "''") + "'" +
                       " , montantRestant_factureclient = " + montantRestant_factureclient.ToString().ToString().Replace(',', '.') +                       
                       "   where numero_factureclient = '" + this.numero_factureclient + "' ";
            return DataBaseConnexion.addOrUpdateElementInDataBase(CommandText, Program.SelectGlobalMessages.ImpUpdateFactureClient);
        }

        public static Boolean supprimerFactureClient(string _numeroFactureClient)
        {
            Boolean  executed = false;            
            if (LigneFactureClient.SupprimerAllLigneFactureClient(_numeroFactureClient)==true)
            {
                string CommandText2 = "delete from  " + DAL.DataBaseTableName.TableFactureClient +
                                      " where numero_factureclient = '"+_numeroFactureClient +"' ";
                executed = DataBaseConnexion.addOrUpdateElementInDataBase(CommandText2, Program.SelectGlobalMessages.ImpDeleteFactureClient);
            }
            return executed;
        }

        public static Boolean supprimerAllFactureClient()
        {
            Boolean executed = false;
            if (LigneFactureClient.SupprimerAllLigneFactureClient() == true)
            {
                string CommandText2 = "delete from  " + DAL.DataBaseTableName.TableFactureClient;
                executed = DataBaseConnexion.addOrUpdateElementInDataBase(CommandText2, Program.SelectGlobalMessages.ImpDeleteFactureClient);
            }
            return executed;
        }

        public int getNbreReglement(string _codFacture)
        {
            int NbreReg = 0;
            OdbcConnection connection = DAL.DataBaseConnexion.getConnection();

            try
            {
                OdbcCommand cmd = connection.CreateCommand();
                cmd.CommandText = "select count(identifiantmode_reglement) from  reglementfacture  where code_facture = '" +
                    _codFacture + "' ;";

                OdbcDataReader reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    NbreReg = reader.GetInt32(0);

                }
                reader.Close();
            }
            catch (OdbcException e)
            {
                MessageBox.Show(e.Message, Program.SelectGlobalMessages.SelectReglement,
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            return (NbreReg);
        }

        public static FactureClient getFactureClient(string _numeroFacture)
        {
            FactureClient facture = null;
            OdbcConnection connection = DAL.DataBaseConnexion.getConnection();
            //try
            //{
                OdbcCommand cmd = connection.CreateCommand();
                cmd.CommandText = "select * from " + DAL.DataBaseTableName.TableFactureClient +
                                    " where numero_factureclient = '" + _numeroFacture + "' ;";
                OdbcDataReader Reader = cmd.ExecuteReader();
                if (Reader.Read())
                {
                    facture = new FactureClient(Reader.GetString(0),Reader.GetString(1),
                        Reader.GetString(2) , Reader.GetDouble(3), Reader.GetDouble(4),
                        Reader.GetDouble(5) , Reader.GetString(6), Reader.GetString(7),
                        Reader.GetDouble(8) , Reader.GetString(9), Reader.GetInt32(10),
                        Reader.GetDouble(11), Reader.GetString(12),Reader.GetDouble(13)
                        );
                }

                Reader.Close();
            //}
            //catch (OdbcException e)
            //{
            //    MessageBox.Show(e.Message, Program.SelectGlobalMessages.SelectFactureClient,
            //        MessageBoxButtons.OK, MessageBoxIcon.Error);
            //}
            //catch (Exception)
            //{
            //    MessageBox.Show("Impossible de recupérer cette Facture Client.",
            //         Program.SelectGlobalMessages.SelectFactureClient,
            //       MessageBoxButtons.OK, MessageBoxIcon.Error);
            //}
            return facture;
        }


        public Boolean updateModeRegFacture(string _newstat)
        {
            string CommandText = "update factureclient set modepayement_factureclient = '" + _newstat +
                 "' where numero_factureclient = '" + this.numero_factureclient + "' ";

            return DataBaseConnexion.addOrUpdateElementInDataBase(CommandText, Program.SelectGlobalMessages.ImpUpdateFactureClient);
        }
       public Boolean updateMtRestantFacture(Double _newMtrestant)
        {
            string CommandText = "update factureclient set mtrestantfact =" + _newMtrestant.ToString().ToString().Replace(',', '.') +
                 " where numero_factureclient = '" + this.numero_factureclient + "';";

            return DataBaseConnexion.addOrUpdateElementInDataBase(CommandText, Program.SelectGlobalMessages.ImpUpdateFactureClient);
        }

       public static Boolean updateModePayementFacture(string _numeroFacture, string _newstat)
        {
            string CommandText = "update " + DAL.DataBaseTableName.TableFactureClient + 
                                " set modepayement_factureclient = '" + _newstat + "' " +
                                " where numero_factureclient = '" + _numeroFacture + "' ";
            return DataBaseConnexion.addOrUpdateElementInDataBase(CommandText, Program.SelectGlobalMessages.ImpUpdateFactureClient);
        }

       public static Boolean updateStatutFacturationOfListOfBonLivraisonByFacture(string _numeroFacture)
        {
            BonLivraison BL = new BonLivraison();
            ArrayList tab = new ArrayList();
            Boolean executedWell = true;
            // récuprer tous les codes BL de la devisClient en cours :
            tab = getALLBLByFacture(_numeroFacture);
            
            if (tab != null)
            {
                for (int i = 0; i < tab.Count; i++)
                {
                    BonLivraison bll = (BonLivraison)tab[i];
                    executedWell = BonLivraison.updateStatutFacturationOfBonLivraison(bll.code_bonlivraison, DAL.VariablesGlobales.EntityFacture);
                }
            }
            return executedWell;           
        }
        
        public ArrayList getFactureImpaye()
        {
            ArrayList tab_factClient = new ArrayList();
            FactureClient v_facture_ouverte = null;
            OdbcConnection connection = DAL.DataBaseConnexion.getConnection();

            try
            {
                OdbcCommand cmd = connection.CreateCommand();

                cmd.CommandText = "select * from " +  DAL.DataBaseTableName.TableFactureClient +
                       " where   statut_factureclient='" + Program.SelectGlobalMessages.FactureOuverte + "' and avoir_factureclient =0;";

                OdbcDataReader Reader = cmd.ExecuteReader();


                while (Reader.Read())
                {
                    v_facture_ouverte = new FactureClient(Reader.GetString(0),
                                                  Reader.GetString(1),
                                                Reader.GetString(2),
                                                Reader.GetDouble(3),
                                                Reader.GetDouble(4),
                                                Reader.GetDouble(5),
                                                Reader.GetString(6),
                                                Reader.GetString(7),
                                                Reader.GetDouble(8),
                                                 Reader.GetString(9), Reader.GetInt32(10), Reader.GetDouble(11),
                                                 Reader.GetString(12), Reader.GetDouble(13));
                    tab_factClient.Add(v_facture_ouverte);
                }

                Reader.Close();

            }
            catch (OdbcException e)
            {
                MessageBox.Show(e.Message, Program.SelectGlobalMessages.SelectFactureClient,
                    MessageBoxButtons.OK, MessageBoxIcon.Error);

            }

            if (tab_factClient.Count == 0)
                return null;
            else
                return tab_factClient;

        }

        //recupérer tous les currentLigneBonLivraison pour un currentFournisseur et un poroduit  passé en paramétre
        public ArrayList getALLFacturesNonPayees()
        {
            ArrayList tab_factClient = new ArrayList();
            FactureClient v_facture_ouverte = null;
            OdbcConnection connection = DAL.DataBaseConnexion.getConnection();
            try
            {
                OdbcCommand cmd = connection.CreateCommand();
                cmd.CommandText = "select * from  " + DAL.DataBaseTableName.TableFactureClient +
                       " where   statut_factureclient='" + Program.SelectGlobalMessages.FactureOuverte + "' and avoir_factureclient =0;";
                OdbcDataReader Reader = cmd.ExecuteReader();
                while (Reader.Read())
                {
                    v_facture_ouverte = new FactureClient(Reader.GetString(0),
                                                  Reader.GetString(1),
                                                Reader.GetString(2),
                                                Reader.GetDouble(3),
                                                Reader.GetDouble(4),
                                                Reader.GetDouble(5),
                                                Reader.GetString(6),
                                                Reader.GetString(7),
                                                Reader.GetDouble(8),
                                                 Reader.GetString(9), Reader.GetInt32(10),
                                                 Reader.GetDouble(11), Reader.GetString(12), Reader.GetDouble(13));
                    tab_factClient.Add(v_facture_ouverte);
                }
                Reader.Close();
            }
            catch (OdbcException e)
            {
                MessageBox.Show(e.Message, Program.SelectGlobalMessages.SelectFactureClient,
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            if (tab_factClient.Count == 0)
                return null;
            else
                return tab_factClient;
        }

        // recupérer tous les factures non payé pour un currentFournisseur  passé en paramétre
        public ArrayList getALLFacture()
        {
            string f = "Libre";
            string f2 = "A partir Devis";
            //A partir BS
            ArrayList tab_factClient = new ArrayList();
            FactureClient v_facture_ouverte = null;
            OdbcConnection connection = DataBaseConnexion.getConnection();
            //  tab_CommandeAchat = null;

            try
            {
                OdbcCommand cmd = connection.CreateCommand();
                cmd.CommandText = "select * from   " + DAL.DataBaseTableName.TableFactureClient + 
                                    "  where avoir_factureclient = 0 and origine_factureclient='" + f + "'" +
                                    " OR  origine_factureclient='" + f2 + "'  order by Format(date_factureclient, 'mm/dd/yyyy')   ";
                                
                OdbcDataReader Reader = cmd.ExecuteReader();
                while (Reader.Read())
                {
                    v_facture_ouverte = new FactureClient(Reader.GetString(0),
                                                  Reader.GetString(1),
                                                Reader.GetString(2),
                                                Reader.GetDouble(3),
                                                Reader.GetDouble(4),
                                                Reader.GetDouble(5),
                                                Reader.GetString(6),
                                                Reader.GetString(7),
                                                Reader.GetDouble(8),
                                                 Reader.GetString(9),
                                                 Reader.GetInt32(10),
                                                 Reader.GetDouble(11), Reader.GetString(12), Reader.GetDouble(13));
                    tab_factClient.Add(v_facture_ouverte);
                }
                Reader.Close();
            }
            catch (OdbcException e)
            {
                MessageBox.Show(e.Message, Program.SelectGlobalMessages.SelectFactureClient,
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            return tab_factClient;
        }
             
        // recupérer tous les factures non payé pour un currentFournisseur  passé en paramétre
        public ArrayList getALLFacturesByClient(int _codeClt)
        {            
            ArrayList tab_factClient = new ArrayList();
            FactureClient v_facture_ouverte = null;
            OdbcConnection connection = DataBaseConnexion.getConnection();          

            try
            {
                int etat = 0;
                OdbcCommand cmd = connection.CreateCommand();
                cmd.CommandText = "select * from   factureclient where  avoir_factureclient =" + etat +
                    "  and   codeclient_factureclient=" + _codeClt + "   order by Format(date_factureclient, 'mm/dd/yyyy') ";
               OdbcDataReader Reader = cmd.ExecuteReader();
                while (Reader.Read())
                {
                    v_facture_ouverte = new FactureClient(Reader.GetString(0),
                                                  Reader.GetString(1),
                                                Reader.GetString(2),
                                                Reader.GetDouble(3),
                                                Reader.GetDouble(4),
                                                Reader.GetDouble(5),
                                                Reader.GetString(6),
                                                Reader.GetString(7),
                                                Reader.GetDouble(8),
                                                 Reader.GetString(9),
                                                 Reader.GetInt32(10),
                                                 Reader.GetDouble(11), Reader.GetString(12), Reader.GetDouble(13));
                    tab_factClient.Add(v_facture_ouverte);
                }
                Reader.Close();
            }
            catch (OdbcException e)
            {
                MessageBox.Show(e.Message, Program.SelectGlobalMessages.SelectFactureClient,
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            return tab_factClient;
        }
                
        // recupérer tous les factures non payé pour un currentFournisseur  passé en paramétre
        public ArrayList getALLFactures()
        {
            //string f = "Libre";
            //string f2 = "A partir Devis";
            ArrayList tab_factClient = new ArrayList();
            FactureClient v_facture_ouverte = null;
            OdbcConnection connection = DataBaseConnexion.getConnection();
            //  tab_CommandeAchat = null;

            try
            {
                OdbcCommand cmd = connection.CreateCommand();

                cmd.CommandText = "select * from   factureclient  where avoir_factureclient = 0    order by Format(date_factureclient, 'mm/dd/yyyy')  ";

                OdbcDataReader Reader = cmd.ExecuteReader();


                while (Reader.Read())
                {
                    v_facture_ouverte = new FactureClient(Reader.GetString(0),
                                                  Reader.GetString(1),
                                                Reader.GetString(2),
                                                Reader.GetDouble(3),
                                                Reader.GetDouble(4),
                                                Reader.GetDouble(5),
                                                Reader.GetString(6),
                                                Reader.GetString(7),
                                                Reader.GetDouble(8),
                                                 Reader.GetString(9),
                                                 Reader.GetInt32(10),
                                                 Reader.GetDouble(11), Reader.GetString(12), Reader.GetDouble(13));
                    tab_factClient.Add(v_facture_ouverte);

                }

                Reader.Close();

            }
            catch (OdbcException e)
            {
                MessageBox.Show(e.Message, Program.SelectGlobalMessages.SelectFactureClient,
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            return tab_factClient;
        }


        /************************************************************************************/
        /************************************************************************************/
        // recupérer tous les factures non payé pour un currentFournisseur  passé en paramétre
        public ArrayList getALLFacturessAvoir()
        {

            ArrayList tab_factClient = new ArrayList();
            FactureClient v_facture_ouverte = null;
            OdbcConnection connection = DataBaseConnexion.getConnection();
            //  tab_CommandeAchat = null;

            try
            {

                OdbcCommand cmd = connection.CreateCommand();

                cmd.CommandText = "select * from   factureclient  where avoir_factureclient = 1  " + 
                    " order by Format(date_factureclient, 'mm/dd/yyyy')";

                //+" and avoir_factureclient = 0;";

                OdbcDataReader Reader = cmd.ExecuteReader();


                while (Reader.Read())
                {
                    v_facture_ouverte = new FactureClient(Reader.GetString(0),
                                                  Reader.GetString(1),
                                                Reader.GetString(2),
                                                Reader.GetDouble(3),
                                                Reader.GetDouble(4),
                                                Reader.GetDouble(5),
                                                Reader.GetString(6),
                                                Reader.GetString(7),
                                                Reader.GetDouble(8),
                                                 Reader.GetString(9),
                                                 Reader.GetInt32(10),
                                                 Reader.GetDouble(11), Reader.GetString(12), Reader.GetDouble(13));
                    tab_factClient.Add(v_facture_ouverte);

                }

                Reader.Close();

            }
            catch (OdbcException e)
            {
                MessageBox.Show(e.Message, Program.SelectGlobalMessages.SelectFactureClient,
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            return tab_factClient;
        }


        /************************************************************************************/
        /************************************************************************************/
        // recupérer tous les factures non payé pour un currentFournisseur  passé en paramétre
        public ArrayList getALLFacturesClient(int _codeClt)
        {
            //string f = "Libre";
            //string f2 = "A partir Devis";
            ArrayList tab_factClient = new ArrayList();
            FactureClient v_facture_ouverte = null;
            OdbcConnection connection = DataBaseConnexion.getConnection();
            //  tab_CommandeAchat = null;
            try
            {
                OdbcCommand cmd = connection.CreateCommand();

                cmd.CommandText = "select * from   factureclient  where avoir_factureclient = 0  and codeclient_factureclient='" +
                                    _codeClt + "' order by Format(date_factureclient, 'mm/dd/yyyy')";

                OdbcDataReader Reader = cmd.ExecuteReader();
                
                while (Reader.Read())
                {
                    v_facture_ouverte = new FactureClient(Reader.GetString(0),
                                                  Reader.GetString(1),
                                                Reader.GetString(2),
                                                Reader.GetDouble(3),
                                                Reader.GetDouble(4),
                                                Reader.GetDouble(5),
                                                Reader.GetString(6),
                                                Reader.GetString(7),
                                                Reader.GetDouble(8),
                                                 Reader.GetString(9),
                                                 Reader.GetInt32(10),
                                                 Reader.GetDouble(11), Reader.GetString(12), Reader.GetDouble(13));
                    tab_factClient.Add(v_facture_ouverte);

                }

                Reader.Close();

            }
            catch (OdbcException e)
            {
                MessageBox.Show(e.Message, Program.SelectGlobalMessages.SelectFactureClient,
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            return tab_factClient;

        }

        // recupérer tous les factures non payé pour un currentFournisseur  passé en paramétre
        public ArrayList getALLFacturesAvoirClient(string _codeClt)
        {
            //string f = "Libre";
            //string f2 = "A partir Devis";
            ArrayList tab_factClient = new ArrayList();
            FactureClient v_facture_ouverte = null;
            OdbcConnection connection = DataBaseConnexion.getConnection();
            //  tab_CommandeAchat = null;

            try
            {
                OdbcCommand cmd = connection.CreateCommand();

                cmd.CommandText = "select * from factureclient  where avoir_factureclient =1 and codeclient_factureclient='" +
                                    _codeClt + "' order by Format(date_factureclient, 'mm/dd/yyyy')  ";

                //+" and avoir_factureclient = 0;";

                OdbcDataReader Reader = cmd.ExecuteReader();


                while (Reader.Read())
                {
                    v_facture_ouverte = new FactureClient(Reader.GetString(0),
                                                  Reader.GetString(1),
                                                Reader.GetString(2),
                                                Reader.GetDouble(3),
                                                Reader.GetDouble(4),
                                                Reader.GetDouble(5),
                                                Reader.GetString(6),
                                                Reader.GetString(7),
                                                Reader.GetDouble(8),
                                                 Reader.GetString(9),
                                                 Reader.GetInt32(10),
                                                 Reader.GetDouble(11), Reader.GetString(12), Reader.GetDouble(13));

                    tab_factClient.Add(v_facture_ouverte);

                }
                Reader.Close();
            }
            catch (OdbcException e)
            {
                MessageBox.Show(e.Message, Program.SelectGlobalMessages.SelectFactureClient,
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            return tab_factClient;
        }

        
        public static ArrayList getALLBLByFacture(string _codeFacture)
        {            
            ArrayList tab_ALLBLByFacture = null;
            OdbcConnection connection = DataBaseConnexion.getConnection();
            BonLivraison BL = null;
            try
            {
                OdbcCommand cmd = connection.CreateCommand();
                cmd.CommandText = "Select codebonlivraison_lignefactureclient from " + 
                    DAL.DataBaseTableName.TableLigneFactureClient +  " Where numero_factureclient = '" + _codeFacture + "'";
                OdbcDataReader Reader = cmd.ExecuteReader();
                if (Reader.HasRows == true)
                {
                    tab_ALLBLByFacture = new ArrayList();


                    while (Reader.Read())
                    {
                        BL = BonLivraison.getBonLivraison(Reader.GetString(0));
                    
                        tab_ALLBLByFacture.Add(BL);
                    }
                    Reader.Close();
                }
                return tab_ALLBLByFacture;
            }
            catch (OdbcException e)
            {
                MessageBox.Show(e.Message, Program.SelectGlobalMessages.SelectFactureClient,
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }            
        }

        public ArrayList getAllReglementFacture(string _codefact)
        {
            ArrayList tab_lignesReg = new ArrayList();
            ReglementFacture regfact = null;
            OdbcConnection connection = DAL.DataBaseConnexion.getConnection();
            try
            {
                OdbcCommand cmd = connection.CreateCommand();

                cmd.CommandText = "select * from  " + DAL.DataBaseTableName.TableReglementFactureClient +
                       " where code_facture = '" + _codefact + "' ORDER BY identifiantmode_reglement ";

                OdbcDataReader Reader = cmd.ExecuteReader();


                while (Reader.Read())
                {
                    regfact = new ReglementFacture(Reader.GetInt32(0),
                                                 Reader.GetString(1),
                                                 Reader.GetDouble(2),
                                                 Reader.GetString(3),
                                                 Reader.GetString(4),
                                                 Reader.GetString(5),
                                                 Reader.GetString(6),
                                                 Reader.GetString(7)
                                             
                                                );
                    tab_lignesReg.Add(regfact);
                }
                Reader.Close();

            }
            catch (OdbcException e)
            {
                MessageBox.Show(e.Message, Program.SelectGlobalMessages.SelectReglement,
                    MessageBoxButtons.OK, MessageBoxIcon.Error);


            }
            if (tab_lignesReg.Count == 0)
                return null;
            else
                return tab_lignesReg;


        }

    }
}
