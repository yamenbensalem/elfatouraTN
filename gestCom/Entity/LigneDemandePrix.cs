using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Windows.Forms;
using T4C_Commercial_Project.DAL;
using System.Globalization;
using System.Data.Odbc;
using System.Collections;


namespace T4C_Commercial_Project.Entity
{
    class LigneDemandePrix
    {
        public static string Separateur = CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator;

        public int numero_lignedemandeprix;
        public string numero_demandeprix;
        public string codeproduit_lignedemandeprix;
        public string designationproduit_lignedemandeprix;
        public double quantite_lignedemandeprix;
        public string unite_lignedemandeprix;        

        public LigneDemandePrix(int _numeroLigne, string _numeroDemande, string _codeProduit, double _quantite, 
                                string _unite, string _designation_prod)
        {
            numero_lignedemandeprix = _numeroLigne;
            numero_demandeprix = _numeroDemande;
            codeproduit_lignedemandeprix = _codeProduit;
            designationproduit_lignedemandeprix = _designation_prod;
            quantite_lignedemandeprix = _quantite;
            unite_lignedemandeprix = _unite;
        }

        public LigneDemandePrix(int _numeroLigne, string _codeProduit)
        {
            numero_lignedemandeprix = _numeroLigne;
            codeproduit_lignedemandeprix = _codeProduit;
        }

        public LigneDemandePrix()
        {

        }

        public Boolean ajouterLigneDemandePrix()
        {
            string CommandText = "insert into " + DAL.DataBaseTableName.TableLigneDemandePrix + " values (" +
                        this.numero_lignedemandeprix +        
                       "'" + this.numero_demandeprix + "', " +
                       "'" + this.codeproduit_lignedemandeprix + "' , " +
                       "'" + this.designationproduit_lignedemandeprix.ToString().Replace("'", "''") + ", " +
                             this.quantite_lignedemandeprix.ToString().ToString().Replace(',', '.') + ", " +
                       "'" + this.unite_lignedemandeprix + "' " +
                       ");";
            return DataBaseConnexion.addOrUpdateElementInDataBase(CommandText, Program.SelectGlobalMessages.ImpAddLigneDemandePrix);
        }

        public Boolean modifierLigneDemandePrix()
        {
            string CommandText = "UPDATE  " + DAL.DataBaseTableName.TableLigneDemandePrix + " set " +
                " designation_prod= '" + this.designationproduit_lignedemandeprix.ToString().Replace("'", "''") + "' , " +      
                " quantite_lignedemandeprix = " + this.quantite_lignedemandeprix.ToString().ToString().Replace(',', '.') + " , " +
                " unite_lignedemandeprix = '" + this.unite_lignedemandeprix + "', " +
                " WHERE numero_demandeprix = '" + this.numero_demandeprix + "'" +
                " AND codeproduit_lignedemandeprix = '" + this.codeproduit_lignedemandeprix + "';";
            return DataBaseConnexion.addOrUpdateElementInDataBase(CommandText, Program.SelectGlobalMessages.ImpUpdateLigneDemandePrix);
        }

        public static Boolean supprimerLigneDemandePrix(string _numero_demandeprix, string _codeproduit_lignedemandeprix)
        {
            string CommandText = "DELETE FROM " + DAL.DataBaseTableName.TableLigneDemandePrix +
                       " WHERE numero_demandeprix = '" + _numero_demandeprix + "'" + 
                       " AND codeproduit_lignedemandeprix = '" + _codeproduit_lignedemandeprix + "';";
            return DataBaseConnexion.addOrUpdateElementInDataBase(CommandText, Program.SelectGlobalMessages.ImpDeleteLigneDemandePrix);
        }

        public static Boolean supprimerALLLignesDemandePrix(string _numeroDemandePrix)
        {
            string CommandText = "DELETE FROM " + DAL.DataBaseTableName.TableLigneDemandePrix +
                       " WHERE numero_demandeprix = '" + _numeroDemandePrix + "'";
            return DataBaseConnexion.addOrUpdateElementInDataBase(CommandText, Program.SelectGlobalMessages.ImpDeleteLigneDemandePrix);
        }

