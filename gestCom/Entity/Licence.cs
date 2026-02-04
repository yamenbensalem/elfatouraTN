using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Odbc;
using System.Linq;
using System.Management;
using System.Text;
using System.Windows.Forms;
using T4C_Commercial_Project.DAL;

namespace T4C_Commercial_Project.Entity
{
    class Licence
    {
        public DateTime date_first;
        public DateTime date_lastAccess;
        public int nb_days;
        public string adr_mac;
        public string num_hard_disk;
        public int nb_alerte_manipulation;
        public string current_key;
        public string future_key;
        public Boolean sended_licence;

        public Licence()
        {
        }
        public Licence(DateTime _dateFirst, DateTime _date_lastAccess, int _nb_days, string _adr_mac,
                            string _num_hard_disk, int _nb_alerte_manipulation, string _current_key,
                            string _future_key,
                            Boolean _sended_licence)
        {
            this.date_first = _dateFirst;
            this.date_lastAccess = _date_lastAccess;
            this.nb_days = _nb_days;
            this.adr_mac = _adr_mac;
            this.num_hard_disk = _num_hard_disk;
            nb_alerte_manipulation = _nb_alerte_manipulation;
            future_key = _future_key;
            current_key = _current_key;
            sended_licence = _sended_licence;
        }

        public Boolean addLicence()
        {
            Boolean terminated = false;
            //try
            //{
            OdbcConnection connection = DAL.DataBaseConnexion.getConnection();
            OdbcCommand command = connection.CreateCommand();
            command.CommandText = "insert into  " + DAL.DataBaseTableName.Licence +
            " values ( '" + this.date_first.ToString("dd/MM/yyyy") + "', " +
            "'" + this.date_lastAccess.ToString("dd/MM/yyyy") + "', " +
                  this.nb_days + ", " +
            "'" + this.adr_mac + "', " +
            "'" + this.num_hard_disk + "', " +
                  this.nb_days + ", " +
            "'" + this.current_key + "', " +
            "'" + this.future_key + "','" +  DAL.VariablesGlobales.NoValue + "');";

            if (command.ExecuteNonQuery() == 0)
            {
                terminated = false;
            }
            else
                terminated = true;

            return terminated;
            //}
            //catch (OdbcException e)
            //{
            //    MessageBox.Show(Program.SelectGlobalMessages.InvalideLicense, Program.SelectGlobalMessages.testLicence,
            //        MessageBoxButtons.OK, MessageBoxIcon.Error);
            //    return terminated;
            //}
            //catch (Exception)
            //{
            //    MessageBox.Show(Program.SelectGlobalMessages.testLiscence,
            //         Program.SelectGlobalMessages.testLiscence, MessageBoxButtons.OK, MessageBoxIcon.Error);
            //    return terminated;
            //}

        }

        public static Licence getLicencekey()
        {
            Licence License = null;

            OdbcConnection connection = DataBaseConnexion.getConnection();

            //try
            //{                 
            OdbcCommand cmd = connection.CreateCommand();
            cmd.CommandText = "Select * from  " + DAL.DataBaseTableName.Licence;
            OdbcDataReader Reader = cmd.ExecuteReader();

            if (Reader.Read())
            {
                Boolean v_sended_licence = false; ;
                if (Reader.GetString(8) == DAL.VariablesGlobales.YesValue)
                    v_sended_licence = true;

                License = new Licence(Reader.GetDateTime(0), Reader.GetDateTime(1), Reader.GetInt32(2),
                    Reader.GetString(3), Reader.GetString(4), Reader.GetInt32(5), Reader.GetString(6),
                    Reader.GetString(7), v_sended_licence);
            }
            Reader.Close();
            //}               
            //catch (OdbcException e)
            //{
            //    MessageBox.Show(e.Message, Program.SelectGlobalMessages.SelectEntreprise,
            //        MessageBoxButtons.OK, MessageBoxIcon.Error);
            //}
            //catch (Exception e)
            //{
            //    MessageBox.Show(Program.SelectGlobalMessages.ImpSelectEntreprise, Program.SelectGlobalMessages.SelectEntreprise,
            //         MessageBoxButtons.OK, MessageBoxIcon.Error);
            //}
            return License;
        }

        public Boolean updateLicence(String _date_first, string _future_key, string _current_key)
        {

            string CommandText = "update " + DAL.DataBaseTableName.Licence +
                    " set nb_days = 0 , " +
                    " date_first = '" + _date_first + "' ," +
                    " date_lastAccess = '" + _date_first + "' ," +
                    " nb_alerte_manipulation = 0 ," +
                    " current_key = '" + _current_key + "', " +
                    " future_key = '" + _future_key + "', " +
                    " licence_sended = '" + DAL.VariablesGlobales.NoValue + "' ";
            return DataBaseConnexion.addOrUpdateElementInDataBase(CommandText, Program.SelectGlobalMessages.UpdateFailed);
        }

        public static Boolean supprimerLicence()
        {

            string CommandText = "delete from " + DAL.DataBaseTableName.Licence;
            return DataBaseConnexion.addOrUpdateElementInDataBase(CommandText, Program.SelectGlobalMessages.UpdateFailed);
        }

