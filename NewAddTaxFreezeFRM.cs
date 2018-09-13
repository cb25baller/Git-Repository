using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace RC_Tax_Freeze_Updated
{
    public partial class NewAddTaxFreezeFRM : Form
    {
        public NewAddTaxFreezeFRM()
        {
            InitializeComponent();
        }
        int taxYearThatUserIsWorkingIn;
        Class1 acctData = new Class1();
        Cons myCons = new Cons();
        SqlConnection con;
        SqlDataReader dr;
        SqlCommand cmd;
        int TFID_ToUseIfRequal;
        UnfrozenLand unfrznLand;
        UnfrozenBuilding unfrznBuilding;
        UnfrozenSFYIorMH unfrznSFYI;

        private void NewAddTaxFreezeFRM_Load(object sender, EventArgs e)
        {
            TaxYearFRM ty = new TaxYearFRM();
            ty.ShowDialog();
            taxYearThatUserIsWorkingIn = int.Parse(ty.taxYear);
            label28.Text = "Tax Year " + taxYearThatUserIsWorkingIn.ToString();
            groupBox1.Text = taxYearThatUserIsWorkingIn.ToString() + " Value Data";
        }

        private void button1_Click(object sender, EventArgs e)
        {
            acctData = new Class1();
            int acct;
            if (int.TryParse(textBox1.Text, out acct))
            {
                acctData.propertyID = acct;
                acctData.taxYear = taxYearThatUserIsWorkingIn;

                if (CheckIfAcctAlreadyInputForCurrentYear() == false)
                {
                    GetAcctCurrentData();
                    FillUnfrozenQualificationsDGV();
                    FillPriorFrozenInfo();
                    if(CheckUnfrozenDataGrid())
                    {
                        if (false == true) //this should be a check for reval years
                        {
                            MessageBox.Show("Records indicate that this account will have Unfrozen Calculations. The values for Tax Year 2018 are not set, and therefore, Unfrozen Calculations cannot be made at this.");
                            acctData = new Class1();
                            dataGridView1.DataSource = null;
                            return;
                        }
                        else
                        {
                            MessageBox.Show("Records indicate that this account will have Unfrozen Calculations. Please double check those calculations before saving.");
                            int unfrzApp = 0, unfrznAssess = 0;
                            if (float.Parse(dataGridView1.Rows[0].Cells[1].Value.ToString()) > 5)
                            {
                                unfrznLand = new UnfrozenLand();
                                unfrznLand.ExecuteUnfrozenLandCalc(acctData.propertyID, acctData.taxYear);
                                unfrzApp = unfrzApp + unfrznLand.totalUnfrozenLand;
                                unfrznAssess = unfrznAssess + unfrznLand.totalUnfrozenLandAssess;
                            }
                           if(int.Parse(dataGridView1.Rows[1].Cells[1].Value.ToString()) > 1 )
                           {
                                unfrznBuilding = new UnfrozenBuilding();
                                unfrznBuilding.ExecuteUnfrozenBuildingCalc(acctData.propertyID, acctData.taxYear);
                                unfrzApp = unfrzApp + unfrznBuilding.totalUnfrozenBuilding;
                                unfrznAssess = unfrznAssess + unfrznBuilding.totalUnfrozenBuildingAssess;
                           }
                           else if(acctData.CheckForUnattachedSFYI())
                           {
                                var runnBuildingCalc = MessageBox.Show("This account has SFYI's that are NOT assigned to a building. Do those need to be UNFROZEN?","", MessageBoxButtons.YesNo);
                                if(runnBuildingCalc == DialogResult.Yes)
                                {
                                    unfrznBuilding = new UnfrozenBuilding();
                                    unfrznBuilding.ExecuteUnfrozenBuildingCalc(acctData.propertyID, acctData.taxYear);
                                    unfrzApp = unfrzApp + unfrznBuilding.totalUnfrozenBuilding;
                                    unfrznAssess = unfrznAssess + unfrznBuilding.totalUnfrozenBuildingAssess;
                                }

                           }
                           if(int.Parse(dataGridView1.Rows[2].Cells[1].Value.ToString()) > 1 || 
                                (int.Parse(dataGridView1.Rows[1].Cells[1].Value.ToString()) > 0 && int.Parse(dataGridView1.Rows[2].Cells[1].Value.ToString()) > 0) )
                           {
                                unfrznSFYI = new UnfrozenSFYIorMH();
                                unfrznSFYI.ExecuteUnfrznSFYICalc(acctData.propertyID, acctData.taxYear);
                                SFYI_SelectFRM select = new SFYI_SelectFRM(unfrznSFYI);
                                select.ShowDialog();
                                unfrzApp = unfrzApp + unfrznSFYI.totalUnfrozenSFYI;
                                unfrznAssess = unfrznAssess + unfrznSFYI.totalUnfrozenSFYIAssess;

                           }


                            acctData.countyUnfrznAppraisal = unfrzApp;
                            acctData.countyUnfrznAssess = unfrznAssess;
                            acctData.countyUnfrznTaxes = 0;
                            if (acctData.district == "515" || acctData.district == "674")
                            {
                                acctData.cityUnfrznAppraisal = unfrzApp;
                                acctData.cityUnfrznAssess = unfrznAssess;
                                acctData.cityUnfrznTaxes = 0;
                            }
                        }
                    }
                    
                    parcelTxtBx.Text = acctData.parcelID;
                    ownerTxtBx.Text = acctData.ownerName;
                    situsAddressTxtBx.Text = acctData.address;
                    districtTxtBx.Text = acctData.district;
                    totalValueTxtBx.Text = acctData.totalAppr.ToString("C0");
                    totalAssValueTxtBx.Text = acctData.totalAssess.ToString("C0");
                    //landAreaTxtBx.Text = acctData.landArea.ToString();

                    
                    if(acctData.countyBaseYear != 0)
                    {                        
                        CheckRequalify(countyRequalChBx, acctData.countyBaseYear);
                        CheckRequalify(cityRequalChBx, acctData.cityBaseYear);
                        
                        FillTextboxes();               
                    }
                    else
                    {
                        FillNewTaxFreezeInfo();
                        if (float.Parse(dataGridView1.Rows[0].Cells[1].Value.ToString()) > 5)
                        {
                            UnfrozenLand unfnLand = new UnfrozenLand();
                            unfnLand.ExecuteUnfrozenLandCalc(acctData.propertyID, acctData.taxYear - 1);
                            acctData.countyFrozenAppraisal = acctData.countyFrozenAppraisal - unfnLand.totalUnfrozenLand;
                            acctData.countyFrozenAssess = acctData.countyFrozenAssess - unfnLand.totalUnfrozenLandAssess;
                            acctData.countyFrozenTaxes = (int)Math.Round(acctData.countyFrozenAssess * acctData.countyFrozenRate/100);
                        }


                            FillTextboxes();
                    }
                    countyGrpBx.Visible = true;
                    if(acctData.district == "515" || acctData.district == "674")
                    { cityGrpBx.Visible = true; }
                }
                else { MessageBox.Show("This account has already been entered for this Tax Year. Please Try another account."); }
            }
            
        }

        void CheckRequalify(CheckBox myCheck, int year)
        {
            if (year < taxYearThatUserIsWorkingIn - 1)
            {
                myCheck.Checked = true;
            }
        }

        void FillTextboxes()
        {
            countyBaseYearTxt.Text = acctData.countyBaseYear.ToString();
            countyFrozenAppTxt.Text = acctData.countyFrozenAppraisal.ToString("C0");
            countyFrznAssessTxt.Text = acctData.countyFrozenAssess.ToString("C0");
            countyFrznDollarsTxt.Text = acctData.countyFrozenTaxes.ToString("C0");
            countyFrznRateTxt.Text = acctData.countyFrozenRate.ToString();
            countyUnfznAppTxt.Text = acctData.countyUnfrznAppraisal.ToString("C0");
            countyUnfznAssessTxt.Text = acctData.countyUnfrznAssess.ToString("C0");
            countyUnfznDollarsTxt.Text = acctData.countyUnfrznTaxes.ToString("C0");
            cityBaseYearTxt.Text = acctData.cityBaseYear.ToString();
            cityFrozenAppTxt.Text = acctData.cityFrozenAppraisal.ToString("C0");
            cityFrznAssessTxt.Text = acctData.cityFrozenAssess.ToString("C0");
            cityFrznDollarsTxt.Text = acctData.cityFrozenTaxes.ToString("C0");
            cityFrznRateTxt.Text = acctData.cityFrozenRate.ToString();
            cityUnfznAppTxt.Text = acctData.cityUnfrznAppraisal.ToString("C0");
            cityUnfznAssessTxt.Text = acctData.cityUnfrznAssess.ToString("C0");
            cityUnfznDollarsTxt.Text = acctData.cityUnfrznTaxes.ToString("C0");
        }

        void FillNewTaxFreezeInfo()
        {
            try
            {
                con = new SqlConnection(myCons.GetFreezeCon());
                con.Open();
                cmd = new SqlCommand("PullNewTaxFreezeNumbers", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@Year", SqlDbType.VarChar).Value = (acctData.taxYear - 1).ToString();
                cmd.Parameters.Add("@PropertyID", SqlDbType.VarChar).Value = acctData.propertyID.ToString();
                dr = cmd.ExecuteReader();
                if (dr.HasRows)
                {
                    while (dr.Read())
                    {
                        acctData.countyBaseYear = acctData.taxYear - 1;
                        acctData.countyFrozenAppraisal = int.Parse(dr.GetValue(1).ToString());
                        acctData.countyFrozenAssess = int.Parse(dr.GetValue(2).ToString());
                        acctData.countyFrozenRate = float.Parse(dr.GetValue(3).ToString());
                        acctData.countyFrozenTaxes = int.Parse(dr.GetValue(4).ToString());
                        if (acctData.district == "515" || acctData.district == "674")
                        {
                            acctData.cityBaseYear = acctData.taxYear - 1;
                            acctData.cityFrozenAppraisal = acctData.countyFrozenAppraisal;
                            acctData.cityFrozenAssess = acctData.countyFrozenAssess;
                            acctData.cityFrozenRate = float.Parse(dr.GetValue(5).ToString());
                            acctData.cityFrozenTaxes = int.Parse(dr.GetValue(6).ToString());
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally { con.Close(); }
        }
    
        bool CheckUnfrozenDataGrid()
        {
            if (float.Parse(dataGridView1.Rows[0].Cells[1].Value.ToString()) > 5 ||
                        int.Parse(dataGridView1.Rows[1].Cells[1].Value.ToString()) > 1 ||
                        int.Parse(dataGridView1.Rows[2].Cells[1].Value.ToString()) > 1 ||
                        dataGridView1.Rows[3].Cells[1].Value.ToString() == "Yes")
            { return true; }
            else return false;
        }

        void FillPriorFrozenInfo()
        {
            try
            {
                con = new SqlConnection(myCons.GetFreezeCon());
                con.Open();
                cmd = new SqlCommand("GetPriorTaxFreezeInformation", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@TaxYear", SqlDbType.Int).Value = acctData.taxYear - 1;
                cmd.Parameters.Add("@ACCTNUM", SqlDbType.Int).Value = acctData.propertyID;
                dr = cmd.ExecuteReader();
                if (dr.HasRows)
                {
                    while (dr.Read())
                    {

                        TFID_ToUseIfRequal = int.Parse(dr.GetValue(0).ToString());
                        acctData.countyBaseYear =         int.Parse(dr.GetValue(2).ToString());
                        acctData.countyFrozenAppraisal =  int.Parse(dr.GetValue(3).ToString());
                        acctData.countyFrozenAssess =     int.Parse(dr.GetValue(4).ToString());
                        acctData.countyFrozenRate =       float.Parse(dr.GetValue(5).ToString());
                        acctData.countyFrozenTaxes =      int.Parse(dr.GetValue(6).ToString());
                        acctData.countyUnfrznAppraisal =  int.Parse(dr.GetValue(7).ToString());
                        acctData.countyUnfrznAssess =     int.Parse(dr.GetValue(8).ToString());
                        acctData.countyUnfrznTaxes =      int.Parse(dr.GetValue(9).ToString());
                        acctData.cityBaseYear =           int.Parse(dr.GetValue(11).ToString());
                        acctData.cityFrozenAppraisal =    int.Parse(dr.GetValue(12).ToString());
                        acctData.cityFrozenAssess = int.Parse(dr.GetValue(13).ToString());
                        acctData.cityFrozenRate =         float.Parse(dr.GetValue(14).ToString());
                        acctData.cityFrozenTaxes = int.Parse(dr.GetValue(15).ToString()); 
                        acctData.cityUnfrznAppraisal = int.Parse(dr.GetValue(16).ToString());
                        acctData.cityUnfrznAssess =       int.Parse(dr.GetValue(17).ToString());
                        acctData.cityUnfrznTaxes =        int.Parse(dr.GetValue(18).ToString());
                        //textBox29.Text = double.Parse(dr.GetValue(17).ToString()).ToString("C0");

                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally { con.Close(); }
        }

        void FillUnfrozenQualificationsDGV()
        {
            DataTable dt = new DataTable();
            SqlConnection myConn = new SqlConnection(myCons.GetFreezeCon());
            myConn.Open();
            try
            {

                SqlCommand myCmd = new SqlCommand("[UnfrozenQualificationPreliminaryCheck]", myConn);
                myCmd.CommandType = CommandType.StoredProcedure;
                myCmd.Parameters.AddWithValue("@YearID", acctData.taxYear);
                myCmd.Parameters.AddWithValue("@AcctNum", acctData.propertyID);
                SqlDataAdapter da = new SqlDataAdapter(myCmd);
                da.Fill(dt);
                dataGridView1.DataSource = dt;
            }
            catch (Exception ex)
            { MessageBox.Show(ex.ToString()); }
            finally { myConn.Close(); }
            dataGridView1.Columns[0].Width = 120;
            dataGridView1.Columns[1].Width = 55;
            dataGridView1.Columns[0].Resizable = DataGridViewTriState.False;
            dataGridView1.Columns[1].Resizable = DataGridViewTriState.False;
            dataGridView1.AllowUserToResizeRows = false;
            dataGridView1.Columns[0].HeaderText = "Unfzn Qualifications";
            dataGridView1.Columns[1].HeaderText = "";
            dataGridView1.RowHeadersVisible=false;
        }

        void GetAcctCurrentData()
        {
            try
            {
                con = new SqlConnection(myCons.GetFreezeCon());
                con.Open();
                cmd = new SqlCommand("TFgetInfo", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@YearID", SqlDbType.Int).Value = acctData.taxYear;
                cmd.Parameters.Add("@AcctNo", SqlDbType.Int).Value = acctData.propertyID;
                dr = cmd.ExecuteReader();
                while (dr.Read())
                {
                    acctData.parcelID = dr.GetValue(1).ToString();
                    acctData.district = dr.GetValue(2).ToString();
                    acctData.ownerName = dr.GetValue(3).ToString() + " " + dr.GetValue(4).ToString().Trim();
                    acctData.address = dr.GetValue(5).ToString();
                    acctData.landArea = int.Parse(dr.GetValue(6).ToString().Trim());
                    acctData.totalAppr = int.Parse(dr.GetValue(7).ToString());
                    acctData.totalAssess = int.Parse(dr.GetValue(8).ToString());
                    acctData.impAppVal = int.Parse(dr.GetValue(9).ToString());
                    acctData.landAppVal = int.Parse(dr.GetValue(10).ToString());
                    
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally { con.Close(); }
        }

        bool CheckIfAcctAlreadyInputForCurrentYear()
        {
            bool alreadyExists = false;
            int countOfAccts = 0;
            try
            {
                con = new SqlConnection(myCons.GetFreezeCon());
                con.Open();
                cmd = new SqlCommand("SELECT count([TFID]) FROM [dbo].[RCTF] WHERE AcctNum=" + acctData.propertyID + " and TaxYear=" + taxYearThatUserIsWorkingIn, con);
                dr = cmd.ExecuteReader();
                while (dr.Read())
                {

                    countOfAccts = (int)dr.GetValue(0);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally { con.Close(); }

            if (countOfAccts > 0)
            { alreadyExists = true; }

            return alreadyExists;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            object i = 0;
            bool success = true;

            try
            {
                con = new SqlConnection(myCons.GetFreezeCon());
                con.Open();
                cmd = new SqlCommand("[AddNewTaxFreeze]", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@TaxYear", acctData.taxYear);
               
                cmd.Parameters.AddWithValue("@AcctNum", acctData.propertyID);
                cmd.Parameters.AddWithValue("@ParcelID",acctData.parcelID);
                cmd.Parameters.AddWithValue("@OwnerName", acctData.ownerName);
                cmd.Parameters.AddWithValue("@PropAddress", acctData.address);
                cmd.Parameters.AddWithValue("@CountyRequal", countyRequalChBx.Checked ? 1:0);
                //cmd.Parameters.AddWithValue("@CountyAdditionalFrozenTaxes", textBox34.Text.Trim());
                cmd.Parameters.AddWithValue("@District", acctData.district);
                cmd.Parameters.AddWithValue("@USERName", Environment.UserName);
                cmd.Parameters.AddWithValue("@CREATEDATE", DateTime.Today);

                cmd.Parameters.AddWithValue("@CountyFznApp", acctData.countyFrozenAppraisal);
                cmd.Parameters.AddWithValue("@CountyFznAssess", acctData.countyFrozenAssess);
                cmd.Parameters.AddWithValue("@CountyFznRate", acctData.countyFrozenRate);
                cmd.Parameters.AddWithValue("@CountyFznTaxes", acctData.countyFrozenTaxes);
                cmd.Parameters.AddWithValue("@FrznInCounty", 1);
                cmd.Parameters.AddWithValue("@CountyBaseYr", acctData.countyBaseYear);
                if (acctData.countyUnfrznAppraisal > 0)
                {
                   cmd.Parameters.AddWithValue("@CountyUnfzn", 1);
                   cmd.Parameters.AddWithValue("@CountyUnfznApp", acctData.countyUnfrznAppraisal);
                   cmd.Parameters.AddWithValue("@CountyUnfznAssess", acctData.countyUnfrznAssess);
                }
                    
                //cmd.Parameters.AddWithValue("@CountyPercTaxChange", textBox33.Text.Trim());
                

                if (cityGrpBx.Visible)
                {
                    cmd.Parameters.AddWithValue("@CityRequal", cityRequalChBx.Checked ? 1 : 0);
                    cmd.Parameters.AddWithValue("@CityFznApp", acctData.cityFrozenAppraisal);
                    cmd.Parameters.AddWithValue("@CityFznAssess", acctData.cityFrozenAssess);
                    cmd.Parameters.AddWithValue("@CityFznRate", acctData.cityFrozenRate);
                    cmd.Parameters.AddWithValue("@CityFznTaxes", acctData.cityFrozenTaxes);
                    cmd.Parameters.AddWithValue("@FrznInCity", 1);
                    cmd.Parameters.AddWithValue("@CityBaseYr", acctData.cityBaseYear);
                    if (acctData.cityUnfrznAppraisal > 0)
                    {
                        cmd.Parameters.AddWithValue("@CityUnfzn", acctData.cityUnfrznAppraisal);
                        cmd.Parameters.AddWithValue("@CityUnfznApp", acctData.cityUnfrznAppraisal);
                        cmd.Parameters.AddWithValue("@CityUnfznAssess", acctData.cityUnfrznAssess);
                    }
                    //cmd.Parameters.AddWithValue("@CityPercTaxChange", textBox43.Text.Trim());
                }

                i = cmd.ExecuteScalar();

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                success = false;
            }
            finally { con.Close(); }



            if (success)
            {
            //    if (dataGridView1.Rows.Count > 1)
            //    {
            //        foreach (DataGridViewRow row in dataGridView1.Rows)
            //        {
            //            if (row.IsNewRow == false && row.Cells[0].Value != null && row.Cells[0].Value.ToString().Trim() != "")
            //            {
            //                try
            //                {
            //                    con = new SqlConnection(myCons.GetFreezeCon());
            //                    con.Open();
            //                    cmd = new SqlCommand("[AddNoteForTF]", con);
            //                    cmd.CommandType = CommandType.StoredProcedure;
            //                    cmd.Parameters.AddWithValue("@AssocTFID", i);
            //                    cmd.Parameters.AddWithValue("@Note", row.Cells[0].Value.ToString());
            //                    cmd.Parameters.AddWithValue("@NoteCreator", Environment.UserName);
            //                    cmd.Parameters.AddWithValue("@NoteCreateDate", DateTime.Today);
            //                    cmd.ExecuteNonQuery();

            //                }
            //                catch (Exception ex)
            //                {
            //                    MessageBox.Show(ex.Message);
            //                }
            //                finally { con.Close(); }
            //            }
            //        }

            //    }

                UpdateAP5_AccountType();
                MessageBox.Show("Account Tax Freeze Information Added Successfully!");
                ResetForm();

            }
        }

        void UpdateAP5_AccountType()
        {
            string user;
            if (Environment.UserName == "connieleonard")
                user = "cleonard";
            else
                user = Environment.UserName;
            con = new SqlConnection(myCons.GetFreezeCon());
            con.Open();
            cmd = new SqlCommand("[UpdateTF_AcctInAP5]", con);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@YEARID", taxYearThatUserIsWorkingIn);
            cmd.Parameters.AddWithValue("@CREATEUSER", user);
            cmd.Parameters.AddWithValue("@PropertyID", acctData.propertyID);

            cmd.ExecuteNonQuery();
        }

        void ResetForm()
        {
            foreach(Control c in this.Controls)
            {
                if(c is TextBox)
                { c.Text = ""; }
                else if(c is CheckBox)
                { ((CheckBox)c).Checked = false; }

            }

            foreach (Control c in countyGrpBx.Controls)
            {
                if (c is TextBox)
                { c.Text = ""; }
                else if (c is CheckBox)
                { ((CheckBox)c).Checked = false; }

            }

            foreach (Control c in cityGrpBx.Controls)
            {
                if (c is TextBox)
                { c.Text = ""; }
                else if (c is CheckBox)
                { ((CheckBox)c).Checked = false; }

            }

            foreach (Control c in groupBox1.Controls)
            {
                if (c is TextBox)
                { c.Text = ""; }
                else if (c is CheckBox)
                { ((CheckBox)c).Checked = false; }

            }

            countyGrpBx.Visible = false;
            cityGrpBx.Visible = false;
            dataGridView1.DataSource = null;
            acctData = new Class1();
        }
    }
}
