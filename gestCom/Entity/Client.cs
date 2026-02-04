using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using T4C_Commercial_Project.DAL;

using System.Windows.Forms;
using System.Collections;
using System.Globalization;
using System.Data.Odbc;

namespace T4C_Commercial_Project.Entity
{
   public  class Client
    {
       //
        //  Les attributs 
       //
       public static String Separateur = CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator;   
       
       public String code_client;
       public String matriculefiscale_client;
       public String nom_client;
       public String typepersonne_client;
       public String typeentreprise_client;
       public String rib_client;
       public String adresse_client;
       public String codepostal_client;
       public String ville_client;
       public String pays_client;
       public String tel_client;
       public String telmobile_client;
       public String fax_client;
       public String email_client;
       public String site_client;       
       public String etat_client;       
       public int    nbtransactions_client;
       public String note_client;
       public String etranger_client;
       public String exonore_client;
       public Double maxcredit_client;
       public int code_devise;
       public String responsable_client;

        //
        // Les constructeurs
        //
        public Client()
       {
        
       }
       
       public Client(string _codeClt )
        {
            this.code_client = _codeClt;
        }

       public Client(string _code_client, String _matriculefiscale_client, String _nom_client, String _typepersonne_client, 
           String _typeentreprise_client, String _rib_client, String _adresse_client, String _codepostal_client, String _ville_client,
           String _pays_client, String _tel_client, String _telmobile_client, String _fax_client, String _email_client, String _site_client,
           String _etat_client, int _nbtransactions_client, String _note_client, String _etranger_client, String _exonore_client, 
           Double _maxcredit_client, int _code_devise, String _responsable_client)
        {
           code_client = _code_client;
           matriculefiscale_client = _matriculefiscale_client;            
           nom_client = _nom_client;
           typepersonne_client = _typepersonne_client;
           typeentreprise_client = _typeentreprise_client;
           rib_client = _rib_client;
           adresse_client = _adresse_client;
           codepostal_client = _codepostal_client;
           ville_client = _ville_client;
           pays_client = _pays_client;
           tel_client = _tel_client;
           telmobile_client = _telmobile_client;
           fax_client = _fax_client;
           email_client = _email_client;
           site_client = _site_client;
           etat_client = _etat_client;           
           nbtransactions_client = _nbtransactions_client;
           note_client = _note_client;
           etranger_client = _etranger_client;
           exonore_client = _exonore_client;
           maxcredit_client = _maxcredit_client;
           code_devise = _code_devise;
            responsable_client = _responsable_client;
        }

       //
       // Les méthodes
       //        
        public  Boolean ajouterClient()
        {
            string CommandText = "INSERT into " +  DataBaseTableName.TableClient +
                    "  VALUES ( '" + code_client + "' " + 
                    " , '" + matriculefiscale_client + "' " +                    
                    " , '" + nom_client.ToString().Replace("'", "''") + "' " +
                    " , '" + typepersonne_client + "' " +                    
                    " , '" + typeentreprise_client + "' " +
                    " , '" + rib_client + "' " +
                    " , '" + adresse_client.ToString().Replace("'", "''") + "' " +
                    " , '" + codepostal_client + "' " +
                    " , '" + ville_client.ToString().Replace("'", "''") + "' " +
                    " , '" + pays_client + "' " +
                    " , '" + tel_client + "' " +
                    " , '" + telmobile_client + "' " +
                    " , '" + fax_client + "' " +
                    " , '" + email_client + "' " +
                    " , '" + site_client + "' " +
                    " , '" + etat_client + "' " +
                    " ,  " + nbtransactions_client +
                    " , '" + note_client.ToString().Replace("'", "''") + "' " +
                    " , '" + etranger_client + "' " +
                    " , '" + exonore_client + "' " +
                    " ,  " + maxcredit_client  +
                    " ,  " + code_devise +
                    " ,  '" + responsable_client + "' " +
                    " ) ";

            return  DataBaseConnexion.addOrUpdateElementInDataBase(CommandText,Program.SelectGlobalMessages.ImpAddClient);
        }

