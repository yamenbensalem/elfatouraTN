using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using T4C_Commercial_Project.DAL;
using System.Windows.Forms;
using System.Data.Odbc;

namespace T4C_Commercial_Project.Entity
{
    public class PathLogo
    {
        public int num;
        public string urllogo;

        public PathLogo(int _num, string _pathlogo)
        {
            num = _num;
            urllogo = _pathlogo;
        }

        public PathLogo()
        {
        }

        public static Boolean addPath(string _newspath)
        {
            string CommandText = "insert into " + DAL.DataBaseTableName.TablePathLogo + " (urllogo)  values('" + _newspath + "');";
            return DataBaseConnexion.addOrUpdateElementInDataBase(CommandText, Program.SelectGlobalMessages.ErrorMessage);
        }

        public static Boolean updatePath(string _newspath)
        {
            string CommandText = "update " + DAL.DataBaseTableName.TablePathLogo + "  set urllogo='" + _newspath + "'";
            return DataBaseConnexion.addOrUpdateElementInDataBase(CommandText, Program.SelectGlobalMessages.ErrorMessage);

        }

        public static PathLogo getPath()
        {
            PathLogo path = null;

            if (DataBaseConnexion.getRowsCount("pathlogo", "num") != 0)
            {
                try
                {
                    OdbcConnection connection = DataBaseConnexion.getConnection(); ;
                    OdbcCommand cmd = connection.CreateCommand();

                    cmd.CommandText = "select * from  " + DAL.DataBaseTableName.TablePathLogo;
                    OdbcDataReader Reader = cmd.ExecuteReader();

                    if (Reader.Read())
                    {
                        path = new PathLogo(Reader.GetInt32(0), Reader.GetString(1));
                    }
                    Reader.Close();
                }
                catch (OdbcException e)
                {
                    MessageBox.Show(e.Message, Program.SelectGlobalMessages.ErrorMessage,
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            
            return path;
        }

    }
}
