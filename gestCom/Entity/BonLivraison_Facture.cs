using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Data;
using T4C_Commercial_Project.DAL;
using System.Collections;
using System.Globalization;

namespace T4C_Commercial_Project.Entity
{
    public partial class BonLivraisonFacture
    {
        public static string Separateur = CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator;

        // les attributs :
        public int code_bonlivraison;
        public int  numero_factureclient;     
        public string date_facturation;

        // les méthodes :
                
        // Ajout d'un BL à une devisClient currentFournisseur : ajout d'un enregistrement dans la table BonLivraisonFacture :
        public static Boolean ajoutBLFacture(int _numfacture, string   _codebl, string _datefact)
        {
            string CommandText = "insert into  " + DAL.DataBaseTableName.TableBonLivraisonFacture +
                         "  values('" + _codebl + "', " + _numfacture + ", '" + _datefact + "');";
            return DataBaseConnexion.addOrUpdateElementInDataBase(CommandText, Program.SelectGlobalMessages.ErrorMessage);
        }

        public static Boolean supprimerALLBLFromFacture(int _numfacture)
        {
            string CommandText = "delete from " + DAL.DataBaseTableName.TableBonLivraisonFacture +
                " where numero_factureclient =" + _numfacture + ";";

            return DataBaseConnexion.addOrUpdateElementInDataBase(CommandText, Program.SelectGlobalMessages.ImpDeleteBonLivraison);

        }
    }

}
