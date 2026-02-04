using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Windows.Forms;
using T4C_Commercial_Project.DAL;
using System.Data.Odbc;

namespace T4C_Commercial_Project.Entity
{
    class Devise
    {
        public int code_devise;
        public string designation_devise;
        
        // Constructeurs :
        public Devise()
        { }
                
        public Devise(int _codeDevise, string _designationDevise)
        {
            this.code_devise = _codeDevise;
            this.designation_devise = _designationDevise;
        }
                
        // Méthodes :        
        public Boolean ajouterDevise()
        {
            string CommandText = "insert into  " + DAL.DataBaseTableName.TableDevise +
                     " values(" + this.code_devise + ",'" + this.designation_devise.ToString().Replace("'", "''") + "');";
            return DataBaseConnexion.addOrUpdateElementInDataBase(CommandText, Program.SelectGlobalMessages.ImpAddDevise);
        }
                
        public Boolean modifierDevise()
        {
            string CommandText = "update " + DAL.DataBaseTableName.TableDevise +
                    " Set designation_devise = '" + this.designation_devise.ToString().Replace("'", "''") + "' " +
                    " Where code_devise = " + this.code_devise;
            return DataBaseConnexion.addOrUpdateElementInDataBase(CommandText, Program.SelectGlobalMessages.ImpUpdateDevise);
        }

        public static Boolean supprimerDevise(int _codeDevise)
        {
            string CommandText = "delete from  " + DAL.DataBaseTableName.TableDevise +
                            " where code_devise = " + _codeDevise;
            return DataBaseConnexion.addOrUpdateElementInDataBase(CommandText, Program.SelectGlobalMessages.ImpDeleteDevise);
        }

        public static Devise getDeviseByCode(int _code_devise)
        {
            Devise devise = null;
            if (DataBaseConnexion.getRowsCount(DAL.DataBaseTableName.TableDevise, "code_devise") != 0)
            {
                OdbcConnection connection = DataBaseConnexion.getConnection();
                try
                {
                    OdbcCommand cmd = connection.CreateCommand();
                    cmd.CommandText = "select * from  " + DAL.DataBaseTableName.TableDevise +
                                    " where code_devise = " + _code_devise;
                    OdbcDataReader Reader = cmd.ExecuteReader();
                    if (Reader.Read())
                    {
                        devise = new Devise(Reader.GetInt32(0), Reader.GetString(1));
                    }
                    Reader.Close();
                }
                catch (OdbcException e)
                {
                    MessageBox.Show(e.Message, Program.SelectGlobalMessages.SelectDevise,
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }                
            }
            return devise;
        }

        public static Devise getDeviseByDesignation(string _designation_devise)
        {
            Devise currentDevise = null;
            if (DataBaseConnexion.getRowsCount(DAL.DataBaseTableName.TableDevise, "code_devise") != 0)
            {
                OdbcConnection connection = DataBaseConnexion.getConnection();
                try
                {
                    OdbcCommand cmd = connection.CreateCommand();
                    cmd.CommandText = "select * from  " + DAL.DataBaseTableName.TableDevise +
                                    " where designation_devise = '" + _designation_devise + "'";
                    OdbcDataReader Reader = cmd.ExecuteReader();
                    if (Reader.Read())
                    {
                        currentDevise = new Devise(Reader.GetInt32(0), Reader.GetString(1));
                    }
                    Reader.Close();
                }
                catch (OdbcException e)
                {
                    MessageBox.Show(e.Message, Program.SelectGlobalMessages.SelectDevise,
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            return currentDevise;
        }
    }
}
