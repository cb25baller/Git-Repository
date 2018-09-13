using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace RealProperyHandNotices
{
    public partial class NoticeCreateFRM : Form
    {
        public NoticeCreateFRM()
        {
            InitializeComponent();
        }

        public object notID = 0;
        Notice thisNotice;
        AcctValueInfo currentYearValue;
        AcctValueInfo noticeValue;
        AcctValueInfo priorYearValue;
        AcctValueInfo priorHandNoticeValue;


        private void NoticeCreateFRM_Load(object sender, EventArgs e)
        {
            EnableDisableRadioButtons(false, this);
            radioButton2.Checked = true;
            if(Environment.UserName.Contains("baxter") || Environment.UserName.Contains("harvey") || Environment.UserName.Contains("couturier") 
                || Environment.UserName.Contains("tarpley"))
            {
                isPersonalPropertyChkBx.Checked = true;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            int propID;
            if(int.TryParse(TextBox1.Text.Trim(),out propID) || (TextBox1.Text.StartsWith("P") && TextBox1.Text.Trim().Length==12))
            {
                thisNotice = new Notice();
                if(isPersonalPropertyChkBx.Checked)
                {
                    if(TextBox1.Text.StartsWith("P"))
                    {
                        thisNotice.GetDataForNewNoticeFromAP5(0, DateTime.Today.Year, "", TextBox1.Text.Trim());
                    }
                    else
                    {
                        thisNotice.GetDataForNewNoticeFromAP5(0, DateTime.Today.Year, TextBox1.Text.Trim(), "");
                    }
                }
                else
                {
                    thisNotice.GetDataForNewNoticeFromAP5(propID, DateTime.Today.Year,"","");
                }
                

                currentYearValue = new AcctValueInfo();
                noticeValue = new AcctValueInfo();
                priorYearValue = new AcctValueInfo();
                priorHandNoticeValue = new AcctValueInfo();

                
                noticeValue.GetPriorValueFromAP5(thisNotice.propertyID, thisNotice.taxYear, 11, thisNotice.isPersonalProperty);
                priorYearValue.GetPriorValueFromAP5(thisNotice.propertyID, thisNotice.taxYear-1, 1, thisNotice.isPersonalProperty);
                priorHandNoticeValue.GetValueFromPriorNotice(thisNotice.propertyID, thisNotice.taxYear);

                thisNotice.UpdateNoticeCurrentValue(currentYearValue);
                thisNotice.UpdatePriorValueInfo(noticeValue);
                NoticeToTextBoxes();
                EnableDisableRadioButtons(true, this);
                radioButton2.Checked = true;
                EnableDisableSpecificRadioButtons(radioButton2, noticeValue);
                EnableDisableSpecificRadioButtons(radioButton1, priorYearValue);
                EnableDisableSpecificRadioButtons(radioButton3, priorHandNoticeValue);
                if(!radioButton1.Enabled && !radioButton2.Enabled && !radioButton3.Enabled)
                {
                    thisNotice.priorAcctType = "";
                    thisNotice.priorAppraisedValue = 0;
                    thisNotice.priorAssessedValue = 0;
                    thisNotice.priorRatio = 0;
                }
            }
            else
            {
                MessageBox.Show("Account Number must be a number. Please Try again");
            }
           
        }

        void NoticeToTextBoxes()
        {
            if(thisNotice.propertyID>0)
            {
                NameTxtBx.Text = thisNotice.name1;
                AddressTxtBx.Text = thisNotice.address1;
                CityTxtBx.Text = thisNotice.city;
                StateTxtBx.Text = thisNotice.state;
                ZipTxtBx.Text = thisNotice.zipcode;
                DistrictTxtBx.Text = thisNotice.district;
                LandUnitsTxtBx.Text = thisNotice.landUnits.ToString();
                UnitsOfMeasureTxtBx.Text = thisNotice.landUnitType;
                ParcelIdTxtBox.Text = thisNotice.parcelID;
                SitusTxtBx.Text = thisNotice.situsLocation;
                PropertyDescTxtBx.Text = thisNotice.propertyDescription;
                PriorValueToTextBoxes();
                CurrentValueToTextBoxes();
            }
        }

        void CurrentValueToTextBoxes()
        {
            CurrentAppraisalTxtBx.Text = thisNotice.currentAppraisedValue.ToString("C0");
            CurrentAssessmentTxtBx.Text = thisNotice.currentAssessedValue.ToString("C0");
            CurrentAcctTypeTxtBx.Text = thisNotice.currentAcctType;
            CurrentRatioTxtBx.Text = thisNotice.currentRatio.ToString("P0");
        }

        void PriorValueToTextBoxes()
        {
            PriorAppraisalTxtBx.Text = thisNotice.priorAppraisedValue.ToString("C0");
            PriorAssessTxtBx.Text = thisNotice.priorAssessedValue.ToString("C0");
            PriorRatioTxtBx.Text = thisNotice.priorRatio.ToString("P0");
            PriorAcctTypeTxtBx.Text = thisNotice.priorAcctType;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if(comboBox1.Text.Length>1 && thisNotice.reason.Length>1)
            {
                thisNotice.CreateNotice();
                thisNotice.AddToPrintLog();
                CRViewerFRM cr = new CRViewerFRM();
                cr.CrystalReport1(thisNotice.noticeID);
                cr.Show();
                MessageBox.Show("Notice Saved! If notice doesn't look corrected notify tech support.");
                ClearAllText(this);
            }
            else
            { MessageBox.Show("A reason must be select in order to process a Hand Notice. Please select a reason and try again."); }
        }
        void ClearAllText(Control con)
        {
            foreach (Control c in con.Controls)
            {
                if (c is TextBox)
                {
                    ((TextBox)c).Clear();
                }
                else if (c is ComboBox)
                {
                    ((ComboBox)c).SelectedIndex = -1;
                }
                else
                    ClearAllText(c);
            }
            EnableDisableRadioButtons(false, con);           
        }

        void EnableDisableRadioButtons(bool enable,Control con)
        {
            foreach (Control c in con.Controls)
            {
                if(c is RadioButton)
                {
                    ((RadioButton)c).Enabled = enable;
                }
            }
        }

        void EnableDisableSpecificRadioButtons(RadioButton rdb, AcctValueInfo acctInfo)
        {
            if (acctInfo.appraisedValue >= 0)
            { rdb.Enabled = true; }
            else { rdb.Enabled = false; }
        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            thisNotice.UpdatePriorValueInfo(priorYearValue);
            PriorValueToTextBoxes();
        }

        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            thisNotice.UpdatePriorValueInfo(noticeValue);
            PriorValueToTextBoxes();
        }

        private void radioButton3_CheckedChanged(object sender, EventArgs e)
        {
            thisNotice.UpdatePriorValueInfo(priorHandNoticeValue);
            PriorValueToTextBoxes();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            thisNotice.UpdateNoticeCurrentValue(currentYearValue);
            CurrentValueToTextBoxes();
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            thisNotice.reason = comboBox1.Text;
        }
    }
}
