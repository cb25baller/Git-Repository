using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RealProperyHandNotices
{
    public class AcctValueInfo
    {
        Cons myCons = new Cons();
        SqlConnection con;
        SqlDataReader dr;
        SqlCommand cmd;

        public int appraisedValue { get; set; }
        public int assessedValue { get; set; }
        public double ratio { get; set; }
        public string acctType { get; set; }


        public void GetPriorValueFromAP5(int propertyID, int year, int categoryID, bool isPersonal)
        {
            appraisedValue = -1;
            con = new SqlConnection(myCons.GetRutherCBCon());
            con.Open();
            cmd = new SqlCommand("GetPriorValueForHandNoticeFromAP5", con);
            cmd.CommandType = System.Data.CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@PropertyID", propertyID);
            cmd.Parameters.AddWithValue("@YearID", year);
            cmd.Parameters.AddWithValue("@xrValueCategoryID", categoryID);
            cmd.Parameters.AddWithValue("@IsPersonalPropertyFlag", isPersonal);

            dr = cmd.ExecuteReader();

            if (dr.HasRows)
            {
                while (dr.Read())
                {
                    appraisedValue = int.Parse(dr[1].ToString());
                    assessedValue = int.Parse(dr[2].ToString());
                    acctType = dr[3].ToString();
                    ratio = double.Parse(dr[4].ToString());
                }
            }
            else
            {
                appraisedValue = 0;
                assessedValue = 0;
                acctType = "";
                ratio = 0;
            }
            con.Close();
        }

        public void GetValueFromPriorNotice(int propertyID, int year)
        {
            appraisedValue = -1;
            con = new SqlConnection(myCons.GetRutherCBCon());
            con.Open();
            cmd = new SqlCommand("GetPriorHandNoticeValue", con);
            cmd.CommandType = System.Data.CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@PropertyID", propertyID);
            cmd.Parameters.AddWithValue("@YearID", year);

            dr = cmd.ExecuteReader();

            if (dr.HasRows)
            {
                while (dr.Read())
                {
                    appraisedValue = int.Parse(dr[0].ToString());
                    assessedValue = int.Parse(dr[1].ToString());
                    acctType = dr[3].ToString();
                    ratio = double.Parse(dr[2].ToString());
                }
            }
            con.Close();
        }

        public void GetCurrentValueFromAP5(int propertyID, int year, bool isPersonal)
        {
            appraisedValue = -1;
            con = new SqlConnection(myCons.GetRutherCBCon());
            con.Open();
            cmd = new SqlCommand("GetCurrentValueForHandNotice", con);
            cmd.CommandType = System.Data.CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@PropertyID", propertyID);
            cmd.Parameters.AddWithValue("@YearID", year);
            cmd.Parameters.AddWithValue("@IsPersonalPropertyFlag", isPersonal);

            dr = cmd.ExecuteReader();

            if (dr.HasRows)
            {
                while (dr.Read())
                {
                    appraisedValue = int.Parse(dr[1].ToString());
                    assessedValue = int.Parse(dr[2].ToString());
                    acctType = dr[3].ToString();
                    ratio = double.Parse(dr[4].ToString());
                }
            }
            con.Close();
        }

    }
}
