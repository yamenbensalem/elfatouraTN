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
    public class LigneFactureClient
    {
        public static string Separateur = CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator;

        public int numero_lignefactureclient;
        public string numero_factureclient;
        public string codeproduit_lignefactureclient;
        public string designationproduit_lignefactureclient;
        public double fodecproduit_lignefactureclient;
        public double quantite_lignefactureclient;
        public double prixunitaire_lignefactureclient;
        public double montantHT_lignefactureclient;
        public double tvaproduit_lignefactureclient;
        public double remise_lignefactureclient;
        public string   codebonlivraison_lignefactureclient;
        public int codebonsortie_lignefactureclient;

        public LigneFactureClient(int _numligneFacture, string _numeroFacture, string _codeProduit, string _designationProduit,
                      double _fodecProduit, double _quantite, double _prixUnitaire, double _montantHT, double _tvaProduit, double _remise,string  _codebonlivraison,int _codebonsortie)
        {
            this.numero_lignefactureclient = _numligneFacture;
            this.numero_factureclient = _numeroFacture;
            this.codeproduit_lignefactureclient = _codeProduit;
            this.designationproduit_lignefactureclient = _designationProduit;
            this.fodecproduit_lignefactureclient = _fodecProduit;
            this.quantite_lignefactureclient = _quantite;
            this.prixunitaire_lignefactureclient = _prixUnitaire;
            this.montantHT_lignefactureclient = _montantHT;
            this.tvaproduit_lignefactureclient = _tvaProduit;
            this.remise_lignefactureclient = _remise;
            this.codebonlivraison_lignefactureclient=_codebonlivraison;
            this.codebonsortie_lignefactureclient = _codebonsortie;
        }

        
        public LigneFactureClient()
        {

        }

        public Boolean ajouterLigneFacture()
        {
            string CommandText = "insert into " + DAL.DataBaseTableName.TableLigneFactureClient + " values(" +
                       this.numero_lignefactureclient + "," +
                       "'" + this.numero_factureclient + "'," +
                       "'" + this.codeproduit_lignefactureclient + "'," +
                       "'" + this.designationproduit_lignefactureclient.ToString().Replace("'", "''") + "'," +
                       this.fodecproduit_lignefactureclient + "," +
                       this.quantite_lignefactureclient.ToString().ToString().Replace(',', '.') + "," +
                       this.prixunitaire_lignefactureclient.ToString().ToString().Replace(',', '.') + "," +
                       this.montantHT_lignefactureclient.ToString().ToString().Replace(',', '.') + "," +
                       this.tvaproduit_lignefactureclient.ToString().ToString().Replace(',', '.') + "," +
                       this.remise_lignefactureclient.ToString().ToString().Replace(',', '.') + ",'" +
                       this.codebonlivraison_lignefactureclient + "'," +
                       this.codebonsortie_lignefactureclient +
                       ");";
            return DataBaseConnexion.addOrUpdateElementInDataBase(CommandText, Program.SelectGlobalMessages.ImpAddLigneFactureClient);
        }

        public Boolean modifierLigneFacture()
        {
           string CommandText = "update " + DAL.DataBaseTableName.TableLigneFactureClient + " set " +
                    " designationproduit_lignefactureclient ='" + this.designationproduit_lignefactureclient.ToString().Replace("'", "''") + "," +
                    " fodecproduit_lignefactureclient = " + this.fodecproduit_lignefactureclient.ToString().ToString().Replace(',', '.') + "," +
                    " quantite_factureclient = " + this.quantite_lignefactureclient.ToString().ToString().Replace(',', '.') + "," +
                    " prixunitaire_factureclient = " + this.prixunitaire_lignefactureclient.ToString().ToString().Replace(',', '.') + "," +
                    " montantHT_factureclient = " + this.montantHT_lignefactureclient.ToString().ToString().Replace(',', '.') + "," +
                    " tvaproduit_factureclient = " + this.tvaproduit_lignefactureclient.ToString().ToString().Replace(',', '.') + "," +
                    " remise_factureclient = " + this.remise_lignefactureclient.ToString().ToString().Replace(',', '.') +
                    " where numero_factureclient = '" + this.numero_factureclient + "'" +
                    " and numero_lignefactureclient = " + this.numero_lignefactureclient +
                    " and codeproduit_factureclient = '" + this.codeproduit_lignefactureclient + "'";
            return DataBaseConnexion.addOrUpdateElementInDataBase(CommandText, Program.SelectGlobalMessages.ImpUpdateLigneFactureClient);
        }

        public static Boolean SupprimerLigneFacture(int _numeroLigneFactureClient, string _numeroFactureClient, string _codeProduit)
        {
            string CommandText = "delete from lignefactureclient " +
                      " where numero_factureclient = '" + _numeroFactureClient + "'" +
                       " and numero_lignefactureclient = " + _numeroLigneFactureClient +
                      " and codeproduit_factureclient = '" + _codeProduit + "' ;";
            return DataBaseConnexion.addOrUpdateElementInDataBase(CommandText, Program.SelectGlobalMessages.ImpDeleteLigneFactureClient);
        }

        public static Boolean SupprimerAllLigneFactureClient(string _numFacture)
        {
            string CommandText = "delete from " + DAL.DataBaseTableName.TableLigneFactureClient +
                    " where numero_factureclient = '"+_numFacture +"' ;";
            return DataBaseConnexion.addOrUpdateElementInDataBase(CommandText, Program.SelectGlobalMessages.ImpDeleteFactureClient);
        }

        public static Boolean SupprimerAllLigneFactureClient()
        {
            string CommandText = "delete from " + DAL.DataBaseTableName.TableLigneFactureClient;
            return DataBaseConnexion.addOrUpdateElementInDataBase(CommandText, Program.SelectGlobalMessages.ImpDeleteFactureClient);
        }

        public static ArrayList getListCodeProduitsFactures(string _numFacture)
        {
            ArrayList produits = null;
            Produit currentProduit = null;
            OdbcConnection connection = DAL.DataBaseConnexion.getConnection();

            try
            {
                OdbcCommand cmd = connection.CreateCommand();
                cmd.CommandText = "select * from " + DAL.DataBaseTableName.TableLigneFactureClient +
                        " where numero_factureclient = '" + _numFacture + "'";
                OdbcDataReader Reader = cmd.ExecuteReader();
                if (Reader.HasRows == true)
                {
                    produits = new ArrayList();

                    while (Reader.Read())
                    {
                        currentProduit = Produit.getProduit(Reader.GetString(2));
                        produits.Add(currentProduit);
                    }

                    Reader.Close();
                }

            }
            catch (OdbcException e)
            {
                MessageBox.Show(e.Message, Program.SelectGlobalMessages.SelectFactureClient,
                    MessageBoxButtons.OK, MessageBoxIcon.Error);

            }
            catch
            {
                MessageBox.Show(Program.SelectGlobalMessages.ImpSelectLigneFactureClient,
                     Program.SelectGlobalMessages.SelectBonReception, MessageBoxButtons.OK, MessageBoxIcon.Error);

            }
            return produits;
        }
        public static ArrayList getListTVAProduitsFactures(string _numFacture)
        {
            ArrayList TABtva = null;
            Produit currentTVA = null;
            OdbcConnection connection = DAL.DataBaseConnexion.getConnection();

            try
            {
                OdbcCommand cmd = connection.CreateCommand();
                cmd.CommandText = "select  tvaproduit_lignefactureclient " + DAL.DataBaseTableName.TableLigneFactureClient +
                        " where numero_factureclient = '" + _numFacture + "'";
                OdbcDataReader Reader = cmd.ExecuteReader();
                if (Reader.HasRows == true)
                {
                    TABtva = new ArrayList();

                    while (Reader.Read())
                    {
                        currentTVA = Produit.getProduit(Reader.GetString(2));
                        TABtva.Add(currentTVA);
                    }

                    Reader.Close();
                }

            }
            catch (OdbcException e)
            {
                MessageBox.Show(e.Message, Program.SelectGlobalMessages.SelectFactureClient,
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch
            {
                MessageBox.Show(Program.SelectGlobalMessages.ImpSelectLigneFactureClient,
                     Program.SelectGlobalMessages.SelectBonReception, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            return TABtva;
        }

        public static LigneFactureClient getOneLigneFactureClient(string _codeFacture, string _codeProduit)
        {
            LigneFactureClient ligneFACT = null;
            OdbcConnection connection = DAL.DataBaseConnexion.getConnection();

            try
            {
                OdbcCommand cmd = connection.CreateCommand();
                cmd.CommandText = "select * from " + DAL.DataBaseTableName.TableLigneFactureClient +
                        " where numero_factureclient = '" + _codeFacture + "' and codeproduit_lignefactureclient ='" + _codeProduit + "';";
                OdbcDataReader Reader = cmd.ExecuteReader();
                if (Reader.Read())
                {
                    ligneFACT = new LigneFactureClient(Reader.GetInt32(0), Reader.GetString(1), Reader.GetString(2), Reader.GetString(3),
                                                       Reader.GetDouble(4), Reader.GetDouble(5), Reader.GetDouble(6), Reader.GetDouble(7),
                                                       Reader.GetDouble(8), Reader.GetDouble(9), Reader.GetString(10), Reader.GetInt32(11));
                }                

                Reader.Close();
            }            
            catch
            {
                MessageBox.Show(Program.SelectGlobalMessages.ImpSelectLigneFactureClient,
                     Program.SelectGlobalMessages.SelectFactureClient, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            return ligneFACT;
        }

        public static ArrayList getAllLignesFactureClient(string _numFacture)
        {
            ArrayList listOfLignesFactureClient = null;
            LigneFactureClient ligneFACT = null;
            OdbcConnection connection = DAL.DataBaseConnexion.getConnection();

            try
            {
                OdbcCommand cmd = connection.CreateCommand();
                cmd.CommandText = "select * from " + DAL.DataBaseTableName.TableLigneFactureClient +
                        " where numero_factureclient = '" + _numFacture + "'";
                OdbcDataReader Reader = cmd.ExecuteReader();
                if (Reader.HasRows == true)
                {
                    listOfLignesFactureClient = new ArrayList();

                    while (Reader.Read())
                    {
                        ligneFACT = new LigneFactureClient(Reader.GetInt32(0), Reader.GetString(1), Reader.GetString(2), Reader.GetString(3),
                                                       Reader.GetDouble(4), Reader.GetDouble(5), Reader.GetDouble(6), Reader.GetDouble(7),
                                                       Reader.GetDouble(8), Reader.GetDouble(9), Reader.GetString(10), Reader.GetInt32(11));
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

        public static DataSet getDataset(string _codeFacture, string _codeProduit)
        {
            OdbcConnection connection = DAL.DataBaseConnexion.getConnection();
            OdbcCommand cmd = connection.CreateCommand();
            DataSet ds = new DataSet();
            connection.Open();
            string requete = " select * from " + DAL.DataBaseTableName.TableLigneFactureClient +
                            " where numero_factureclient = '" + _codeFacture + "'" +
                            " and codeProduit_lignefactureclient = '" + _codeProduit + "' ;";
            OdbcDataAdapter da = new OdbcDataAdapter(requete, connection);
            da.Fill(ds);
            return ds;
        }

        public static bool isExistedLigneFacture(string _codeFacture, string _codeProduit, ref bool isException)
        {
            bool isExist = false;
            isException = false;
            OdbcConnection connection = DAL.DataBaseConnexion.getConnection();
            try
            {
                OdbcCommand cmd = connection.CreateCommand();
                cmd.CommandText = "select * from " + DAL.DataBaseTableName.TableLigneFactureClient +
                                    " where numero_factureclient = '" + _codeFacture + "'" +
                                    " and codeproduit_lignefactureclient = '" + _codeProduit + "'";
                OdbcDataReader Reader = cmd.ExecuteReader();
                if (Reader.Read())
                {
                    isExist = true;
                }

                Reader.Close();
                return isExist;
            }
            catch (OdbcException e)
            {
                MessageBox.Show(e.Message, Program.SelectGlobalMessages.SelectFactureClient,
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return isException = true;
            }
            catch
            {
                MessageBox.Show(Program.SelectGlobalMessages.ImpSelectFactureClient,
                     Program.SelectGlobalMessages.SelectFactureClient, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return isException = true;
            }
        }

        public static ArrayList getTVA_produitFactureClient(string _numFacture)
        {
            ArrayList listOfTVA_produit = new ArrayList();
            LigneFactureClient ligneFACT = null;
            OdbcConnection connection = DAL.DataBaseConnexion.getConnection();
           double TVA_pr;
            try
            {
                OdbcCommand cmd = connection.CreateCommand();
                cmd.CommandText = "select montantHT_lignefactureclient,tvaproduit_lignefactureclient from " +
                    DAL.DataBaseTableName.TableLigneFactureClient +
                        " where numero_factureclient = '" + _numFacture + "'" ;
                OdbcDataReader Reader = cmd.ExecuteReader();
     
                    while (Reader.Read())
                    { TVA_pr = Reader.GetInt32(0);
                    if (TVA_pr != 0)
                    {
                        listOfTVA_produit.Add(TVA_pr);
                    }
                    }

                    Reader.Close();
                }            
            catch
            {
                MessageBox.Show(Program.SelectGlobalMessages.ImpSelectLigneFactureClient,
                     Program.SelectGlobalMessages.SelectBonReception, MessageBoxButtons.OK, MessageBoxIcon.Error);

            }
            return listOfTVA_produit;
        }

        public static ArrayList getCodesBonsLivraisonByFactureClient(string _codefact)
        {
            ArrayList tab_lignesBL = new ArrayList();
            string codeBl;
            OdbcConnection connection = DAL.DataBaseConnexion.getConnection();

            try
            {
                OdbcCommand cmd = connection.CreateCommand();
                cmd.CommandText = "select distinct codebonlivraison_lignefactureclient from " + DAL.DataBaseTableName.TableLigneFactureClient +
                                    " where numero_factureclient = '" + _codefact + "' " +
                                    " and codebonlivraison_lignefactureclient IS NOT NULL Order by codebonlivraison_lignefactureclient";
                OdbcDataReader Reader = cmd.ExecuteReader();
                while (Reader.Read())
                {
                    codeBl = Reader.GetString(0);
                    if (codeBl != "0")
                    {
                        tab_lignesBL.Add(codeBl);
                    }
                }
                Reader.Close();
                if (tab_lignesBL.Count == 0)
                    return null;
                else
                    return tab_lignesBL;
            }
            catch (OdbcException e)
            {
                MessageBox.Show(e.Message, Program.SelectGlobalMessages.SelectLigneBonLivraison,
                    MessageBoxButtons.OK, MessageBoxIcon.Error);

                return null;
            }
        }
    }
}