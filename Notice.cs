using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RealProperyHandNotices
{
    public class Notice
    {
        Cons myCons = new Cons();
        SqlConnection con;
        SqlDataReader dr;
        SqlCommand cmd;

        public int noticeID                     { get; set; }
        public int taxYear                      { get; set; }
        public int propertyID                   { get; set; }
        public string parcelID                  { get; set; }
        public bool isPersonalProperty          { get; set; }
        public string userAccount               { get; set; }
        public string name1                     { get; set; }
        public string name2                     { get; set; }
        public string address1                  { get; set; }
        public string city                      { get; set; }
        public string state                     { get; set; }
        public string zipcode                   { get; set; }
        public string district                  { get; set; }
        public string situsLocation             { get; set; }
        public int currentAppraisedValue        { get; set; }
        public int currentAssessedValue         { get; set; }
        public double currentRatio               { get; set; }
        public string currentAcctType           { get; set; }
        public int priorAppraisedValue          { get; set; }
        public int priorAssessedValue           { get; set; }
        public double priorRatio                 { get; set; }
        public string priorAcctType             { get; set; }
        public double landUnits                  { get; set; }
        public string landUnitType              { get; set; }
        public string propertyDescription       { get; set; }
        public string reason                    { get; set; }
        public bool deleted                     { get; set; }
        public DateTime createDate              { get; set; }
        public string createUser                { get; set; }
        public int legacyNoticeID               { get; set; }
        public int informalHearingID            { get; set; }
        public bool informalHearingValid        { get; set; }



        public void GetDataForNewNoticeFromAP5(int acctNum, int year, string userAcct, string ppID)
        {
            propertyID = acctNum;
            userAccount = userAcct;
            parcelID = ppID;
            taxYear = year;
            deleted = false;
            legacyNoticeID = 0;
            informalHearingID = 0;
            createUser = Environment.UserName;

            con = new SqlConnection(myCons.GetRutherCBCon());
            con.Open();
            cmd = new SqlCommand("GetInfoForNewHandNotice", con);
            cmd.CommandType = System.Data.CommandType.StoredProcedure;
            if(acctNum>0)
            {
                cmd.Parameters.AddWithValue("@PropertyID", propertyID);
            }
            else if(userAcct.Trim() !="")
            {
                cmd.Parameters.AddWithValue("@UserAccount", userAcct);
            }
            else
            {
                cmd.Parameters.AddWithValue("@ParcelID", ppID);
            }
           
            cmd.Parameters.AddWithValue("@YearID", taxYear);

            dr = cmd.ExecuteReader();

            if (dr.HasRows)
            {
                while (dr.Read())
                {
                    propertyID = int.Parse(dr[0].ToString());
                    parcelID = dr[1].ToString();
                    isPersonalProperty = dr.GetBoolean(2);
                    userAccount  = dr[3].ToString();
                    name1 = dr[4].ToString();
                    name2 = dr[5].ToString();
                    address1 = dr[6].ToString();
                    city = dr[7].ToString();
                    state = dr[8].ToString();
                    zipcode = dr[9].ToString();
                    district = dr[10].ToString();
                    situsLocation = dr[11].ToString();
                    if (!isPersonalProperty)
                    {
                        landUnits = double.Parse(dr[12].ToString());
                        landUnitType = dr[13].ToString();                        
                    }
                    propertyDescription = dr[14].ToString();
                }
            }
            con.Close();
        }

        public void UpdatePriorValueInfo(AcctValueInfo info)
        {
            priorAppraisedValue = info.appraisedValue;
            priorAssessedValue = info.assessedValue;
            priorAcctType = info.acctType;
            priorRatio = info.ratio;
        }

        public void UpdateNoticeCurrentValue(AcctValueInfo info)
        {
            info.GetCurrentValueFromAP5(propertyID, taxYear, isPersonalProperty);

            currentAppraisedValue = info.appraisedValue;
            currentAssessedValue = info.assessedValue;
            currentAcctType = info.acctType;
            currentRatio = info.ratio;
        }

        public void AddToPrintLog()
        {
            con = new SqlConnection(myCons.GetRutherCBCon());
            
            con.Open();
            cmd = new SqlCommand("InsertIntoChangeNoticePrintLog", con);
            cmd.CommandType = System.Data.CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@NoticeID", noticeID);
            cmd.Parameters.AddWithValue("@User", Environment.UserName);
            cmd.ExecuteNonQuery();
            con.Close();
         }

        public void CreateNotice()
        {
            createDate = DateTime.Now;
            con = new SqlConnection(myCons.GetRutherCBCon());
            con.Open();
            try { 
            cmd = new SqlCommand("InsertHandNotice", con);
            cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@PropertyID", propertyID);
                cmd.Parameters.AddWithValue("@YearID", taxYear);
                cmd.Parameters.AddWithValue("@ParcelID", parcelID);
                cmd.Parameters.AddWithValue("@IsPersonalProperty", isPersonalProperty);
                cmd.Parameters.AddWithValue("@UserAccount", userAccount);
                cmd.Parameters.AddWithValue("@Name1", name1);
                if (name2.Trim().Length > 1)
                { cmd.Parameters.AddWithValue("@Name2", name2); }
                cmd.Parameters.AddWithValue("@Address", address1);
                cmd.Parameters.AddWithValue("@City", city);
                cmd.Parameters.AddWithValue("@State", state);
                cmd.Parameters.AddWithValue("@Zip", zipcode);
                cmd.Parameters.AddWithValue("@District", district);
                cmd.Parameters.AddWithValue("@Situs", situsLocation);
                cmd.Parameters.AddWithValue("@CurrentAppraisedValue", currentAppraisedValue);
                cmd.Parameters.AddWithValue("@CurrentAssessedValue", currentAssessedValue);
                cmd.Parameters.AddWithValue("@CurrentRatio", float.Parse(currentRatio.ToString()));
                cmd.Parameters.AddWithValue("@CurrentAcctType", currentAcctType);
                cmd.Parameters.AddWithValue("@PriorAppraisedValue", priorAppraisedValue);
                cmd.Parameters.AddWithValue("@PriorAssessedValue", priorAssessedValue);
                cmd.Parameters.AddWithValue("@PriorRatio", float.Parse(priorRatio.ToString()));
                cmd.Parameters.AddWithValue("@PriorAcctType", priorAcctType);
                cmd.Parameters.AddWithValue("@LandUnits", float.Parse(landUnits.ToString()));
                cmd.Parameters.AddWithValue("@LandUnitType", landUnitType);
                cmd.Parameters.AddWithValue("@PropertyDesc", propertyDescription);
                cmd.Parameters.AddWithValue("@Reason", reason);
                cmd.Parameters.AddWithValue("@CreateDate", createDate);
                cmd.Parameters.AddWithValue("@CreateUser", createUser);
                noticeID = int.Parse(cmd.ExecuteScalar().ToString());
            }
            catch(Exception ex)
            {

            }
            finally
            {                
                con.Close();
            }
        }

        public void GetExistingNoticeInfo()
        {
            con = new SqlConnection(myCons.GetRutherCBCon());
            con.Open();
            cmd = new SqlCommand("GetNoticeFromNoticeDB", con);
            cmd.CommandType = System.Data.CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@NoticeID", noticeID);
            
            dr = cmd.ExecuteReader();

            if (dr.HasRows)
            {
                while (dr.Read())
                {
                    taxYear = int.Parse(dr[1].ToString());
                    propertyID = int.Parse(dr[2].ToString());
                    parcelID = dr[3].ToString();
                    isPersonalProperty = dr.GetBoolean(4);
                    userAccount = dr[5].ToString();
                    name1 = dr[6].ToString();
                    name2 = dr[7].ToString();
                    address1 = dr[8].ToString();
                    city = dr[9].ToString();
                    state = dr[10].ToString();
                    zipcode = dr[11].ToString();
                    district = dr[12].ToString();
                    situsLocation = dr[13].ToString();
                    currentAppraisedValue = int.Parse(dr[14].ToString());
                    currentAssessedValue = int.Parse(dr[15].ToString());
                    currentRatio = double.Parse(dr[16].ToString());
                    currentAcctType = dr[17].ToString();
                    priorAppraisedValue = int.Parse(dr[18].ToString());
                    priorAssessedValue = int.Parse(dr[19].ToString());
                    priorRatio = double.Parse(dr[20].ToString());
                    priorAcctType = dr[21].ToString();

                    if (!isPersonalProperty)
                    {
                        landUnits = double.Parse(dr[22].ToString());
                        landUnitType = dr[23].ToString();
                        propertyDescription = dr[24].ToString();
                    }

                    reason = dr[25].ToString();
                    deleted = false;
                    createDate = DateTime.Parse(dr[27].ToString());
                    createUser = dr[28].ToString();
                    legacyNoticeID= int.Parse(dr[29].ToString());
                    informalHearingID = int.Parse(dr[30].ToString());
                    informalHearingValid = dr.GetBoolean(31);
                }
            }
            con.Close();
        }

        public void ValidateNotice()
        {
            con = new SqlConnection(myCons.GetRutherCBCon());
            con.Open();
            cmd = new SqlCommand("ValidateNotice", con);
            cmd.CommandType = System.Data.CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@NoticeID", noticeID);
            cmd.ExecuteNonQuery();
        }

    }
}
