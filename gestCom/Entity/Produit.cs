using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Data;
using T4C_Commercial_Project.DAL;
using System.Globalization;
using System.Collections;
using System.Data.Odbc;

namespace T4C_Commercial_Project.Entity
{
    public class Produit
    {
        //
        // les attributs
        //
        public static string Separateur = CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator;

        public string code_produit;
        public string designation_produit;
        public double prixunitaire_produit;
        public int code_devise;
        public double quantite_produit;
        public string code_fournisseur;
        public int code_uniteproduit;
        public double prixachatTTC_produit;
        public double tauxmarge_produit;
        public double prixventeHT_produit;
        public double remise_produit;
        public int code_tvaproduit;
        public double fodec_produit;
        public double prixventeTTC_produit;
        public int code_categorieproduit;
        public int code_magasinproduit;
        public int code_fabriquantproduit;
        public int code_paysproduit;
        public int code_douaneproduit;
        public double stockminimal_produit;
        public double remisemaximale_produit;
        public string rayon_produit;
        public string etage_produit;

        //
        // Les constructeurs
        //
        public Produit()
        { }

        public Produit(string _code_produit, string _designation_produit, double _prixunitaire_produit, int _code_devise,
                      double _quantite_produit, string _code_fournisseur, int _code_uniteproduit, double _prixachatTTC_produit,
                      double _tauxmarge_produit, double _prixventeHT_produit, double _remise_produit, int _code_tvaproduit,
                      double _fodec_produit, double _prixventeTTC_produit, int _code_categorieproduit, int _code_magasinproduit,
                      int _code_fabriquantproduit, int _code_paysproduit, int _code_douaneproduit, double _stockminimal_produit,
                      double _remisemaximale_produit, string _rayon_produit, string _etage_produit)
        {
            // Class Produit contient 22 attributs:
            code_produit = _code_produit;
            designation_produit = _designation_produit;
            prixunitaire_produit = _prixunitaire_produit;
            code_devise = _code_devise;
            quantite_produit = _quantite_produit;
            code_fournisseur = _code_fournisseur;
            code_uniteproduit = _code_uniteproduit;
            prixachatTTC_produit = _prixachatTTC_produit;
            tauxmarge_produit = _tauxmarge_produit;
            prixventeHT_produit = _prixventeHT_produit;
            remise_produit = _remise_produit;
            code_tvaproduit = _code_tvaproduit;
            fodec_produit = _fodec_produit;
            prixventeTTC_produit = _prixventeTTC_produit;
            code_categorieproduit = _code_categorieproduit;
            code_magasinproduit = _code_magasinproduit;
            code_fabriquantproduit = _code_fabriquantproduit;
            code_paysproduit = _code_paysproduit;
            code_douaneproduit = _code_douaneproduit;
            stockminimal_produit = _stockminimal_produit;
            remisemaximale_produit = _remisemaximale_produit;
            rayon_produit = _rayon_produit;
            etage_produit = _etage_produit;
        }

        //
        // Les méthodes
        //
        public Boolean ajouterProduit()
        {
            string CommandText = "INSERT INTO " + DAL.DataBaseTableName.TableProduit + " VALUES ('" + this.code_produit + "'" +
           " ,'" + this.designation_produit.ToString().Replace("'", "''") + "' " +
           " , " + this.prixunitaire_produit.ToString().Replace(',', '.') +
           " , " + this.code_devise +
           " , " + this.quantite_produit.ToString().ToString().Replace(',', '.') +
           " , '" + this.code_fournisseur + "' " +
           " , " + this.code_uniteproduit +
           " , " + this.prixachatTTC_produit.ToString().ToString().Replace(',', '.') +
           " , " + this.tauxmarge_produit.ToString().ToString().Replace(',', '.') +
           " , " + this.prixventeHT_produit.ToString().ToString().Replace(',', '.') +
           " , " + this.remise_produit.ToString().ToString().Replace(',', '.') +
           " , " + this.code_tvaproduit +
           " , " + this.fodec_produit.ToString().ToString().Replace(',', '.') +
           " , " + this.prixventeTTC_produit.ToString().ToString().Replace(',', '.') +
           " , " + this.code_categorieproduit +
           " , " + this.code_magasinproduit +
           " , " + this.code_fabriquantproduit +
           " , " + this.code_paysproduit +
           " , " + this.code_douaneproduit +
           " , " + this.stockminimal_produit.ToString().ToString().Replace(',', '.') +
           " , " + this.remisemaximale_produit.ToString().ToString().Replace(',', '.') +
           " ,'" + this.rayon_produit + "' " +
           " ,'" + this.etage_produit + "');";
            return DataBaseConnexion.addOrUpdateElementInDataBase(CommandText, Program.SelectGlobalMessages.ImpAddProduit);
        }

