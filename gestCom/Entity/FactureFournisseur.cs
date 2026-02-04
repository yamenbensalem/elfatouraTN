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
using T4C_Commercial_Project.Forms;

namespace T4C_Commercial_Project.Entity
{
    public class FactureFournisseur
    {

        public static string Separateur = CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator;

        public int  numero_facturefournisseur;
        public string codefournisseur_facturefournisseur;
        public string date_facturefournisseur;
        public string statut_facturefournisseur;
        public string modePayement_facturefournisseur;
        public string originefacture_facturefournisseur;
        public double montantHT_facturefournisseur;
        public double apayer_facturefournisseur;
        public double timbre_facturefournisseur;
        public double remise_facturefournisseur;
        public double montantRestant_facturefournisseur;
        public string notes_facturefournisseur;

        public FactureFournisseur(int _numero, string _codeFournisseur, string _date, double _remise, double _montantHT, double _apayer, string _OrigineFacture,
                                            string _statut ,  double _timbre, string _modePayement, double _mtrestantfact,string _note)
        {
            this.numero_facturefournisseur = _numero;
            this.codefournisseur_facturefournisseur = _codeFournisseur;
            this.date_facturefournisseur = _date;
            this.remise_facturefournisseur = _remise;
            this.montantHT_facturefournisseur = _montantHT;
            this.apayer_facturefournisseur = _apayer;
            this.originefacture_facturefournisseur = _OrigineFacture;
            this.statut_facturefournisseur = _statut;
            this.timbre_facturefournisseur = _timbre;
            this.modePayement_facturefournisseur = _modePayement;
            this.montantRestant_facturefournisseur = _mtrestantfact;
            this.notes_facturefournisseur = _note;
           
        }
        
        public FactureFournisseur(int   numero_Facture)
        {
            this.numero_facturefournisseur = numero_Facture;
        }

        public FactureFournisseur()
        {

        }
                
        public Boolean ajouterFactureFournisseur()
        {
            string CommandText = "INSERT INTO " + DAL.DataBaseTableName.TableFactureFournisseur + 
                       " VALUES ("+
                       this.numero_facturefournisseur +",'" +
                       this.codefournisseur_facturefournisseur + "','" +
                       this.date_facturefournisseur + "'," +
                       this.remise_facturefournisseur.ToString().ToString().Replace(',', '.') + "," +
                       this.montantHT_facturefournisseur.ToString().ToString().Replace(',', '.') + "," +
                       this.apayer_facturefournisseur.ToString().ToString().Replace(',', '.') + ",'" +
                       this.originefacture_facturefournisseur + "','" +
                       this.statut_facturefournisseur +"',"+
                       this.timbre_facturefournisseur.ToString().ToString().Replace(',', '.') + ",'" +
                       this.modePayement_facturefournisseur + "'," +
                       this.montantRestant_facturefournisseur.ToString().ToString().Replace(',', '.') + ",'" +
                       this.notes_facturefournisseur.ToString().Replace("'", "''") +"'"+
                        ")";
            return DataBaseConnexion.addOrUpdateElementInDataBase(CommandText,Program.SelectGlobalMessages.ImpAddFactureFournisseur);
        }
        public Boolean modifierFactureFournisseur()
        {
            string CommandText = "update " + DAL.DataBaseTableName.TableFactureFournisseur +
                       "  set date_facturefournisseur ='" + this.date_facturefournisseur +
                       "',remise_facturefournisseur =" + this.remise_facturefournisseur.ToString().ToString().Replace(',', '.') +
                       ", montantHT_facturefournisseur =" + this.montantHT_facturefournisseur.ToString().ToString().Replace(',', '.') +
                       ", apayer_facturefournisseur =" + this.apayer_facturefournisseur.ToString().ToString().Replace(',', '.') +
                       ", originefacture_facturefournisseur ='" + this.originefacture_facturefournisseur +
                       "',statut_facturefournisseur ='" + this.statut_facturefournisseur +
                       "',timbre_facturefournisseur =" + this.timbre_facturefournisseur.ToString().ToString().Replace(',', '.') +
                       ", modePayement_facturefournisseur ='" + this.modePayement_facturefournisseur +
                       "',montantRestant_facturefournisseur=" + this.montantRestant_facturefournisseur.ToString().ToString().Replace(',', '.') +
                       "  where numero_facturefournisseur =" + this.numero_facturefournisseur +
                       "  AND codefournisseur_facturefournisseur ='" + this.codefournisseur_facturefournisseur + "';";                  
            return DataBaseConnexion.addOrUpdateElementInDataBase(CommandText, Program.SelectGlobalMessages.ImpUpdateFactureFournisseur);
        }

