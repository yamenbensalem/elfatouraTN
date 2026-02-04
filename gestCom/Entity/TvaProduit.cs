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
    class TvaProduit
    {
        // les attributs:
        public static string Separateur = CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator;
        public int code_tvaproduit;
        public double designation_tvaproduit;
        
        // Les constructeurs:
        public TvaProduit()
        { }

        public TvaProduit(int _code)
        {
            this.code_tvaproduit = _code;
        }
        
        public TvaProduit(int _code, double _designation)
        {
            this.code_tvaproduit = _code;
            this.designation_tvaproduit = _designation;
        }

        // Les méthodes:        
        public Boolean ajouterTvaProduit()
        {
            string CommandText = "insert into " + DAL.DataBaseTableName.TableTvaProduit + " values(" +
                   this.code_tvaproduit +
                   this.designation_tvaproduit.ToString().Replace("'", "''") + "');";
            return DataBaseConnexion.addOrUpdateElementInDataBase(CommandText, Program.SelectGlobalMessages.ImpAddTVAProduit);
        }
       
        public Boolean modifierTvaProduit()
        {
            string CommandText = "update " + DAL.DataBaseTableName.TableTvaProduit + " set designation_tvaproduit=" +
                    this.designation_tvaproduit + " where code_tvaproduit =" + this.code_tvaproduit + ";";
            return DataBaseConnexion.addOrUpdateElementInDataBase(CommandText, Program.SelectGlobalMessages.ImpUpdateTVAProduit);
        }

        public static Boolean supprimerTvaProduit(int _code_tvaproduit)
        {
            string CommandText = "delete from " + DAL.DataBaseTableName.TableTvaProduit + " where code_tvaproduit=" +
                    _code_tvaproduit + ";";
            return DataBaseConnexion.addOrUpdateElementInDataBase(CommandText, Program.SelectGlobalMessages.ImpDeleteTVAProduit);
        }
        
        public static TvaProduit getTvaProduit(int _code)
        {
            TvaProduit tvaProduit = null;
            if (DataBaseConnexion.getRowsCount(DAL.DataBaseTableName.TableTvaProduit, "code_tvaproduit") != 0)
            {
                OdbcConnection connection = DataBaseConnexion.getConnection();
                //try
                //{
                    OdbcCommand cmd = connection.CreateCommand();
                    cmd.CommandText = "select * from " + DAL.DataBaseTableName.TableTvaProduit + " where code_tvaproduit=" + _code;
                    OdbcDataReader Reader = cmd.ExecuteReader();
                    if (Reader.Read())
                    {
                        tvaProduit = new TvaProduit(Reader.GetInt32(0), Reader.GetDouble(1));
                    }
                    Reader.Close();
                //}
                //catch (OdbcException e)
                //{
                //    MessageBox.Show(e.Message, Program.SelectGlobalMessages.SelectTvaProduit,
                //        MessageBoxButtons.OK, MessageBoxIcon.Error);
                //}
                //catch (Exception)
                //{
                //    MessageBox.Show(Program.SelectGlobalMessages.ImpResearchTVAProduit,
                //  Program.SelectGlobalMessages.SelectTvaProduit, MessageBoxButtons.OK, MessageBoxIcon.Error);
                //}
            }
            return tvaProduit;
        }

        public static int getCode(string _designation)
        {
            int code = -1;
            if (DataBaseConnexion.getRowsCount(DAL.DataBaseTableName.TableTvaProduit, "code_tvaproduit") != 0)
            {
                OdbcConnection connection = DataBaseConnexion.getConnection();
                //try
                //{
                    OdbcCommand cmd = connection.CreateCommand();
                    cmd.CommandText = "Select code_tvaproduit from  " + DAL.DataBaseTableName.TableTvaProduit +
                                      " where designation_tvaproduit =" + _designation ;
                    OdbcDataReader Reader = cmd.ExecuteReader();
                    if (Reader.Read())
                    {
                        code = Reader.GetInt32(0);
                    }
                    Reader.Close();
                //}
                //catch (OdbcException e)
                //{
                //    MessageBox.Show(e.Message, Program.SelectGlobalMessages.ResearchTvaProduit,
                //        MessageBoxButtons.OK, MessageBoxIcon.Error);
                //}
                //catch (Exception)
                //{
                //    MessageBox.Show(Program.SelectGlobalMessages.ImpResearchTVAProduit,
                //         Program.SelectGlobalMessages.ResearchTvaProduit, MessageBoxButtons.OK, MessageBoxIcon.Error);
                //}
            }
            return code;
        }
        
        public static double getDesignation(int _code)
        {
            double designation = 0;

            if (DataBaseConnexion.getRowsCount(DAL.DataBaseTableName.TableTvaProduit, "code_tvaproduit") != 0)
            {
                OdbcConnection connection = DataBaseConnexion.getConnection();
                //try
                //{
                    OdbcCommand cmd = connection.CreateCommand();
                    cmd.CommandText = "Select designation_tvaproduit  from " + DAL.DataBaseTableName.TableTvaProduit + 
                                      " where code_tvaproduit =" + _code + " ;";
                    OdbcDataReader Reader = cmd.ExecuteReader();
                    if (Reader.Read())
                    {
                        designation = Reader.GetDouble(0);
                    }
                    Reader.Close();
                //}
                //catch (OdbcException e)
                //{
                //    MessageBox.Show(e.Message, Program.SelectGlobalMessages.ResearchTvaProduit,
                //        MessageBoxButtons.OK, MessageBoxIcon.Error);
                //}
                //catch (Exception)
                //{
                //    MessageBox.Show(Program.SelectGlobalMessages.ImpResearchTVAProduit,
                //         Program.SelectGlobalMessages.ResearchTvaProduit, MessageBoxButtons.OK, MessageBoxIcon.Error);
                //}
            }
            return designation;
        }

    }
}