        public Boolean updateNombreJour(int _nb_days)
        {
            Boolean terminated = false;
            OdbcConnection connection = DAL.DataBaseConnexion.getConnection();

            //try
            //{                 
            OdbcCommand command = connection.CreateCommand();
            command.CommandText = "update  " + DAL.DataBaseTableName.Licence +
                                   " set nb_days = " + _nb_days;

            if (command.ExecuteNonQuery() == 0)
            {
                terminated = false;
            }
            else
                terminated = true;

            return terminated;
            //}

            //catch (OdbcException e)
            //{
            //    MessageBox.Show(Program.SelectGlobalMessages.UpdateFailed, Program.SelectGlobalMessages.testLicence,
            //        MessageBoxButtons.OK, MessageBoxIcon.Error);
            //    return terminated;
            //   }
            //catch (Exception)
            //{
            //    MessageBox.Show(Program.SelectGlobalMessages.testLiscence,
            //         Program.SelectGlobalMessages.testLiscence, MessageBoxButtons.OK, MessageBoxIcon.Error);
            //    return terminated;
            //}             
        }

        public Boolean updateStatusLicence(String _sended_licence)
        {
            Boolean terminated = false;
            OdbcConnection connection = DAL.DataBaseConnexion.getConnection();

            //try
            //{                 
            OdbcCommand command = connection.CreateCommand();           

            command.CommandText = "update  " + DAL.DataBaseTableName.Licence +
                                   " set licence_sended = '" + _sended_licence + "'";

            if (command.ExecuteNonQuery() == 0)
            {
                terminated = false;
            }
            else
                terminated = true;

            return terminated;
            //}

            //catch (OdbcException e)
            //{
            //    MessageBox.Show(Program.SelectGlobalMessages.UpdateFailed, Program.SelectGlobalMessages.testLicence,
            //        MessageBoxButtons.OK, MessageBoxIcon.Error);
            //    return terminated;
            //   }
            //catch (Exception)
            //{
            //    MessageBox.Show(Program.SelectGlobalMessages.testLiscence,
            //         Program.SelectGlobalMessages.testLiscence, MessageBoxButtons.OK, MessageBoxIcon.Error);
            //    return terminated;
            //}             
        }

        public Boolean updateLastAccessDate(String _date_lastAccess)
        {
            Boolean terminated = false;
            OdbcConnection connection = DAL.DataBaseConnexion.getConnection();
            //try
            //{
            OdbcCommand command = connection.CreateCommand();
            command.CommandText = "update  " + DAL.DataBaseTableName.Licence +
                " set date_lastAccess = '" + Convert.ToDateTime(_date_lastAccess) + "'";     
            //.ToString("dd/mm/yyyy")           

            if (command.ExecuteNonQuery() == 0)
            {
                terminated = false;
            }
            else
                terminated = true;

            return terminated;
            //}
            //catch (OdbcException e)
            //{
            //    MessageBox.Show(e.Message, Program.SelectGlobalMessages.testLicence ,
            //        MessageBoxButtons.OK, MessageBoxIcon.Error);
            //    return terminated;
            //}
            //catch (Exception)
            //{
            //    MessageBox.Show(Program.SelectGlobalMessages.testLicence ,
            //         Program.SelectGlobalMessages.testLicence, MessageBoxButtons.OK, MessageBoxIcon.Error);
            //    return terminated;
            //}

        }

        public Boolean incrementerNb_alerte_manipulation()
        {            
            Boolean terminated = false;
            OdbcConnection connection = DAL.DataBaseConnexion.getConnection();
            //try
            //{
            OdbcCommand command = connection.CreateCommand();
            command.CommandText = "update  " + DAL.DataBaseTableName.Licence +
                " set nb_alerte_manipulation =  nb_alerte_manipulation + 1;";           

            if (command.ExecuteNonQuery() == 0)
            {
                terminated = false;
            }
            else
                terminated = true;

            return terminated;
            //}
            //catch (OdbcException e)
            //{
            //    MessageBox.Show(e.Message, Program.SelectGlobalMessages.testLicence ,
            //        MessageBoxButtons.OK, MessageBoxIcon.Error);
            //    return terminated;
            //}
            //catch (Exception)
            //{
            //    MessageBox.Show(Program.SelectGlobalMessages.testLicence ,
            //         Program.SelectGlobalMessages.testLicence, MessageBoxButtons.OK, MessageBoxIcon.Error);
            //    return terminated;
            //}

        
        }

        public static ArrayList getInformationMachine()
        {
            ArrayList tab = new ArrayList();

            ManagementClass mc = new ManagementClass("Win32_NetworkAdapterConfiguration");

            ManagementObjectCollection mcCol = mc.GetInstances();

            foreach (ManagementObject mcObj in mcCol)
            {

                if ((bool)mcObj["IPEnabled"])
                {
                    //MessageBox.Show(mcObj["InterfaceIndex"].ToString());
                    //MessageBox.Show(mcObj["Caption"].ToString());

                    if (mcObj["MacAddress"] != null)
                    {
                        //MessageBox.Show(mcObj["MacAddress"].ToString());
                        tab.Add(mcObj["MacAddress"].ToString());
                    }
                    else
                    {
                        //MessageBox.Show("");
                        tab.Add("NoMacAddress");
                    }

                    if (mcObj["IPAddress"] != null)
                    {
                        string[] ips = (string[])mcObj["IPAddress"];
                        foreach (string ip in ips)
                        {
                            //MessageBox.Show(ip);
                            tab.Add(ip);
                        }
                    }
                    else
                    {
                        //MessageBox.Show("");
                        tab.Add("NoIPAddress");
                    }
                }
            }
            return tab;
        }
    }
}

