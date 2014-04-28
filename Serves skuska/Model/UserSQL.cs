using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Management;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Wordik.SQL;

namespace Tabber.Model
{
    public class UserSQL
    {
        public int user_id;

        public UserSQL()
        {
            string unique_id = get_my_uniq_id();
            this.user_id = get_by_uniq_id(unique_id);
        }

        public UserSQL(string crypted_pass, string name, string email, int annota_id)
        {
            string unique_id = get_my_uniq_id();
            this.user_id = get_by_uniq_id(unique_id);
        }


        public int get_by_uniq_id(string uniq_id)
        {

            Main_SQL.OpenConnestion();
            Main_SQL.AddParameter("uniq_id", uniq_id);
            Main_SQL.AddCommand("SELECT ID FROM [User] WHERE pc_uniq = @uniq_id");
            DataSet ds = Main_SQL.Commit_List();

            int id = analyzeFound(ds.Tables[0]);
            if (id == -1)
            {
                Main_SQL.OpenConnestion();
                Main_SQL.AddParameter("uniq_id", uniq_id);
                Main_SQL.AddCommand("INSERT INTO [User](pc_uniq) VALUES (@uniq_id)");
                Main_SQL.Odpal();

                using (WebClient client = new WebClient())
                {
                    byte[] response = client.UploadValues("http://77.234.226.34:3000/user/update_ip", new System.Collections.Specialized.NameValueCollection() { { "uniq_pc", uniq_id } });
                }

                return get_by_uniq_id(uniq_id);
            }
            return id;


        }

        public int analyzeFound(DataTable dt)
        {
            if (dt.Rows.Count > 0)
            {
                return int.Parse(dt.Rows[0][0].ToString());
            }
            else
            {
                return -1;
            }
        }

        public static string get_my_uniq_id()
        {
            ManagementObjectCollection mbsList = null;
            ManagementObjectSearcher mbs = new ManagementObjectSearcher("Select * From Win32_processor");
            mbsList = mbs.Get();
            string id = "";
            foreach (ManagementObject mo in mbsList)
            {
                id = mo["ProcessorID"].ToString();
            }

            ManagementObjectSearcher mos = new ManagementObjectSearcher("SELECT * FROM Win32_BaseBoard");
            ManagementObjectCollection moc = mos.Get();
            string motherBoard = "";
            foreach (ManagementObject mo in moc)
            {
                motherBoard = (string)mo["SerialNumber"];
            }

            string myUniqueID = id + motherBoard;
            return myUniqueID;
        }




    }
}
