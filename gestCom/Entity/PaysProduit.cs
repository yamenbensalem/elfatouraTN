using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Data;
using System.Globalization;
using T4C_Commercial_Project.DAL;
using System.Data.Odbc;

namespace T4C_Commercial_Project.Entity
{
    class PaysProduit
    {
        public static string Separateur = CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator;
        public int code_paysproduit;
        public string designation_paysproduit;

        // Constructeurs :
        public PaysProduit()
        { }
                
        public PaysProduit(int _code)
        {
            this.code_paysproduit = _code;
        }
                
        public PaysProduit(int _code, string _designation)
        {
            this.code_paysproduit = _code;
            this.designation_paysproduit = _designation;
        }
        
        // Méthodes :
        public Boolean ajouterPaysProduit()
        {
            string CommandText = "insert into " + DAL.DataBaseTableName.TablePaysProduit + " values(" +
                this.code_paysproduit + ",'" +
                this.designation_paysproduit.ToString().Replace("'", "''") + "');";
            return DataBaseConnexion.addOrUpdateElementInDataBase(CommandText, Program.SelectGlobalMessages.ImpAddPaysProduit);
        }
        
        public Boolean modifierPaysProduit()
        {
            string CommandText = "update " + DAL.DataBaseTableName.TablePaysProduit + " set designation_paysproduit='" +
                this.designation_paysproduit.ToString().Replace("'", "''") + "' where code_paysproduit =" + this.code_paysproduit;
            return DataBaseConnexion.addOrUpdateElementInDataBase(CommandText, Program.SelectGlobalMessages.ImpUpdatePaysProduit);
        }

        public static Boolean supprimerPaysProduit(int _codePaysProduit)
        {
            string CommandText = "delete from " + DAL.DataBaseTableName.TablePaysProduit + " where code_paysproduit=" + _codePaysProduit;
            return DataBaseConnexion.addOrUpdateElementInDataBase(CommandText, Program.SelectGlobalMessages.ImpDeletePaysProduit);
        }

        public static PaysProduit getPaysProduitByCode(int _codePaysProduit)
        {
            PaysProduit paysProduit = null;
            if (DataBaseConnexion.getRowsCount(DAL.DataBaseTableName.TablePaysProduit, "code_paysproduit") != 0)
            {
                OdbcConnection connection = DataBaseConnexion.getConnection();
                try
                {
                    OdbcCommand cmd = connection.CreateCommand();
                    cmd.CommandText = "select * from " + DAL.DataBaseTableName.TablePaysProduit +
                                    " where code_paysproduit=" + _codePaysProduit;
                    OdbcDataReader Reader = cmd.ExecuteReader();
                    if (Reader.Read())
                    {
                        paysProduit = new PaysProduit(Reader.GetInt32(0), Reader.GetString(1));
                    }
                    Reader.Close();
                }
                catch (OdbcException e)
                {
                    MessageBox.Show(e.Message, Program.SelectGlobalMessages.SelectPays,
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                catch (Exception)
                {
                    MessageBox.Show(Program.SelectGlobalMessages.ImpSelectPaysProduit,
                      Program.SelectGlobalMessages.SelectPays, MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            return paysProduit;
        }
        
        public static PaysProduit getPaysProduitByDesignation(String _designationPaysProduit)
        {
            PaysProduit paysProduit = null;
            if (DataBaseConnexion.getRowsCount(DAL.DataBaseTableName.TablePaysProduit, "code_paysproduit") != 0)
            {
                OdbcConnection connection = DataBaseConnexion.getConnection();
                try
                {
                    OdbcCommand cmd = connection.CreateCommand();
                    cmd.CommandText = "select * from " + DAL.DataBaseTableName.TablePaysProduit +
                                    " where designation_paysproduit='" + _designationPaysProduit + "'";
                    OdbcDataReader Reader = cmd.ExecuteReader();
                    if (Reader.Read())
                    {
                        paysProduit = new PaysProduit(Reader.GetInt32(0), Reader.GetString(1));
                    }
                    Reader.Close();
                }
                catch (OdbcException e)
                {
                    MessageBox.Show(e.Message, Program.SelectGlobalMessages.SelectPays,
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                catch (Exception)
                {
                    MessageBox.Show(Program.SelectGlobalMessages.ImpSelectPaysProduit,
                      Program.SelectGlobalMessages.SelectPays, MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            return paysProduit;
        }
     }
}
