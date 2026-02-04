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
    public partial class UniteProduit
    {
        public static string Separateur = CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator;
        
        // Les attributs:
        public int code_uniteproduit;
        public String designation_uniteproduit;

        // Les constructeurs:
        public UniteProduit()
        { }

        public UniteProduit(int _codeUnite)
        {
            this.code_uniteproduit = _codeUnite;
        }

        public UniteProduit(int _codeUnite, String _designationUnite)
        {
            this.code_uniteproduit = _codeUnite;
            this.designation_uniteproduit = _designationUnite;
        }

        // Les méthodes d'instances :
        public Boolean ajouterUniteProduit()
        {
            string CommandText = "insert into " + DAL.DataBaseTableName.TableUniteProduit  +" values(" +
               this.code_uniteproduit + ",'" +
               this.designation_uniteproduit.ToString().Replace("'", "''") + "');";
            return DataBaseConnexion.addOrUpdateElementInDataBase(CommandText, Program.SelectGlobalMessages.ImpAddUniteProduit);
        }
                
        public Boolean modifierUniteProduit()
        {
            string CommandText = "update " + DAL.DataBaseTableName.TableUniteProduit + " set designation_uniteproduit ='" +
                  this.designation_uniteproduit.ToString().Replace("'", "''") + "' where code_uniteproduit =" + this.code_uniteproduit + ";";
            return DataBaseConnexion.addOrUpdateElementInDataBase(CommandText, Program.SelectGlobalMessages.ImpUpdateUniteProduit);
        }

        public static Boolean supprimerUniteProduit(int _codeUnite)
        {
            string CommandText = "delete from " + DAL.DataBaseTableName.TableUniteProduit +
                " where code_uniteproduit=" + _codeUnite + ";";
            return DataBaseConnexion.addOrUpdateElementInDataBase(CommandText, Program.SelectGlobalMessages.ImpDeleteUniteProduit);
        }
        
        // Les méthodes statiques:
        public static UniteProduit getUniteProduitByCode(int _code_uniteproduit)
        {
            UniteProduit uniteProduit = null;
            if (DataBaseConnexion.getRowsCount(DAL.DataBaseTableName.TableUniteProduit, "code_uniteproduit") != 0)
            {
                OdbcConnection connection = DataBaseConnexion.getConnection();
                //try
                //{
                    OdbcCommand cmd = connection.CreateCommand();
                    cmd.CommandText = "select * from " + DAL.DataBaseTableName.TableUniteProduit +
                                        " where code_uniteproduit=" + _code_uniteproduit + ";";
                    OdbcDataReader Reader = cmd.ExecuteReader();
                    if (Reader.Read())
                    {
                        uniteProduit = new UniteProduit(Reader.GetInt32(0), Reader.GetString(1));
                    }

                    Reader.Close();
                //}
                //catch (OdbcException e)
                //{
                //    MessageBox.Show(e.Message, Program.SelectGlobalMessages.SelectUnite,
                //        MessageBoxButtons.OK, MessageBoxIcon.Error);
                //}
                //catch (Exception)
                //{
                //    MessageBox.Show(Program.SelectGlobalMessages.ImpResearchUniteProduit,
                //      Program.SelectGlobalMessages.SelectUnite, MessageBoxButtons.OK, MessageBoxIcon.Error);
                //}
            }
            return uniteProduit;
        }

        public static UniteProduit getUniteProduitByDesignation(String _designation_uniteproduit)
        {
            UniteProduit uniteProduit = null;
            if (DataBaseConnexion.getRowsCount(DAL.DataBaseTableName.TableUniteProduit, "code_uniteproduit") != 0)
            {
                OdbcConnection connection = DataBaseConnexion.getConnection();
                //try
                //{
                OdbcCommand cmd = connection.CreateCommand();
                cmd.CommandText = "select * from " + DAL.DataBaseTableName.TableUniteProduit +
                                    " where designation_uniteproduit='" + _designation_uniteproduit + "';";
                OdbcDataReader Reader = cmd.ExecuteReader();
                if (Reader.Read())
                {
                    uniteProduit = new UniteProduit(Reader.GetInt32(0), Reader.GetString(1));
                }

                Reader.Close();
                //}
                //catch (OdbcException e)
                //{
                //    MessageBox.Show(e.Message, Program.SelectGlobalMessages.SelectUnite,
                //        MessageBoxButtons.OK, MessageBoxIcon.Error);
                //}
                //catch (Exception)
                //{
                //    MessageBox.Show(Program.SelectGlobalMessages.ImpResearchUniteProduit,
                //      Program.SelectGlobalMessages.SelectUnite, MessageBoxButtons.OK, MessageBoxIcon.Error);
                //}
            }
            return uniteProduit;
        }

 }
}
