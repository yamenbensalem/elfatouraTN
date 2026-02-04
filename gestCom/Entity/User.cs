using System;
using System.Collections.Generic;
using System.Linq;
using System.Text; 
using System.Windows.Forms;
using T4C_Commercial_Project.DAL;
using System.Globalization;
using System.Data.Odbc;

namespace T4C_Commercial_Project.Entity
{
    class User
    {
        public static string Separateur = CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator;
        
        //les attributs d'un User:
        public int id_user;
        public String prenom_user;
        public String nom_user;
        public String adresse_user;
        public String role_user;
        public String login_user;
        public String password_user;
        
        //les contructeurs:
        public User()
        {

        }

        public User(int _id,String _prenom,String _nom,String _adresse, String _role, String _login,String _password)
        {
            this.id_user  = _id;
            this.prenom_user = _prenom;
            this.nom_user = _nom;
            this.adresse_user = _adresse;            
            this.login_user = _login;
            this.password_user = _password;
            this.role_user = _role;
        }

        //les methodes:

        public Boolean ajouterUser()
        {
            string CommandText = "insert into " + DAL.DataBaseTableName.TableUser +
                    " values(" +
                    this.id_user  + ", '" +
                    this.prenom_user + "', '" +
                    this.nom_user + "', '" +
                    this.adresse_user  + "', '" +                    
                    this.login_user + "', '" +
                    this.password_user + "', '" +
                    this.role_user + "' " +
                    ");";
            return DataBaseConnexion.addOrUpdateElementInDataBase ( CommandText, Program.SelectGlobalMessages.ImpAddBonLivraison );
        }
        
        public static User getUser(string _login, string _password)
        {
            User oneUser = null;
            try
            {
                OdbcConnection connection = DataBaseConnexion.getConnection(); ;
                OdbcCommand cmd = connection.CreateCommand();

                cmd.CommandText = "select * from " + DAL.DataBaseTableName.TableUser +
                    " where login_user ='" + _login + "' " +
                    " and password_user ='" + _password + "';";
                OdbcDataReader Reader = cmd.ExecuteReader();

                if (Reader.Read())
                {
                    oneUser = new User(Reader.GetInt32(0), Reader.GetString(1), Reader.GetString(2), Reader.GetString(3),
                                        Reader.GetString(4), Reader.GetString(5), Reader.GetString(6));
                }
                Reader.Close();

                return oneUser;
            }
            catch (OdbcException e)
            {
                MessageBox.Show(e.Message, Program.SelectGlobalMessages.ErrorFailedLogin,
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            return null;
        }

        public static Boolean isExistUser(string _login, string _password)
        {
            if (getUser(_login, _password) == null)
                return false;
            else
                return true;
        }
    }
}
