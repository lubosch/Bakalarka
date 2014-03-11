using Microsoft.Win32;
using Microsoft.Win32.TaskScheduler;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using Updates_namespace;
using System.Web;
using Newtonsoft.Json.Linq;
using Instalacka.SQL;

namespace Instalacka
{
    public partial class Form1 : Form
    {
        String installedFolder = "";

        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (!registrujAnnota())
            {
                return;
            }
            if (!checkBox1.Checked)
            {
                Error.Message("Podpora pre iné prehliadače zatiaľ neexistuje. Nič sa nenainštaluje. Ďakujem aj tak.");
                return;
            }
            if (getInstallationFolder() == "")
            {
                instalujTabber("");
                poSpusteni();
            }

            if (checkBox3.Checked)
            {
                registrujCertifikat();
                instalujVSTO("");
            }

            instalujAddon();
            Error.Message("Nainštalované, Ďakujem.");
            spust();

        }

        private Boolean overRegistraciuAnnoty()
        {
            if (textBox1.Text.Length < 4)
            {
                Error.Message("Nevyplneny login");
                return false;
            }
            if (!isEmail(textBox2.Text))
            {
                Error.Message("Zly email");
                return false;
            }
            if (textBox3.Text.Length < 4 || textBox4.Text.Length < 4)
            {
                Error.Message("Nevyplnene heslo");
                return false;
            }
            if (textBox3.Text != textBox4.Text)
            {
                Error.Message("Hesla sa nezhoduju");
                return false;
            }
            return true;

        }

        private Boolean registrujAnnota()
        {
            if (radioButton1.Checked)
                return registruj();
            else
                return true;
            //return prihlas();

        }

        private Boolean registruj()
        {
            if (!overRegistraciuAnnoty())
            {
                return false;
            }

            String url = "http://annota-test.fiit.stuba.sk/best_pages/users.json";

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "POST";
            request.Accept = "application/json";
            //request.Headers["X-Accept"] = "application/json";
            System.Text.UTF8Encoding encoding = new System.Text.UTF8Encoding();
            string json = "{\"utf8\": \"\", \"user\":{\"name\":\"" + textBox1.Text + "\", \"password\":\"" + textBox3.Text.Trim() + "\",\"password_confirmation\":\"" + textBox4.Text + "\",\"email\":\"" + textBox2.Text + "\"},\"commit\":\"Register\"}";
            Byte[] byteArray = encoding.GetBytes(json);

            request.ContentLength = byteArray.Length;
            request.ContentType = @"application/json";

            using (Stream dataStream = request.GetRequestStream())
            {
                dataStream.Write(byteArray, 0, byteArray.Length);
            }
            long length = 0;
            try
            {
                using (WebResponse response = request.GetResponse())
                {
                    var rawJson = new StreamReader(response.GetResponseStream()).ReadToEnd();
                    JObject joResponse = JObject.Parse(rawJson);
                    //JObject ojObject = (JObject)joResponse["response"];
                    string crypted_pass = joResponse.GetValue("crypted_password").ToString();
                    string email = joResponse.GetValue("email").ToString();
                    string name = joResponse.GetValue("name").ToString();
                    int id = int.Parse(joResponse.GetValue("id").ToString());

                    User.UpdateUser(crypted_pass, name, email, id);

                    //MessageBox.Show(crypted_pass + "  " + email + "  " + name + " " + id.ToString());

                    Debug.WriteLine(rawJson.ToString());

                    return true;

                }
            }
            catch (WebException ex)
            {
                // Log exception and throw as for GET example above
                if (ex.Message.IndexOf("Unprocessable Entity") > 0)
                {
                    Error.Message("Meno alebo email obsadene");
                }
                else
                {
                    Error.Show(ex, System.Environment.MachineName);
                    Error.Message("Nie ste pripojený k internetu alebo blokovanie firewallom");
                }
                return false;
            }
            catch (Exception ex)
            {
                Error.Show(ex, System.Environment.MachineName);
                return false;
            }

        }
        private Boolean prihlas()
        {
            String url = "http://annota.fiit.stuba.sk/user_sessions";

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "POST";
            request.Accept = "application/json";
            System.Text.UTF8Encoding encoding = new System.Text.UTF8Encoding();
            string json = "{\"user_session\": {\"name\":\"" + textBox5.Text + "\", \"password\":\"" + textBox6.Text + "\"}}";
            Byte[] byteArray = encoding.GetBytes(json);

            request.ContentLength = byteArray.Length;
            request.ContentType = @"application/json";

            using (Stream dataStream = request.GetRequestStream())
            {
                dataStream.Write(byteArray, 0, byteArray.Length);
            }
            long length = 0;
            try
            {
                using (WebResponse response = request.GetResponse())
                {
                    var rawJson = new StreamReader(response.GetResponseStream()).ReadToEnd();
                    MessageBox.Show(rawJson.ToString());

                    JObject joResponse = JObject.Parse(rawJson);
                    //JObject ojObject = (JObject)joResponse["response"];
                    //string crypted_pass = joResponse.GetValue("crypted_password").ToString();
                    //string email = joResponse.GetValue("email").ToString();
                    //string name = joResponse.GetValue("name").ToString();
                    //int id = int.Parse(joResponse.GetValue("id").ToString());

                    //MessageBox.Show(crypted_pass + "  " + email + "  " + name + " " + id.ToString());
                    MessageBox.Show(joResponse.ToString());

                    Debug.WriteLine(rawJson.ToString());

                    return true;

                }
            }
            catch (WebException ex)
            {
                // Log exception and throw as for GET example above
                if (ex.Message.IndexOf("Unprocessable Entity") > 0)
                {
                    Error.Message("Zle meno, email alebo heslo");
                }
                else if (ex.Message.IndexOf("Unprocessable Entity") > 0)
                {
                    Error.Show(ex.Message, System.Environment.MachineName);
                    Error.Message("Nie ste pripojený k internetu alebo blokovanie firewallom");
                }
                return false;
            }
            catch (Exception ex)
            {
                Error.Show(ex, System.Environment.MachineName);
                return false;
            }


            return false;
        }