        public Boolean modifierClient()
        {            
              string CommandText = "UPDATE " +   DataBaseTableName.TableClient +
                    " SET matriculefiscale_client = '" + matriculefiscale_client + "' " +
                    " , nom_client = '" + nom_client.ToString().Replace("'", "''") + "' " +
                    " , typepersonne_client = '" + typepersonne_client + "' " +
                    " , typeentreprise_client = '" + typeentreprise_client + "' " +
                    " , rib_client = '" + rib_client + "' " +
                    " , adresse_client = '" + adresse_client.ToString().Replace("'", "''") + "' " +
                    " , codepostal_client = '" + codepostal_client + "' " +
                    " , ville_client = '" + ville_client.ToString().Replace("'", "''") + "' " +
                    " , pays_client = '" + pays_client + "' " +
                    " , tel_client = '" + tel_client + "' " +
                    " , telmobile_client = '" + telmobile_client + "' " +
                    " , fax_client = '" + fax_client + "' " +
                    " , email_client = '" + email_client + "' " +
                    " , site_client = '" + site_client + "' " +
                    " , etat_client = '" + etat_client + "' " +
                    //" , nbtransactions_client = " + nbtransactions_client +
                    " , note_client = '" + note_client.ToString().Replace("'", "''") + "' " +
                    " , etranger_client =  '" + etranger_client + "' " +
                    " , exonore_client =  '" + exonore_client + "' " +
                    " , maxcredit_client = " + maxcredit_client +                    
                    " , code_devise =  " + code_devise +
                    " , responsable_client =  '" + responsable_client + "' " +
                    
                    " WHERE code_client = '" + code_client + "' " ;
                    return  DataBaseConnexion.addOrUpdateElementInDataBase(CommandText,Program.SelectGlobalMessages.ImpUpdateClient);
        }

        public static Boolean supprimerClient(string _code_client)
        {
                string CommandText = "DELETE FROM " +   DataBaseTableName.TableClient +
                    " WHERE code_client = '" + _code_client + "' " ;
                return  DataBaseConnexion.addOrUpdateElementInDataBase(CommandText, Program.SelectGlobalMessages.ImpDeleteClient);

        }

        public static Boolean supprimerAllClient()
        {
            string CommandText = "DELETE FROM " + DataBaseTableName.TableClient +
                " WHERE code_client <> '1' ";
            return DataBaseConnexion.addOrUpdateElementInDataBase(CommandText, Program.SelectGlobalMessages.ImpDeleteClient);
        }

        public static Client getClient(string _code_client)
        {
            Client client = null;
            if ( DataBaseConnexion.getMaxNumberOfStringColumn( DataBaseTableName.TableClient, "code_client") != 0)
            {
                OdbcConnection connection = DataBaseConnexion.getConnection();

                try
                {
                    OdbcCommand cmd = connection.CreateCommand();

                    cmd.CommandText = "select * from  " +  DataBaseTableName.TableClient +
                            " where code_client = '" + _code_client + "' ;";


                    OdbcDataReader Reader = cmd.ExecuteReader();
                  
                        if (Reader.Read())
                        {
                            client = new Client(Reader.GetString(0), Reader.GetString(1), Reader.GetString(2), Reader.GetString(3),
                                Reader.GetString(4), Reader.GetString(5), Reader.GetString(6), Reader.GetString(7), Reader.GetString(8),
                                Reader.GetString(9), Reader.GetString(10), Reader.GetString(11), Reader.GetString(12), Reader.GetString(13),
                                Reader.GetString(14), Reader.GetString(15), Reader.GetInt32(16), Reader.GetString(17), Reader.GetString(18),
                                Reader.GetString(19), Reader.GetDouble(20), Reader.GetInt32(21),
                                Reader.GetString(22));
                        }
                        Reader.Close();
                   
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.Message, Program.SelectGlobalMessages.ImpSelectClient,
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                   
                }
            }
            return client;            
        }

