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


namespace T4C_Commercial_Project.Forms
{
    public partial class ReglementFacture
    {

        public static string Separateur = CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator;


        //les attributs d'un Reglement:              
      //  public int code_reglement;
        public string code_facture;
        public double montant_reglement;
        public string date_reglement;
        public string echeance_reglement;
        public string mode_reglement;
        public int  identifiantmode_reglement;
        public string numcheque;
        public string agence;

        // constructeur 
        //les constructeurs:
        public ReglementFacture(int  _idmodeR)
        {
            identifiantmode_reglement = _idmodeR;
        }

        public ReglementFacture()
        {

        }

        public ReglementFacture(  int  _idmodeR,string _codeFact, double _montantR,
                            String _dateR, string _echeanceR, String _modeR,string _numcheque,string _agence)
        {
            this.identifiantmode_reglement = _idmodeR;
            this.code_facture = _codeFact;
            this.montant_reglement = _montantR;
            this.date_reglement = _dateR;
            this.echeance_reglement = _echeanceR;
            this.mode_reglement = _modeR;
            this.numcheque = _numcheque;
            this.agence = _agence;
        }


        /************************************************************************************/
        /************************************************************************************/
        //les methodes:
        public Boolean ajouterReglement()
        {
            string CommandText = "insert into  " + DAL.DataBaseTableName.TableReglementFactureClient +
                           "  values (" + this.identifiantmode_reglement.ToString().Replace("'", "''")  +
                           ", '" + code_facture + "'" +
                           ", " + montant_reglement.ToString().ToString().Replace(',', '.') +
                           ", '" + date_reglement +
                           "', '" + echeance_reglement + 
                           "', '" + mode_reglement + 
                           "', '" + numcheque + 
                           "', '" + agence + "' );";

            return DataBaseConnexion.addOrUpdateElementInDataBase(CommandText, Program.SelectGlobalMessages.ImpAddReglement);
        }


        /************************************************************************************/
        /************************************************************************************/
        public Boolean modifierReglementfacture()
        {
            string CommandText = "update " + DAL.DataBaseTableName.TableReglementFactureClient +
                       " set identifiantmode_reglement = " + this.identifiantmode_reglement +
                       " , code_facture = '" + this.code_facture + "'" + 
                       " , montant_reglement = " + this.montant_reglement.ToString().ToString().Replace(',', '.') +
                       " , date_reglement= '" + this.date_reglement + "' " +
                       " , echeance_reglement = '" + this.echeance_reglement + "' " +
                       ", mode_reglement = '" + mode_reglement + "'" +
                       ", numcheque = '" + numcheque + "'" +
                       ", agence = '" + agence + "'" +
                       " where identifiantmode_reglement= " + this.identifiantmode_reglement + ";";
            return DataBaseConnexion.addOrUpdateElementInDataBase(CommandText, Program.SelectGlobalMessages.ImpUpdateReglement);

        }
        /************************************************************************************/
        /************************************************************************************/
        public Boolean supprimerReglementFacture()
        {
            string CommandText = " delete from " + DAL.DataBaseTableName.TableReglementFactureClient +
                                 " where identifiantmode_reglement =" + this.identifiantmode_reglement + ";";
            return DataBaseConnexion.addOrUpdateElementInDataBase(CommandText, Program.SelectGlobalMessages.ImpDeleteReglement);

        }