        public static bool isExistedLigneDemandePrix(string _numero_demandeprix, string _codeProduit, ref bool isException)
        {
            bool isExist = false;
            isException = false;
            OdbcConnection connection = DAL.DataBaseConnexion.getConnection();
            try
            {
                OdbcCommand cmd = connection.CreateCommand();
                cmd.CommandText = "select * from " + DAL.DataBaseTableName.TableLigneDemandePrix +
                        " where numero_demandeprix = '" + _numero_demandeprix + "'" + 
                        " and codeproduit_lignedemandeprix ='" + _codeProduit + "';";

                OdbcDataReader Reader = cmd.ExecuteReader();
                if (Reader.Read())
                {
                    isExist = true;

                }
                Reader.Close();

                return isExist;
            }
            catch (OdbcException e)
            {
                MessageBox.Show(e.Message, Program.SelectGlobalMessages.SelectDemandePrix,
                    MessageBoxButtons.OK, MessageBoxIcon.Error);

                return isException = true;
            }
            catch
            {
                MessageBox.Show(Program.SelectGlobalMessages.ImpSelectLigneDemandePrix,
                     Program.SelectGlobalMessages.SelectDevis, MessageBoxButtons.OK, MessageBoxIcon.Error);

                return isException = true;
            }
        }

        public static LigneDemandePrix getLigneDemandePrix(string _numeroDemandePrix, string _codeProduit)
        {
            LigneDemandePrix ligneDevis = null;
            OdbcConnection connection = DAL.DataBaseConnexion.getConnection();

            try
            {
                OdbcCommand cmd = connection.CreateCommand();
                cmd.CommandText = "select * from " + DAL.DataBaseTableName.TableLigneDemandePrix +
                        " where numero_demandeprix = '" + _numeroDemandePrix + "'" + 
                        " and codeproduit_lignedemandeprix ='" + _codeProduit + "';";
                OdbcDataReader Reader = cmd.ExecuteReader();
                if (Reader.Read())
                {
                    ligneDevis = new LigneDemandePrix(Reader.GetInt32(0), Reader.GetString(1), Reader.GetString(2), Reader.GetDouble(3), 
                                                        Reader.GetString(4), Reader.GetString(5));
                }
                else
                    throw new Exception();

                Reader.Close();
            }
            catch (OdbcException e)
            {
                MessageBox.Show(e.Message, Program.SelectGlobalMessages.SelectLigneDemandePrix,
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch
            {
                MessageBox.Show(Program.SelectGlobalMessages.ImpSelectLigneDemandePrix,
                     Program.SelectGlobalMessages.SelectLigneDemandePrix, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            return ligneDevis;
        }
        
        public static ArrayList getALLLignesDemandePrix(int _numeroDemandePrix)
        {
            ArrayList tab_lignesDemandePrix = new ArrayList();
            LigneDemandePrix ligneDemandePrix = null;
            OdbcConnection connection = DAL.DataBaseConnexion.getConnection();

            try
            {
                OdbcCommand cmd = connection.CreateCommand();
                cmd.CommandText = "select * from  " + DAL.DataBaseTableName.TableLigneDemandePrix +
                       " where numero_demandeprix =" + _numeroDemandePrix + ";";

                OdbcDataReader Reader = cmd.ExecuteReader();


                while (Reader.Read())
                {
                    ligneDemandePrix = new LigneDemandePrix(Reader.GetInt32(0), Reader.GetString(1), Reader.GetString(2),
                                                Reader.GetDouble(3), Reader.GetString(4), Reader.GetString(5)
                                                );
                    tab_lignesDemandePrix.Add(ligneDemandePrix);
                }
                Reader.Close();
            }
            catch (OdbcException e)
            {
                MessageBox.Show(e.Message, Program.SelectGlobalMessages.SelectDemandePrix,
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            return tab_lignesDemandePrix;
        }
    }
}