        public Boolean modifierProduit()
        {
            string CommandText = " UPDATE " + DAL.DataBaseTableName.TableProduit +
            " SET designation_produit = '" + this.designation_produit.ToString().Replace("'", "''") + "' " +
            " , prixunitaire_produit = " + this.prixunitaire_produit.ToString().Replace(",", ".") +
            " , code_devise = " + this.code_devise +
            " , quantite_produit = " + this.quantite_produit.ToString().ToString().Replace(',', '.') +
            " , code_fournisseur = '" + this.code_fournisseur + " '" +
            " , code_uniteproduit = " + this.code_uniteproduit +
            " , prixachatTTC_produit = " + this.prixventeHT_produit.ToString().ToString().Replace(',', '.') +
            " , tauxmarge_produit = " + this.tauxmarge_produit.ToString().ToString().Replace(',', '.') +
            " , prixventeHT_produit = " + this.prixventeHT_produit.ToString().ToString().Replace(',', '.') +
            " , remise_produit = " + this.remise_produit.ToString().ToString().Replace(',', '.') +
            " , code_tvaproduit = " + this.code_tvaproduit +
            " , fodec_produit = " + this.fodec_produit.ToString().ToString().Replace(',', '.') +
            " , prixventeTTC_produit = " + this.prixventeTTC_produit.ToString().ToString().Replace(',', '.') +
            " , code_categorieproduit = " + this.code_categorieproduit +
            " , code_magasinproduit = " + this.code_magasinproduit +
            " , code_fabriquantproduit = " + this.code_fabriquantproduit +
            " , code_paysproduit = " + this.code_paysproduit +
            " , code_douaneproduit = " + this.code_douaneproduit +
            " , stockminimal_produit = " + this.stockminimal_produit.ToString().ToString().Replace(',', '.') +
            " , remisemaximale_produit = " + this.remisemaximale_produit.ToString().ToString().Replace(',', '.') +
            " , rayon_produit = '" + this.rayon_produit + "' " +
            " , etage_produit = '" + this.etage_produit + "' " +
            "   Where code_produit = '" + this.code_produit + "' ";
            return DataBaseConnexion.addOrUpdateElementInDataBase(CommandText, Program.SelectGlobalMessages.ImpUpdateProduit);
        }
        public static Boolean supprimerProduit(String _codeProduit)
        {
            string CommandText = "delete from " + DAL.DataBaseTableName.TableProduit + " where code_produit='" + _codeProduit + "'";
            return DataBaseConnexion.addOrUpdateElementInDataBase(CommandText, Program.SelectGlobalMessages.ImpDeleteProduit);
        }

        public static Boolean supprimerAllProduit()
        {
            string CommandText = "delete from " + DAL.DataBaseTableName.TableProduit;
            return DataBaseConnexion.addOrUpdateElementInDataBase(CommandText, Program.SelectGlobalMessages.ImpDeleteProduit);
        }

