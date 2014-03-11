using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Tabber.Model
{
    public class UserSQL
    {
        public User user;

        public UserSQL()
        {
            string unique_id = get_my_uniq_id();
            this.user = Add_user(unique_id);

        }

        public UserSQL(string crypted_pass, string name, string email, int annota_id)
        {
            string unique_id = get_my_uniq_id();
            this.user = Add_user(unique_id);
            UpdateUser(crypted_pass, name, email, annota_id);
        }

        public User Add_user(string uniq_id)
        {
            User u = get_by_uniq_id(uniq_id);
            if (u == null)
            {
                var db = new BakalarkaEntities();
                u = new User
                {
                    pc_uniq = uniq_id
                };
                db.Users.Add(u);
                db.SaveChanges();
                u = get_by_uniq_id(uniq_id);
            }
            if (u.ip == null)
            {
                using (WebClient client = new WebClient())
                {
                    byte[] response = client.UploadValues("http://77.234.226.34:3000/user/update_ip", new System.Collections.Specialized.NameValueCollection() { { "uniq_pc", uniq_id } });
                }
            }
            return u;
        }

        public void UpdateUser(string crypted_pass, string name, string email, int annota_id)
        {
            if (this.user == null)
            {
                return;
            }

            var db = new BakalarkaEntities();

            this.user.pass = crypted_pass;
            this.user.name = name;
            this.user.email = email;
            this.user.annota_id = annota_id;

            db.SaveChanges();

        }


        public User get_by_uniq_id(string uniq_id)
        {
            var db = new BakalarkaEntities();
            var users = db.Users.Where(user => user.pc_uniq == uniq_id);

            User u;
            if (users.Count() > 0)
            {
                u = users.First();
                return u;
            }
            else
            {
                return null;
            }
        }


        public string get_my_uniq_id()
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
