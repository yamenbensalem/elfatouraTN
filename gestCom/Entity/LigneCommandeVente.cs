using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Windows.Forms;
using System.Data;
using T4C_Commercial_Project.DAL;
using System.Collections;
using System.Globalization;
using System.Data.Odbc;

namespace T4C_Commercial_Project.Entity
{
    class LigneCommandeVente
    {
        public static string Separateur = CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator;
        
        // Les attributs :
        public int numero_lignecommandevente;
        public int code_commandevente;
        public string codeproduit_lignecommandevente;
        public string designationproduit_lignecommandevente;
        public double quantite_lignecommandevente;
        public double prixunitaire_lignecommandevente;
        public double montantHT_lignecommandevente;
        public double tvaproduit_lignecommandevente;
        public double remise_lignecommandevente;      

        // Les constructeurs :        
        public LigneCommandeVente(int _numeroLigne, int _codeCommandeVente, string _codeproduit, string _designationProduit, 
                                double _quantite, double _prixunitaire, double _montantHT, double _tvaproduit, double _remise)
        {
            numero_lignecommandevente = _numeroLigne;
            code_commandevente = _codeCommandeVente;
            codeproduit_lignecommandevente = _codeproduit;
            designationproduit_lignecommandevente = _designationProduit;
            quantite_lignecommandevente = _quantite;
            prixunitaire_lignecommandevente = _prixunitaire;
            montantHT_lignecommandevente = _montantHT;
            tvaproduit_lignecommandevente = _tvaproduit;
            remise_lignecommandevente = _remise;            
        }

        public LigneCommandeVente(int _codeCommandeVente, string _codeproduit)
        {
            code_commandevente = _codeCommandeVente;
            codeproduit_lignecommandevente = _codeproduit;
        }

        public LigneCommandeVente(int _codeCommandeVente)
        {
            code_commandevente = _codeCommandeVente;
        }

        public LigneCommandeVente()
        {

        }

        // Les méthodes :
        public bool ajouterLigneCommandeVente()
        {
            string CommandText = "insert into " + DAL.DataBaseTableName.TableLigneCommandeVente + " values( " +
                    code_commandevente + "," +
                    "'" + codeproduit_lignecommandevente + "', " + 
                    "'" + this.designationproduit_lignecommandevente.ToString().Replace("'", "''") +  "', " +
                    quantite_lignecommandevente.ToString().ToString().Replace(',', '.') + "," +
                    prixunitaire_lignecommandevente.ToString().ToString().Replace(',', '.') + "," +
                    montantHT_lignecommandevente.ToString().ToString().Replace(',', '.') + "," +
                    tvaproduit_lignecommandevente.ToString().ToString().Replace(',', '.') + "," +
                    remise_lignecommandevente.ToString().ToString().Replace(',', '.') +
                    ")";
            return DataBaseConnexion.addOrUpdateElementInDataBase(CommandText, Program.SelectGlobalMessages.ImpDeleteLigneCommandeVente);
        }

        public bool modifierLigneCommandeVente()
        {
            string CommandText = "update " + DAL.DataBaseTableName.TableLigneCommandeVente + " Set " +
                    " designationproduit_lignecommandevente ='" + this.designationproduit_lignecommandevente.ToString().Replace("'", "''") + "'" +
                    ", quantite_lignecommandevente =" + this.quantite_lignecommandevente.ToString().ToString().Replace(',', '.') +
                    ", prixunitaire_lignecommandevente=" + this.prixunitaire_lignecommandevente.ToString().ToString().Replace(',', '.') +
                    ", montantHT_lignecommandevente=" + this.montantHT_lignecommandevente.ToString().ToString().Replace(',', '.') +
                    ", tvaproduit_lignecommandevente=" + this.tvaproduit_lignecommandevente.ToString().ToString().Replace(',', '.') +
                    ", remise_lignecommandevente =" + this.remise_lignecommandevente.ToString().ToString().Replace(',', '.') +
                    " WHERE  code_commandevente = " + this.code_commandevente +
                    " AND codeproduit_lignecommandevente = '" + this.codeproduit_lignecommandevente + "';";
            return DataBaseConnexion.addOrUpdateElementInDataBase(CommandText, Program.SelectGlobalMessages.ImpUpdateLigneCommandeVente);
        }
                
        public static bool supprimerLigneCommandeVente(int _codeCommandeVente, string _codeProduit)
        {
            string CommandText = "delete from " + DAL.DataBaseTableName.TableLigneCommandeVente +
                       " where code_commandevente = " + _codeCommandeVente +
                       " and codeproduit_lignecommandevente= '" + _codeProduit + "' ";
            return DataBaseConnexion.addOrUpdateElementInDataBase(CommandText, Program.SelectGlobalMessages.ImpDeleteLigneCommandeVente);
        }

