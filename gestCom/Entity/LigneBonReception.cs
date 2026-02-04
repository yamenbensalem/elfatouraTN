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
    class LigneBonReception
    {
        public static string Separateur = CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator;

        // Les attributs:
        public int numero_lignebonreception;
        public string code_bonreception;
        public String codeproduit_lignebonreception;
        public String designationproduit_lignebonreception;
        public double quantite_lignebonreception;
        public double prixunitaire_lignebonreception;
        public double montantHT_lignebonreception;
        public double tvaproduit_lignebonreception;
        public double remise_lignebonreception;
        
        // Les méthodes :
        public LigneBonReception(int _numero_lignebonreception, string _code_bonreception, String _codeproduit, string _designation_prod,
                                double _quantite, double _prixunitaire, double _montantHT, double _tvaProduit, double _remise)
        {
            numero_lignebonreception = _numero_lignebonreception;
            code_bonreception = _code_bonreception;
            codeproduit_lignebonreception = _codeproduit;
            designationproduit_lignebonreception = _designation_prod;
            quantite_lignebonreception = _quantite;
            prixunitaire_lignebonreception = _prixunitaire;
            montantHT_lignebonreception = _montantHT;
            tvaproduit_lignebonreception = _tvaProduit;
            remise_lignebonreception = _remise;
        }

        public LigneBonReception(int _numero_lignebonreception, string _code_bonreception, String _codeProduit)
        {
            numero_lignebonreception = _numero_lignebonreception;
            code_bonreception = _code_bonreception;
            codeproduit_lignebonreception = _codeProduit;
        }

        public LigneBonReception(string _code)
        {
            code_bonreception = _code;
        }

        public LigneBonReception()
        {

        }

        public Boolean ajouterLigneBonReception()
        {
            string CommandText = "insert into " + DAL.DataBaseTableName.TableLigneBonReception +
                        " values (" +  this.numero_lignebonreception +
                        " ,  '" + this.code_bonreception + "' " +
                        " , '" + this.codeproduit_lignebonreception + "' " +
                        " , '" + this.designationproduit_lignebonreception.ToString().Replace("'", "''") + "' " +
                        " ,  " + this.quantite_lignebonreception.ToString().ToString().Replace(',', '.') +
                        " ,  " + this.prixunitaire_lignebonreception.ToString().ToString().Replace(',', '.') + 
                        " ,  " + this.montantHT_lignebonreception.ToString().ToString().Replace(',', '.') +
                        " ,  " + this.tvaproduit_lignebonreception.ToString().ToString().Replace(',', '.') + 
                        " ,  " + this.remise_lignebonreception.ToString().ToString().Replace(',', '.') +                         
                        " ) ";
            return DataBaseConnexion.addOrUpdateElementInDataBase(CommandText, Program.SelectGlobalMessages.ImpAddLigneBonReception);
        }

        public Boolean modifierLigneBonReception()
        {
            string CommandText = "update " + DAL.DataBaseTableName.TableLigneBonReception +
                       " set " +
                       " designation_prod='" + this.designationproduit_lignebonreception.ToString().Replace("'", "''") + "'" +
                       ", quantite_lignebonreception =" + this.quantite_lignebonreception.ToString().ToString().Replace(',', '.') +
                       ", prixunitaire_lignebonreception=" + this.prixunitaire_lignebonreception.ToString().ToString().Replace(',', '.') +
                       ", montantHT_lignebonreception=" + this.montantHT_lignebonreception.ToString().ToString().Replace(',', '.') +
                       ", tvaProduit_lignebonreception=" + this.tvaproduit_lignebonreception.ToString().ToString().Replace(',', '.') +
                       ", remise_lignebonreception=" + this.remise_lignebonreception.ToString().ToString().Replace(',', '.') +
                       " where code_bonreception = '" + this.code_bonreception + "' " +
                       " and codeproduit_lignebonreception ='" + this.codeproduit_lignebonreception + "' ;";
            return DataBaseConnexion.addOrUpdateElementInDataBase(CommandText, Program.SelectGlobalMessages.ImpUpdateLigneBonReception);
        }

        public static Boolean supprimerLigneBonReception(string _code_bonreception, String _codeproduit_lignebonreception)
        {
            string CommandText = "delete from " + DAL.DataBaseTableName.TableLigneBonReception +
                       " where code_bonreception = '" + _code_bonreception + "' " +
                       " and codeproduit_lignebonreception= '" + _codeproduit_lignebonreception  + "' ";
            return DataBaseConnexion.addOrUpdateElementInDataBase(CommandText, Program.SelectGlobalMessages.ImpDeleteLigneBonReception);
        }

        public static LigneBonReception getLigneBonReception(string _code_bonreception, String _codeproduit_lignebonreception)
        {
            LigneBonReception ligneBonReception = null;
            OdbcConnection connection = DAL.DataBaseConnexion.getConnection();
            try
            {
                OdbcCommand cmd = connection.CreateCommand();
                cmd.CommandText = "select * from " + DAL.DataBaseTableName.TableLigneBonReception +
                        " where code_bonreception = '" + _code_bonreception + "' " + 
                        " and codeproduit_lignebonreception ='" + _codeproduit_lignebonreception + "';";
                OdbcDataReader Reader = cmd.ExecuteReader();
                if (Reader.Read())
                {
                    ligneBonReception = new LigneBonReception(Reader.GetInt32(0), Reader.GetString(1), Reader.GetString(2), Reader.GetString(3),
                                                              Reader.GetDouble(4), Reader.GetDouble(5), Reader.GetDouble(6), Reader.GetDouble(7), 
                                                              Reader.GetDouble(8));
                }
                Reader.Close();
            }
            catch (OdbcException e)
            {
                MessageBox.Show(e.Message, Program.SelectGlobalMessages.SelectLigneBonReception,
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch
            {
                MessageBox.Show(Program.SelectGlobalMessages.ImpSelectLigneBonReception,
                     Program.SelectGlobalMessages.SelectLigneBonReception, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            return ligneBonReception;
        }

        public static ArrayList getALLLigneBonReception(string _codeBR)
        {
            ArrayList listOfLigneBonReception = new ArrayList();
            LigneBonReception ligneBonReception = null;
            OdbcConnection connection = DAL.DataBaseConnexion.getConnection();
            try
            {
                OdbcCommand cmd = connection.CreateCommand();
                cmd.CommandText = "select * from " + DAL.DataBaseTableName.TableLigneBonReception +
                        " where code_bonreception = '" + _codeBR + "' " + ";";
                OdbcDataReader Reader = cmd.ExecuteReader();
                if (Reader.HasRows)
                { }
                while (Reader.Read())
                {
                    ligneBonReception = new LigneBonReception(Reader.GetInt32(0), Reader.GetString(1), Reader.GetString(2), Reader.GetString(3),
                                                              Reader.GetDouble(4), Reader.GetDouble(5), Reader.GetDouble(6), Reader.GetDouble(7),
                                                              Reader.GetDouble(8));
                    listOfLigneBonReception.Add(ligneBonReception);
                }
                Reader.Close();
            }
            catch (OdbcException e)
            {
                MessageBox.Show(e.Message, Program.SelectGlobalMessages.SelectBonReception,
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch
            {
                MessageBox.Show(Program.SelectGlobalMessages.ImpSelectBonReception,
                     Program.SelectGlobalMessages.SelectBonReception, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            return listOfLigneBonReception;
        }
                
        public bool isExistedLigneBonReception(string _code_bonreception, string _codeproduit_lignebonreception, ref bool isException)
        {
            bool isExist = false;
            isException = false;
            OdbcConnection connection = DAL.DataBaseConnexion.getConnection();
            try
            {
                OdbcCommand cmd = connection.CreateCommand();
                cmd.CommandText = "select * from " + DAL.DataBaseTableName.TableLigneBonReception +
                        " where code_bonreception = '" + _code_bonreception + "' " + 
                        " and codeproduit_lignebonreception ='" + _codeproduit_lignebonreception + "';";
                OdbcDataReader Reader = cmd.ExecuteReader();
                if (Reader.Read())
                {
                    isExist = true;
                }                
                Reader.Close();                
            }
            catch (OdbcException e)
            {
                MessageBox.Show(e.Message, Program.SelectGlobalMessages.SelectBonReception,
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                isException = true;                 
            }
            catch
            {
                MessageBox.Show(Program.SelectGlobalMessages.ImpSelectBonReception,
                     Program.SelectGlobalMessages.SelectBonReception, MessageBoxButtons.OK, MessageBoxIcon.Error);

                isException = true;
            }
            return isExist;
        }

        public LigneBonReception getOneLigneBonReception(string _codeBR, string _codeProd)
        {
            LigneBonReception ligneBonReception = null;
            OdbcConnection connection = DAL.DataBaseConnexion.getConnection();
            try
            {
                OdbcCommand cmd = connection.CreateCommand();
                cmd.CommandText = "select * from " + DAL.DataBaseTableName.TableLigneBonReception +
                        " where code_bonreception = '" + _codeBR + "' and codeproduit_lignebonreception = '" + _codeProd + "' ;";
                OdbcDataReader Reader = cmd.ExecuteReader();
                if (Reader.Read())
                {
                    ligneBonReception = new LigneBonReception(Reader.GetInt32(0), Reader.GetString(1), Reader.GetString(2), Reader.GetString(3),
                                                              Reader.GetDouble(4), Reader.GetDouble(5), Reader.GetDouble(6), Reader.GetDouble(7),
                                                              Reader.GetDouble(8));
                }
                Reader.Close();
            }
            catch (OdbcException e)
            {
                MessageBox.Show(e.Message, Program.SelectGlobalMessages.SelectBonReception,
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch
            {
                MessageBox.Show(Program.SelectGlobalMessages.ImpSelectBonReception,
                     Program.SelectGlobalMessages.SelectBonReception, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            return ligneBonReception;
        }
                
        public static Boolean supprimerAllLigneBonReception(string _codeBr)
        {
            String CommandText1 = "delete from " + DAL.DataBaseTableName.TableLigneBonReception +
                    " where code_bonreception = '" + _codeBr + "' ";
            return DataBaseConnexion.deleteElementFromDataBase(CommandText1, Program.SelectGlobalMessages.ImpDeleteBonReception);
        }

        public static Boolean supprimerAllLigneBonReception()
        {
            string CommandText = "delete from " + DAL.DataBaseTableName.TableLigneBonReception;
            return DataBaseConnexion.addOrUpdateElementInDataBase(CommandText, Program.SelectGlobalMessages.ImpDeleteLigneBonReception);
        }
    }
}