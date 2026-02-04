using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Windows.Forms;


using System.Data;

using System.Collections;
using System.Globalization;
using T4C_Commercial_Project.DAL;
using System.Data.Odbc;


namespace T4C_Commercial_Project.Entity
{
    public partial class FabriquantProduit
    {
        // attributs
        public static string Separateur = CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator;
        public int code_fabriquant;
        public string designation_fabriquant;


        //les constructeurs:        
        public FabriquantProduit()
        {  }

        public FabriquantProduit(int _code, String _designation)
        {
            this.code_fabriquant = _code;
            this.designation_fabriquant = _designation;
        }
               
        public Boolean ajouterFabriquant()
        {
            string CommandText = "insert into " + DAL.DataBaseTableName.TableFabriquantProduit + " values(" +
                   this.code_fabriquant + ",'" +  this.designation_fabriquant.ToString().Replace("'", "''") + "');";
            return DataBaseConnexion.addOrUpdateElementInDataBase(CommandText, Program.SelectGlobalMessages.ImpAddFabriquantProduit);
        }
                
        public Boolean modifierFabriquant()
        {
            string CommandText = "update " + DAL.DataBaseTableName.TableFabriquantProduit + " set designation_fabriquant='" +
                     this.designation_fabriquant.ToString().Replace("'", "''") + "' where code_fabriquant =" + this.code_fabriquant;
            return DataBaseConnexion.addOrUpdateElementInDataBase(CommandText, Program.SelectGlobalMessages.ImpUpdateFabriquantProduit);
        }

        public static Boolean supprimerFabriquant(int _code_fabriquant)
        {
            string CommandText = "delete from " + DAL.DataBaseTableName.TableFabriquantProduit + 
                                " where code_fabriquant = " + _code_fabriquant;
            return DataBaseConnexion.addOrUpdateElementInDataBase(CommandText, Program.SelectGlobalMessages.ImpDeleteFabriquantProduit);
        }

        public static FabriquantProduit getFabriquantByCode(int _code_fabriquantproduit)
        {
            FabriquantProduit fabriquant = null;
            if (DataBaseConnexion.getRowsCount(DAL.DataBaseTableName.TableFabriquantProduit, "code_fabriquant") != 0)
            {
                OdbcConnection connection = DataBaseConnexion.getConnection();
                try
                {
                    OdbcCommand cmd = connection.CreateCommand();
                    cmd.CommandText = "select * from " + DAL.DataBaseTableName.TableFabriquantProduit + 
                                        " where code_fabriquant=" + _code_fabriquantproduit;
                    OdbcDataReader Reader = cmd.ExecuteReader();
                    if (Reader.Read())
                    {
                        fabriquant = new FabriquantProduit(Reader.GetInt32(0), Reader.GetString(1));
                    }
                    Reader.Close();
                }
                catch (OdbcException e)
                {
                    MessageBox.Show(e.Message, Program.SelectGlobalMessages.SelectFabriquant,
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                catch (Exception)
                {
                    MessageBox.Show(Program.SelectGlobalMessages.ImpSelectFabriquantProduit,
                      Program.SelectGlobalMessages.SelectFabriquant, MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            return fabriquant;
        }
        
        public static FabriquantProduit getFabriquantByDesignation(String _designation_fabriquantproduit)
        {
            FabriquantProduit fabriquant = null;
            if (DataBaseConnexion.getRowsCount(DAL.DataBaseTableName.TableFabriquantProduit, "code_fabriquant") != 0)
            {
                OdbcConnection connection = DataBaseConnexion.getConnection();
                try
                {
                    OdbcCommand cmd = connection.CreateCommand();
                    cmd.CommandText = "select * from " + DAL.DataBaseTableName.TableFabriquantProduit +
                                    " where designation_fabriquant='" + _designation_fabriquantproduit +"'";
                    OdbcDataReader Reader = cmd.ExecuteReader();
                    if (Reader.Read())
                    {
                        fabriquant = new FabriquantProduit(Reader.GetInt32(0), Reader.GetString(1));
                    }
                    Reader.Close();
                }
                catch (OdbcException e)
                {
                    MessageBox.Show(e.Message, Program.SelectGlobalMessages.SelectFabriquant,
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }                
            }
            return fabriquant;
        }
    }
}
