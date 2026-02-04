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
    public partial class BonLivraison
    {
        public static string Separateur = CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator;

        //les attributs d'un bon de livraison: 
             
        public string  code_bonlivraison;
        public string codeclient_bonlivraison;
        public String date_bonlivraison;
        public String dateliv_bonlivraison;
        public String adresse_bonlivraison;
        public String statut_bonlivraison;
        public string isFacture_bonlivraison;
        public double apayer_bonlivraison;
        public String notes_bonlivraison;        

        //les constructeurs:

        public BonLivraison(string    b_code)
        {
            code_bonlivraison = b_code;
        }

        public BonLivraison()
        {

        }

        public BonLivraison(string    _code, string _codeClient, String _date, String _dateLiv, String _adresse,
                            String _statut, string _isFacture_bonlivraison, double _apayer_bonlivraison, String _notes_bonlivraison)
        {
            this.code_bonlivraison = _code;
            this.codeclient_bonlivraison = _codeClient;
            this.date_bonlivraison = _date;
            this.dateliv_bonlivraison = _dateLiv;
            this.adresse_bonlivraison = _adresse;
            this.statut_bonlivraison = _statut;
            this.isFacture_bonlivraison = _isFacture_bonlivraison;
            this.apayer_bonlivraison = _apayer_bonlivraison;            
            this.notes_bonlivraison = _notes_bonlivraison;
        }
               
        //les méthodes:

        public Boolean ajouterBonLivraison()
        {
            string CommandText = "insert into  " + DAL.DataBaseTableName.TableBonLivraison +
                                      "  values ( '" +code_bonlivraison +"'"+
                                      ", '" + codeclient_bonlivraison + "' " +
                                      ", '" + date_bonlivraison + "' " +
                                      ", '" + dateliv_bonlivraison + "' " +
                                      ", '" + adresse_bonlivraison.ToString().Replace("'", "''") + "' " +
                                      ", '" + statut_bonlivraison + "' " +
                                      ", '" + isFacture_bonlivraison + "' " +                                      
                                      ",  " + apayer_bonlivraison.ToString().ToString().Replace(',', '.') +
                                      ", '" + notes_bonlivraison.ToString().Replace("'", "''") + "' " +
                                      ")";
            return DataBaseConnexion.addOrUpdateElementInDataBase(CommandText, Program.SelectGlobalMessages.ImpAddBonLivraison);
        }
               
        public Boolean modifierBonLivraison()
        {
            string CommandText = "update " + DAL.DataBaseTableName.TableBonLivraison +
                     "   set codeClient_bonLivraison ='" + this.codeclient_bonlivraison + "' " +
                     " , date_bonLivraison ='" + this.date_bonlivraison + "' " +
                     " , dateLiv_bonlivraison = '" + this.dateliv_bonlivraison + "' " +
                     " , adresse_bonlivraison = '" + this.adresse_bonlivraison.ToString().Replace("'", "''") + "' " +
                     " , statut_bonLivraison='" + this.statut_bonlivraison + "' " +
                     " , isFacture_bonlivraison ='" + isFacture_bonlivraison + "' " +
                     " , apayer_bonlivraison =" + apayer_bonlivraison.ToString().ToString().Replace(',', '.') +                     
                     " , notes_bonLivraison ='" + this.notes_bonlivraison.ToString().Replace("'", "''") + "' " +
                     "   where code_bonLivraison='"+ code_bonlivraison +"';";
            return DataBaseConnexion.addOrUpdateElementInDataBase(CommandText, Program.SelectGlobalMessages.ImpUpdateBonLivraison);
        }
        public static Boolean updateApayerBonLivraison(string  _codeBL, double _apayer)
        {
            string CommandText = "update " + DAL.DataBaseTableName.TableBonLivraison +
                     " set apayer_bonlivraison = " + _apayer.ToString().ToString().Replace(',', '.') +
                     " where code_bonLivraison = '" +_codeBL+"'";
            return DataBaseConnexion.addOrUpdateElementInDataBase(CommandText, Program.SelectGlobalMessages.ImpUpdateBonLivraison);
        }
        public static Boolean updateStatutLivraisonOfBonLivraison(string  _codeBL, String _newStatut)
        {
            string CommandText = "update " + DAL.DataBaseTableName.TableBonLivraison +
                     " set statut_bonlivraison='" + _newStatut + "' " +
                     " where code_bonLivraison='" + _codeBL + "';";
            return DataBaseConnexion.addOrUpdateElementInDataBase(CommandText, Program.SelectGlobalMessages.ImpUpdateBonLivraison);
        }
        public static Boolean updateStatutFacturationOfBonLivraison(string  _codeBL, String _newstat)
        {
            string CommandText = "update " + DAL.DataBaseTableName.TableBonLivraison +
                     " set isFacture_bonlivraison = '" + _newstat + "' " +
                     " where code_bonLivraison = '" + _codeBL + "';";
            return DataBaseConnexion.addOrUpdateElementInDataBase(CommandText, Program.SelectGlobalMessages.ImpUpdateBonLivraison);
        }

        public static Boolean supprimerBonLivraison(string  _code_bonlivraison)
        {
            Boolean executed = false;
            if (LigneBonLivraison.SupprimerAllLigneBonLivraison(_code_bonlivraison) == true)
            {
                string CommandText = "delete from " + DAL.DataBaseTableName.TableBonLivraison +
                    " where code_bonLivraison='" + _code_bonlivraison + "';";
          
                executed= DataBaseConnexion.addOrUpdateElementInDataBase(CommandText, Program.SelectGlobalMessages.ImpDeleteBonLivraison);
            }
            return executed;
        }
                
        public static BonLivraison getBonLivraison(string  _codeBonLivraison)
        {
            BonLivraison bonLivraison = null;
            if (DataBaseConnexion.getMaxNumberOfStringColumn(DAL.DataBaseTableName.TableBonLivraison, "code_bonLivraison") != 0)
            {
                OdbcConnection connection = DAL.DataBaseConnexion.getConnection();
                //try
                //{
                    OdbcCommand cmd = connection.CreateCommand();

                    cmd.CommandText = "select * from  " + DAL.DataBaseTableName.TableBonLivraison +
                        " where code_bonLivraison= '" + _codeBonLivraison+"'";
                    OdbcDataReader Reader = cmd.ExecuteReader();

                    if (Reader.Read())
                    {
                        bonLivraison = new BonLivraison(Reader.GetString(0),
                            Reader.GetString(1),
                            Reader.GetString(2), Reader.GetString(3),
                            Reader.GetString(4), Reader.GetString(5),
                            Reader.GetString(6), Reader.GetDouble(7), Reader.GetString(8));
                   }
                    Reader.Close();
                //}
                //catch (Exception)
                //{
                //    MessageBox.Show(Program.SelectGlobalMessages.ImpSelectBonLivraison,
                //         Program.SelectGlobalMessages.SelectBonLivraison, MessageBoxButtons.OK, MessageBoxIcon.Error);
                //}
            }
            return bonLivraison;
        }             
            
        public static Boolean isBLFacture(string    _codebl)
        {
            Boolean isFacture = false;

            OdbcConnection connection = DAL.DataBaseConnexion.getConnection();
            try
            {
                OdbcCommand cmd = connection.CreateCommand();
                cmd.CommandText = "select isFacture_bonlivraison from " +  DAL.DataBaseTableName.TableBonLivraison +
                                  " where code_bonlivraison = '" + _codebl+"'";

                OdbcDataReader reader = cmd.ExecuteReader();

                if (reader.Read() == true)
                {
                    if (reader.GetString(0) == DAL.VariablesGlobales.EntityFacture)
                        isFacture = true ;
                }
                 reader.Close();
            }

            catch (OdbcException e)
            {
                MessageBox.Show(e.Message, Program.SelectGlobalMessages.SelectBonLivraison,
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            return isFacture;
        }

        public static ArrayList getALLBonLivraison()
        {
            ArrayList tab_lignesBL = new ArrayList();
            BonLivraison lignebl = null;
            OdbcConnection connection = DAL.DataBaseConnexion.getConnection();

            try
            {
                OdbcCommand cmd = connection.CreateCommand();
                cmd.CommandText = "select * from  " + DAL.DataBaseTableName.TableBonLivraison + 
                                    " order by Format(date_bonlivraison, 'mm/dd/yyyy') ";
                OdbcDataReader Reader = cmd.ExecuteReader();
                while (Reader.Read())
                {
                    lignebl = new BonLivraison(Reader.GetString(0),
                            Reader.GetString(1),
                            Reader.GetString(2), Reader.GetString(3),
                            Reader.GetString(4), Reader.GetString(5),
                            Reader.GetString(6), Reader.GetDouble(7), Reader.GetString(8));

                    tab_lignesBL.Add(lignebl);
                }
                Reader.Close();
                if (tab_lignesBL.Count == 0)
                    return null;
                else
                    return tab_lignesBL;
            }
            catch (OdbcException e)
            {
                MessageBox.Show(e.Message, Program.SelectGlobalMessages.SelectLigneBonLivraison,
                    MessageBoxButtons.OK, MessageBoxIcon.Error);

                return null;
            }
        }

        public static ArrayList getALLBonLivraisonByClient(string _codeclient_bonlivraison)
        {
            ArrayList tab_lignesBL = new ArrayList();
            BonLivraison lignebl = null;
            OdbcConnection connection = DAL.DataBaseConnexion.getConnection();

            try
            {
                OdbcCommand cmd = connection.CreateCommand();
                cmd.CommandText = "select * from  " + DAL.DataBaseTableName.TableBonLivraison +
                                    " where codeclient_bonlivraison = '" + _codeclient_bonlivraison + "'" +
                                    " order by Format(date_bonlivraison, 'mm/dd/yyyy')  ";
                OdbcDataReader Reader = cmd.ExecuteReader();
                while (Reader.Read())
                {
                    lignebl = new BonLivraison(Reader.GetString(0),
                            Reader.GetString(1),
                            Reader.GetString(2), Reader.GetString(3),
                            Reader.GetString(4), Reader.GetString(5),
                            Reader.GetString(6), Reader.GetDouble(7), Reader.GetString(8));

                    tab_lignesBL.Add(lignebl);
                }
                Reader.Close();
                if (tab_lignesBL.Count == 0)
                    return null;
                else
                    return tab_lignesBL;
            }
            catch (OdbcException e)
            {
                MessageBox.Show(e.Message, Program.SelectGlobalMessages.SelectBonLivraison,
                    MessageBoxButtons.OK, MessageBoxIcon.Error);

                return null;

            }
        }

        public static ArrayList getALLBonLivraisonNonFacture()
        {
            ArrayList tab_lignesBL = new ArrayList();
            BonLivraison lignebl = null;
            OdbcConnection connection = DAL.DataBaseConnexion.getConnection();
            
            try
            {
                OdbcCommand cmd = connection.CreateCommand();
                cmd.CommandText = "select * from  " + DAL.DataBaseTableName.TableBonLivraison +
                                    " where isFacture_bonlivraison = '" + DAL.VariablesGlobales.EntityFacture + "' " + 
                                    " order by Format(date_bonlivraison, 'mm/dd/yyyy') ";
                OdbcDataReader Reader = cmd.ExecuteReader();
                while (Reader.Read())
                {
                    lignebl = new BonLivraison(Reader.GetString(0),
                            Reader.GetString(1),
                            Reader.GetString(2), Reader.GetString(3),
                            Reader.GetString(4), Reader.GetString(5),
                            Reader.GetString(6), Reader.GetDouble(7), Reader.GetString(8));

                    tab_lignesBL.Add(lignebl);
                }
                Reader.Close();
                if (tab_lignesBL.Count == 0)
                    return null;
                else
                    return tab_lignesBL;
            }
            catch (OdbcException e)
            {
                MessageBox.Show(e.Message, Program.SelectGlobalMessages.SelectLigneBonLivraison,
                    MessageBoxButtons.OK, MessageBoxIcon.Error);

                return null;
            }
        }

        public static ArrayList getALLBonLivraisonNonFactureByClient(string _codeClt)
        {
            ArrayList tab_lignesBL = new ArrayList();
            BonLivraison unBL = null;
            OdbcConnection connection = DAL.DataBaseConnexion.getConnection();            
            try
            {
                OdbcCommand cmd = connection.CreateCommand();
                cmd.CommandText = "select * from  " + DAL.DataBaseTableName.TableBonLivraison +
                                    " where codeclient_bonlivraison='" + _codeClt + "'" +
                                    " and isFacture_bonlivraison = '" + DAL.VariablesGlobales.EntityNonFacture + "'" +
                                    " order by Format(date_bonlivraison, 'mm/dd/yyyy')   ";
                OdbcDataReader Reader = cmd.ExecuteReader();
                while (Reader.Read())
                {
                    unBL = new BonLivraison(Reader.GetString(0),
                            Reader.GetString(1),
                            Reader.GetString(2), Reader.GetString(3),
                            Reader.GetString(4), Reader.GetString(5),
                            Reader.GetString(6), Reader.GetDouble(7), Reader.GetString(8));

                    tab_lignesBL.Add(unBL);
                }
                Reader.Close();
                if (tab_lignesBL.Count == 0)
                    return null;
                else
                    return tab_lignesBL;
            }
            catch (OdbcException e)
            {
                MessageBox.Show(e.Message, Program.SelectGlobalMessages.SelectLigneBonLivraison,
                    MessageBoxButtons.OK, MessageBoxIcon.Error);

                return null;
            }
        }
                
       
        public static ArrayList getListOfBonLivraisonLivreByClient(string _code_Client)
        {
            ArrayList listBonLivraison = new ArrayList();
            BonLivraison BL = new BonLivraison();
            OdbcConnection connection = DAL.DataBaseConnexion.getConnection();

            try
            {
                OdbcCommand cmd = connection.CreateCommand();


                cmd.CommandText = "select * from  " + DAL.DataBaseTableName.TableBonLivraison +
                       " where codeclient_bonlivraison = '" + _code_Client + "' " +
                       " AND statut_bonlivraison = '" + DAL.VariablesGlobales.EntityLivre + "';";

                OdbcDataReader Reader = cmd.ExecuteReader();


                while (Reader.Read())
                {
                    BL = new BonLivraison(Reader.GetString(0),
                            Reader.GetString(1),
                            Reader.GetString(2), Reader.GetString(3),
                            Reader.GetString(4), Reader.GetString(5),
                            Reader.GetString(6), Reader.GetDouble(7), Reader.GetString(8));
                    listBonLivraison.Add(BL);
                }
                Reader.Close();

            }
            catch (OdbcException e)
            {
                MessageBox.Show(e.Message, Program.SelectGlobalMessages.SelectClient,
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            //catch
            //{
            //    MessageBox.Show(Program.SelectGlobalMessages.ImpSelectClient,
            //          Program.SelectGlobalMessages.SelectClient, MessageBoxButtons.OK, MessageBoxIcon.Error);
            //}

            if (listBonLivraison.Count == 0)
                return null;
            else

                return listBonLivraison;
        }


        public static ArrayList getdateBonsLivraisonByFactureClient(string  _code_bonlivraison)
        {
            ArrayList tab_lignesBL = new ArrayList();
            string dateBl;
            OdbcConnection connection = DAL.DataBaseConnexion.getConnection();
            try
            {
                OdbcCommand cmd = connection.CreateCommand();
                cmd.CommandText = "select distinct date_bonlivraison from " + DAL.DataBaseTableName.TableBonLivraison +"," +
                                    DAL.DataBaseTableName.TableLigneFactureClient +
                                    " where codebonlivraison_lignefactureclient = '" + _code_bonlivraison + "'";

                OdbcDataReader Reader = cmd.ExecuteReader();
                while (Reader.Read())
                {
                    dateBl = Reader.GetString(0);
                    if (dateBl != "")
                    {
                        tab_lignesBL.Add(dateBl);
                    }
                }
                Reader.Close();
                if (tab_lignesBL.Count == 0)
                    return null;
                else
                    return tab_lignesBL;
            }
            catch (OdbcException e)
            {
                MessageBox.Show(e.Message, Program.SelectGlobalMessages.SelectLigneBonLivraison,
                    MessageBoxButtons.OK, MessageBoxIcon.Error);

                return null;
            }
        }

        public static Boolean supprimerALLBonLivraison()
        {
            Boolean executed = false;
            if (LigneBonLivraison.SupprimerAllLigneBonLivraison() == true)
            {
                string CommandText = "delete from " + DAL.DataBaseTableName.TableBonLivraison;

                executed = DataBaseConnexion.addOrUpdateElementInDataBase(CommandText, Program.SelectGlobalMessages.ImpDeleteBonLivraison);
            }
            return executed;
        }

    }

}