        public static Boolean supprimerFactureFournisseur(int   _numero_facturefournisseur)
        {
            string CommandText = "";
            LigneFactureFournisseur ligneFactureFournisseur = new LigneFactureFournisseur(_numero_facturefournisseur);
            if (ligneFactureFournisseur.SupprimerAllLigneFromFacture() == true)
            {
                updateStatutdesBR(_numero_facturefournisseur);
                CommandText = "delete from " + DAL.DataBaseTableName.TableFactureFournisseur + " where numero_facturefournisseur=" + _numero_facturefournisseur + ";";
            }
                return DataBaseConnexion.addOrUpdateElementInDataBase(CommandText, Program.SelectGlobalMessages.ImpDeleteFactureFournisseur);
            
        }
        public static FactureFournisseur getFactureFournisseur(string _numeroFacture)
        {
            FactureFournisseur facture = null;
            OdbcConnection connection = DAL.DataBaseConnexion.getConnection();
            try
            {
                OdbcCommand cmd = connection.CreateCommand();
                cmd.CommandText = "SELECT * FROM " + DAL.DataBaseTableName.TableFactureFournisseur + 
                    " where numero_facturefournisseur ='" + _numeroFacture + "'";
                OdbcDataReader Reader = cmd.ExecuteReader();
                if (Reader.Read())
                {
                    facture = new FactureFournisseur(Reader.GetInt32 (0),Reader.GetString(1),
                                                     Reader.GetString(2), Reader.GetInt32(3), Reader.GetInt32(4),
                                                     Reader.GetInt32(5), Reader.GetString(6), Reader.GetString(7),
                                                     Reader.GetDouble(8), Reader.GetString(9), Reader.GetDouble(10),
                                                     Reader.GetString(11));

                }
                Reader.Close();
            }
            catch (OdbcException e)
            {
                MessageBox.Show(e.Message, Program.SelectGlobalMessages.SelectFactureFournisseur,
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception)
            {
                MessageBox.Show("Impossible de recupérer cette Facture Fournisseur.",
                     Program.SelectGlobalMessages.SelectFactureFournisseur,
                   MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            return facture;
        }

        public static ArrayList getAllLigneFactureFournisseur(int _codefact)
        {
            ArrayList tab_lignesBL = new ArrayList();
            LigneFactureFournisseur lignefact = null;
            OdbcConnection connection = DAL.DataBaseConnexion.getConnection();
            try
            {
                OdbcCommand cmd = connection.CreateCommand();

                cmd.CommandText = "select * from  " + DAL.DataBaseTableName.TableLigneFactureFournisseur +
                       " where numero_facturefournisseur=" + _codefact + " ORDER BY num_lignefacturefournisseur ";
                OdbcDataReader Reader = cmd.ExecuteReader();
                while (Reader.Read())
                {
                    lignefact = new LigneFactureFournisseur(Reader.GetInt32(0),
                                                Reader.GetInt32(1),
                                                Reader.GetString(2),
                                                Reader.GetString(3),
                                                Reader.GetDouble(4),
                                                Reader.GetDouble(5),
                                                Reader.GetDouble(6),
                                                Reader.GetDouble(7),
                                                Reader.GetDouble(8),
                                                Reader.GetDouble(9),
                                                Reader.GetInt32(10)
                                              
                                                );
                    tab_lignesBL.Add(lignefact);
                }
                Reader.Close();
            }
            catch (OdbcException e)
            {
                MessageBox.Show(e.Message, Program.SelectGlobalMessages.SelectFactureFournisseur,
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            if (tab_lignesBL.Count == 0)
                return null;
            else
                return tab_lignesBL;
        }

        public static int[] nombreBRFacture(int _codefact)
        {
            // int nbre = 0;
            int[] tabCodBl = new int[10];
            OdbcConnection connection = DAL.DataBaseConnexion.getConnection();
            try
            {
                OdbcCommand cmd = connection.CreateCommand();
                cmd.CommandText = "select  code_bonreception from " +
                     DAL.DataBaseTableName.TableBonReceptionFacture +
                     " where numero_facturefournisseur=" + _codefact;
                OdbcDataReader reader = cmd.ExecuteReader();
                int i = 0;
                while (reader.Read() == true)
                {
                    tabCodBl[i] = reader.GetInt32(0);
                    i = i + 1;
                    // nbre = reader.GetInt32(0); 
                }
                reader.Close();
            }
            catch (OdbcException e)
            {
                MessageBox.Show(e.Message, Program.SelectGlobalMessages.SelectBonReception,
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            return tabCodBl;
        }
                
        public Boolean updateModeReg(string _newstat)
        {
            string CommandText = "update " + DAL.DataBaseTableName.TableFactureFournisseur + " set modePayement_facturefournisseur = '" +
                                _newstat + "' where numero_facturefournisseur= " + this.numero_facturefournisseur + ";";
            return DataBaseConnexion.addOrUpdateElementInDataBase(CommandText, Program.SelectGlobalMessages.ImpUpdateFactureFournisseur);
        }

        public Boolean updateMontantRestant(Double _newsMt)
        {
            string CommandText = "update " + DAL.DataBaseTableName.TableFactureFournisseur + " set 	mtrestantfact = " + _newsMt.ToString().ToString().Replace(',', '.') +
               " where numero_facturefournisseur = " + this.numero_facturefournisseur + ";";
            return DataBaseConnexion.addOrUpdateElementInDataBase(CommandText, Program.SelectGlobalMessages.ImpUpdateFactureFournisseur);
        }

        public Boolean updateStatut(string _newstat)
        {
            string CommandText = "update " + DAL.DataBaseTableName.TableFactureFournisseur + " set statut_facturefournisseur = '" + _newstat +
                  "' where numero_facturefournisseur = " + this.numero_facturefournisseur + ";";
            return DataBaseConnexion.addOrUpdateElementInDataBase(CommandText, Program.SelectGlobalMessages.ImpUpdateFactureFournisseur);
        }

        public static void updateStatutdesBR(int  _numfactBr)
        {            
            ArrayList tab = new ArrayList();
            tab = getBrFactFournisseur(_numfactBr);
            if (tab != null)
            {
                for (int i = 0; i < tab.Count; i++)
                {
                    BonReception.setBonReceptionAsFacture((string)tab[i],DAL.VariablesGlobales.EntityFacture);
                }
            }
        }

        public static int getNbreReglment(int _codFacture)
        {
            int NbreReg = 0;
            OdbcConnection connection = DAL.DataBaseConnexion.getConnection();
            try
            {
                OdbcCommand cmd = connection.CreateCommand();
                cmd.CommandText = "select count(code_reglement) from  reglementfacture   where code_facture=" + _codFacture + ";";
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
            return NbreReg;
        }

        public static ArrayList getALLFacturesOfOneFournisseur(int _codeFourni)
        {
            //string f = "Libre";
            //string f2 = "A partir Devis";
            ArrayList tab_factClient = new ArrayList();
            FactureFournisseur v_facture_ouverte = null;
            OdbcConnection connection = DataBaseConnexion.getConnection();
            //  tab_CommandeAchat = null;

            try
            {
                OdbcCommand cmd = connection.CreateCommand();
                cmd.CommandText = "select * from   facturefournisseur  where  codefournisseur_facturefournisseur=" + 
                                        _codeFourni + " order by Format(date_facturefournisseur, 'mm/dd/yyyy')  ";
                OdbcDataReader Reader = cmd.ExecuteReader();
                while (Reader.Read())
                {
                    v_facture_ouverte = new FactureFournisseur(Reader.GetInt32(0), Reader.GetString(1),
                        Reader.GetString(2), Reader.GetInt32(3), Reader.GetInt32(4),
                        Reader.GetInt32(5), Reader.GetString(6), Reader.GetString(7),
                        Reader.GetDouble(8), Reader.GetString(9), Reader.GetDouble(10),
                        Reader.GetString(11) 
                        );
                    tab_factClient.Add(v_facture_ouverte);
                }
                Reader.Close();
            }
            catch (OdbcException e)
            {
                MessageBox.Show(e.Message, Program.SelectGlobalMessages.SelectFactureFournisseur,
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            return tab_factClient;
        }

        public static ArrayList getALLFacturesFournisseur()
        {           
            ArrayList tab_factClient = new ArrayList();
            FactureFournisseur v_facture_ouverte = null;
            OdbcConnection connection = DataBaseConnexion.getConnection();
            try
            {
                OdbcCommand cmd = connection.CreateCommand();
                cmd.CommandText = "select * from   facturefournisseur   order by Format(date_facturefournisseur, 'mm/dd/yyyy')   ";               
                OdbcDataReader Reader = cmd.ExecuteReader();
                while (Reader.Read())
                {
                    v_facture_ouverte = new FactureFournisseur(Reader.GetInt32(0), Reader.GetString(1),
                        Reader.GetString(2), Reader.GetInt32(3), Reader.GetInt32(4),
                        Reader.GetInt32(5), Reader.GetString(6), Reader.GetString(7),
                        Reader.GetDouble(8), Reader.GetString(9), Reader.GetDouble(10),
                        Reader.GetString(11) 

                        );
                    tab_factClient.Add(v_facture_ouverte);
                }
                Reader.Close();
            }
            catch (OdbcException e)
            {
                MessageBox.Show(e.Message, Program.SelectGlobalMessages.SelectFactureFournisseur,
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            return tab_factClient;
        }

        public static ArrayList getBrFactFournisseur(int   _codeFactfourni)
        {           
            ArrayList tab_factClient = new ArrayList();            
            int codeBr = 0;
            OdbcConnection connection = DataBaseConnexion.getConnection();
            try
            {
                OdbcCommand cmd = connection.CreateCommand();
                cmd.CommandText = " select distinct codeBonReception_lignefacturefournisseur from  lignefacturefournisseur where  numero_facturefournisseur=" + 
                                    _codeFactfourni;
                OdbcDataReader Reader = cmd.ExecuteReader();
                while (Reader.Read())
                {
                    codeBr = Reader.GetInt32(0);
                    tab_factClient.Add(codeBr);
                }
                Reader.Close();
            }
            catch (OdbcException e)
            {
                MessageBox.Show(e.Message, Program.SelectGlobalMessages.SelectFactureFournisseur,
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            return tab_factClient;
        }


         public ArrayList getAllReglementFacture(int _codefact)
        {
            ArrayList tab_lignesReg = new ArrayList();
            ReglementFournisseur regfact = null;
            OdbcConnection connection = DAL.DataBaseConnexion.getConnection();
            try
            {
                OdbcCommand cmd = connection.CreateCommand();

                cmd.CommandText = "select * from  " + DAL.DataBaseTableName.TableReglementFactureFournisseur +
                       " where numfacture_reglement=" + _codefact + " ORDER BY code_reglement ";

                OdbcDataReader Reader = cmd.ExecuteReader();


                while (Reader.Read())
                {
                    regfact = new ReglementFournisseur(Reader.GetInt32(0),
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