        /************************************************************************************/
        /************************************************************************************/
        // lancer une requete qui retourne le BL avec le _codeBonLivraison
        public ReglementFacture getReglementfacture(int _codeReg)
        {
            ReglementFacture reglementFacture = null;
            if (DAL.DataBaseConnexion.getRowsCount(DAL.DataBaseTableName.TableReglementFactureClient, "identifiantmode_reglement") != 0)
            {
                OdbcConnection connection = DAL.DataBaseConnexion.getConnection();
                try
                {
                    OdbcCommand cmd = connection.CreateCommand();

                    cmd.CommandText = "select * from  " + DAL.DataBaseTableName.TableReglementFactureClient +
                        " where identifiantmode_reglement=" + _codeReg;

                    OdbcDataReader Reader = cmd.ExecuteReader();

                    if (Reader.Read())
                    {
                      reglementFacture = new ReglementFacture(Reader.GetInt32(0),
                            Reader.GetString(1),
                            Reader.GetDouble(2),
                            Reader.GetString(3),
                            Reader.GetString(4),
                            Reader.GetString(5),
                            Reader.GetString(6),
                            Reader.GetString(7));
                                             
                    }
                    else
                        throw new Exception();

                    Reader.Close();

                }
                catch (OdbcException e)
                {
                    MessageBox.Show(e.Message, Program.SelectGlobalMessages.SelectReglement,
                        MessageBoxButtons.OK, MessageBoxIcon.Error);

                }
                catch (Exception)
                {
                    MessageBox.Show(Program.SelectGlobalMessages.ImpSelectReglement,
                         Program.SelectGlobalMessages.SelectReglement, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return reglementFacture;
                }
            }
            return reglementFacture;
        }

        /************************************************************************************/
        /************************************************************************************/
        //recupérer tous les ligneBonLivraison pour un BonLivraison dont son code est passé en paramétre
        public ArrayList getAllReglementFacture(string _codeFact)
        {
            ArrayList tab_reglement = new ArrayList();
            ReglementFacture Reglement = null;
            OdbcConnection connection = DAL.DataBaseConnexion.getConnection();
            try
            {
                OdbcCommand cmd = connection.CreateCommand();

                cmd.CommandText = "select * from  " + DAL.DataBaseTableName.TableReglementFactureClient +
                                  " where code_facture='" + _codeFact + "'";
                OdbcDataReader Reader = cmd.ExecuteReader();
                while (Reader.Read())
                {
                    Reglement = new ReglementFacture(Reader.GetInt32(0),
                                               Reader.GetString(1),
                                                Reader.GetDouble(2),
                                                Reader.GetString(3),
                                                Reader.GetString(4),
                                                Reader.GetString(5),
                                                Reader.GetString(6),
                                                Reader.GetString(7)
                                             );
                    tab_reglement.Add(Reglement);
                }
                Reader.Close();

            }
            catch (OdbcException e)
            {
                MessageBox.Show(e.Message, Program.SelectGlobalMessages.SelectReglement,
                    MessageBoxButtons.OK, MessageBoxIcon.Error);


            }

            if (tab_reglement.Count == 0)
                return null;
            else
                return tab_reglement;

        }
        /************************************************************************************/
        /************************************************************************************/
        //recupérer tous les ligneBonLivraison pour un BonLivraison dont son code est passé en paramétre
        public ArrayList getAllReglement()
        {
            ArrayList tab_reglement = new ArrayList();
            ReglementFacture Reglement = null;
            OdbcConnection connection = DAL.DataBaseConnexion.getConnection();

            try
            {
                OdbcCommand cmd = connection.CreateCommand();

                cmd.CommandText = "select * from  " + DAL.DataBaseTableName.TableReglementFactureClient + " order by code_reglement ";

                OdbcDataReader Reader = cmd.ExecuteReader();


                while (Reader.Read())
                {
                    Reglement = new ReglementFacture(Reader.GetInt32(0),
                                               Reader.GetString(1),
                                                Reader.GetDouble(2),
                                                Reader.GetString(3),
                                                Reader.GetString(4),
                                                Reader.GetString(5),
                                                Reader.GetString(6),
                                                Reader.GetString(7)
                                             
                                                );
                    tab_reglement.Add(Reglement);
                }
                Reader.Close();

            }
            catch (OdbcException e)
            {
                MessageBox.Show(e.Message, Program.SelectGlobalMessages.SelectReglement,
                    MessageBoxButtons.OK, MessageBoxIcon.Error);


            }
            if (tab_reglement.Count == 0)
                return null;
            else
                return tab_reglement;
        }

        /************************************************************************************/
        /************************************************************************************/
        //recupérer tous les ligneBonLivraison pour un BonLivraison dont son code est passé en paramétre
        public ArrayList getAllReglementss()
        {
            ArrayList tab_reglement = new ArrayList();
            ReglementFacture Reglement = null;
            OdbcConnection connection = DAL.DataBaseConnexion.getConnection();

            try
            {
                OdbcCommand cmd = connection.CreateCommand();

                cmd.CommandText = "select * from  " + DAL.DataBaseTableName.TableReglementFactureClient +
                    " order by Format(date_reglement, 'mm/dd/yyyy') ";

                OdbcDataReader Reader = cmd.ExecuteReader();


                while (Reader.Read())
                {
                    Reglement = new ReglementFacture(Reader.GetInt32(0),
                                                Reader.GetString(1),
                                                Reader.GetDouble(2),
                                                Reader.GetString(3),
                                                Reader.GetString(4),
                                                Reader.GetString(5),
                                                Reader.GetString(6),
                                                Reader.GetString(7)
                                               
                                                );
                    tab_reglement.Add(Reglement);
                }
                Reader.Close();

            }
            catch (OdbcException e)
            {
                MessageBox.Show(e.Message, Program.SelectGlobalMessages.SelectReglement,
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            if (tab_reglement.Count == 0)
                return null;
            else
                return tab_reglement;
        }

        /************************************************************************************/
        /************************************************************************************/
        //recupérer tous les ligneBonLivraison pour un BonLivraison dont son code est passé en paramétre
        public ArrayList getAllReglementssClient(int _codeclient)
        {
            ArrayList tab_reglement = new ArrayList();
            ReglementFacture Reglement = null;
            OdbcConnection connection = DAL.DataBaseConnexion.getConnection();

            try
            {
                OdbcCommand cmd = connection.CreateCommand();

                cmd.CommandText = "select bl.*   from    reglementfacture  bl  , factureclient fc " +
                " where fc.numero_factureclient =  bl.code_facture and  fc.codeclient_factureclient =" + _codeclient +
                "  order by Format(bl.date_reglement, 'mm/dd/yyyy') ";

                OdbcDataReader Reader = cmd.ExecuteReader();
                while (Reader.Read())
                {
                    Reglement = new ReglementFacture(Reader.GetInt32(0),
                                                Reader.GetString(1),
                                                Reader.GetDouble(2),
                                                Reader.GetString(3),
                                                Reader.GetString(4),
                                                Reader.GetString(5),
                                                Reader.GetString(6),
                                                Reader.GetString(7)
                                                );
                    tab_reglement.Add(Reglement);
                }
                Reader.Close();

            }
            catch (OdbcException e)
            {
                MessageBox.Show(e.Message, Program.SelectGlobalMessages.SelectReglement,
                    MessageBoxButtons.OK, MessageBoxIcon.Error);


            }
            if (tab_reglement.Count == 0)
                return null;
            else
                return tab_reglement;
        }
    }
}
