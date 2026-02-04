using System;
using System.Collections.Generic;
using System.Data.Odbc;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using T4C_Commercial_Project.DAL;

namespace T4C_Commercial_Project.Entity
{
    class ParametreDecimales
    {
        public static string Separateur = System.Globalization.CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator;

        public int Prix;
        public int Quantites;

        // Constructeurs :
        public ParametreDecimales()
        { }
                
        
                
        public ParametreDecimales(int _Prix, int _Quantites)
        {
            this.Prix = _Prix;
            this.Quantites = _Quantites;
        }
        
        // Méthodes :
        public Boolean ajouterParametresDecimales()
        {
            string CommandText = "insert into " + DataBaseTableName.TableParametresDecimales + 
                    " values ( " +  this.Prix + "," + this.Quantites + ");";
            return DataBaseConnexion.addOrUpdateElementInDataBase(CommandText, Program.SelectGlobalMessages.ImpAddDouaneProduit);
        }
                
        public Boolean modifierParametresDecimales()
        {
            string CommandText = "update " + DataBaseTableName.TableParametresDecimales + 
                " Set Quantites = " + this.Quantites +
                ", Prix = " + this.Prix;
            return DataBaseConnexion.addOrUpdateElementInDataBase(CommandText, Program.SelectGlobalMessages.ImpUpdateDouaneProduit);
        }

        
        public static ParametreDecimales getParametresDecimales()
        {
            ParametreDecimales parametresDecimales = null;
            if (DataBaseConnexion.getRowsCount(DAL.DataBaseTableName.TableParametresDecimales, "Prix") != 0)
            {
                OdbcConnection connection = DataBaseConnexion.getConnection();
                try
                {
                    OdbcCommand cmd = connection.CreateCommand();
                    cmd.CommandText = "select * from " + DataBaseTableName.TableParametresDecimales ;
                    OdbcDataReader Reader = cmd.ExecuteReader();
                    if (Reader.Read())
                    {
                        parametresDecimales = new ParametreDecimales(Reader.GetInt32(0), Reader.GetInt32(1));
                    }
                    Reader.Close();
                }
                catch (OdbcException e)
                {
                    MessageBox.Show(Program.SelectGlobalMessages.ImpSelectEntity + Program.SelectGlobalMessages.ParametresDecimalesEntiyName,
                        Program.SelectGlobalMessages.ManagemenEntity + Program.SelectGlobalMessages.ParametresDecimalesEntiyName,
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }               
            }
            return parametresDecimales;
        }

        public static String getFormatDecimaleForPrix()
        {
            String formatPrix = "0."; //  "0.##0";
            int nombreDecimalesForPrix = ParametreDecimales.getParametresDecimales().Prix;
            for (int i = 0; i < nombreDecimalesForPrix-1; i++)
                formatPrix += "#";

                return formatPrix;
        }

        public static String getFormatDecimaleForQuantites()
        {
            String formatQte = "0."; //  "0.##0";
            int nombreDecimalesForQuantites = ParametreDecimales.getParametresDecimales().Quantites;
            for (int i = 0; i < nombreDecimalesForQuantites - 1; i++)
                formatQte += "#";

            return formatQte;
        }
    }
    }
