using System;
using System.Collections.Generic;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Collections;
using System.Diagnostics;
using System.Globalization;

namespace DataGridFromHaasQuotes
{
    //learn to use MultiBinding and IMultiValueConverter to be able to set row color to red if status = Pending
    //and close date is before last day of current month. (Multibinding project has been downloaded from C# Corner
    //site and saved in VS 2017\Projects)
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            
            GetUserLogin();

            if (intUserID == 0)
            {
                //terminate execution if user did not enter valid userID
                return;
            }
            

            FillDataGrid();
            //grdHaasQuotes.Columns[0].MaxWidth = 0;
            FillComboStatus();
            FillWinProb();
            //HideColumns();
            //MessageBox.Show("No. of columns in grdHaasQuotes is " + grdHaasQuotes.Columns.Count + ".");
        }

        string ConString = ConfigurationManager.ConnectionStrings["ConString"].ConnectionString.ToString();

        public int intUserID = 0;
        private string quoteID = String.Empty;
        string filterAll = String.Empty;
        bool SelChangedAfterUpdate = false;
        bool quoteUpdated = false;

        private bool userChangedStatus = true;
        private bool userChangedWinProb = true;
        private bool userChangedCloseDate = true;
        private bool userChangedDate = false;

        //private void HideColumns()
        //{
        //    grdHaasQuotes.Columns[0].Visibility = Visibility.Hidden;
        //}

        private void GetUserLogin()
        {
            UserLogin userLogin = new UserLogin();
            if (userLogin.ShowDialog() == true)
            {
                intUserID = int.Parse(userLogin.userIDValue);
                //MessageBox.Show("User ID = " + intUserID.ToString());
            }
            else
            {
                MessageBox.Show("Valid password was not entered. \n Application will now be closed.");
                this.Close();
            }
            //userLogin.ShowDialog();
            //MessageBox.Show("User ID = " + intUserID.ToString());
        }
        

        private void FillDataGrid()
        {
            //string ConString = ConfigurationManager.ConnectionStrings["ConString"].ConnectionString.ToString();
            //string CmdString = string.Empty;
            string CmdString = "SELECT * FROM QLDataByQuoteWPF2 " +
                    "ORDER BY quote_id DESC, Rev DESC";

            using (SqlConnection con = new SqlConnection(ConString))
            {
                //CmdString = "SELECT * FROM QLDataByQuoteWPF2 " +
                //    "ORDER BY quote_id DESC, Rev DESC";
                
                TryReconnecting:
                try
                {
                    SqlCommand cmd = new SqlCommand(CmdString, con);
                    SqlDataAdapter sda = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable("HaasQuotes");
                    sda.Fill(dt);
                    grdHaasQuotes.ItemsSource = dt.DefaultView;
                    
                    //grdHaasQuotes.Columns[0].Visibility = Visibility.Hidden;
                }
                catch (SqlException sqlex)
                {
                    MessageBox.Show("There was a problem generating the quote log data table from the turtleQuote dBase. \n\n" +
                        "Error message: " + sqlex.Message +"\n\n" +
                        "Error no.: " + sqlex.Number.ToString());
                    string message = "Verify you have an internet connection then click 'Yes' to try again or click 'No' to close the application.";
                    if (MessageBox.Show(message, "Try Again?", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                    {
                        goto TryReconnecting;
                    }
                    else
                    {
                        this.Close();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("There was a problem generating the quote log data table from the turtleQuote dBase. \n\n" +
                        "Error message: " + ex.Message + "\n\nThe application will be shut down.");
                    this.Close();
                }
                
                con.Close();
            }
            //grdHaasQuotes.Columns[3].Width = 150;
            //MessageBox.Show("No. of columns in grdHaasQuotes is " + grdHaasQuotes.Columns.Count + ".");
            txtblkNumRecords.Text = grdHaasQuotes.Items.Count.ToString();

            //return here fr 9/3/18 ***********************
            //trying to set specific column widths for SubjL1, CustName and EndUser which can be very long causing user to have to scroll right to
            //be able to see quote amounts. Need to be able to set fixed column width but allow user to make wider if necessary to be able to
            //read the full text.
            /* CODE SECTION BELOW COMMENTED 9/4/18
            foreach(var c in grdHaasQuotes.Columns)
            {
                //double width = double(c.Width);
                if (c.Header.ToString() == "SubjL1")
                {
                    if(c.Width > 100)
                    {
                        c.Width = 100;
                    }
                }
                
            }
            END OF CODE SECTION COMMENTED 9/4/18 */
            //grdHaasQuotes.Columns[0].Visibility = Visibility.Hidden;
        }

        private void grdHaasQuotes_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SelChangedAfterUpdate)
            {
                SelChangedAfterUpdate = false;
                return;
            }

            DataGrid gd = (DataGrid)sender;
            DataRowView row_selected = gd.SelectedItem as DataRowView;
            if (row_selected != null)
            {
                txtblkProjectID.Text = row_selected["project_id"].ToString();
                quoteID = row_selected["quote_id"].ToString();
                txtblkQuoteID.Text = quoteID;

                //string ConString = ConfigurationManager.ConnectionStrings["ConString"].ConnectionString.ToString();
                string CmdString = "SELECT CONVERT(varchar(8),CONVERT(datetime, [qcdate] - 2), 1) AS Date, " +
                    "U.[UserInitials] AS Who, [qcomment] AS Comment, [qc_id], QC.[quote_id] " +
                    "FROM[turtleQuote].[dbo].[QuoteComments] AS QC " +
                    "INNER JOIN ProjectQuotes AS PQ ON QC.quote_id = PQ.quote_id " +
                    "INNER JOIN Customers AS C ON PQ.cust_id = C.cust_id " +
                    "INNER JOIN Users AS U ON QC.user_id = U.user_id " +
                    "WHERE QC.quote_id = " + quoteID + " " +
                    "ORDER BY [qc_id] DESC";
                using (SqlConnection con = new SqlConnection(ConString))
                {
                    /*
                    CmdString = "SELECT CONVERT(varchar(8),CONVERT(datetime, [qcdate] - 2), 1) AS Date, " +
                    "U.[UserInitials] AS Who, [qcomment] AS Comment, [qc_id], QC.[quote_id] " +
                    "FROM[turtleQuote].[dbo].[QuoteComments] AS QC " +
                    "INNER JOIN ProjectQuotes AS PQ ON QC.quote_id = PQ.quote_id " +
                    "INNER JOIN Customers AS C ON PQ.cust_id = C.cust_id " +
                    "INNER JOIN Users AS U ON PQ.user_id = U.user_id " +
                    "WHERE QC.quote_id = " + quoteID + " " +
                    "ORDER BY [qc_id] DESC";
                    */
            TryReconnecting:
                    try
                    {
                        SqlCommand cmd2 = new SqlCommand(CmdString, con);
                        SqlDataAdapter sda2 = new SqlDataAdapter(cmd2);
                        DataTable dt2 = new DataTable("QuoteComments");
                        sda2.Fill(dt2);
                        grdQuoteComments.ItemsSource = dt2.DefaultView;
                    }
                    catch (SqlException sqlex)
                    {
                        MessageBox.Show("There was a problem generating the quote comments table from the turtleQuote dBase. \n\n" +
                        "Error message: " + sqlex.Message + "\n\n" +
                        "Error no.: " + sqlex.Number.ToString());
                        string message = "Verify you have an internet connection then click 'Yes' to try again or click 'No' to close the application.";
                        if (MessageBox.Show(message, "Try Again?", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                        {
                            goto TryReconnecting;
                        }
                        else
                        {
                            this.Close();
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("There was a problem generating the quote comments table from the turtleQuote dBase. \n\n" +
                        "Error message: " + ex.Message + "\n\nThe application will be shut down.");
                        this.Close();
                    }

                    //SqlCommand cmd2 = new SqlCommand(CmdString, con);
                    //SqlDataAdapter sda2 = new SqlDataAdapter(cmd2);
                    //DataTable dt2 = new DataTable("QuoteComments");
                    //sda2.Fill(dt2);
                    //grdQuoteComments.ItemsSource = dt2.DefaultView;

                    con.Close();
                }
                btnAddComment.Visibility = System.Windows.Visibility.Visible;

                txtblkCustomer.Text = row_selected["CustName"].ToString();
                txtblkOldStatusID.Text = row_selected["status_id"].ToString();
                txtblkOldStatus.Text = row_selected["Status"].ToString();
                txtblkOldCloseDate.Text = row_selected["AwardDateSales"].ToString();
                txtblkQuoteNum.Text= "SPX-" + row_selected["quotenum"].ToString() + "Rev(" + row_selected["Rev"].ToString() + ")";
                double oldCloseDateSerial = double.Parse(txtblkOldCloseDate.Text);
                DateTime oldCloseDate = DateTime.FromOADate(oldCloseDateSerial);
                txtblkOldCloseDateDt.Text = oldCloseDate.ToShortDateString();

                txtblkOldWinProbID.Text = row_selected["WinProbSales_id"].ToString();
                txtblkOldWinProb.Text = row_selected["winpercentamt"].ToString();


                userChangedCloseDate = false;
                CloseDateCalendar.DisplayDate = oldCloseDate;
                userChangedCloseDate = true;
                //DateTime.TryParse("43281", out oldCloseDate);
                //MessageBox.Show("Old Close Date is " + oldCloseDate.ToShortDateString());

                txtblkProjDesc.Text = row_selected["SubjL1"].ToString();
                txtblkNewStatusID.Text = String.Empty;
                txtblkNewStatus.Text = String.Empty;
                txtblkNewCloseDate.Text = String.Empty;
                txtblkNewCloseDateDt.Text = String.Empty;
                txtblkNewWinProbID.Text = String.Empty;
                txtblkNewWinProb.Text = String.Empty;

                userChangedStatus = false;
                cmbxStatus.SelectedValue = row_selected["status_id"];
                userChangedStatus = true;

                userChangedWinProb = false;
                cmbxWinProb.SelectedValue = row_selected["WinProbSales_id"];
                userChangedWinProb = true;

                SetCalendarColor(row_selected["Status"].ToString(), CloseDateCalendar.DisplayDate);
            }
        }

        private void FillComboStatus()
        {
            List<ProjectStatus> projectStatus = new List<ProjectStatus>
            {
                new ProjectStatus() { Id = 1, Status = "Pending"},
                new ProjectStatus() { Id = 2, Status = "Booked"},
                new ProjectStatus() { Id = 3, Status = "Lost"},
                new ProjectStatus() { Id = 4, Status = "Abandoned"}
            };
            cmbxStatus.ItemsSource = projectStatus;
            cmbxStatus.DisplayMemberPath = "Status";
            cmbxStatus.SelectedValuePath = "Id";
        }

        private void FillWinProb()
        {
            List<QuoteWinProb> winProb = new List<QuoteWinProb>
            {
                new QuoteWinProb() { Id = 1, WinProbPercent = 10},
                new QuoteWinProb() { Id = 2, WinProbPercent = 25},
                new QuoteWinProb() { Id = 3, WinProbPercent = 50},
                new QuoteWinProb() { Id = 4, WinProbPercent = 75},
                new QuoteWinProb() { Id = 5, WinProbPercent = 90}
            };
            cmbxWinProb.ItemsSource = winProb;
            cmbxWinProb.DisplayMemberPath = "WinProbPercent";
            cmbxWinProb.SelectedValuePath = "Id";
        }

        private void btnUpdate_Click(object sender, RoutedEventArgs e)
        {
            //Update Projects SET AwardDateSales = ?? WHERE project_id = ??
            //Update ProjectQuotes SET status_id = ??, WinProbSales_id = ?? WHERE quote_id = ??
            //Refresh data from dBase

            //string ConString = ConfigurationManager.ConnectionStrings["ConString"].ConnectionString.ToString();
            string CmdString = string.Empty;
            int updatesApplied = 0;

            quoteID = String.Empty;
            if (txtblkQuoteID.Text == String.Empty)
            {
                //return;   this would exit this method
                goto FillGrid;
            }
            else
            {
                quoteID = txtblkQuoteID.Text;
            }

            //update status_id in ProjectQuotes table
            if (!String.IsNullOrEmpty(txtblkNewStatusID.Text))
            {
                if (txtblkNewStatusID.Text != txtblkOldStatusID.Text)
                {
                    updatesApplied++;
                    string UpdateClause = "UPDATE [dbo].[ProjectQuotes] SET [status_id] = " + txtblkNewStatusID.Text;
                    string WhereClause = "WHERE [quote_id] = " + txtblkQuoteID.Text;
                    using (SqlConnection con = new SqlConnection(ConString))
                    {
                        CmdString = UpdateClause + " " + WhereClause;
                        //MessageBox.Show(CmdString);
                        TryReconnecting:
                        try
                        {
                            con.Open();
                            SqlCommand cmd = new SqlCommand(CmdString, con);
                            cmd.ExecuteNonQuery();
                        }
                        catch (SqlException sqlex)
                        {
                            MessageBox.Show("There was a problem updating the quote data in the turtleQuote dBase. \n\n" +
                                "Error message: " + sqlex.Message + "\n\n" +
                                "Error no.: " + sqlex.Number.ToString());
                            string message = "Verify you have an internet connection then click 'Yes' to try again or click 'No' to close the application.";
                            if (MessageBox.Show(message, "Try Again?", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                            {
                                goto TryReconnecting;
                            }
                            else
                            {
                                this.Close();
                            }
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("There was a problem updating the quote data in the turtleQuote dBase. \n\n" +
                                "Error message: " + ex.Message + "\n\nThe application will be shut down.");
                            this.Close();
                        }

                        //con.Open();
                        //SqlCommand cmd = new SqlCommand(CmdString, con);
                        //cmd.ExecuteNonQuery();

                        con.Close();
                    }
                }
            }
            


            //update WinProbSales_id in ProjectQuotes table
            if (!String.IsNullOrEmpty(txtblkNewWinProbID.Text))
            {
                if (txtblkNewWinProbID.Text != txtblkOldWinProbID.Text)
                {
                    updatesApplied++;
                    string UpdateClause = "UPDATE [dbo].[ProjectQuotes] SET [WinProbSales_id] = " + txtblkNewWinProbID.Text;
                    string WhereClause = "WHERE [quote_id] = " + txtblkQuoteID.Text;
                    using (SqlConnection con = new SqlConnection(ConString))
                    {
                        CmdString = UpdateClause + " " + WhereClause;
                        //MessageBox.Show(CmdString);
                        TryReconnecting:
                        try
                        {
                            con.Open();
                            SqlCommand cmd = new SqlCommand(CmdString, con);
                            cmd.ExecuteNonQuery();
                        }
                        catch (SqlException sqlex)
                        {
                            MessageBox.Show("There was a problem updating the quote data in the turtleQuote dBase. \n\n" +
                                "Error message: " + sqlex.Message + "\n\n" +
                                "Error no.: " + sqlex.Number.ToString());
                            string message = "Verify you have an internet connection then click 'Yes' to try again or click 'No' to close the application.";
                            if (MessageBox.Show(message, "Try Again?", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                            {
                                goto TryReconnecting;
                            }
                            else
                            {
                                this.Close();
                            }
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("There was a problem updating the quote data in the turtleQuote dBase. \n\n" +
                                "Error message: " + ex.Message + "\n\nThe application will be shut down.");
                            this.Close();
                        }

                        //CmdString = UpdateClause + " " + WhereClause;
                        ////MessageBox.Show(CmdString);

                        //con.Open();
                        //SqlCommand cmd = new SqlCommand(CmdString, con);
                        //cmd.ExecuteNonQuery();

                        con.Close();
                    }
                }
            }




            //update AwardDateSales in Projects table
            bool closeDateTooEarly = CloseDateBeforeThisMonth(CloseDateCalendar.DisplayDate);

            string status = String.Empty;
            if (String.IsNullOrEmpty(txtblkNewStatus.Text))
            {
                status = txtblkOldStatus.Text;
            }
            else
            {
                status = txtblkNewStatus.Text;
            }

            if (status == "Pending" && closeDateTooEarly)
            {
                MessageBox.Show("Close date not valid. Select current month or later.");
                grdHaasQuotes.Focus();
                return;
            }
            else
            {
                if (!String.IsNullOrEmpty(txtblkNewCloseDate.Text))
                {
                    //DateTime newDate = CloseDateCalendar.DisplayDate;
                    //DateTime lastDayOfMonth = new DateTime(newDate.Year, newDate.Month,
                    //    DateTime.DaysInMonth(newDate.Year, newDate.Month));
                    //string message = "Displayed Date is " + lastDayOfMonth.ToShortDateString();
                    //MessageBox.Show(message);
                    //double displayedCloseDateSerial = lastDayOfMonth.ToOADate();
                    //txtblkNewCloseDate.Text = displayedCloseDateSerial.ToString();
                    //message = "OA Serial Value of Displayed Date is " + displayedCloseDateSerial.ToString();
                    //MessageBox.Show(message);

                    double oldSerialCloseDate = double.Parse(txtblkOldCloseDate.Text);
                    double newSerialCloseDate = double.Parse(txtblkNewCloseDate.Text);


                    if (oldSerialCloseDate != newSerialCloseDate)
                    {
                        //before writing back to dBase, must adjust serial date b/c of the adjustment made in construction of
                        //dBase view QLDataByQuoteWPF2 due to difference between how serial date is calculated in VBA code
                        //the serial date in QLDataByQuoteWPF2 = serial date stored in Projects - 2. By doing this, C# FromOADate method
                        //converts QLDataByQuoteWPF2 serial date to the same date (e.g 6/30/2018) as the date generated in VBA using
                        //the CDate function using WLDataByQuoteWPF serial date + 2.
                        updatesApplied++;
                        double newSerialCloseDateAdjusted = newSerialCloseDate;

                        //string UpdateClause = "UPDATE [dbo].[Projects] SET [AwardDateSales] = " + txtblkNewCloseDate.Text;
                        string UpdateClause = "UPDATE [dbo].[Projects] SET [AwardDateSales] = " + newSerialCloseDateAdjusted.ToString();
                        string WhereClause = "WHERE [project_id] = " + txtblkProjectID.Text;
                        using (SqlConnection con = new SqlConnection(ConString))
                        {
                            CmdString = UpdateClause + " " + WhereClause;
                            //MessageBox.Show(CmdString);
                            TryReconnecting:
                            try
                            {
                                con.Open();
                                SqlCommand cmd = new SqlCommand(CmdString, con);
                                cmd.ExecuteNonQuery();
                            }
                            catch (SqlException sqlex)
                            {
                                MessageBox.Show("There was a problem updating the quote data in the turtleQuote dBase. \n\n" +
                                    "Error message: " + sqlex.Message + "\n\n" +
                                    "Error no.: " + sqlex.Number.ToString());
                                string message = "Verify you have an internet connection then click 'Yes' to try again or click 'No' to close the application.";
                                if (MessageBox.Show(message, "Try Again?", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                                {
                                    goto TryReconnecting;
                                }
                                else
                                {
                                    this.Close();
                                }
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show("There was a problem updating the quote data in the turtleQuote dBase. \n\n" +
                                    "Error message: " + ex.Message + "\n\nThe application will be shut down.");
                                this.Close();
                            }

                            //CmdString = UpdateClause + " " + WhereClause;
                            ////MessageBox.Show(CmdString);
                            //con.Open();
                            //SqlCommand cmd = new SqlCommand(CmdString, con);
                            //cmd.ExecuteNonQuery();


                            con.Close();
                        }
                    }
                }
            }

            FillGrid:
            //re-apply the existing filter (if applicable)
            if (updatesApplied > 0 && filterAll.Length > 0)
            {
                CmdString = "SELECT * FROM QLDataByQuoteWPF2 " +
                        "ORDER BY quote_id DESC, Rev DESC";
                quoteUpdated = true;
                ApplyFilter(CmdString, filterAll);
                //quoteUpdated = false;
            }
            else if (updatesApplied > 0 && filterAll.Length == 0)
            {
                FillDataGrid();
            }
            else if (updatesApplied == 0 && filterAll.Length > 0)
            {
                //just re-fill the grid
                MessageBox.Show("You did not update a quote. However, quote log has been updated to capture any other user's updates.");
                CmdString = "SELECT * FROM QLDataByQuoteWPF2 " +
                        "ORDER BY quote_id DESC, Rev DESC";
                ApplyFilter(CmdString, filterAll);
            }
            else
            {
                //just re-fill the grid
                MessageBox.Show("You did not update a quote. However, quote log has been updated to capture any other user's updates.");
                FillDataGrid();
            }

            txtblkNumRecords.Text = grdHaasQuotes.Items.Count.ToString();

            //txtblkCustomer.Text = String.Empty;
            //txtblkQuoteNum.Text = String.Empty;
            //txtblkProjDesc.Text = String.Empty;
            //grdQuoteComments.ItemsSource = null;
            //grdQuoteComments.Items.Refresh();
            //btnAddComment.Visibility = System.Windows.Visibility.Hidden;

            //refresh data in datagrid (believe ApplyFilter will do that)
            //FillGrid:
            //FillDataGrid();

            //re-apply filter (if applicable)


            //if user has selected a row, return to the row that was selected before the update
            if (quoteID != String.Empty)
            {
                SelChangedAfterUpdate = true;
                grdHaasQuotes.SelectedValue = int.Parse(quoteID);
                grdHaasQuotes.Focus();
            }
            else
            {
                txtblkCustomer.Text = String.Empty;
                txtblkQuoteNum.Text = String.Empty;
                txtblkProjDesc.Text = String.Empty;
                grdQuoteComments.ItemsSource = null;
                grdQuoteComments.Items.Refresh();
                btnAddComment.Visibility = System.Windows.Visibility.Hidden;
                txtblkQuoteID.Text = String.Empty;

                //if (!quoteUpdated)
                //{
                //    MessageBox.Show("You did not have a quote selected so you did not update anything. \n" +
                //    "However, quote log has been updated to capture any other user's updates.");
                //}
            }
            quoteUpdated = false;
        }

        private void cmbxStatus_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (userChangedStatus)
            {
                ComboBox cmbx = (ComboBox)sender;

                //txtblkNewStatusID.Text = cmbx.ToString();
                int statusID = (int)cmbx.SelectedValue;
                txtblkNewStatusID.Text = cmbx.SelectedValue.ToString();

                switch (statusID)
                {
                    case 1:
                        txtblkNewStatus.Text = "Pending";
                        break;
                    case 2:
                        txtblkNewStatus.Text = "Booked";
                        break;
                    case 3:
                        txtblkNewStatus.Text = "Lost";
                        break;
                    case 4:
                        txtblkNewStatus.Text = "Abandoned";
                        break;
                }
                //txtblkNewStatus.Text = ((DataRowView)cmbx.Items[e.Index])["Name"].ToString;
                //txtblkNewStatus
                string message = "SelectedIndex of cmbxStatus = " + cmbx.SelectedIndex.ToString() + " .";
                //MessageBox.Show(message);
                message = "SelectedValue of cmbxStatus = " + cmbx.SelectedValue.ToString() + " .";
                //MessageBox.Show(message);
                DateTime newDate = CloseDateCalendar.DisplayDate;
                DateTime lastDayOfMonth = new DateTime(newDate.Year, newDate.Month,
                    DateTime.DaysInMonth(newDate.Year, newDate.Month));
                message = "Displayed Date is " + lastDayOfMonth.ToShortDateString();
                //MessageBox.Show(message);
                double displayedCloseDateSerial = lastDayOfMonth.ToOADate();
                message = "OA Serial Value of Displayed Date is " + displayedCloseDateSerial.ToString();
                //MessageBox.Show(message);
            }
            grdHaasQuotes.Focus();
            //userChangedStatus = true;
        }

        private void cmbxWinProb_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (userChangedWinProb)
            {
                ComboBox cmbx = (ComboBox)sender;

                //txtblkNewStatusID.Text = cmbx.ToString();
                int winProbID = (int)cmbx.SelectedValue;
                txtblkNewWinProbID.Text = cmbx.SelectedValue.ToString();

                switch (winProbID)
                {
                    case 1:
                        txtblkNewWinProb.Text = "10";
                        break;
                    case 2:
                        txtblkNewWinProb.Text = "25";
                        break;
                    case 3:
                        txtblkNewWinProb.Text = "50";
                        break;
                    case 4:
                        txtblkNewWinProb.Text = "75";
                        break;
                    case 5:
                        txtblkNewWinProb.Text = "90";
                        break;
                }

                string message = "SelectedIndex of cmbxWinProb = " + cmbx.SelectedIndex.ToString() + " .";
                //MessageBox.Show(message);
                message = "SelectedValue of cmbxWinProb = " + cmbx.SelectedValue.ToString() + " .";
                //MessageBox.Show(message);

                /*
                DateTime todaysDate = DateTime.Today;
                CloseDateCalendar.DisplayDate = new DateTime(todaysDate.Year, todaysDate.Month,
                    DateTime.DaysInMonth(todaysDate.Year, todaysDate.Month));
                message = "Displayed Date is " + CloseDateCalendar.DisplayDate.ToShortDateString();
                MessageBox.Show(message);
                */
            }
            grdHaasQuotes.Focus();
            //userChangedWinProb = true;
        }

        /*
        private void Calendar_SelectedDatesChanged(object sender, SelectionChangedEventArgs e)
        {
            var calendar = sender as Calendar;

            if (calendar.SelectedDate.HasValue)
            {
                DateTime date = calendar.SelectedDate.Value;
                string message = "Selected Date is " + date.ToShortDateString();
                MessageBox.Show(message);
            }
        }
        */

        private void btnFilter_Click(object sender, RoutedEventArgs e)
        {
            filterAll = String.Empty;

            string filterMkt = String.Empty;
            string filterDistributor = String.Empty;
            string filterCountry = string.Empty;
            string filterCloseDate = String.Empty;
            string filterLPVal = String.Empty;
            string filterHPVal = String.Empty;
            string filterPFVal = String.Empty;
            string filterTotalVal = String.Empty;
            string filterRegion = String.Empty;
            string filterQuoteBy = String.Empty;
            string filterRespParty = String.Empty;
            string filterStatus = String.Empty;
            string filterWinProb = String.Empty;

            /* --- section commented 9/3/18
            if (CheckedFilterExists())
            {
                MessageBox.Show("At least one filter is checked.");
            }
            else
            {
                MessageBox.Show("No checkboxes checked.");
            }
            */

            if (ckbxIndustrial.IsChecked == (bool?)true)
            {
                if (filterMkt == String.Empty)
                {
                    filterMkt = "marketdesc = '" + ckbxIndustrial.Content.ToString() + "'";
                }
                else
                {
                    filterMkt = filterMkt + " OR marketdesc = '" + ckbxIndustrial.Content.ToString() + "'";
                }
            }

            if (ckbxPower.IsChecked == (bool?)true)
            {
                if (filterMkt == String.Empty)
                {
                    filterMkt = "marketdesc = '" + ckbxPower.Content.ToString() + "'";
                }
                else
                {
                    filterMkt = filterMkt + " OR marketdesc = '" + ckbxPower.Content.ToString() + "'";
                }
            }

            if (ckbxTransportation.IsChecked == (bool?)true)
            {
                if (filterMkt == String.Empty)
                {
                    filterMkt = "marketdesc = '" + ckbxTransportation.Content.ToString() + "'";
                }
                else
                {
                    filterMkt = filterMkt + " OR marketdesc = '" + ckbxTransportation.Content.ToString() + "'";
                }
            }

            if (ckbxCommercial.IsChecked == (bool?)true)
            {
                if (filterMkt == String.Empty)
                {
                    filterMkt = "marketdesc = '" + ckbxCommercial.Content.ToString() + "'";
                }
                else
                {
                    filterMkt = filterMkt + " OR marketdesc = '" + ckbxCommercial.Content.ToString() + "'";
                }
            }

            if (ckbxSemicon.IsChecked == (bool?)true)
            {
                if (filterMkt == String.Empty)
                {
                    filterMkt = "marketdesc = '" + ckbxSemicon.Content.ToString() + "'";
                }
                else
                {
                    filterMkt = filterMkt + " OR marketdesc = '" + ckbxSemicon.Content.ToString() + "'";
                }
            }

            if (ckbxMarine.IsChecked == (bool?)true)
            {
                if (filterMkt == String.Empty)
                {
                    filterMkt = "marketdesc = '" + ckbxMarine.Content.ToString() + "'";
                }
                else
                {
                    filterMkt = filterMkt + " OR marketdesc = '" + ckbxMarine.Content.ToString() + "'";
                }
            }

            if (filterMkt.Contains(" OR "))
            {
                filterMkt = "(" + filterMkt + ")";
            }

            if (filterMkt != String.Empty)
            {
                filterAll = NewFilterAll(filterMkt, filterAll);
            }



            if (txtbxCustName.Text != String.Empty)
            {
                filterDistributor = "CustName LIKE '%" + txtbxCustName.Text + "%'";
                filterAll = NewFilterAll(filterDistributor, filterAll);
            }

            if (txtbxCountry.Text != String.Empty)
            {
                filterCountry = "country LIKE '%" + txtbxCountry.Text + "%'";
                filterAll = NewFilterAll(filterCountry, filterAll);
            }


            if (ckbxAfrica.IsChecked == (bool?)true)
            {
                if (filterRegion == String.Empty)
                {
                    filterRegion = "region_abbrev = '" + ckbxAfrica.Content.ToString() + "'";
                }
                else
                {
                    filterRegion = filterRegion + " OR region_abbrev = '" + ckbxAfrica.Content.ToString() + "'";
                }
            }
            if (ckbxAsia.IsChecked == (bool?)true)
            {
                if (filterRegion == String.Empty)
                {
                    filterRegion = "region_abbrev = '" + ckbxAsia.Content.ToString() + "'";
                }
                else
                {
                    filterRegion = filterRegion + " OR region_abbrev = '" + ckbxAsia.Content.ToString() + "'";
                }
            }
            if (ckbxAUS.IsChecked == (bool?)true)
            {
                if (filterRegion == String.Empty)
                {
                    filterRegion = "region_abbrev = '" + ckbxAUS.Content.ToString() + "'";
                }
                else
                {
                    filterRegion = filterRegion + " OR region_abbrev = '" + ckbxAUS.Content.ToString() + "'";
                }
            }
            if (ckbxEurope.IsChecked == (bool?)true)
            {
                if (filterRegion == String.Empty)
                {
                    filterRegion = "region_abbrev = '" + ckbxEurope.Content.ToString() + "'";
                }
                else
                {
                    filterRegion = filterRegion + " OR region_abbrev = '" + ckbxEurope.Content.ToString() + "'";
                }
            }
            if (ckbxLACarib.IsChecked == (bool?)true)
            {
                if (filterRegion == String.Empty)
                {
                    filterRegion = "region_abbrev = '" + ckbxLACarib.Content.ToString() + "'";
                }
                else
                {
                    filterRegion = filterRegion + " OR region_abbrev = '" + ckbxLACarib.Content.ToString() + "'";
                }
            }
            if (ckbxMiddleEast.IsChecked == (bool?)true)
            {
                if (filterRegion == String.Empty)
                {
                    filterRegion = "region_abbrev = '" + ckbxMiddleEast.Content.ToString() + "'";
                }
                else
                {
                    filterRegion = filterRegion + " OR region_abbrev = '" + ckbxMiddleEast.Content.ToString() + "'";
                }
            }
            if (ckbxNA.IsChecked == (bool?)true)
            {
                if (filterRegion == String.Empty)
                {
                    filterRegion = "region_abbrev = '" + ckbxNA.Content.ToString() + "'";
                }
                else
                {
                    filterRegion = filterRegion + " OR region_abbrev = '" + ckbxNA.Content.ToString() + "'";
                }
            }

            if (filterRegion.Contains(" OR "))
            {
                filterRegion = "(" + filterRegion + ")";
            }

            if (filterRegion != String.Empty)
            {
                filterAll = NewFilterAll(filterRegion, filterAll);
            }




            string message = String.Empty;
            double closeDateFromSerial = 0;
            DateTime closeDateFromDt;
            if (txtbxCloseFrom.Text != String.Empty)
            {
                if (DateTime.TryParse(txtbxCloseFrom.Text, out closeDateFromDt))
                {
                    closeDateFromSerial = closeDateFromDt.ToOADate();
                }
                else
                {
                    message = "Entry for FROM date is not valid. Enter like 6/30/18. \n Close Date filter will not be appied.";
                    MessageBox.Show(message);
                    goto EndOfCloseDateFilter;
                }
                
            }
            
            double closeDateToSerial = 0;
            DateTime closeDateToDt;
            if (txtbxCloseTo.Text != String.Empty)
            {
                if (DateTime.TryParse(txtbxCloseTo.Text, out closeDateToDt))
                {
                    closeDateToSerial = closeDateToDt.ToOADate();
                }
                else
                {
                    message = "Entry for TO date is not valid. Enter like 6/30/18. \n"
                        + "Close Date filter will not be appied.";
                    MessageBox.Show(message);
                    goto EndOfCloseDateFilter;
                }

            }

            if (closeDateFromSerial > 0 && closeDateToSerial > 0)
            {
                //user want to filter between dates
                if (closeDateFromSerial > closeDateToSerial)
                {
                    message = "FROM date must be before TO date in filter. Revise dates. \n"
                        + "Close Date filter will not be appied.";
                    MessageBox.Show(message);
                    goto EndOfCloseDateFilter;
                }
                else
                {
                    filterCloseDate = "AwardDateSales >= " + Convert.ToString(closeDateFromSerial) +
                        " AND AwardDateSales <= " + Convert.ToString(closeDateToSerial);
                    filterAll = NewFilterAll(filterCloseDate, filterAll);
                }
            }
            else if (closeDateFromSerial == 0 && closeDateToSerial > 0)
            {
                //user want to filter all records before closeDateToSerial
                filterCloseDate = "AwardDateSales <= " + Convert.ToString(closeDateToSerial);
                filterAll = NewFilterAll(filterCloseDate, filterAll);
            }
            else if (closeDateFromSerial > 0 && closeDateToSerial == 0)
            {
                //user want to filter all records after closeDateFromSerial
                filterCloseDate = "AwardDateSales >= " + Convert.ToString(closeDateFromSerial);
                filterAll = NewFilterAll(filterCloseDate, filterAll);
            }

            EndOfCloseDateFilter:


            //set up value filters
            message = String.Empty;
            float LPValMin = 0F;
            if (txtbxLPValMin.Text != String.Empty)
            {
                if (!float.TryParse(txtbxLPValMin.Text, out LPValMin))
                {
                    message = "Entry for min LP value is not valid. Enter like 123000. \n\n Try again.";
                    MessageBox.Show(message, "Invalid Data Entry", MessageBoxButton.OK, MessageBoxImage.Hand);
                    txtbxLPValMin.Focus();
                    txtbxLPValMin.SelectAll();
                    return;
                }
            }

            float LPValMax = 0F;
            if (txtbxLPValMax.Text != String.Empty)
            {
                if (!float.TryParse(txtbxLPValMax.Text, out LPValMax))
                {
                    message = "Entry for max LP value is not valid. Enter like 123000. \n\n Try again.";
                    MessageBox.Show(message, "Invalid Data Entry", MessageBoxButton.OK, MessageBoxImage.Hand);
                    txtbxLPValMax.Focus();
                    txtbxLPValMax.SelectAll();
                    return;
                }
            }

            float HPValMin = 0F;
            if (txtbxHPValMin.Text != String.Empty)
            {
                if (!float.TryParse(txtbxHPValMin.Text, out HPValMin))
                {
                    message = "Entry for min HP value is not valid. Enter like 123000. \n\n Try again.";
                    MessageBox.Show(message, "Invalid Data Entry", MessageBoxButton.OK, MessageBoxImage.Hand);
                    txtbxHPValMin.Focus();
                    txtbxHPValMin.SelectAll();
                    return;
                }
            }

            float HPValMax = 0F;
            if (txtbxHPValMax.Text != String.Empty)
            {
                if (!float.TryParse(txtbxHPValMax.Text, out HPValMax))
                {
                    message = "Entry for max HP value is not valid. Enter like 123000. \n\n Try again.";
                    MessageBox.Show(message, "Invalid Data Entry", MessageBoxButton.OK, MessageBoxImage.Hand);
                    txtbxHPValMax.Focus();
                    txtbxHPValMax.SelectAll();
                    return;
                }
            }

            float PFValMin = 0F;
            if (txtbxPFValMin.Text != String.Empty)
            {
                if (!float.TryParse(txtbxPFValMin.Text, out PFValMin))
                {
                    message = "Entry for min pipe/fittings value is not valid. Enter like 123000. \n\n Try again.";
                    MessageBox.Show(message, "Invalid Data Entry", MessageBoxButton.OK, MessageBoxImage.Hand);
                    txtbxPFValMin.Focus();
                    txtbxPFValMin.SelectAll();
                    return;
                }
            }

            float PFValMax = 0F;
            if (txtbxPFValMax.Text != String.Empty)
            {
                if (!float.TryParse(txtbxPFValMax.Text, out PFValMax))
                {
                    message = "Entry for max pipe/fittings value is not valid. Enter like 123000. \n\n Try again.";
                    MessageBox.Show(message, "Invalid Data Entry", MessageBoxButton.OK, MessageBoxImage.Hand);
                    txtbxPFValMax.Focus();
                    txtbxPFValMax.SelectAll();
                    return;
                }
            }

            float TotalValMin = 0F;
            if (txtbxTotalValMin.Text != String.Empty)
            {
                if (!float.TryParse(txtbxTotalValMin.Text, out TotalValMin))
                {
                    message = "Entry for min total value is not valid. Enter like 123000. \n\n Try again.";
                    MessageBox.Show(message, "Invalid Data Entry", MessageBoxButton.OK, MessageBoxImage.Hand);
                    txtbxTotalValMin.Focus();
                    txtbxTotalValMin.SelectAll();
                    return;
                }
            }

            float TotalValMax = 0F;
            if (txtbxTotalValMax.Text != String.Empty)
            {
                if (!float.TryParse(txtbxTotalValMax.Text, out TotalValMax))
                {
                    message = "Entry for max total value is not valid. Enter like 123000. \n\n Try again.";
                    MessageBox.Show(message, "Invalid Data Entry", MessageBoxButton.OK, MessageBoxImage.Hand);
                    txtbxTotalValMax.Focus();
                    txtbxTotalValMax.SelectAll();
                    return;
                }
            }




            //LPVal filter
            if (LPValMin > 0 && LPValMax > 0)
            {
                //user want to filter between LP values
                if (LPValMin > LPValMax)
                {
                    message = "Minimum LP Val must be less than maximum LP Val in filter. Revise values. \n"
                        + "LP Val filter will not be appied.";
                    MessageBox.Show(message, "Invalid Data Entry", MessageBoxButton.OK, MessageBoxImage.Hand);
                    goto EndOfLPValFilter;
                }
                else
                {
                    filterLPVal = "LPVal >= " + Convert.ToString(LPValMin) +
                        " AND LPVal <= " + Convert.ToString(LPValMax);
                    filterAll = NewFilterAll(filterLPVal, filterAll);
                }
            }
            else if (LPValMin == 0 && LPValMax > 0)
            {
                //user want to filter all records with value < LPValMax
                filterLPVal = "LPVal <= " + Convert.ToString(LPValMax);
                filterAll = NewFilterAll(filterLPVal, filterAll);
            }
            else if (LPValMin > 0 && LPValMax == 0)
            {
                //user want to filter all records with value > LPValMin
                filterLPVal = "LPVal >= " + Convert.ToString(LPValMin);
                filterAll = NewFilterAll(filterLPVal, filterAll);
            }

            EndOfLPValFilter:


            //HPVal filter
            if (HPValMin > 0 && HPValMax > 0)
            {
                //user want to filter between HP Values
                if (HPValMin > HPValMax)
                {
                    message = "Minimum HP Val must be less than maximum HP Val in filter. Revise values. \n"
                        + "HP Val filter will not be appied.";
                    MessageBox.Show(message, "Invalid Data Entry", MessageBoxButton.OK, MessageBoxImage.Hand);
                    goto EndOfHPValFilter;
                }
                else
                {
                    filterHPVal = "HPVal >= " + Convert.ToString(HPValMin) +
                        " AND HPVal <= " + Convert.ToString(HPValMax);
                    filterAll = NewFilterAll(filterHPVal, filterAll);
                }
            }
            else if (HPValMin == 0 && HPValMax > 0)
            {
                //user want to filter all records with value < HPValMax
                filterHPVal = "HPVal <= " + Convert.ToString(HPValMax);
                filterAll = NewFilterAll(filterHPVal, filterAll);
            }
            else if (HPValMin > 0 && HPValMax == 0)
            {
                //user want to filter all records with value > HPValMin
                filterHPVal = "HPVal >= " + Convert.ToString(HPValMin);
                filterAll = NewFilterAll(filterHPVal, filterAll);
            }

            EndOfHPValFilter:


            //PFVal filter
            if (PFValMin > 0 && PFValMax > 0)
            {
                //user want to filter between PF Values
                if (PFValMin > PFValMax)
                {
                    message = "Minimum PF Val must be less than maximum PF Val in filter. Revise values. \n"
                        + "PF Val filter will not be appied.";
                    MessageBox.Show(message, "Invalid Data Entry", MessageBoxButton.OK, MessageBoxImage.Hand);
                    goto EndOfPFValFilter;
                }
                else
                {
                    filterPFVal = "PFVal >= " + Convert.ToString(PFValMin) +
                        " AND PFVal <= " + Convert.ToString(PFValMax);
                    filterAll = NewFilterAll(filterPFVal, filterAll);
                }
            }
            else if (PFValMin == 0 && PFValMax > 0)
            {
                //user want to filter all records with value < PFValMax
                filterPFVal = "PFVal <= " + Convert.ToString(PFValMax);
                filterAll = NewFilterAll(filterPFVal, filterAll);
            }
            else if (PFValMin > 0 && PFValMax == 0)
            {
                //user want to filter all records with value > PFValMin
                filterPFVal = "PFVal >= " + Convert.ToString(PFValMin);
                filterAll = NewFilterAll(filterPFVal, filterAll);
            }

            EndOfPFValFilter:


            //TotalVal filter
            if (TotalValMin > 0 && TotalValMax > 0)
            {
                //user want to filter between Total Values
                if (TotalValMin > TotalValMax)
                {
                    message = "Minimum Total Val must be less than maximum Total Val in filter. Revise values. \n"
                        + "Total Val filter will not be appied.";
                    MessageBox.Show(message, "Invalid Data Entry", MessageBoxButton.OK, MessageBoxImage.Hand);
                    goto EndOfTotalValFilter;
                }
                else
                {
                    filterTotalVal = "TotalVal >= " + Convert.ToString(TotalValMin) +
                        " AND TotalVal <= " + Convert.ToString(TotalValMax);
                    filterAll = NewFilterAll(filterTotalVal, filterAll);
                }
            }
            else if (TotalValMin == 0 && TotalValMax > 0)
            {
                //user want to filter all records with value < TotalValMax
                filterTotalVal = "TotalVal <= " + Convert.ToString(TotalValMax);
                filterAll = NewFilterAll(filterTotalVal, filterAll);
            }
            else if (TotalValMin > 0 && TotalValMax == 0)
            {
                //user want to filter all records with value > TotalValMin
                filterTotalVal = "TotalVal >= " + Convert.ToString(TotalValMin);
                filterAll = NewFilterAll(filterTotalVal, filterAll);
            }

            EndOfTotalValFilter:








            if (ckbxQuoteByGH.IsChecked == (bool?)true)
            {
                if (filterQuoteBy == String.Empty)
                {
                    filterQuoteBy = "QGenInitials = '" + ckbxQuoteByGH.Content.ToString() + "'";
                }
                else
                {
                    filterQuoteBy = filterQuoteBy + " OR QGenInitials = '" + ckbxQuoteByGH.Content.ToString() + "'";
                }
            }
            if (ckbxQuoteByEH.IsChecked == (bool?)true)
            {
                if (filterQuoteBy == String.Empty)
                {
                    filterQuoteBy = "QGenInitials = '" + ckbxQuoteByEH.Content.ToString() + "'";
                }
                else
                {
                    filterQuoteBy = filterQuoteBy + " OR QGenInitials = '" + ckbxQuoteByEH.Content.ToString() + "'";
                }
            }
            if (ckbxQuoteByPH.IsChecked == (bool?)true)
            {
                if (filterQuoteBy == String.Empty)
                {
                    filterQuoteBy = "QGenInitials = '" + ckbxQuoteByPH.Content.ToString() + "'";
                }
                else
                {
                    filterQuoteBy = filterQuoteBy + " OR QGenInitials = '" + ckbxQuoteByPH.Content.ToString() + "'";
                }
            }

            if (filterQuoteBy.Contains(" OR "))
            {
                filterQuoteBy = "(" + filterQuoteBy + ")";
            }

            if (filterQuoteBy != String.Empty)
            {
                filterAll = NewFilterAll(filterQuoteBy, filterAll);
            }

            if (ckbxRespPartyGH.IsChecked == (bool?)true)
            {
                if (filterRespParty == String.Empty)
                {
                    filterRespParty = "RespPartyInitials = '" + ckbxRespPartyGH.Content.ToString() + "'";
                }
                else
                {
                    filterRespParty = filterRespParty + " OR RespPartyInitials = '" + ckbxRespPartyGH.Content.ToString() + "'";
                }
            }
            if (ckbxRespPartyEH.IsChecked == (bool?)true)
            {
                if (filterRespParty == String.Empty)
                {
                    filterRespParty = "RespPartyInitials = '" + ckbxRespPartyEH.Content.ToString() + "'";
                }
                else
                {
                    filterRespParty = filterRespParty + " OR RespPartyInitials = '" + ckbxRespPartyEH.Content.ToString() + "'";
                }
            }
            if (ckbxRespPartyAP.IsChecked == (bool?)true)
            {
                if (filterRespParty == String.Empty)
                {
                    filterRespParty = "RespPartyInitials = '" + ckbxRespPartyAP.Content.ToString() + "'";
                }
                else
                {
                    filterRespParty = filterRespParty + " OR RespPartyInitials = '" + ckbxRespPartyAP.Content.ToString() + "'";
                }
            }
            if (ckbxRespPartyPH.IsChecked == (bool?)true)
            {
                if (filterRespParty == String.Empty)
                {
                    filterRespParty = "RespPartyInitials = '" + ckbxRespPartyPH.Content.ToString() + "'";
                }
                else
                {
                    filterRespParty = filterRespParty + " OR RespPartyInitials = '" + ckbxRespPartyPH.Content.ToString() + "'";
                }
            }

            if (filterRespParty.Contains(" OR "))
            {
                filterRespParty = "(" + filterRespParty + ")";
            }

            if (filterRespParty != String.Empty)
            {
                filterAll = NewFilterAll(filterRespParty, filterAll);
            }

            if (ckbxPending.IsChecked == (bool?)true)
            {
                if (filterStatus == String.Empty)
                {
                    filterStatus = "Status = '" + ckbxPending.Content.ToString() + "'";
                }
                else
                {
                    filterStatus = filterStatus + " OR Status = '" + ckbxPending.Content.ToString() + "'";
                }
            }
            if (ckbxBooked.IsChecked == (bool?)true)
            {
                if (filterStatus == String.Empty)
                {
                    filterStatus = "Status = '" + ckbxBooked.Content.ToString() + "'";
                }
                else
                {
                    filterStatus = filterStatus + " OR Status = '" + ckbxBooked.Content.ToString() + "'";
                }
            }
            if (ckbxLost.IsChecked == (bool?)true)
            {
                if (filterStatus == String.Empty)
                {
                    filterStatus = "Status = '" + ckbxLost.Content.ToString() + "'";
                }
                else
                {
                    filterStatus = filterStatus + " OR Status = '" + ckbxLost.Content.ToString() + "'";
                }
            }
            if (ckbxAbandoned.IsChecked == (bool?)true)
            {
                if (filterStatus == String.Empty)
                {
                    filterStatus = "Status = '" + ckbxAbandoned.Content.ToString() + "'";
                }
                else
                {
                    filterStatus = filterStatus + " OR Status = '" + ckbxAbandoned.Content.ToString() + "'";
                }
            }

            if (filterStatus.Contains(" OR "))
            {
                filterStatus = "(" + filterStatus + ")";
            }

            if (filterStatus != String.Empty)
            {
                filterAll = NewFilterAll(filterStatus, filterAll);
            }


            if (ckbx10.IsChecked == (bool?)true)
            {
                if (filterWinProb == String.Empty)
                {
                    filterWinProb = "winpercentamt = '" + ckbx10.Content.ToString() + "'";
                }
                else
                {
                    filterWinProb = filterWinProb + " OR winpercentamt = '" + ckbx10.Content.ToString() + "'";
                }
            }
            if (ckbx25.IsChecked == (bool?)true)
            {
                if (filterWinProb == String.Empty)
                {
                    filterWinProb = "winpercentamt = '" + ckbx25.Content.ToString() + "'";
                }
                else
                {
                    filterWinProb = filterWinProb + " OR winpercentamt = '" + ckbx25.Content.ToString() + "'";
                }
            }
            if (ckbx50.IsChecked == (bool?)true)
            {
                if (filterWinProb == String.Empty)
                {
                    filterWinProb = "winpercentamt = '" + ckbx50.Content.ToString() + "'";
                }
                else
                {
                    filterWinProb = filterWinProb + " OR winpercentamt = '" + ckbx50.Content.ToString() + "'";
                }
            }
            if (ckbx75.IsChecked == (bool?)true)
            {
                if (filterWinProb == String.Empty)
                {
                    filterWinProb = "winpercentamt = '" + ckbx75.Content.ToString() + "'";
                }
                else
                {
                    filterWinProb = filterWinProb + " OR winpercentamt = '" + ckbx75.Content.ToString() + "'";
                }
            }
            if (ckbx90.IsChecked == (bool?)true)
            {
                if (filterWinProb == String.Empty)
                {
                    filterWinProb = "winpercentamt = '" + ckbx90.Content.ToString() + "'";
                }
                else
                {
                    filterWinProb = filterWinProb + " OR winpercentamt = '" + ckbx90.Content.ToString() + "'";
                }
            }

            if (filterWinProb.Contains(" OR "))
            {
                filterWinProb = "(" + filterWinProb + ")";
            }

            if (filterWinProb != String.Empty)
            {
                filterAll = NewFilterAll(filterWinProb, filterAll);
            }

            //MessageBox.Show("filterAll is " + filterAll);

            if (filterAll != String.Empty)
            {
                //string ConString = ConfigurationManager.ConnectionStrings["ConString"].ConnectionString.ToString();
                //string CmdString = string.Empty;
                string CmdString = "SELECT * FROM QLDataByQuoteWPF2 " +
                        "ORDER BY quote_id DESC, Rev DESC";
                ApplyFilter(CmdString, filterAll);
                /*
                 * below code section moved to ApplyFilter method
                using (SqlConnection con = new SqlConnection(ConString))
                {
                    //CmdString = "SELECT * FROM QLDataByQuoteWPF2 " +
                    //    "ORDER BY quote_id DESC, Rev DESC";
                    SqlCommand cmd = new SqlCommand(CmdString, con);
                    SqlDataAdapter sda = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable("HaasQuotes");

                    sda.Fill(dt);

                    DataView dv = new DataView(dt);
                    //dv.RowFilter = "market_id = 2";
                    //dv.RowFilter = "CustName LIKE '%Levitt%' AND market_id = 2";
                    //dv.RowFilter = "country LIKE '%USA%' AND market_id = 2";
                    //dv.RowFilter = "NumSystems > 1 AND country LIKE '%USA%'";
                    //dv.RowFilter = "status_id < 3 AND RespPartyInitials = 'GH'";
                    //dv.RowFilter = "marketdesc = 'Industrial' OR marketdesc = 'Power'";

                    dv.RowFilter = filterAll;

                    grdHaasQuotes.ItemsSource = dv;

                    //grdHaasQuotes.ItemsSource = dt.DefaultView;

                    con.Close();

                }
                */


                txtblkNumRecords.Text = grdHaasQuotes.Items.Count.ToString();

                txtblkCustomer.Text = String.Empty;
                txtblkQuoteNum.Text = String.Empty;
                txtblkProjDesc.Text = String.Empty;
                grdQuoteComments.ItemsSource = null;
                grdQuoteComments.Items.Refresh();
                btnAddComment.Visibility = System.Windows.Visibility.Hidden;
                txtblkQuoteID.Text = String.Empty;
                quoteID = String.Empty;

            } 
        }

        private string NewFilterAll(string paramFilter, string OldFilterAll)
        {
            if (OldFilterAll == String.Empty)
            {
                OldFilterAll = paramFilter;
            }
            else
            {
                OldFilterAll = OldFilterAll + " AND " + paramFilter;
            }
            return OldFilterAll;
        }

        private void ApplyFilter(string commandString, string filterString)
        {
            using (SqlConnection con = new SqlConnection(ConString))
            {
                //CmdString = "SELECT * FROM QLDataByQuoteWPF2 " +
                //    "ORDER BY quote_id DESC, Rev DESC";
                SqlCommand cmd = new SqlCommand(commandString, con);
                SqlDataAdapter sda = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable("HaasQuotes");

                TryReconnecting:
                try
                {
                    sda.Fill(dt);

                    DataView dv = new DataView(dt);
                    //dv.RowFilter = "market_id = 2";
                    //dv.RowFilter = "CustName LIKE '%Levitt%' AND market_id = 2";
                    //dv.RowFilter = "country LIKE '%USA%' AND market_id = 2";
                    //dv.RowFilter = "NumSystems > 1 AND country LIKE '%USA%'";
                    //dv.RowFilter = "status_id < 3 AND RespPartyInitials = 'GH'";
                    //dv.RowFilter = "marketdesc = 'Industrial' OR marketdesc = 'Power'";

                    dv.RowFilter = filterString;

                    //if filtering after an update, check to see if the updated quote_id is still in the data set
                    //if not, clear upper LH textboxes and string quoteID
                    if (quoteUpdated)
                    {
                        SelChangedAfterUpdate = true;
                        dv.Sort = "quote_id";
                        int index = dv.Find(quoteID);

                        if (index < 0)
                        {
                            //quoteID was not in the dataset after filtering
                            quoteID = String.Empty;
                        }
                    }

                    grdHaasQuotes.ItemsSource = dv;
                }
                catch (SqlException sqlex)
                {
                    MessageBox.Show("There was a problem applying the filter to data in the turtleQuote dBase. \n\n" +
                                "Error message: " + sqlex.Message + "\n\n" +
                                "Error no.: " + sqlex.Number.ToString());
                    string message = "Verify you have an internet connection then click 'Yes' to try again or click 'No' to close the application.";
                    if (MessageBox.Show(message, "Try Again?", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                    {
                        goto TryReconnecting;
                    }
                    else
                    {
                        this.Close();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("There was a problem applying the filter to data in the turtleQuote dBase. \n\n" +
                                "Error message: " + ex.Message + "\n\nThe application will be shut down.");
                    this.Close();
                }
                

                //grdHaasQuotes.ItemsSource = dt.DefaultView;

                con.Close();

            }
        }

        private void CloseDateCalendar_DisplayDateChanged(object sender, CalendarDateChangedEventArgs e)
        {
            if (userChangedCloseDate)
            {
                DateTime newDate = CloseDateCalendar.DisplayDate;
                DateTime lastDayOfDisplayedMonth = new DateTime(newDate.Year, newDate.Month,
                    DateTime.DaysInMonth(newDate.Year, newDate.Month));

                string status = String.Empty;
                if (String.IsNullOrEmpty(txtblkNewStatus.Text))
                {
                    status = txtblkOldStatus.Text;
                }
                else
                {
                    status = txtblkNewStatus.Text;
                }

                SetCalendarColor(status, CloseDateCalendar.DisplayDate);

                double displayedCloseDateSerial = lastDayOfDisplayedMonth.ToOADate();
                txtblkNewCloseDate.Text = displayedCloseDateSerial.ToString();

                double newSerialCloseDate = double.Parse(txtblkNewCloseDate.Text);
                txtblkNewCloseDateDt.Text = lastDayOfDisplayedMonth.ToShortDateString();
                userChangedDate = true;
                //CloseDateCalendar.SelectedDate = lastDayOfDisplayedMonth;
            }
            //grdHaasQuotes.Focus();
        }

        private bool CloseDateBeforeThisMonth(DateTime displayedCalendarDate)
        {
            DateTime lastDayOfDisplayedMonth = new DateTime(displayedCalendarDate.Year, displayedCalendarDate.Month,
                    DateTime.DaysInMonth(displayedCalendarDate.Year, displayedCalendarDate.Month));
            DateTime thisDay = DateTime.Today;
            DateTime lastDayOfThisMonth = new DateTime(thisDay.Year, thisDay.Month,
                    DateTime.DaysInMonth(thisDay.Year, thisDay.Month));
            int result = DateTime.Compare(lastDayOfThisMonth, lastDayOfDisplayedMonth);

            bool isCloseDateBeforeThisMonth = true;
            if (result <= 0)
            {
                //lastDayOfThisMonth is before or equal to lastDayOfDisplayedMonth
                isCloseDateBeforeThisMonth = false;
            }
            return isCloseDateBeforeThisMonth;
        }

        private void SetCalendarColor(string strStatus, DateTime calendarDate)
        {
            bool closeDateTooEarly = CloseDateBeforeThisMonth(calendarDate);

            if (closeDateTooEarly && strStatus == "Pending")
            {
                CloseDateCalendar.Background = Brushes.Red;
            }
            else
            {
                CloseDateCalendar.ClearValue(Control.BackgroundProperty);
            }            
        }

        private void btnClearAllFilters_Click(object sender, RoutedEventArgs e)
        {
            ckbx10.IsChecked = false;
            ckbx25.IsChecked = false;
            ckbx50.IsChecked = false;
            ckbx75.IsChecked = false;
            ckbx90.IsChecked = false;
            ckbxAbandoned.IsChecked = false;
            ckbxAfrica.IsChecked = false;
            ckbxAsia.IsChecked = false;
            ckbxAUS.IsChecked = false;
            ckbxBooked.IsChecked = false;
            ckbxCommercial.IsChecked = false;
            ckbxEurope.IsChecked = false;
            ckbxIndustrial.IsChecked = false;
            ckbxLACarib.IsChecked = false;
            ckbxLost.IsChecked = false;
            ckbxMarine.IsChecked = false;
            ckbxMiddleEast.IsChecked = false;
            ckbxMiddleEast.IsChecked = false;
            ckbxNA.IsChecked = false;
            ckbxPending.IsChecked = false;
            ckbxPower.IsChecked = false;
            ckbxQuoteByEH.IsChecked = false;
            ckbxQuoteByGH.IsChecked = false;
            ckbxQuoteByPH.IsChecked = false;
            ckbxRespPartyAP.IsChecked = false;
            ckbxRespPartyEH.IsChecked = false;
            ckbxRespPartyGH.IsChecked = false;
            ckbxRespPartyPH.IsChecked = false;
            ckbxSemicon.IsChecked = false;
            ckbxTransportation.IsChecked = false;

            txtbxCloseFrom.Text = String.Empty;
            txtbxCloseTo.Text = String.Empty;
            txtbxCountry.Text = String.Empty;
            txtbxCustName.Text = String.Empty;

            FillDataGrid();
            txtblkCustomer.Text = String.Empty;
            txtblkQuoteNum.Text = String.Empty;
            txtblkProjDesc.Text = String.Empty;
            //
            grdQuoteComments.ItemsSource = null;
            grdQuoteComments.Items.Refresh();
            btnAddComment.Visibility = System.Windows.Visibility.Hidden;
            filterAll = String.Empty;
        }

        private void btnReSelect_Click(object sender, RoutedEventArgs e)
        {
            grdHaasQuotes.Focus();
        }

        private void CloseDateCalendar_SelectedDatesChanged(object sender, SelectionChangedEventArgs e)
        {
            MessageBox.Show("SelectedDateChanged event fired.");
            grdHaasQuotes.Focus();
        }

        private void CloseDateCalendar_MouseLeave(object sender, MouseEventArgs e)
        {
            if (userChangedDate)
            {
                grdHaasQuotes.Focus();
                userChangedDate = false;
            }
        }

        private void ReselectRow(bool reSelectRow)
        {
            if (reSelectRow)
            {
                grdHaasQuotes.Focus();
            }
        }

        private void ckbxIndustrial_Click(object sender, RoutedEventArgs e)
        {
            grdHaasQuotes.Focus();
        }

        private void ckbxPower_Click(object sender, RoutedEventArgs e)
        {
            grdHaasQuotes.Focus();
        }

        private void ckbxTransportation_Click(object sender, RoutedEventArgs e)
        {
            grdHaasQuotes.Focus();
        }

        private void ckbxCommercial_Click(object sender, RoutedEventArgs e)
        {
            grdHaasQuotes.Focus();
        }

        private void ckbxAfrica_Click(object sender, RoutedEventArgs e)
        {
            grdHaasQuotes.Focus();
        }

        private void ckbxAsia_Click(object sender, RoutedEventArgs e)
        {
            grdHaasQuotes.Focus();
        }

        private void ckbxAUS_Click(object sender, RoutedEventArgs e)
        {
            grdHaasQuotes.Focus();
        }

        private void ckbxEurope_Click(object sender, RoutedEventArgs e)
        {
            grdHaasQuotes.Focus();
        }

        private void ckbxLACarib_Click(object sender, RoutedEventArgs e)
        {
            grdHaasQuotes.Focus();
        }

        private void ckbxMiddleEast_Click(object sender, RoutedEventArgs e)
        {
            grdHaasQuotes.Focus();
        }

        private void ckbxNA_Click(object sender, RoutedEventArgs e)
        {
            grdHaasQuotes.Focus();
        }

        private void ckbxQuoteByGH_Click(object sender, RoutedEventArgs e)
        {
            grdHaasQuotes.Focus();
        }

        private void ckbxQuoteByEH_Click(object sender, RoutedEventArgs e)
        {
            grdHaasQuotes.Focus();
        }

        private void ckbxQuoteByPH_Click(object sender, RoutedEventArgs e)
        {
            grdHaasQuotes.Focus();
        }

        private void ckbxRespPartyGH_Click(object sender, RoutedEventArgs e)
        {
            grdHaasQuotes.Focus();
        }

        private void ckbxRespPartyEH_Click(object sender, RoutedEventArgs e)
        {
            grdHaasQuotes.Focus();
        }

        private void ckbxRespPartyAP_Click(object sender, RoutedEventArgs e)
        {
            grdHaasQuotes.Focus();
        }

        private void ckbxRespPartyPH_Click(object sender, RoutedEventArgs e)
        {
            grdHaasQuotes.Focus();
        }

        private void ckbxPending_Click(object sender, RoutedEventArgs e)
        {
            grdHaasQuotes.Focus();
        }

        private void ckbxBooked_Click(object sender, RoutedEventArgs e)
        {
            grdHaasQuotes.Focus();
        }

        private void ckbxLost_Click(object sender, RoutedEventArgs e)
        {
            grdHaasQuotes.Focus();
        }

        private void ckbxAbandoned_Click(object sender, RoutedEventArgs e)
        {
            grdHaasQuotes.Focus();
        }

        private void ckbx10_Click(object sender, RoutedEventArgs e)
        {
            grdHaasQuotes.Focus();
        }

        private void ckbx25_Click(object sender, RoutedEventArgs e)
        {
            grdHaasQuotes.Focus();
        }

        private void ckbx50_Click(object sender, RoutedEventArgs e)
        {
            grdHaasQuotes.Focus();
        }

        private void ckbx75_Click(object sender, RoutedEventArgs e)
        {
            grdHaasQuotes.Focus();
        }

        private void ckbx90_Click(object sender, RoutedEventArgs e)
        {
            grdHaasQuotes.Focus();
        }

        private void btnAddComment_Click(object sender, RoutedEventArgs e)
        {
            QuoteComment qcomment = new QuoteComment(intUserID.ToString(), quoteID);
            //qcomment.setCreatingWindow = this;
            if (qcomment.ShowDialog() == true)
            {
                MessageBox.Show("Comment has been added.");

                //string ConString = ConfigurationManager.ConnectionStrings["ConString"].ConnectionString.ToString();
                string CmdString = string.Empty;
                using (SqlConnection con = new SqlConnection(ConString))
                {
                    CmdString = "SELECT CONVERT(varchar(8),CONVERT(datetime, [qcdate] - 2), 1) AS Date, " +
                    "U.[UserInitials] AS Who, [qcomment] AS Comment, [qc_id], QC.[quote_id] " +
                    "FROM[turtleQuote].[dbo].[QuoteComments] AS QC " +
                    "INNER JOIN ProjectQuotes AS PQ ON QC.quote_id = PQ.quote_id " +
                    "INNER JOIN Customers AS C ON PQ.cust_id = C.cust_id " +
                    "INNER JOIN Users AS U ON QC.user_id = U.user_id " +
                    "WHERE QC.quote_id = " + quoteID + " " +
                    "ORDER BY [qc_id] DESC";
                    SqlCommand cmd2 = new SqlCommand(CmdString, con);
                    SqlDataAdapter sda2 = new SqlDataAdapter(cmd2);
                    DataTable dt2 = new DataTable("QuoteComments");
                    sda2.Fill(dt2);
                    grdQuoteComments.ItemsSource = dt2.DefaultView;

                    con.Close();
                }

            }
            else
            {
                MessageBox.Show("No comment was added.");
            }
            grdHaasQuotes.Focus();
        }

        public string quoteIDValue
        {
            get { return quoteID; }
        }

        public string userIDValue
        {
            get { return intUserID.ToString(); }
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Debug.WriteLine("Mouse Down");
        }

        private void txtbxCustName_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            Debug.WriteLine("txtbxCustName Mouse Up");
        }

        private void txtbxCustName_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            Debug.WriteLine("txtbxCustName Mouse Down");
        }

        private void CloseDateCalendar_DisplayModeChanged(object sender, CalendarModeChangedEventArgs e)
        {
            if (CloseDateCalendar.DisplayMode == CalendarMode.Decade)
            {
                //2 is enumeration for Decade Mode
                CloseDateCalendar.DisplayMode = CalendarMode.Month;
            }
        }

        private void grdHaasQuotes_AutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            if (e.PropertyName == "project_id" || e.PropertyName == "quote_id" || e.PropertyName == "status_id"
                || e.PropertyName == "AwardDateSales" || e.PropertyName == "WinProbSales_id")
            {
                e.Column.Visibility = Visibility.Collapsed;
            }

            if (e.PropertyName == "SubjL1" || e.PropertyName == "CustName" || e.PropertyName == "EndUser"
                || e.PropertyName == "Contact")
            {
                e.Column.Width = 150;
            }

            if (e.PropertyName == "quotenum" || e.PropertyName == "NumSystems" || e.PropertyName == "CustCity"
                || e.PropertyName == "QGenInitials" || e.PropertyName == "RespPartyInitials" || e.PropertyName == "winpercentamt")
            {
                e.Column.Width = 35;
            }

            if (e.PropertyType == typeof(DateTime))
            {
                (e.Column as DataGridTextColumn).Binding.StringFormat = "MM/dd/yy";
                //above keeps data type stored in col as DateTime but forces it to display as MM/dd/yy.  This produces "normal" UI display
                //but preserves DateTime format so that datagrid's automatic sorting works properly.
            }
        }


        /* SECTION BELOW COMMENTED 9/3/18
        private bool CheckedFilterExists()
        {
            //IEnumerable<CheckBox> myBoxes = FindVisualChildren<CheckBox>(this);  (THIS LINE WAS THROWING ERROR SO COMMENTED 9/3/18)

            int numChecked = 0;
            CheckBox cb;
            foreach (var item in LogicalTreeHelper.GetChildren(lstbxMarkets))
            {
                if(item is CheckBox)
                {
                    cb = (CheckBox)item;
                    if((bool)cb.IsChecked == true)
                    {
                        numChecked++;
                    }  
                }

                if (numChecked > 0)
                    break;
            }

            if (numChecked > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        END OF CODE SECTION COMMENTED 9/3/18*/

    }

    public class IsBeforeTodayConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            /*
            if (value is string)
            {
                string strDate = value.ToString();
                DateTime date = DateTime.Parse(strDate);
                return date < DateTime.Today;
            }
            else
            {
                return DependencyProperty.UnsetValue;
            }
            */
            
            if (value is DateTime)
            {
                return ((DateTime)value).Date < DateTime.Today;
            }
            else
            {
                return DependencyProperty.UnsetValue;
            }
            
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }

    public class ProjectStatus
    {
        public int Id { get; set; }
        public string Status { get; set; }
    }

    public class QuoteWinProb
    {
        public int Id { get; set; }
        public int WinProbPercent { get; set; }
    }
    
}