        public Boolean isEmail(string email)
        {
            bool isEmail = Regex.IsMatch(email, @"\A(?:[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?)\Z");
            return isEmail;
        }

        private void poSpusteni()
        {
            string exePath = getInstallationFolder();
            if (exePath.Length == 0)
            {
                return;
            }
            else
            {
                exePath = exePath + "\\Tabber.exe";
            }

            using (TaskService ts = new TaskService())
            {
                try
                {

                    // Create a new task definition and assign properties
                    TaskDefinition td = ts.NewTask();
                    td.RegistrationInfo.Description = "Spusti Tabber po spusteni";

                    // Create a trigger that will fire the task at this time every other day
                    LogonTrigger lt = new LogonTrigger();
                    lt.Delay = new System.TimeSpan(0, 5, 0);
                    td.Triggers.Add(lt);

                    td.Principal.RunLevel = TaskRunLevel.Highest;

                    // Create an action that will launch Notepad whenever the trigger fires
                    td.Actions.Add(new ExecAction(exePath, null, null));

                    // Register the task in the root folder
                    ts.RootFolder.RegisterTaskDefinition(@"Tabber", td);

                    // Remove the task we just created
                    //ts.RootFolder.DeleteTask("Test");
                }
                catch (System.Exception ex)
                {

                }
            }
        }

        private void registrujCertifikat()
        {
            try
            {
                string file; // Contains name of certificate file
                file = @".\Certifikat\Wordik_TemporaryKey.pfx";
                X509Store store = new X509Store(StoreName.Root, StoreLocation.LocalMachine);
                store.Open(OpenFlags.ReadWrite);

                X509Certificate2Collection collection = new X509Certificate2Collection();
                collection.Import(file, "aassdd", X509KeyStorageFlags.PersistKeySet);

                store.Add(collection[0]);
                store.Close();
            }
            catch (System.Exception ex)
            {
                Error.Show(ex.ToString(), System.Environment.MachineName);
            }

        }

        private void instalujVSTO(String path)
        {
            string thisFolder = System.IO.Path.GetDirectoryName(Application.ExecutablePath);

            //Updates.extractInstallation(thisFolder + "\\Wordik\\setup.exe", thisFolder + "\\Wordik\\temp_install");
            Process proc = new Process();
            proc.StartInfo.FileName = thisFolder + "\\Wordik\\setup.exe";
            proc.Start();

            proc.WaitForInputIdle();
            proc.WaitForExit();
            proc.Close();

        }

        private void instalujTabber(String path)
        {

            string thisFolder = System.IO.Path.GetDirectoryName(Application.ExecutablePath);

            Process proc = new Process();
            proc.StartInfo.FileName = thisFolder + "\\Tabber\\setup.exe";
            proc.Start();

            proc.WaitForInputIdle();
            proc.WaitForExit();
            proc.Close();
        }

        private void instalujAddon()
        {
            string thisFolder = System.IO.Path.GetDirectoryName(Application.ExecutablePath);

            string location = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            location = location + @"\Mozilla\Firefox\Profiles";
            foreach (String profile in Directory.EnumerateDirectories(location))
            {
                File.Copy(thisFolder + "\\Extension\\annota.xpi", profile + "\\extensions\\annota@gmail.com.xpi",true);
            }

        }

        private String getInstallationFolder()
        {
            if (installedFolder.Length != 0)
            {
                return installedFolder;
            }

            RegistryKey regKey = Registry.LocalMachine.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Uninstall");
            string location = FindByDisplayName(regKey, "Tabber");
            if (location.Length == 0)
            {
                regKey = Registry.LocalMachine.OpenSubKey(@"Software\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall");
                location = FindByDisplayName(regKey, "Tabber");
            }
            if (location.Length == 0)
            {
                return "";
            }
            location = location.Substring(0, location.Length - 1);
            installedFolder = location;
            return location;
        }

        private void copy(string source, string destination)
        {
            //Now Create all of the directories
            foreach (string dirPath in Directory.GetDirectories(source, "*",
                SearchOption.AllDirectories))
                Directory.CreateDirectory(dirPath.Replace(source, destination));

            //Copy all the files
            foreach (string newPath in Directory.GetFiles(source, "*.*",
                SearchOption.AllDirectories))
                File.Copy(newPath, newPath.Replace(source, destination), true);
        }

        private string FindByDisplayName(RegistryKey parentKey, string name)
        {
            string[] nameList = parentKey.GetSubKeyNames();
            for (int i = 0; i < nameList.Length; i++)
            {
                RegistryKey regKey = parentKey.OpenSubKey(nameList[i]);
                try
                {
                    if (regKey.GetValue("DisplayName").ToString() == name)
                    {
                        return regKey.GetValue("InstallLocation").ToString();
                    }
                    else
                    {
                        //MessageBox.Show(regKey.GetValue("DisplayName").ToString());
                    }
                }
                catch (Exception e)
                {
                    //MessageBox.Show(e.ToString());
                }
            }
            return "";
        }

        private void spust()
        {
            string exePath = getInstallationFolder();
            Process.Start(exePath + "\\Tabber.exe");
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            new Updates(0);
        }

        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton2.Checked)
                radioButton1.Checked = false;
        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton1.Checked == true)
            {
                radioButton2.Checked = false;
            }
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start("http://annota-test.fiit.stuba.sk/best_pages/register");
        }

    }
}
