using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using T4C_Commercial_Project.DAL;
using System.Globalization;
using System.Data.Odbc;
using System.Collections;

namespace T4C_Commercial_Project.Entity
{
    class LigneDevisClient
    {
        public static string Separateur = CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator;
        
        //les attributs:
        public int numero_lignedevis;
        public string numeroDevis_lignedevis;
        public string codeProduit_lignedevis;
        public string designationProduit_lignedevis;
        public double fodec_lignedevis;
        public double quantite_lignedevis;
        public double prixUnitaire_lignedevis;
        public double montantHT_lignedevis;
        public double tvaProduit_lignedevis;        
        public double remise_lignedevis;
        
                
        // les constructeurs:
        public LigneDevisClient(int _numero_lignedevis, string _numeroDevis, string _codeProduit, string _designation_prod, 
                double _fodec_lignedevis, double _quantite, double _prixunitaire, double _montantHT, double _tvaProduit, double _remise)
        {
            numero_lignedevis = _numero_lignedevis;
            numeroDevis_lignedevis = _numeroDevis;
            codeProduit_lignedevis = _codeProduit;
            quantite_lignedevis = _quantite;
            prixUnitaire_lignedevis = _prixunitaire;
            montantHT_lignedevis = _montantHT;
            tvaProduit_lignedevis = _tvaProduit;
            remise_lignedevis = _remise;
            designationProduit_lignedevis = _designation_prod;
            fodec_lignedevis = _fodec_lignedevis;
        }

        public LigneDevisClient(int _numero_lignedevis, string _codeDevis, string _codeProduit)
        {
            numero_lignedevis = _numero_lignedevis ;
            numeroDevis_lignedevis = _codeDevis;
            codeProduit_lignedevis = _codeProduit;
        }

        public LigneDevisClient()
        {

        }
        
        //Les méthodes :
        public Boolean ajouterLigneDevis()
        {
                string CommandText = "insert into " + DAL.DataBaseTableName.TableLigneDevis  +" values(" +
                this.numero_lignedevis + ","+    
                "'" + this.numeroDevis_lignedevis + "', " +
                "'" + this.codeProduit_lignedevis + "' , " +
                "'" + this.designationProduit_lignedevis.ToString().Replace("'", "''") + "'," +
                this.fodec_lignedevis.ToString().ToString().Replace(',', '.') + "," +
                this.quantite_lignedevis.ToString().ToString().Replace(',', '.') + "," +
                this.prixUnitaire_lignedevis.ToString().ToString().Replace(',', '.') + "," +
                this.montantHT_lignedevis.ToString().ToString().Replace(',', '.') + ", " +
                this.tvaProduit_lignedevis.ToString().ToString().Replace(',', '.') + ", " +
                this.remise_lignedevis.ToString().ToString().Replace(',', '.') +                
                      ");";
            return DataBaseConnexion.addOrUpdateElementInDataBase(CommandText, Program.SelectGlobalMessages.ImpAddLigneDevis);
        }
        public Boolean modifierLigneDevis()
        {
            string CommandText = "update  " + DAL.DataBaseTableName.TableLigneDevis +" set " +
                       " quantite_lignedevis = " + this.quantite_lignedevis.ToString().ToString().Replace(',', '.') + " , " +
                       " prixunitaire_lignedevis = " + this.prixUnitaire_lignedevis.ToString().ToString().Replace(',', '.') + " , " +
                       " montantHT_lignedevis = " + this.montantHT_lignedevis.ToString().ToString().Replace(',', '.') + " , " +
                       " tvaproduit_lignedevis = " + this.tvaProduit_lignedevis.ToString().ToString().Replace(',', '.') + " , " +
                       " remise_lignedevis = " + this.remise_lignedevis.ToString().ToString().Replace(',', '.') + " , " +
                       " designationProduit_lignedevis ='" + this.designationProduit_lignedevis.ToString().Replace("'", "''") + "', " + 
                       " fodec_lignedevis = " + this.fodec_lignedevis.ToString().ToString().Replace(',', '.') + "  " +
                       " where numero_lignedevis = " + this.numero_lignedevis + " and  " +
                       " code_devis = '" + this.numeroDevis_lignedevis + "' and " +
                       " codeproduit_lignedevis = '" + this.codeProduit_lignedevis + "';";
            return DataBaseConnexion.addOrUpdateElementInDataBase(CommandText, Program.SelectGlobalMessages.ImpUpdateLigneDevis);
        }

        public static Boolean supprimerLigneDevis(int _numero_lignedevis, string _numero_Devis, string _codeProduit_ligneDevis)
        {
            string CommandText = "delete from " + DAL.DataBaseTableName.TableLigneDevis +
                       " where numero_lignedevis = " + _numero_lignedevis + " and numero_devis = '" + _numero_Devis + "' "+
                       " and  codeproduit_lignedevis ='" + _codeProduit_ligneDevis + "';";
            return DataBaseConnexion.addOrUpdateElementInDataBase(CommandText, Program.SelectGlobalMessages.ImpDeleteLigneDevis);
        }