        public static Client getClientByMatriculeFiscale(string _matriculefiscale_client)
        {
            Client client = null;
            if (DataBaseConnexion.getMaxNumberOfStringColumn(DataBaseTableName.TableClient, "code_client") != 0)
            {
                OdbcConnection connection = DataBaseConnexion.getConnection();

                try
                {
                    OdbcCommand cmd = connection.CreateCommand();

                    cmd.CommandText = "select * from  " + DataBaseTableName.TableClient +
                            " where matriculefiscale_client = '" + _matriculefiscale_client + "' ;";


                    OdbcDataReader Reader = cmd.ExecuteReader();

                    if (Reader.Read())
                    {
                        client = new Client(Reader.GetString(0), Reader.GetString(1), Reader.GetString(2), Reader.GetString(3),
                            Reader.GetString(4), Reader.GetString(5), Reader.GetString(6), Reader.GetString(7), Reader.GetString(8),
                            Reader.GetString(9), Reader.GetString(10), Reader.GetString(11), Reader.GetString(12), Reader.GetString(13),
                            Reader.GetString(14), Reader.GetString(15), Reader.GetInt32(16), Reader.GetString(17), Reader.GetString(18),
                            Reader.GetString(19), Reader.GetDouble(20), Reader.GetInt32(21), Reader.GetString(22));
                    }
                    Reader.Close();

                }
                catch (Exception e)
                {
                    MessageBox.Show(e.Message, Program.SelectGlobalMessages.ImpSelectClient,
                        MessageBoxButtons.OK, MessageBoxIcon.Error);

                }
            }
            return client;
        }

        public Boolean incrementerNombreTransactions()
        {
            string CommandText = "update " + DAL.DataBaseTableName.TableClient +
                    " set nbtransactions_client = nbtransactions_client + 1 " +
                    " where code_client = '" + code_client + "' ";

            return DataBaseConnexion.addOrUpdateElementInDataBase(CommandText, Program.SelectGlobalMessages.ImpUpdateClient);
        }
               
        public static Boolean updateEtatClient(string _code_client, String _newEtat)
        { 
            string CommandText = "update " + DAL.DataBaseTableName.TableClient +
                    " set etat_client = '" + _newEtat + "' " +
                    " where code_client = '" + _code_client + "' ";

            return DataBaseConnexion.addOrUpdateElementInDataBase(CommandText, Program.SelectGlobalMessages.ImpUpdateClient);
        }

        public static Boolean updateMaxCreditClient(string _code_client, Double _maxcredit_client)
        {
            string CommandText = "update " + DAL.DataBaseTableName.TableClient +
                    " set maxcredit_client = " + _maxcredit_client +
                    " where code_client = '" + _code_client + "' ";

            return DataBaseConnexion.addOrUpdateElementInDataBase(CommandText, Program.SelectGlobalMessages.ImpUpdateClient);
        }


       public ArrayList getAllClient()
       {
            ArrayList tabclient = null;
            Client client = null;
            OdbcConnection connection = DAL.DataBaseConnexion.getConnection();
                try
                {
                    OdbcCommand cmd = connection.CreateCommand();

                    cmd.CommandText =    " SELECT nom_client  FROM " + DAL.DataBaseTableName.TableClient + " where etatinactif = 0";

                    OdbcDataReader Reader = cmd.ExecuteReader();

                    if (Reader.Read())
                    {
                        client = new Client(Reader.GetString(0), Reader.GetString(1), Reader.GetString(2), Reader.GetString(3), 
                            Reader.GetString(4), Reader.GetString(5), Reader.GetString(6), Reader.GetString(7), Reader.GetString(8),
                            Reader.GetString(9), Reader.GetString(10), Reader.GetString(11), Reader.GetString(12), Reader.GetString(13),
                            Reader.GetString(14), Reader.GetString(15), Reader.GetInt32(16), Reader.GetString(17), Reader.GetString(18),
                            Reader.GetString(19), Reader.GetDouble(20), Reader.GetInt32(21), Reader.GetString(22));
                             tabclient.Add(client);
                    }
                    Reader.Close();

                }
                catch (OdbcException e)
                {
                    MessageBox.Show(e.Message, Program.SelectGlobalMessages.ImpSelectClient,
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                   
                }

                return tabclient;
        }

        }
   }
    