        public static bool supprimerLigneCommandeVente(int _codeCommandeVente)
        {
            string CommandText = "delete from " + DAL.DataBaseTableName.TableLigneCommandeVente +
                                    " where code_commandevente=" + _codeCommandeVente ;
            return DataBaseConnexion.addOrUpdateElementInDataBase(CommandText, Program.SelectGlobalMessages.ImpDeleteLigneCommandeVente);
        }

        public static LigneCommandeVente getLigneCommandeVente(int _codeCommandeVente, String _codeProduit)
        {            
            LigneCommandeVente lignecmdVente = null;
            OdbcConnection connection = DAL.DataBaseConnexion.getConnection();
            try
            {
                OdbcCommand cmd = connection.CreateCommand();

                cmd.CommandText = " select * from  " + DAL.DataBaseTableName.TableLigneCommandeVente +
                                  " where code_commandevente=" + _codeCommandeVente + 
                                  " and codeproduit_lignecommandevente= '" + _codeProduit + "';";
                OdbcDataReader Reader = cmd.ExecuteReader();
                if (Reader.Read())
                {
                    lignecmdVente = new LigneCommandeVente(Reader.GetInt32(0), Reader.GetInt32(1), Reader.GetString(2), Reader.GetString(3),
                                    Reader.GetDouble(4), Reader.GetDouble(5), Reader.GetDouble(6), Reader.GetDouble(7), Reader.GetDouble(8));
                }
                Reader.Close();
            }
            //catch (OdbcException e)
            //{
            //    MessageBox.Show(e.Message, Program.SelectGlobalMessages.SelectLigneCommandeVente,
            //        MessageBoxButtons.OK, MessageBoxIcon.Error);

            //}
            catch
            {
                MessageBox.Show(Program.SelectGlobalMessages.ImpSelectLigneCommandeVente,
                      Program.SelectGlobalMessages.SelectLigneCommandeVente, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            return lignecmdVente;
        }
 
        public static ArrayList getAllLignesCommandeVente(int _codeCommandeVente)
        {

            ArrayList lignescmdsVente = new ArrayList();
            LigneCommandeVente lignecmdVente = null;
            OdbcConnection connection = DAL.DataBaseConnexion.getConnection();
            try
            {
                OdbcCommand cmd = connection.CreateCommand();

                cmd.CommandText = "select * from  " + DAL.DataBaseTableName.TableLigneCommandeVente +
                       " where code_commandevente=" + _codeCommandeVente;

                OdbcDataReader Reader = cmd.ExecuteReader();


                while (Reader.Read())
                {
                    lignecmdVente = new LigneCommandeVente(Reader.GetInt32(0), Reader.GetInt32(1), Reader.GetString(2), Reader.GetString(3),
                                    Reader.GetDouble(4), Reader.GetDouble(5), Reader.GetDouble(6), Reader.GetDouble(7), Reader.GetDouble(8));
                    lignescmdsVente.Add(lignecmdVente);
                }
                Reader.Close();
            }
            //catch (OdbcException e)
            //{
            //    MessageBox.Show(e.Message, Program.SelectGlobalMessages.SelectLigneCommandeVente,
            //        MessageBoxButtons.OK, MessageBoxIcon.Error);

            //}
            catch
            {
                MessageBox.Show(Program.SelectGlobalMessages.ImpSelectLigneCommandeVente,
                      Program.SelectGlobalMessages.SelectLigneCommandeVente, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            if (lignescmdsVente.Count == 0)
                return null;
            else
                return lignescmdsVente;
        }

        public static bool isExistedLigneCommandeVente(int _codeCommandeVente, string _codeProduit, ref bool isException)
        {
            bool isExisted=false;
            isException = false;
            OdbcConnection connection = DAL.DataBaseConnexion.getConnection();
            try
            {
                OdbcCommand cmd = connection.CreateCommand();
                cmd.CommandText = "select * from " + DAL.DataBaseTableName.TableLigneCommandeVente +
                                  " where code_commandevente=" + _codeCommandeVente +
                                  " and codeproduit_lignecommandevente ='" + _codeProduit + "';";
                OdbcDataReader Reader = cmd.ExecuteReader();
                if (Reader.Read())
                {
                    isExisted = true;
                }
                Reader.Close();
                return isExisted;
            }
            catch (OdbcException e)
            {
                MessageBox.Show(e.Message, Program.SelectGlobalMessages.SelectCommandeVente,
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return isException = true;
            }
            catch
            {
                MessageBox.Show(Program.SelectGlobalMessages.ImpSelectLigneCommandeVente,
                     Program.SelectGlobalMessages.SelectCommandeVente, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return isException = true;
            }
        }

        public static Boolean SupprimerAllLigneFromCMD(int _codeCommandeVente)
        {
            string CommandText1 = " delete from " + DAL.DataBaseTableName.TableLigneCommandeVente +
                                  " where code_commandevente=" + _codeCommandeVente ;
            return DataBaseConnexion.addOrUpdateElementInDataBase(CommandText1, Program.SelectGlobalMessages.ImpDeleteCommandeVente);
        }
    }

}