        public static Boolean supprimerAllLigneDevis(string _numeroDevis)
        {
            string CommandText = "delete from " + DAL.DataBaseTableName.TableLigneDevis +
                       " where numero_devis = '" + _numeroDevis + "'";
            return DataBaseConnexion.addOrUpdateElementInDataBase(CommandText, Program.SelectGlobalMessages.ImpDeleteLigneDevis);
        }

        public static  Boolean isExistedLigneDevis(int _numero_lignedevis, string _numero_devis, string _codeproduit_ligneDevis, ref bool isException)
        {
            bool isExist = false;
            isException = false;
            OdbcConnection connection = DAL.DataBaseConnexion.getConnection();
            try
            {
                OdbcCommand cmd = connection.CreateCommand();
                cmd.CommandText = "select * from " + DAL.DataBaseTableName.TableLigneDevis +
                        " where numero_lignedevis = " + _numero_lignedevis + 
                        " numero_devis = '" + _numero_devis + "' " +
                        " and codeproduit_lignedevis ='" + _codeproduit_ligneDevis + "';";

                OdbcDataReader Reader = cmd.ExecuteReader();
                if (Reader.Read())
                {
                    isExist = true;
                }
                
                Reader.Close();

                
            }
            catch (OdbcException e)
            {
                MessageBox.Show(e.Message, Program.SelectGlobalMessages.SelectDevis,
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                isException = true;                
            }
            catch
            {
                MessageBox.Show(Program.SelectGlobalMessages.ImpSelectLigneDevis,
                     Program.SelectGlobalMessages.SelectDevis, MessageBoxButtons.OK, MessageBoxIcon.Error);
                isException = true;
            }
            return isExist;
        }

        public static LigneDevisClient getLigneDevis(int _numero_lignedevis, string _numero_Devis, string _codeProduit_ligneDevis)
        {
            LigneDevisClient ligneDevis = null;
            OdbcConnection connection = DAL.DataBaseConnexion.getConnection();
            try
            {
                OdbcCommand cmd = connection.CreateCommand();
                cmd.CommandText = "select * from " + DAL.DataBaseTableName.TableLigneDevis +
                        " where numero_lignedevis = "+ _numero_lignedevis +
                        " and numero_devis=" + _numero_Devis + 
                        " and codeProduit_lignedevis ='" + _codeProduit_ligneDevis + "';";

                OdbcDataReader Reader = cmd.ExecuteReader();
                if (Reader.Read())
                {
                    ligneDevis = new LigneDevisClient(Reader.GetInt32(0), Reader.GetString(1), Reader.GetString(2),
                                                      Reader.GetString(3), Reader.GetDouble(4), Reader.GetDouble(5),
                                                      Reader.GetDouble(6),Reader.GetDouble(7), Reader.GetDouble(8),Reader.GetDouble(9));
                }                

                Reader.Close();

            }
            catch (OdbcException e)
            {
                MessageBox.Show(e.Message, Program.SelectGlobalMessages.SelectLigneDevis,
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch
            {
                MessageBox.Show(Program.SelectGlobalMessages.ImpSelectLigneDevis,
                     Program.SelectGlobalMessages.SelectLigneDevis, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            return ligneDevis;
        }


        public static ArrayList getAllLigneDevis(string _numeroDevis)
        {
            ArrayList tab_lignesDevis = new ArrayList();
            LigneDevisClient ligneDevis = null;
            OdbcConnection connection = DAL.DataBaseConnexion.getConnection();

            try
            {
                OdbcCommand cmd = connection.CreateCommand();
                cmd.CommandText = "select * from  " + DAL.DataBaseTableName.TableLigneDevis +
                       " where numero_devis = '" + _numeroDevis + "' order by numero_lignedevis ;";
                OdbcDataReader Reader = cmd.ExecuteReader();

                while (Reader.Read())
                {
                    ligneDevis = new LigneDevisClient(Reader.GetInt32(0), Reader.GetString(1), Reader.GetString(2),
                                                          Reader.GetString(3), Reader.GetDouble(4), Reader.GetDouble(5),
                                                          Reader.GetDouble(6), Reader.GetDouble(7), Reader.GetDouble(8), Reader.GetDouble(9));
                    tab_lignesDevis.Add(ligneDevis);
                }

                Reader.Close();
                if (tab_lignesDevis.Count == 0)
                    return null;
                else
                    return tab_lignesDevis;
            }
            catch (OdbcException e)
            {
                MessageBox.Show(e.Message, Program.SelectGlobalMessages.SelectDevis,
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }            
        }

        public static Boolean supprimerAllLigneDevis()
        {
            string CommandText = "delete from " + DAL.DataBaseTableName.TableLigneDevis;
            return DataBaseConnexion.addOrUpdateElementInDataBase(CommandText, Program.SelectGlobalMessages.ImpDeleteLigneDevis);
        }
    }

}