        public static Produit getProduit(String code_produit)
        {
            Produit v_produit = null;
            if (DataBaseConnexion.getRowsCount(DAL.DataBaseTableName.TableProduit, "code_produit") != 0)
            {
                OdbcConnection connection = DataBaseConnexion.getConnection();
                try
                {
                    OdbcCommand cmd = connection.CreateCommand();
                    cmd.CommandText = "select * from " + DAL.DataBaseTableName.TableProduit + " where code_produit='" + code_produit + "'";

                    OdbcDataReader Reader = cmd.ExecuteReader();
                    if (Reader.Read())
                    {
                        v_produit = new Produit(Reader.GetString(0), Reader.GetString(1), Reader.GetDouble(2), Reader.GetInt32(3),
                                    Reader.GetDouble(4), Reader.GetString(5), Reader.GetInt32(6), Reader.GetDouble(7), Reader.GetDouble(8),
                                    Reader.GetDouble(9), Reader.GetDouble(10), Reader.GetInt32(11), Reader.GetDouble(12),
                                    Reader.GetDouble(13), Reader.GetInt32(14), Reader.GetInt32(15), Reader.GetInt32(16),
                                    Reader.GetInt32(17), Reader.GetInt32(18), Reader.GetDouble(19), Reader.GetDouble(20),
                                    Reader.GetString(21), Reader.GetString(22));
                    }
                    Reader.Close();
                    return v_produit;
                }

                catch (OdbcException e)
                {
                    MessageBox.Show(e.Message, Program.SelectGlobalMessages.SelectProduct,
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            return v_produit;
        }

        public static ArrayList getALLProduits()
        {
            ArrayList tabProd = new ArrayList();
            Produit v_produit = null;
            if (DataBaseConnexion.getRowsCount(DAL.DataBaseTableName.TableProduit, "code_produit") != 0)
            {
                OdbcConnection connection = DataBaseConnexion.getConnection();
                try
                {
                    OdbcCommand cmd = connection.CreateCommand();
                    cmd.CommandText = "select * from " + DAL.DataBaseTableName.TableProduit;
                    OdbcDataReader Reader = cmd.ExecuteReader();
                    while (Reader.Read())
                    {
                        v_produit = new Produit(Reader.GetString(0), Reader.GetString(1), Reader.GetDouble(2), Reader.GetInt32(3),
                                    Reader.GetDouble(4), Reader.GetString(5), Reader.GetInt32(6), Reader.GetDouble(7), Reader.GetDouble(8),
                                    Reader.GetDouble(9), Reader.GetDouble(10), Reader.GetInt32(11), Reader.GetDouble(12),
                                    Reader.GetDouble(13), Reader.GetInt32(14), Reader.GetInt32(15), Reader.GetInt32(16),
                                    Reader.GetInt32(17), Reader.GetInt32(18), Reader.GetDouble(19), Reader.GetDouble(20),
                                    Reader.GetString(21), Reader.GetString(22));

                        tabProd.Add(v_produit);
                    }
                    Reader.Close();
                }
                catch (OdbcException)
                {
                    MessageBox.Show(Program.SelectGlobalMessages.ErrorMessage, Program.SelectGlobalMessages.SelectProduct,
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            return tabProd;
        }

        public static Boolean diminuerQteProduit(string _code_produit, double _quantite)
        {
            string CommandText = "update " + DAL.DataBaseTableName.TableProduit +
                " set quantite_produit =  quantite_produit - " + _quantite.ToString().ToString().Replace(',', '.') +
                 " where code_produit = '" + _code_produit + "' ;";

            return DataBaseConnexion.addOrUpdateElementInDataBase(CommandText, Program.SelectGlobalMessages.ImpUpdateProduit);
        }

        public static Boolean augmenterQteProduit(string _code_produit, double _quantite)
        {
            string CommandText = "update " + DAL.DataBaseTableName.TableProduit +
                    " set quantite_produit =  quantite_produit + " + _quantite.ToString().Replace(',', '.') +
                    " where code_produit ='" + _code_produit + "' ;";

            return DataBaseConnexion.addOrUpdateElementInDataBase(CommandText, Program.SelectGlobalMessages.ImpUpdateProduit);
        }

        public Boolean updateFodecProduit(double _newFodec)
        {
            string CommandText = "Update " + DAL.DataBaseTableName.TableProduit +
                                " Set fodec_produit = " + _newFodec.ToString().Replace(',', '.') +
                                " where code_produit = '" + code_produit + "' ;";

            return DataBaseConnexion.addOrUpdateElementInDataBase(CommandText, Program.SelectGlobalMessages.ImpUpdateProduit);
        }

        public Boolean updatePUProduit(double _newPU)
        {
            string CommandText = "Update " + DAL.DataBaseTableName.TableProduit +
                                " Set prixunitaire_produit = " + _newPU.ToString().Replace(',', '.') +
                                " where code_produit = '" + code_produit + "' ;";

            return DataBaseConnexion.addOrUpdateElementInDataBase(CommandText, Program.SelectGlobalMessages.ImpUpdateProduit);
        }
        
        public static Boolean designationproduit(string _code_produit, double _newQuantite)
        {
            string CommandText = "select désignation from " + DAL.DataBaseTableName.TableProduit;


            return DataBaseConnexion.addOrUpdateElementInDataBase(CommandText, Program.SelectGlobalMessages.ImpUpdateProduit);
        }

        public static int getNbreProduits()
        {
            int nombre = 0;
            if (DataBaseConnexion.getRowsCount(DAL.DataBaseTableName.TableProduit, "code_produit") != 0)
            {
                OdbcConnection connection = DataBaseConnexion.getConnection();
                try
                {
                    OdbcCommand cmd = connection.CreateCommand();
                    cmd.CommandText = "Select  count(code_produit) 	from " + DAL.DataBaseTableName.TableProduit;
                    OdbcDataReader Reader = cmd.ExecuteReader();
                    if (Reader.Read())
                    {
                        nombre = Reader.GetInt32(0);
                    }

                    Reader.Close();
                }
                catch (OdbcException e)
                {
                    MessageBox.Show(e.Message, Program.SelectGlobalMessages.ResearchProduct,
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            return nombre;
        }

        public static int getNbreProduitByTVA(int _code_tvaproduit)
        {
            int nombre = 0;
            if (DataBaseConnexion.getRowsCount(DAL.DataBaseTableName.TableProduit, "code_produit") != 0)
            {
                OdbcConnection connection = DataBaseConnexion.getConnection();
                try
                {
                    OdbcCommand cmd = connection.CreateCommand();
                    cmd.CommandText = "Select  count(code_produit) 	from " + DAL.DataBaseTableName.TableProduit +
                                       " where _code_tvaproduit =" + _code_tvaproduit + " ;";
                    OdbcDataReader Reader = cmd.ExecuteReader();
                    if (Reader.Read())
                    {
                        nombre = Reader.GetInt32(0);
                    }

                    Reader.Close();
                }
                catch (OdbcException e)
                {
                    MessageBox.Show(e.Message, Program.SelectGlobalMessages.ResearchProduct,
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            return nombre;
        }

        public static int getNbreProduitByPays(int _code_paysproduit)
        {
            int nombre = 0;
            if (DataBaseConnexion.getRowsCount(DAL.DataBaseTableName.TableProduit, "code_produit") != 0)
            {
                OdbcConnection connection = DataBaseConnexion.getConnection();
                try
                {
                    OdbcCommand cmd = connection.CreateCommand();
                    cmd.CommandText = "Select  count(code_produit) 	from " + DAL.DataBaseTableName.TableProduit +
                                       " where code_paysproduit =" + _code_paysproduit + " ;";
                    OdbcDataReader Reader = cmd.ExecuteReader();
                    if (Reader.Read())
                    {
                        nombre = Reader.GetInt32(0);
                    }

                    Reader.Close();
                }
                catch (OdbcException e)
                {
                    MessageBox.Show(e.Message, Program.SelectGlobalMessages.ResearchProduct,
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            return nombre;
        }

        public static int getNbreProduitByMagasin(int _code_magasinproduit)
        {
            int nombre = 0;
            if (DataBaseConnexion.getRowsCount(DAL.DataBaseTableName.TableProduit, "code_produit") != 0)
            {
                OdbcConnection connection = DataBaseConnexion.getConnection();
                try
                {
                    OdbcCommand cmd = connection.CreateCommand();
                    cmd.CommandText = "Select  count(code_produit) 	from " + DAL.DataBaseTableName.TableProduit +
                                       " where code_magasinproduit =" + _code_magasinproduit + " ;";
                    OdbcDataReader Reader = cmd.ExecuteReader();
                    if (Reader.Read())
                    {
                        nombre = Reader.GetInt32(0);
                    }

                    Reader.Close();
                }
                catch (OdbcException e)
                {
                    MessageBox.Show(e.Message, Program.SelectGlobalMessages.ResearchProduct,
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            return nombre;
        }

        public static int getNbreProduitByDouane(int _code_douaneproduit)
        {
            int nombre = 0;
            if (DataBaseConnexion.getRowsCount(DAL.DataBaseTableName.TableProduit, "code_produit") != 0)
            {
                OdbcConnection connection = DataBaseConnexion.getConnection();
                try
                {
                    OdbcCommand cmd = connection.CreateCommand();
                    cmd.CommandText = "Select  count(code_produit) 	from " + DAL.DataBaseTableName.TableProduit +
                                       " where code_douaneproduit =" + _code_douaneproduit + " ;";
                    OdbcDataReader Reader = cmd.ExecuteReader();
                    if (Reader.Read())
                    {
                        nombre = Reader.GetInt32(0);
                    }

                    Reader.Close();
                }
                catch (OdbcException e)
                {
                    MessageBox.Show(e.Message, Program.SelectGlobalMessages.ResearchProduct,
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            return nombre;
        }

        public static int getNbreProduitByDevise(int _code_devise)
        {
            int nombre = 0;
            if (DataBaseConnexion.getRowsCount(DAL.DataBaseTableName.TableProduit, "code_produit") != 0)
            {
                OdbcConnection connection = DataBaseConnexion.getConnection();
                try
                {
                    OdbcCommand cmd = connection.CreateCommand();
                    cmd.CommandText = "Select  count(code_produit) 	from " + DAL.DataBaseTableName.TableProduit +
                                       " where code_devise =" + _code_devise + " ;";
                    OdbcDataReader Reader = cmd.ExecuteReader();
                    if (Reader.Read())
                    {
                        nombre = Reader.GetInt32(0);
                    }

                    Reader.Close();
                }
                catch (OdbcException e)
                {
                    MessageBox.Show(e.Message, Program.SelectGlobalMessages.ResearchProduct,
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            return nombre;
        }

        public static int getNbreProduitByCategorie(int _code_categorieproduit)
        {
            int nombre = 0;
            if (DataBaseConnexion.getRowsCount(DAL.DataBaseTableName.TableProduit, "code_produit") != 0)
            {
                OdbcConnection connection = DataBaseConnexion.getConnection();
                try
                {
                    OdbcCommand cmd = connection.CreateCommand();
                    cmd.CommandText = "Select  count(code_produit) 	from " + DAL.DataBaseTableName.TableProduit +
                                       " where code_douaneproduit =" + _code_categorieproduit + " ;";
                    OdbcDataReader Reader = cmd.ExecuteReader();
                    if (Reader.Read())
                    {
                        nombre = Reader.GetInt32(0);
                    }
                    Reader.Close();
                }
                catch (OdbcException e)
                {
                    MessageBox.Show(e.Message, Program.SelectGlobalMessages.ResearchProduct,
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            return nombre;
        }

        public static int getNbreProduitByFabriquant(int _code_fabriquantproduit)
        {
            int nombre = 0;
            if (DataBaseConnexion.getRowsCount(DAL.DataBaseTableName.TableProduit, "code_produit") != 0)
            {
                OdbcConnection connection = DataBaseConnexion.getConnection();


                try
                {
                    OdbcCommand cmd = connection.CreateCommand();
                    cmd.CommandText = "Select  count(code_produit) 	from " + DAL.DataBaseTableName.TableProduit +
                                       " where code_fabriquantproduit =" + _code_fabriquantproduit + " ;";
                    OdbcDataReader Reader = cmd.ExecuteReader();
                    if (Reader.Read())
                    {
                        nombre = Reader.GetInt32(0);
                    }

                    Reader.Close();
                }
                catch (OdbcException e)
                {
                    MessageBox.Show(e.Message, Program.SelectGlobalMessages.ResearchProduct,
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            return nombre;
        }


        public static int getNbreProduitByUnite(int _code_uniteproduit)
        {
            int nombre = 0;
            if (DataBaseConnexion.getRowsCount(DAL.DataBaseTableName.TableProduit, "code_produit") != 0)
            {
                OdbcConnection connection = DataBaseConnexion.getConnection();
                try
                {
                    OdbcCommand cmd = connection.CreateCommand();
                    cmd.CommandText = "Select  count(code_produit) 	from " + DAL.DataBaseTableName.TableProduit +
                                       " where code_uniteproduit =" + _code_uniteproduit + " ;";
                    OdbcDataReader Reader = cmd.ExecuteReader();
                    if (Reader.Read())
                    {
                        nombre = Reader.GetInt32(0);
                    }

                    Reader.Close();
                }
                catch (OdbcException e)
                {
                    MessageBox.Show(e.Message, Program.SelectGlobalMessages.ResearchProduct,
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            return nombre;
        }


    }

}