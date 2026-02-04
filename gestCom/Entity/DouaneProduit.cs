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
    class DouaneProduit
    {
        public static string Separateur = CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator;

        public int code_douaneproduit;
        public string designation_douaneproduit;

        // Constructeurs :
        public DouaneProduit()
        { }
                
        public DouaneProduit(int _code)
        {
            this.code_douaneproduit = _code;
        }
                
        public DouaneProduit(int _code, string _designation)
        {
            this.code_douaneproduit = _code;
            this.designation_douaneproduit = _designation;
        }
        
        // Méthodes :
        public Boolean ajouterDouaneProduit()
        {
            string CommandText = "insert into " + DataBaseTableName.TableDouaneProduit + 
                    " values ( " +  this.code_douaneproduit + ",'" + this.designation_douaneproduit.ToString().Replace("'", "''") + "');";
            return DataBaseConnexion.addOrUpdateElementInDataBase(CommandText, Program.SelectGlobalMessages.ImpAddDouaneProduit);
        }
                
        public Boolean modifierDouaneProduit()
        {
            string CommandText = "update " + DataBaseTableName.TableDouaneProduit + 
                " Set designation_douaneproduit = '" + this.designation_douaneproduit.ToString().Replace("'", "''") + "' " +
                " where code_douaneproduit =" + this.code_douaneproduit;
            return DataBaseConnexion.addOrUpdateElementInDataBase(CommandText, Program.SelectGlobalMessages.ImpUpdateDouaneProduit);
        }

        public static Boolean supprimerDouaneProduit(int _codeDouaneproduit)
        {
            string CommandText = "delete from " + DataBaseTableName.TableDouaneProduit + 
                            " where code_douaneproduit=" + _codeDouaneproduit;
            return DataBaseConnexion.addOrUpdateElementInDataBase(CommandText, Program.SelectGlobalMessages.ImpDeleteDouaneProduit);
        }

        public static DouaneProduit getDouaneProduitByCode(int _codeDouaneProduit)
        {
            DouaneProduit douaneProduit = null;
            if (DataBaseConnexion.getRowsCount(DAL.DataBaseTableName.TableDouaneProduit, "code_douaneproduit") != 0)
            {
                OdbcConnection connection = DataBaseConnexion.getConnection();
                try
                {
                    OdbcCommand cmd = connection.CreateCommand();
                    cmd.CommandText = "select * from " + DataBaseTableName.TableDouaneProduit + 
                        " where code_douaneproduit = " + _codeDouaneProduit;
                    OdbcDataReader Reader = cmd.ExecuteReader();
                    if (Reader.Read())
                    {
                        douaneProduit = new DouaneProduit(Reader.GetInt32(0), Reader.GetString(1));
                    }
                    Reader.Close();
                }
                catch (OdbcException e)
                {
                    MessageBox.Show(e.Message, Program.SelectGlobalMessages.SelectDouane,
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }               
            }
            return douaneProduit;
        }

        public static DouaneProduit getDouaneProduitByDesignation(String _designation_douaneproduit)
        {
            DouaneProduit douaneProduit = null;
            if (DataBaseConnexion.getRowsCount(DAL.DataBaseTableName.TableDouaneProduit, "code_douaneproduit") != 0)
            {
                OdbcConnection connection = DataBaseConnexion.getConnection();
                try
                {
                    OdbcCommand cmd = connection.CreateCommand();
                    cmd.CommandText = "select * from " + DataBaseTableName.TableDouaneProduit +
                        " where designation_douaneproduit = '" + _designation_douaneproduit + "'";
                    OdbcDataReader Reader = cmd.ExecuteReader();
                    if (Reader.Read())
                    {
                        douaneProduit = new DouaneProduit(Reader.GetInt32(0), Reader.GetString(1));
                    }
                    Reader.Close();
                }
                catch (OdbcException e)
                {
                    MessageBox.Show(e.Message, Program.SelectGlobalMessages.SelectDouane,
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            return douaneProduit;
        }
       
    }
}
