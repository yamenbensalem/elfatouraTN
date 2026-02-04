using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Odbc;
using T4C_Commercial_Project.DAL;
using System.Windows.Forms;

namespace T4C_Commercial_Project.Entity
{
    class MagasinProduit
    {
        public int code_magasinproduit;
        public string designation_magasinproduit;

        public MagasinProduit(int _codeMagasin, string _designationMagasin)
        {
            this.code_magasinproduit = _codeMagasin;
            this.designation_magasinproduit = _designationMagasin;
         }
        
        public Boolean ajouterMagasinProduit()
        {
            string commandtext = "insert into " + DAL.DataBaseTableName.TableMagasinProduit +
                                " values ( " + this.code_magasinproduit + ",'" + this.designation_magasinproduit + "');";
            return DataBaseConnexion.addOrUpdateElementInDataBase(commandtext, Program.SelectGlobalMessages.ImpAddMagasin);
        }

        public static Boolean supprimerMagasinProduit(int _codeMagasin)
        {
            string commandtext = "delete from " + DAL.DataBaseTableName.TableMagasinProduit +
                                " where code_magasin=" + _codeMagasin + ";";
            return DataBaseConnexion.addOrUpdateElementInDataBase(commandtext, Program.SelectGlobalMessages.ImpDeleteMagasin);
        }

        public static MagasinProduit getMagasinProduitByCode(int _codeMagasin)
        {
            MagasinProduit currentMagasinProduit = null;
            if (DataBaseConnexion.getRowsCount(DAL.DataBaseTableName.TableMagasinProduit, "code_magasinproduit") != 0)
            {
                OdbcConnection connection = DataBaseConnexion.getConnection();
                //try
                //{
                    OdbcCommand cmd = connection.CreateCommand();
                    cmd.CommandText = "Select * from  " + DAL.DataBaseTableName.TableMagasinProduit +
                                        " where code_magasinproduit = " + _codeMagasin ;
                    OdbcDataReader Reader = cmd.ExecuteReader();
                    if (Reader.Read())
                    {
                        currentMagasinProduit = new MagasinProduit(Reader.GetInt32(0), Reader.GetString(1));
                    }

                    Reader.Close();
                //}
                //catch (OdbcException e)
                //{
                //    MessageBox.Show(e.Message, Program.SelectGlobalMessages.ResearchMagasin,
                //        MessageBoxButtons.OK, MessageBoxIcon.Error);
                //}                
            }
            return currentMagasinProduit;
        }
        
        public static MagasinProduit getMagasinProduitByDesignation(string _designationMagasin)
        {
            MagasinProduit currentMagasinProduit = null;
            if (DataBaseConnexion.getRowsCount(DAL.DataBaseTableName.TableMagasinProduit, "code_magasinproduit") != 0)
            {
                OdbcConnection connection = DataBaseConnexion.getConnection();
                //try
                //{
                    OdbcCommand cmd = connection.CreateCommand();
                    cmd.CommandText = "Select * from  " + DAL.DataBaseTableName.TableMagasinProduit +
                                        " where designation_magasinproduit like '" + _designationMagasin + "' ;";
                    OdbcDataReader Reader = cmd.ExecuteReader();
                    if (Reader.Read())
                    {
                        currentMagasinProduit = new MagasinProduit(Reader.GetInt32(0), Reader.GetString(1));
                    }

                    Reader.Close();
                //}
                //catch (OdbcException e)
                //{
                //    MessageBox.Show(e.Message, Program.SelectGlobalMessages.ResearchMagasin,
                //        MessageBoxButtons.OK, MessageBoxIcon.Error);
                //}                
            }
            return currentMagasinProduit;
        }

    }
}
