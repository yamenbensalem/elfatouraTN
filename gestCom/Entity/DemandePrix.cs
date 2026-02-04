using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using System.Data;
using T4C_Commercial_Project.Entity;
using T4C_Commercial_Project.DAL;
using System.Collections;
using System.Globalization;
using System.Data.Odbc;
namespace T4C_Commercial_Project.Entity
{
    public class DemandePrix
    {
        public static string Separateur = CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator;
        
        //les attributs:
        public int numero_demandeprix;
        public int codefournisseur_demandeprix;
        public string date_demandeprix;
        public string statut_demandeprix;
        public string validite_demandeprix;
        public string notes_demandeprix;

        //les constructeurs:
        public DemandePrix(int v_numero, int v_codeFournisseur, string v_date, string v_statut, string v_validite, string v_notes)
        {
            numero_demandeprix = v_numero;
            codefournisseur_demandeprix = v_codeFournisseur;
            date_demandeprix = v_date;
            statut_demandeprix = v_statut;
            validite_demandeprix = v_validite;
            notes_demandeprix = v_notes;
        }

        public DemandePrix(int v_numero)
        {
            this.numero_demandeprix = v_numero;
        }

        public DemandePrix()
        {

        }

        //les methodes:
        public Boolean ajouterDemandePrix()
        {
            string CommandText = "insert into " + DAL.DataBaseTableName.TableDemandePrix +
                      " values ( " +
                      this.numero_demandeprix + "," +
                      this.codefournisseur_demandeprix + ", '" +
                      this.date_demandeprix + "' , '" +
                      this.statut_demandeprix + "' , '" +
                      this.validite_demandeprix + "' , '" +
                      this.notes_demandeprix.ToString().Replace("'", "''") + "' );";
            return DataBaseConnexion.addOrUpdateElementInDataBase(CommandText, Program.SelectGlobalMessages.ImpAddDemandePrix);
        }

        public Boolean modifierDemandePrix()
        {
            string CommandText = "update " + DAL.DataBaseTableName.TableDemandePrix +
                       " set codefournisseur_demandeprix = " + this.codefournisseur_demandeprix +
                        " , date_demandeprix = '" + this.date_demandeprix + "' " +
                        " , statut_demandeprix = '" + this.statut_demandeprix + "' " +
                        " , validite_demandeprix = '" + this.validite_demandeprix + "' " +
                        " , notes_demandeprix = '" + this.notes_demandeprix.ToString().Replace("'", "''") + "' " +
                        " where numero_demandeprix = " + numero_demandeprix + ";";
            return DataBaseConnexion.addOrUpdateElementInDataBase(CommandText, Program.SelectGlobalMessages.ImpUpdateDemandePrix);
        }

        public static Boolean supprimerDemandePrix(string _numero_demandeprix)
        {   
            LigneDemandePrix.supprimerALLLignesDemandePrix(_numero_demandeprix);
            String CommandText = "delete from " + DAL.DataBaseTableName.TableDemandePrix +
                " where numero_demandeprix = '" + _numero_demandeprix + "'";
            return DataBaseConnexion.addOrUpdateElementInDataBase(CommandText, Program.SelectGlobalMessages.ImpDeleteDemandePrix);
        }

        public static DemandePrix getDemandePrix(string _numero_demandeprix)
        {
            DemandePrix demandePrix = null;
            OdbcConnection connection = DAL.DataBaseConnexion.getConnection();
            try
            {
                OdbcCommand cmd = connection.CreateCommand();
                cmd.CommandText = "select * from  " + DAL.DataBaseTableName.TableDemandePrix +
                        " where numero_demandeprix = '" + _numero_demandeprix + "'";
                OdbcDataReader Reader = cmd.ExecuteReader();
                if (Reader.Read())
                {
                    demandePrix = new DemandePrix(Reader.GetInt32(0), Reader.GetInt32(1),
                         Reader.GetString(2), Reader.GetString(3), Reader.GetString(4), Reader.GetString(4));
                }
                Reader.Close();
            }
            catch (OdbcException e)
            {
                MessageBox.Show(e.Message, Program.SelectGlobalMessages.SelectDemandePrix,
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch
            {
                MessageBox.Show(Program.SelectGlobalMessages.ImpSelectDevis, Program.SelectGlobalMessages.SelectDemandePrix,
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            return demandePrix;
        }
     }
}
