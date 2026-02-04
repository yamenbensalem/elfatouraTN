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
    public class LigneBonLivraison
    {

        public static string Separateur = CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator;
        public int numero_lignebonlivraison;
        public string    code_bonlivraison;
        public String codeproduit_lignebonlivraison;
        public string designationproduit_lignebonlivraison;
        public double quantite_lignebonlivraison;
        public double prixunitaire_lignebonlivraison;
        public double montantHT_lignebonlivraison;
        public double tvaproduit_lignebonlivraison;
        public double remise_lignebonlivraison;
        public double fodec_lignebonlivraison;

        /************************************************************************************/
        /************************************************************************************/
        public LigneBonLivraison(int _numeroLigne, string    _codeBL, String _codeProduit, string _designationProduit, double _quantite,
            double _prixunitaire, double _montantHT, double _tvaProduit, double _remise, double _fodec)
        {
            numero_lignebonlivraison = _numeroLigne;
            code_bonlivraison = _codeBL;
            codeproduit_lignebonlivraison = _codeProduit;
            designationproduit_lignebonlivraison = _designationProduit;
            quantite_lignebonlivraison = _quantite;
            prixunitaire_lignebonlivraison = _prixunitaire;
            montantHT_lignebonlivraison = _montantHT;
            tvaproduit_lignebonlivraison = _tvaProduit;
            remise_lignebonlivraison = _remise;
            fodec_lignebonlivraison = _fodec;
        }

        public LigneBonLivraison(string   _code, String _codeProduit)
        {
            code_bonlivraison = _code;
            codeproduit_lignebonlivraison = _codeProduit;
        }

        public LigneBonLivraison(string    _code)
        {
            code_bonlivraison = _code;
        }

        public LigneBonLivraison()
        {

        }
        
        public Boolean ajouterLigneBonLivraison()
        {
            string CommandText = "insert into " + DAL.DataBaseTableName.TableLigneBonLivraison +
                    " values (" + 
                    numero_lignebonlivraison + " , '" + 
                    code_bonlivraison +"' , '"+ 
                    codeproduit_lignebonlivraison + "' , '" +
                    designationproduit_lignebonlivraison.ToString().Replace("'", "''") + "' , " + 
                    quantite_lignebonlivraison.ToString().ToString().Replace(',', '.') + " , " +
                    prixunitaire_lignebonlivraison.ToString().ToString().Replace(',', '.') + " , " +
                    montantHT_lignebonlivraison.ToString().ToString().Replace(',', '.') + " , " +
                    tvaproduit_lignebonlivraison.ToString().ToString().Replace(',', '.') + " , " +
                    remise_lignebonlivraison.ToString().ToString().Replace(',', '.') + " , " +
                    fodec_lignebonlivraison.ToString().ToString().Replace(',', '.') + 
                    ");";
                    return DataBaseConnexion.addOrUpdateElementInDataBase(CommandText, Program.SelectGlobalMessages.ImpAddLigneBonLivraison);
        }

        public Boolean modifierLigneBonLivraison()
        {
            string CommandText = "Update " + DAL.DataBaseTableName.TableLigneBonLivraison +
                    " Set " +
                    " numero_lignebonlivraison = " + this.numero_lignebonlivraison +
                    ", designationproduit_lignebonlivraison = '" + this.designationproduit_lignebonlivraison.ToString().Replace("'", "''") + "'" +
                    ", quantite_ligneBonLivraison = " + this.quantite_lignebonlivraison.ToString().ToString().Replace(',', '.') +
                    ", prixunitaire_lignebonlivraison = " + this.prixunitaire_lignebonlivraison.ToString().ToString().Replace(',', '.')  +
                    ", montantHT_ligneBonLivraison = " + this.montantHT_lignebonlivraison.ToString().ToString().Replace(',', '.') +
                    ", tvaproduit_lignebonlivraison = " + this.tvaproduit_lignebonlivraison.ToString().ToString().Replace(',', '.') + 
                    ", remise_lignebonlivraison = " + this.remise_lignebonlivraison.ToString().ToString().Replace(',', '.') +
                    ", fodec_lignebonlivraison = " + this.fodec_lignebonlivraison.ToString().ToString().Replace(',', '.') +       
                    " WHERE  code_bonlivraison = '" + this.code_bonlivraison +"'"+
                    " AND codeproduit_lignebonlivraison = '"+ this.codeproduit_lignebonlivraison+ "'";
            return DataBaseConnexion.addOrUpdateElementInDataBase(CommandText, Program.SelectGlobalMessages.ImpUpdateLigneBonLivraison);
        }

        public Boolean supprimerLigneBonLivraison()
        {
            string CommandText = "Delete from " + DAL.DataBaseTableName.TableLigneBonLivraison +
                       " Where code_bonLivraison='" + code_bonlivraison +"'"+ 
                       " And codeproduit_lignebonlivraison= '" + codeproduit_lignebonlivraison + "' ";
            return DataBaseConnexion.addOrUpdateElementInDataBase(CommandText, Program.SelectGlobalMessages.ImpDeleteLigneBonLivraison);
        }
        
        public static LigneBonLivraison getOneLigneBonLivraison(string    _codeBL, string codePr)
        {
            LigneBonLivraison ligneBonLivraison = null;
            OdbcConnection connection = DAL.DataBaseConnexion.getConnection();
            try
            {
                OdbcCommand cmd = connection.CreateCommand();
                cmd.CommandText = "select * from " + DAL.DataBaseTableName.TableLigneBonLivraison +
                      " where code_bonLivraison='" + _codeBL +"'"+ 
                      " and codeproduit_lignebonlivraison ='" + codePr + "';";

                OdbcDataReader Reader = cmd.ExecuteReader();
                if (Reader.Read())
                {
                    ligneBonLivraison = new LigneBonLivraison(Reader.GetInt32(0), Reader.GetString(1), Reader.GetString(2),
                        Reader.GetString(3), Reader.GetDouble(4), Reader.GetDouble(5), Reader.GetDouble(6),
                        Reader.GetDouble(7), Reader.GetDouble(8), Reader.GetDouble(9));
                }
                else
                    throw new Exception();

                Reader.Close();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, Program.SelectGlobalMessages.SelectBonLivraison,
                    MessageBoxButtons.OK, MessageBoxIcon.Error);

            }
            //catch
            //{
            //    MessageBox.Show( Program.SelectGlobalMessages.ImpSelectLigneBonLivraison,
            //         Program.SelectGlobalMessages.SelectBonLivraison, MessageBoxButtons.OK, MessageBoxIcon.Error);
            //    connection.Close();
            //    return currentLigneBonLivraison;
            //}

            return ligneBonLivraison;
        }

        public bool isExistedLigneBonLivraison(string    _codeBL, string _codeProduit, ref bool isException)
        {
            bool isExist = false ;
            isException = false;
            OdbcConnection connection = DAL.DataBaseConnexion.getConnection();
            try
            {
                OdbcCommand cmd = connection.CreateCommand();
                cmd.CommandText = "select * from " + DAL.DataBaseTableName.TableLigneBonLivraison +
                                  " where code_bonLivraison='"+ _codeBL +"'"+
                                  " And codeproduit_lignebonlivraison ='" + _codeProduit + "';";
                OdbcDataReader Reader = cmd.ExecuteReader();
                if (Reader.Read())
                {
                    isExist = true;
                }
                Reader.Close();
            }
            catch (OdbcException e)
                {
                 MessageBox.Show(e.Message, Program.SelectGlobalMessages.SelectBonLivraison,
                                 MessageBoxButtons.OK, MessageBoxIcon.Error);

                 isException = true;
               }
            //catch
            //{
            //    MessageBox.Show(Program.SelectGlobalMessages.ImpSelectLigneBonLivraison,
            //    Program.SelectGlobalMessages.SelectBonLivraison, MessageBoxButtons.OK, MessageBoxIcon.Error);
            //    connection.Close();
            //    return isException = true;
            //}
            return isExist;
        }

        public static ArrayList getAllLigneBonLivraison(string  _codeBL)
        {
            ArrayList tab_lignesBL = new ArrayList();
            LigneBonLivraison lignebl = null;
            OdbcConnection connection = DAL.DataBaseConnexion.getConnection();
            try
            {
                OdbcCommand cmd = connection.CreateCommand();
                cmd.CommandText = "select * from  " + DAL.DataBaseTableName.TableLigneBonLivraison +
                       " where code_bonlivraison='" + _codeBL + "'";
                OdbcDataReader Reader = cmd.ExecuteReader();
                while (Reader.Read())
                {
                    lignebl = new LigneBonLivraison(Reader.GetInt32(0), Reader.GetString(1), Reader.GetString(2),
                                    Reader.GetString(3), Reader.GetDouble(4), Reader.GetDouble(5), Reader.GetDouble(6),
                                    Reader.GetDouble(7), Reader.GetDouble(8), Reader.GetDouble(9));
                    tab_lignesBL.Add(lignebl);
                }
                Reader.Close();
                //MessageBox.Show("ici...", "Suivi Erreur",
                //    MessageBoxButtons.OK, MessageBoxIcon.Error); 

            }
            catch (OdbcException e)
            {
                MessageBox.Show(e.Message, Program.SelectGlobalMessages.SelectLigneBonLivraison,
                    MessageBoxButtons.OK, MessageBoxIcon.Error);                
            }

            //finally
            //{
                if (tab_lignesBL.Count == 0)
                    return null;
                else
                    return tab_lignesBL;
            //}

        }

        public static Boolean SupprimerAllLigneBonLivraison(string  _code_bonlivraison)
        {
            string CommandText1 = "delete from " + DAL.DataBaseTableName.TableLigneBonLivraison +
                                  " where code_bonlivraison='" + _code_bonlivraison+"'" ;
            return DataBaseConnexion.addOrUpdateElementInDataBase(CommandText1, Program.SelectGlobalMessages.ImpDeleteLigneBonLivraison);
        }
        
        public long getMaxNumberOfLigneBonLivraison(string    _codeBL)
        {
            string CommandText = "select MAX(numero_lignebonlivraison) from " + DAL.DataBaseTableName.TableLigneBonLivraison +
                                 " where code_bonLivraison='" + _codeBL+"'";            
            return  DAL.DataBaseConnexion.getMaxNumberOfStringColumn(CommandText);  
        }

        public static ArrayList getALLLigneBonLivraisonByClientAndProduit(string _codeClient, string _codeProd)
        {
            ArrayList tab_lignesBL = new ArrayList();
            LigneBonLivraison lignebl = null;
            OdbcConnection connection = DataBaseConnexion.getConnection();
            // listOfCommandesVente = null;
            try
            {
                OdbcCommand cmd = connection.CreateCommand();

                cmd.CommandText = "select " + DataBaseTableName.TableLigneBonLivraison +".* from " +
                     DataBaseTableName.TableLigneBonLivraison + "," + DataBaseTableName.TableBonLivraison +
                       " where  codeclient_bonlivraison = '" + _codeClient + "'" +  
                       " and  " + DataBaseTableName.TableLigneBonLivraison + ".code_bonlivraison = "+
                       DataBaseTableName.TableBonLivraison + ".code_bonlivraison" +
                       " and codeproduit_lignebonlivraison='" + _codeProd + "' ;";

                OdbcDataReader Reader = cmd.ExecuteReader();

                int i = 0;
                while (Reader.Read() && i < 3)
                {
                    lignebl = new LigneBonLivraison(Reader.GetInt32(0), Reader.GetString(1), Reader.GetString(2),
                        Reader.GetString(3), Reader.GetDouble(4), Reader.GetDouble(5),
                        Reader.GetDouble(6), Reader.GetDouble(7), Reader.GetDouble(8), Reader.GetDouble(9));
                    tab_lignesBL.Add(lignebl);
                    i = i + 1;
                }
                Reader.Close();

            }
            catch (OdbcException e)
            {
                MessageBox.Show(e.Message, Program.SelectGlobalMessages.SelectLigneBonLivraison,
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            return tab_lignesBL;

        }

        public static Boolean SupprimerAllLigneBonLivraison()
        {
            string CommandText1 = "delete from " + DAL.DataBaseTableName.TableLigneBonLivraison;
            return DataBaseConnexion.addOrUpdateElementInDataBase(CommandText1, Program.SelectGlobalMessages.ImpDeleteLigneBonLivraison);
        }
    }

}