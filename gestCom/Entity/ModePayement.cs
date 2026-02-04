using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using T4C_Commercial_Project.DAL;
using System.Data.Odbc;

namespace T4C_Commercial_Project.Entity
{
    class ModePayement
    {
        public int code;//Code   ',
        public string libelle;//Libelle ',


        public ModePayement()
        {

        }

        public ModePayement(int _code_modepayement, string _libelle)
        {
            this.code = _code_modepayement;
            this.libelle = _libelle;
        }

        public ModePayement(string _libelle)
        {
            this.libelle = _libelle;
        }



        public Boolean ajoutermodepayement()
        {
            string CommandText = "insert into  modepayement (libelle) values ('" + this.libelle + "');";
            return DataBaseConnexion.addOrUpdateElementInDataBase(CommandText, Program.SelectGlobalMessages.ImpAddModePayement);

        }


        public Boolean modifiermodepayement()
        {
            string CommandText = "update modepayement  set libelle='" + this.libelle + "' where code=" + this.code;
            return DataBaseConnexion.addOrUpdateElementInDataBase(CommandText, Program.SelectGlobalMessages.ImpUpdateModePayement);

        }


        public Boolean deletemodepayement()
        {
            string CommandText = "delete from modepayement   where code_modepayement=" + this.code;
            return DataBaseConnexion.addOrUpdateElementInDataBase(CommandText, Program.SelectGlobalMessages.ImpDeleteModePayement);

        }



        public ModePayement getmodepayement(int _cod_modepayement)
        {
            ModePayement TypAbs = null;

            if (DataBaseConnexion.getRowsCount("modepayement", "code") != 0)
            {

                try
                {
                    OdbcConnection connection = DataBaseConnexion.getConnection(); ;
                    OdbcCommand cmd = connection.CreateCommand();

                    cmd.CommandText = "select * from  modepayement  where code=" + _cod_modepayement;
                    OdbcDataReader Reader = cmd.ExecuteReader();

                    if (Reader.Read())
                    {
                        TypAbs = new ModePayement(Reader.GetInt32(0), Reader.GetString(1));
                    }
                    Reader.Close();
                }
                catch (OdbcException e)
                {
                    MessageBox.Show(e.Message, Program.SelectGlobalMessages.ImpSelectModePayement,
                        MessageBoxButtons.OK, MessageBoxIcon.Error);


                }
            }
            else
            {
                TypAbs = null;
            }
            return TypAbs;
        }





        public int getCodemodepayement(string _libelee)
        {
            int codeType = 0;

            if (DataBaseConnexion.getRowsCount("modepayement", "code") != 0)
            {

                try
                {
                    OdbcConnection connection = DataBaseConnexion.getConnection(); ;
                    OdbcCommand cmd = connection.CreateCommand();

                    cmd.CommandText = "select code  from  " + DAL.DataBaseTableName.TableModePayement + "  where libelle like'" + _libelee + "'";
                    OdbcDataReader Reader = cmd.ExecuteReader();

                    if (Reader.Read())
                    {
                        codeType = Reader.GetInt32(0);
                    }
                    Reader.Close();
                }
                catch (OdbcException e)
                {
                    MessageBox.Show(e.Message, Program.SelectGlobalMessages.ImpSelectModePayement,
                        MessageBoxButtons.OK, MessageBoxIcon.Error);


                }
            }
            else
            {
                codeType = 0;
            }
            return codeType;
        }








    }
}
