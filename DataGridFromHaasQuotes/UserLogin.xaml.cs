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
    /// Interaction logic for UserLogin.xaml
    /// </summary>
    public partial class UserLogin : Window
    {
        public UserLogin()
        {
            InitializeComponent();
            PopulateUserList();
            //ShowUserNames();
            bool CapsLockIsOn = Keyboard.IsKeyToggled(Key.CapsLock);
            if (CapsLockIsOn)
            {
                txtblkCapsLockMsg.Visibility = Visibility.Visible;
            }
            else
            {
                txtblkCapsLockMsg.Visibility = Visibility.Hidden;
            }
            pwbxPassword.Focus();
        }

        string ConString = ConfigurationManager.ConnectionStrings["ConString"].ConnectionString.ToString();
        private string strUserID = String.Empty;

        public string userIDValue
        {
            get { return strUserID; }
        }

        private void ShowUserNames()
        {
            foreach (ProgramUser progUser in listOfUsers)
            {
                string fullname = progUser.firstName + " " + progUser.lastName;
                MessageBox.Show("Full name of program user is " + fullname);
            }
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            strUserID = GetUserID(pwbxPassword.Password);

            if (strUserID != String.Empty)
            {
                //PW was accepted
                PopulateMessagesList(strUserID);
                int numberOfMessages = MessageList.Count;
                Random random = new Random();
                int randomMessageNumber = random.Next(0, numberOfMessages);
                string message = MessageList[randomMessageNumber];
                MessageBox.Show(message);
                this.DialogResult = true;
                this.Close();
            }
            else
            {
                MessageBox.Show("Password is not valid. \n\n Try again or click 'Cancel'.");
                pwbxPassword.Focus();
                pwbxPassword.SelectAll();
            }

            /*if (txtbxUserPW.Text == "spxgene")
            {
                strUserID = "1";
                this.DialogResult = true;
                this.Close();
            }
            else if (txtbxUserPW.Text == "spxeric")
            {
                strUserID = "2";
                this.DialogResult = true;
                this.Close();
            }
            else if (txtbxUserPW.Text == "spxandy")
            {
                strUserID = "5";
                this.DialogResult = true;
                this.Close();
            }
            else
            {
                MessageBox.Show("Password is not valid. \n Try again or click 'Cancel'.");
            }
            */
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        private string GetUserID (string pw)
        {
            string value = String.Empty;
            foreach (ProgramUser progUser in listOfUsers)
            {
                string userPW = progUser.userPassword;
                if (pw == userPW)
                {
                    value = progUser.userID.ToString();
                    break;
                }
            }
            return value;
        }

        List<string> MessageList;
        private void PopulateMessagesList(string uID)
        {
            MessageList = new List<string>();

            string CmdString = "SELECT msg FROM UserMessages WHERE user_id = " + uID;
            using (SqlConnection con = new SqlConnection(ConString))
            {
                //ProgramUser programUser;
                SqlCommand cmd = new SqlCommand(CmdString, con);
                con.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    //ProgramUser programUser;
                    while (reader.Read())
                    {
                        MessageList.Add(reader.GetString(0));
                    }
                }
                con.Close();
            }
        }

        List<ProgramUser> listOfUsers;
        private void PopulateUserList()
        {
            //List<ProgramUser> listOfUsers = new List<ProgramUser>();
            listOfUsers = new List<ProgramUser>();

            string fieldnames = "user_id, LastName, FirstName, UserEmail, UserPhone1, UserExt, UserPhone2, UserTitle1, " +
                "UserTitle2, UserPassword, ComboCC, UserInitials, role_id";
            string CmdString = "SELECT " + fieldnames + " FROM Users ORDER BY user_id";
            using (SqlConnection con = new SqlConnection(ConString))
            {
                ProgramUser programUser;
                SqlCommand cmd = new SqlCommand(CmdString, con);
                con.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    //ProgramUser programUser;
                    while (reader.Read())
                    {
                        programUser = new ProgramUser();
                        programUser.userID = reader.GetInt32(0);
                        programUser.lastName = reader.GetString(1);
                        programUser.firstName = reader.GetString(2);
                        programUser.userEmail = reader.GetString(3);
                        programUser.userPhone1 = reader.GetString(4);
                        programUser.userExt = reader.GetString(5);
                        programUser.userPhone2 = reader.GetString(6);
                        programUser.userTitle1 = reader.GetString(7);
                        programUser.userTitle2 = reader.GetString(8);
                        programUser.userPassword = reader.GetString(9);
                        programUser.comboCC = reader.GetInt32(10);
                        programUser.userInitials = reader.GetString(11);
                        programUser.roleID = reader.GetInt32(12);

                        listOfUsers.Add(programUser);
                    }
                }
                con.Close();
                //foreach (ProgramUser progUser in listOfUsers)
                //{
                //    string fullname = progUser.firstName + " " + progUser.lastName;
                //    MessageBox.Show("Full name of program user is " + fullname);
                //}
            }

            //foreach (ProgramUser progUser in listOfUsers)
            //{
            //    string fullname = progUser.firstName + " " + progUser.lastName;
            //    MessageBox.Show("Full name of program user is " + fullname);
            //}

            /*
            listOfUsers.ForEach(delegate (ProgramUser programUser)
            {
                string fullname = programUser.firstName + " " + programUser.lastName;
                MessageBox.Show("Full name of program user is " + fullname);
            });
            */
        }

        private void Logon_Click(object sender, RoutedEventArgs e)
        {
            strUserID = GetUserID(pwbxPassword.Password);

            if (strUserID != String.Empty)
            {
                //PW was accepted
                PopulateMessagesList(strUserID);
                int numberOfMessages = MessageList.Count;
                Random random = new Random();
                int randomMessageNumber = random.Next(0, numberOfMessages);
                string message = MessageList[randomMessageNumber];
                MessageBox.Show(message);
                this.DialogResult = true;
                this.Close();
            }
            else
            {
                MessageBox.Show("Password is not valid. \n\n Try again or click 'Cancel'.");
                pwbxPassword.Focus();
                pwbxPassword.SelectAll();
            }
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            bool CapsLockIsOn = Keyboard.IsKeyToggled(Key.CapsLock);
            if (CapsLockIsOn)
            {
                txtblkCapsLockMsg.Visibility = Visibility.Visible;
            }
            else
            {
                txtblkCapsLockMsg.Visibility = Visibility.Hidden;
            }
        }
    }

    public class ProgramUser
    {
        public int userID { get; set; }
        public string lastName { get; set; }
        public string firstName { get; set; }
        public string userEmail { get; set; }
        public string userPhone1 { get; set; }
        public string userExt { get; set; }
        public string userPhone2 { get; set; }
        public string userTitle1 { get; set; }
        public string userTitle2 { get; set; }
        public string userPassword { get; set; }
        public int comboCC { get; set; }
        public string userInitials { get; set; }
        public int roleID { get; set; }
    }
    public class UserMessages
    {
        public string message { get; set; }
    }
}
