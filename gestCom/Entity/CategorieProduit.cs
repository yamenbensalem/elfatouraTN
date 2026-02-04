using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Data;
using T4C_Commercial_Project.DAL;
using System.Globalization;
using System.Data.Odbc;

namespace T4C_Commercial_Project.Entity
{
    class CategorieProduit
    {
        public static string Separateur = CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator;
            
        public int code_categorieproduit;
        public  string designation_categorieproduit;

        // Constructeurs :
        public CategorieProduit()
        {   }
               
        public CategorieProduit(int  _code_categorie, string _designation_categorie)
        {
            this.code_categorieproduit = _code_categorie;
            this.designation_categorieproduit = _designation_categorie;
        }
        
        // Méthodes :
        public Boolean ajouterCategorieProduit()
        {
           string CommandText = "insert into " + DataBaseTableName.TableCategorieProduit +
                    " values(" +   this.code_categorieproduit + ",'"+ this.designation_categorieproduit.ToString().Replace("'", "''") + "');";
                return DataBaseConnexion.addOrUpdateElementInDataBase(CommandText,Program.SelectGlobalMessages.ImpAddCategorieProduit);
        }
                
        public Boolean modifierCategorieProduit()
        {            
            string CommandText = "Update " +  DataBaseTableName.TableCategorieProduit +
                    " Set designation_categorieproduit = '" + this.designation_categorieproduit.ToString().Replace("'", "''") + "' " +
                    " Where code_categorieproduit = " + this.code_categorieproduit;
                    return DataBaseConnexion.addOrUpdateElementInDataBase(CommandText,Program.SelectGlobalMessages.ImpUpdateCategorieProduit);
        }

        public static Boolean supprimerCategorieProduit(int _code_categorie)
        {
             string CommandText = "Delete from " +  DataBaseTableName.TableCategorieProduit +
                    " Where code_categorieproduit =" + _code_categorie;
                   return DataBaseConnexion.addOrUpdateElementInDataBase(CommandText,Program.SelectGlobalMessages.ImpDeleteCategorieProduit);
        }

        public static CategorieProduit getCategorieProduitByCode(int _code_categorie)
        {
            CategorieProduit categorieProduit = null;
            if (DataBaseConnexion.getRowsCount(DataBaseTableName.TableCategorieProduit, "code_categorieproduit") != 0)
            {
                OdbcConnection connection = DataBaseConnexion.getConnection();
                try
                {
                    OdbcCommand cmd = connection.CreateCommand();
                    cmd.CommandText = "Select * from " + DataBaseTableName.TableCategorieProduit +
                        " Where code_categorieproduit=" + _code_categorie;
                    OdbcDataReader Reader = cmd.ExecuteReader();
                    if (Reader.Read())
                    {
                        categorieProduit = new CategorieProduit(Reader.GetInt32(0), Reader.GetString(1));
                    }
                    Reader.Close();
                }
                catch (OdbcException e)
                {
                    MessageBox.Show(e.Message, Program.SelectGlobalMessages.SelectCategorie,
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    
                }                
            }
            return categorieProduit;
        }
        
        public static CategorieProduit getCategorieProduitByDesignation(String _designation_categorie)
        {
            CategorieProduit categorieProduit = null;
            if (DataBaseConnexion.getRowsCount(DataBaseTableName.TableCategorieProduit, "code_categorieproduit") != 0)
            {
                OdbcConnection connection = DataBaseConnexion.getConnection();
                try
                {
                    OdbcCommand cmd = connection.CreateCommand();
                    cmd.CommandText = "Select * from " + DataBaseTableName.TableCategorieProduit +
                        " Where designation_categorieproduit = '" + _designation_categorie + "'";
                    OdbcDataReader Reader = cmd.ExecuteReader();
                    if (Reader.Read())
                    {
                        categorieProduit = new CategorieProduit(Reader.GetInt32(0), Reader.GetString(1));
                    }
                    Reader.Close();
                }
                catch (OdbcException e)
                {
                    MessageBox.Show(e.Message, Program.SelectGlobalMessages.SelectCategorie,
                        MessageBoxButtons.OK, MessageBoxIcon.Error);

                }
            }
            return categorieProduit;
        }
    }

}
