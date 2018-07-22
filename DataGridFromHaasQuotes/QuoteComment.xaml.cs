using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace DataGridFromHaasQuotes
{
    /// <summary>
    /// Interaction logic for QuoteComment.xaml
    /// </summary>
    public partial class QuoteComment : Window
    {
        public QuoteComment(string strUserIDFromMain, string strQuoteIDFromMain)
        {
            InitializeComponent();
            strUserID = strUserIDFromMain;
            strQuoteID = strQuoteIDFromMain;
            txtbxComment.Focus();
            //GetUserID_QuoteID();
        }

        /*
        public Window setCreatingWindow
        {
            get { return creatingWindow; }
            set { creatingWindow = value; }
        }
        */

        string strUserID = String.Empty;
        string strQuoteID = String.Empty;
        DateTime thisDay = DateTime.Today;
        double serialQuoteDate = DateTime.Today.ToOADate();

        

        /*
        private void GetUserID_QuoteID()
        {
            //Window mw = Application.Current(MainWindow);
            strUserID = creatingWindow.userIDValue;
            MessageBox.Show("User ID = " + strUserID);
            
        }
        */

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (txtbxComment.Text.Length != 0)
            {
                string ConString = ConfigurationManager.ConnectionStrings["ConString"].ConnectionString.ToString();
                string CmdString = string.Empty;

                string sqlString = txtbxComment.Text.Replace("'", "''");

                string InsertClause = "INSERT INTO [dbo].[QuoteComments] ([quote_id], [user_id], [qcdate], [qcomment]) " +
                    "VALUES (" + strQuoteID + ", " + strUserID + ", " + serialQuoteDate.ToString() + ", '" + sqlString + "')";
                //string WhereClause = "WHERE [quote_id] = " + txtblkQuoteID.Text;
                using (SqlConnection con = new SqlConnection(ConString))
                {
                    CmdString = InsertClause;
                    //MessageBox.Show("INSERT clause = '" + CmdString + "'.");
                    con.Open();
                    SqlCommand cmd = new SqlCommand(CmdString, con);
                    cmd.ExecuteNonQuery();

                    con.Close();
                }

                this.DialogResult = true;
                this.Close();
            }
            else
            {
                MessageBox.Show("No comment has been entered. There is nothing to save.");
            }
            
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        private void txtbxComment_TextChanged(object sender, TextChangedEventArgs e)
        {
            string s = txtbxComment.Text;
            txtblkCharRemaining.Text = (1000 - s.Length).ToString();
            if (int.Parse(txtblkCharRemaining.Text) == 0)
            {
                txtbxComment.Text = s.Substring(0, s.Length - 1);
                MessageBox.Show("Max comment length of " + txtbxComment.MaxLength.ToString() + " has been reached.");
            }

            if (s.Length > 0)
            {
                btnSave.IsDefault = true;
                btnCancel.IsDefault = false;
            }
            else
            {
                btnSave.IsDefault = false;
                btnCancel.IsDefault = true;
            }
        }

        private void txtbxComment_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                if (btnSave.IsDefault)
                {
                    this.btnSave_Click(this.btnSave, null);
                }
                else
                {
                    this.btnCancel_Click(this.btnCancel, null);
                }
            }
        }
    }
}
