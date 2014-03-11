using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;
using System.Management;
using System.IO;
using System.Collections;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Runtime.InteropServices;
using Tabber.Model;
using System.Net;
using Updates_namespace;
using NetFwTypeLib;
using System.Security.Principal;
using Tabber.Widgety;
using DirectShowLib;
using DirectShowLib.DES;

namespace Tabber
{
    public partial class Form1 : Form
    {
        private NotifyIcon trayIcon;
        private ContextMenu trayMenu;

        private static WinEventDelegate winEventProc;
        private static WinEventDelegate winEventProcTextChange;
        public static int user_ID = -1;

        ArrayList myProcessArray = new ArrayList();
        private Process myProcess;

        public static Boolean should_log = true;

        //private string queueName = @".\private$\server_skuska";



        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            Updates updates = new Updates(1);
            AddFirewall();

            Visible = false;
            ShowInTaskbar = false;
            SysTrayApp();


            Boolean isElevated = UacHelper.IsProcessElevated;

            //Error.Message(isElevated);


            try
            {
                updates.aktualizacie("BakalarkaDebug", "{8766CBFD-294B-446B-A02A-E9498BA98B04}", Form1.get_user_id().ToString());
            }
            catch (System.Exception ex)
            {
                Error.Message("Nepodarilo sa pripojiť k internetu. Prosím, pripojte sa k internetu alebo povoľte firewall pre aplikáciu Tabber. Ďakujem");
                Error.Show(ex, "");
                //pauseLog(300000);
            }


            //Thread t2;
            winEventProc = new WinEventDelegate(WinEventProc);
            winEventProcTextChange = new WinEventDelegate(WinEventProcTextChanges);
            //t2 = new Thread(() => SetWinEventHook(EVENT_SYSTEM_FOREGROUND, EVENT_SYSTEM_FOREGROUND, IntPtr.Zero, dele, 0, 0, WINEVENT_OUTOFCONTEXT));
            //t2.Start();

            Thread t = new Thread(() => getFileProcesses());
            t.Start();

            SetWinEventHook(EVENT_SYSTEM_FOREGROUND, EVENT_SYSTEM_FOREGROUND, IntPtr.Zero, winEventProc, 0, 0, WINEVENT_OUTOFCONTEXT);
            //SetWinEventHook(EVENT_OBJECT_NAMECHANGE, EVENT_OBJECT_NAMECHANGE, IntPtr.Zero, winEventProcTextChange, 0, 0, WINEVENT_OUTOFCONTEXT);


        }

        public void AddFirewall()
        {
            try
            {
                Type NetFwMgrType = Type.GetTypeFromProgID("HNetCfg.FwMgr", false);
                INetFwMgr mgr = (INetFwMgr)Activator.CreateInstance(NetFwMgrType);
                bool Firewallenabled = mgr.LocalPolicy.CurrentProfile.FirewallEnabled;

                if (!Firewallenabled)
                {
                    return;
                }

                INetFwRule firewallRule = (INetFwRule)Activator.CreateInstance(Type.GetTypeFromProgID("HNetCfg.FWRule"));
                firewallRule.Action = NET_FW_ACTION_.NET_FW_ACTION_ALLOW;
                firewallRule.Description = "Allow tabber";
                firewallRule.ApplicationName = Application.ExecutablePath;
                firewallRule.Enabled = true;
                firewallRule.InterfaceTypes = "All";
                firewallRule.Name = "Tabber";

                INetFwPolicy2 firewallPolicy = (INetFwPolicy2)Activator.CreateInstance(
                    Type.GetTypeFromProgID("HNetCfg.FwPolicy2"));
                firewallPolicy.Rules.Add(firewallRule);
            }
            catch (System.Exception ex)
            {
                Error.Show(ex, Form1.get_user_id().ToString());
            }
        }

        public void SysTrayApp()
        {
            // Create a simple tray menu with only one item.
            trayMenu = new ContextMenu();
            trayMenu.MenuItems.Add("Exit", OnExit);
            trayMenu.MenuItems.Add("Stop logovaniu na 1 hodinu", pauseLog);
            trayMenu.MenuItems.Add("Stop logovaniu", quitLog);
            //trayMenu.MenuItems.Add("Skryt", );

            // Create a tray icon. In this example we use a
            // standard system icon for simplicity, but you
            // can of course use your own custom icon too.
            trayIcon = new NotifyIcon();
            trayIcon.Text = "Tabebr";
            trayIcon.Icon = new Icon(this.Icon, 40, 40);

            // Add menu to tray icon and show it.
            trayIcon.ContextMenu = trayMenu;
            trayIcon.Visible = true;
        }

        private void OnExit(object sender, EventArgs e)
        {
            Environment.Exit(0);
        }

        private void quitLog(object sender, EventArgs e)
        {
            should_log = false;
            Error.Message("Logovanie vypnute");
            trayMenu.MenuItems.RemoveAt(2);
            trayMenu.MenuItems.Add("Zapnúť logovanie, prosím", startAgainLog);
        }

        private void startAgainLog(object sender, EventArgs e)
        {
            should_log = true;
            trayMenu.MenuItems.RemoveAt(2);
            trayMenu.MenuItems.Add("Stop logovaniu", quitLog);
        }
        private void pauseLog(object sender, EventArgs e)
        {
            Error.Message("Logovanie pauznute na 1 hodinu");
            pauseLog(3600000);
        }

        private void pauseLog(int time)
        {
            System.Console.WriteLine("Pauzujem " + time / 1000);
            should_log = false;
            Thread.Sleep(3600000);
            should_log = true;

        }

        private void button1_Click(object sender, EventArgs e)
        {
            String url = "http://annota.fiit.stuba.sk/user_sessions";

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "POST";
            System.Text.UTF8Encoding encoding = new System.Text.UTF8Encoding();
            Byte[] byteArray = encoding.GetBytes("{\"utf8\": \"\", \"authenticity_token\":\"\", \"user_session\": {\"name\":\"Lu_bosch\", \"password\":\"aassdd\"}, \"commit\":\"Submit\"}");

            request.ContentLength = byteArray.Length;
            request.ContentType = @"application/json";

            using (Stream dataStream = request.GetRequestStream())
            {
                dataStream.Write(byteArray, 0, byteArray.Length);
            }
            long length = 0;
            try
            {
                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                {
                    length = response.ContentLength;
                    var rawJson = new StreamReader(response.GetResponseStream()).ReadToEnd();
                    foreach (Cookie c in response.Cookies)
                    {
                        Debug.WriteLine(c.Name);

                    }

                    Debug.WriteLine(rawJson.ToString());

                }
            }
            catch (WebException ex)
            {
                // Log exception and throw as for GET example above
                Error.Show(ex.ToString(), Form1.get_user_id().ToString());
            }

        }

        private void button2_Click(object sender, EventArgs e)
        {
            Thread t = new Thread(() => getFileProcesses());
            t.Start();
            //getFileProcesses();
        }

        private void getFileProcesses()
        {
            while (true)
            {
                if (!should_log)
                {
                    Thread.Sleep(5000);
                    continue;
                }

                try
                {
                    Process analyzator = new Process();
                    analyzator.StartInfo = new System.Diagnostics.ProcessStartInfo(".\\handle.exe", "-a");
                    analyzator.StartInfo.CreateNoWindow = true;
                    analyzator.StartInfo.UseShellExecute = false;
                    analyzator.StartInfo.RedirectStandardOutput = true;
                    myProcessArray.Clear();
                    int i = 0;
                    int previous_audio_lock = 0;
                    int previous_video_lock = 0;

                    Dictionary<string, Regex> programs = programs_regex_init();

                    string program;
                    StringBuilder sb = new StringBuilder();
                    analyzator.Start();
                    foreach (string line in analyzator.StandardOutput.ReadToEnd().Split('\n'))
                    {
                        //sb.AppendLine(line);
                        String anal_result = anal_file(line.ToString());
                        if (anal_result == "audio" || anal_result == "video")
                        {

                            string subor = line.Substring(line.IndexOf(":\\") - 1);
                            subor = subor.Substring(0, subor.Length - 1);
                            subor = fix_path(subor);
                            ProcessData prc = getProcessOfFile(subor);
                            if (prc == null)
                            {
                                continue;
                            }
                            switch (anal_result)
                            {
                                case "audio":

                                    try
                                    {
                                        previous_audio_lock++;
                                        //Console.WriteLine("|" + prc + " : " + subor + " > " + line + "  " + previous_audio_lock);

                                        TagLib.File f = TagLib.File.Create(subor);
                                        Console.WriteLine("Audio | " + f.Tag.FirstArtist + "|" + f.Tag.Title + " > " + f.Properties.Duration.TotalSeconds + " |");
                                        String artist = "", genre = "", name = f.Tag.Title;
                                        if (f.Tag.Artists.Count() > 0)
                                            artist = f.Tag.Artists[0];
                                        if (f.Tag.Genres.Count() > 0)
                                            genre = f.Tag.Genres[0];
                                        if (name == null)
                                        {
                                            name = subor.Substring(subor.LastIndexOf("\\") + 1);
                                            name = name.Substring(0, name.LastIndexOf("."));
                                        }
                                        //MessageBox.Show("|" + name + "|" + subor);

                                        using (WebClient client = new WebClient())
                                        {
                                            byte[] response = client.UploadValues("http://77.234.226.34:3000/song", new System.Collections.Specialized.NameValueCollection() { { "filename", subor }, { "name", name }, { "length", f.Properties.Duration.TotalSeconds.ToString() }, { "artist", artist }, { "genre", genre }, { "software_name", prc.processName } });
                                        }


                                        //Song s = new Song
                                        //{
                                        //    fileName = subor,
                                        //    name = name,
                                        //    length = (long)f.Properties.Duration.TotalSeconds,
                                        //    artist = artist,
                                        //    genre = genre
                                        //};
                                        //if (previous_audio_lock == 1)
                                        //{
                                        //    SongSQL.logSong(prc, s, get_user_id());
                                        //}
                                        //Error.Message("|" + f.Tag.FirstArtist + "|" + f.Tag.FirstGenre);
                                    }
                                    catch (System.Exception ex)
                                    {
                                        //pauseLog(300000);
                                        Error.Show(ex, Form1.get_user_id().ToString());

                                    }
                                    break;
                                case "video":
                                    try
                                    {

                                        using (WebClient client = new WebClient())
                                        {
                                            byte[] response = client.UploadValues("http://77.234.226.34:3000/video", new System.Collections.Specialized.NameValueCollection() { { "filename", subor }, { "name", getNameByFilename(subor) }, { "length", getMovieLength(subor).ToString() }, { "software_name", prc.processName } });
                                        }
                                        //previous_video_lock++;
                                        //Console.WriteLine("Video |" + subor + "|" + prc.PID);
                                        //if (previous_video_lock == 1)
                                        //{
                                        //    VideoSQL videoSQL = new VideoSQL(subor, getNameByFilename(subor), prc);
                                        //    videoSQL.save(get_user_id());
                                        //    //Error.Show("Video | " + subor);
                                        //}
                                    }
                                    catch (System.Exception ex)
                                    {
                                        //if (ex is System.Data.SqlClient.SqlException || ex is System.Data.Entity.Core.EntityException)
                                        //{
                                        //    pauseLog(300000);
                                        //}
                                        //else
                                        //{
                                        Error.Show(ex, Form1.get_user_id().ToString());
                                        //}
                                    }
                                    break;
                            }
                        }
                        //Debug.WriteLine(line + "  " + line.IndexOf(".mp3") + "  " + line.Length);

                        //Error.Show(line + "  " + line.IndexOf(".mp3") + "  " + line.Length);
                    }
                    analyzator.WaitForExit();
                    //Console.Out.Write("FINITO");

                }
                catch (Exception exception)
                {
                    //Error.Show(exception.ToString());
                    Console.WriteLine(("Error : " + exception.ToString() + "  "));
                }
            }
        }

        public int getMovieLength(String file_name)
        {
            return 0;
            try
            {


                var mediaDet = (IMediaDet)new MediaDet();
                DsError.ThrowExceptionForHR(mediaDet.put_Filename(file_name));

                // find the video stream in the file
                int index;
                var type = Guid.Empty;
                for (index = 0; index < 1000 && type != MediaType.Video; index++)
                {
                    mediaDet.put_CurrentStream(index);
                    mediaDet.get_StreamType(out type);
                }

                // retrieve some measurements from the video
                double frameRate;
                mediaDet.get_FrameRate(out frameRate);

                var mediaType = new AMMediaType();
                mediaDet.get_StreamMediaType(mediaType);
                var videoInfo = (VideoInfoHeader)Marshal.PtrToStructure(mediaType.formatPtr, typeof(VideoInfoHeader));
                DsUtils.FreeAMMediaType(mediaType);
                var width = videoInfo.BmiHeader.Width;
                var height = videoInfo.BmiHeader.Height;

                double mediaLength;
                mediaDet.get_StreamLength(out mediaLength);
                var frameCount = (int)(frameRate * mediaLength);
                var duration = frameCount / frameRate;
                return int.Parse(Math.Round(duration).ToString());
            }
            catch (System.Exception ex)
            {
                return 0;
            }
        }

        public String getNameByFilename(String fileName)
        {
            string name = fileName.Substring(fileName.LastIndexOf(@"\") + 1);
            name = name.Substring(0, name.LastIndexOf("."));
            return name;
        }

        public ProcessData getProcessOfFile(String filename)
        {
            Process tool = new Process();
            //tool.StartInfo = new System.Diagnostics.ProcessStartInfo("C:\\Users\\Fred\\Desktop\\handle.exe", "-a");
            ProcessData prc = null;
            tool.StartInfo.FileName = ".\\handle.exe";
            tool.StartInfo.Arguments = "\"" + filename + "\"";
            tool.StartInfo.UseShellExecute = false;
            tool.StartInfo.RedirectStandardOutput = true;
            tool.StartInfo.CreateNoWindow = true;
            tool.Start();
            tool.WaitForExit();
            string outputTool = tool.StandardOutput.ReadToEnd();
            string matchPattern = @"(?<=\s+pid:\s+)\b(\d+)\b(?=\s+)";
            foreach (Match match in Regex.Matches(outputTool, matchPattern))
            {
                prc = new ProcessData("", int.Parse(match.Value));
            }
            return prc;
        }


        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            Environment.Exit(0);

        }

        private Dictionary<string, Regex> programs_regex_init()
        {
            Dictionary<string, Regex> programs = new Dictionary<string, Regex>();
            foreach (string s in Tabber.Properties.Resources.programming_extensions.ToString().Split('\n'))
            {
                string[] vyraz = s.ToLower().Trim().Split(' ');
                string pattern = @"^.+\.(";
                string jazyk = vyraz[0];

                for (int i = 1; i < vyraz.Length; i++)
                {
                    if (vyraz[i][0] != '.') jazyk += " " + vyraz[i];
                    else
                    {
                        pattern += vyraz[i].Substring(1) + '|';
                    }
                }
                pattern = pattern.Substring(0, pattern.Length - 1);
                pattern += ")";
                //System.Console.WriteLine(jazyk + " > " + pattern);
                programs.Add(jazyk, new Regex(pattern));
            }
            return programs;
        }



        private string anal_file(string line)
        {
            if (line.Length < 5) return "unknown";
            Regex audio = new Regex(@"^.+\.(mp3|mpc|ogg|flac|wav|m4a)$", RegexOptions.RightToLeft);
            Regex video = new Regex(@"^.+\.(flv|avi|mp4|mov|mp4|mpg|divx|mpeg|wmv|mkv|3gp|vob|m4v)$", RegexOptions.RightToLeft);

            if (audio.IsMatch(line.ToLower().Trim()))
            {
                //Error.Show(line.ToLower().Trim() + "|");
                return "audio";
            }
            if (video.IsMatch(line.ToLower().Trim()))
            {
                return "video";
            }
            return "unknown";

        }

        private string anal_file(string line, Dictionary<string, Regex> programs)
        {

            if (line.Length < 5) return null;
            foreach (string program in programs.Keys)
            {
                if (programs[program].IsMatch(line.ToLower().Trim()))
                {
                    //Error.Show(programs[program].IsMatch(line.ToLower().Trim()) + "|");
                    return program;
                }
            }
            return null;
        }



        delegate void WinEventDelegate(IntPtr hWinEventHook, uint eventType, IntPtr hwnd, int idObject, int idChild, uint dwEventThread, uint dwmsEventTime);

        [DllImport("user32.dll")]
        static extern IntPtr SetWinEventHook(uint eventMin, uint eventMax, IntPtr hmodWinEventProc, WinEventDelegate winEventProc, uint idProcess, uint idThread, uint dwFlags);

        private const uint WINEVENT_OUTOFCONTEXT = 0;
        private const uint EVENT_SYSTEM_FOREGROUND = 3;

        [DllImport("user32.dll")]
        static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int count);

        [DllImport("kernel32.dll")]
        private static extern IntPtr OpenProcess(uint dwDesiredAccess, bool bInheritHandle, uint dwProcessId);

        [DllImport("user32.dll")]
        private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        [DllImport("kernel32.dll")]
        private static extern bool CloseHandle(IntPtr handle);

        [DllImport("psapi.dll")]
        private static extern uint GetModuleFileNameEx(IntPtr hWnd, IntPtr hModule, StringBuilder lpFileName, int nSize);


        public ProcessData GetTopWindowProcessData()
        {

            try
            {
                IntPtr hWnd = GetForegroundWindow();
                uint lpdwProcessId;
                GetWindowThreadProcessId(hWnd, out lpdwProcessId);
                IntPtr hProcess = OpenProcess(0x0410, false, lpdwProcessId);
                StringBuilder text = new StringBuilder(1000);

                //GetModuleBaseName(hProcess, IntPtr.Zero, text, text.Capacity);
                GetModuleFileNameEx(hProcess, IntPtr.Zero, text, text.Capacity);

                CloseHandle(hProcess);


                //Debug.WriteLine("end");
                return new ProcessData(text.ToString(), (int)lpdwProcessId);
            }
            catch (System.Exception ex)
            {
                return new ProcessData();
            }

        }

        public void WinEventProc(IntPtr hWinEventHook, uint eventType, IntPtr hwnd, int idObject, int idChild, uint dwEventThread, uint dwmsEventTime)
        {
            try
            {
                if (!Form1.should_log)
                {
                    return;
                }

                Debug.WriteLine(DateTime.Now.ToShortTimeString() + " zaciatok");
                ProcessData process = GetTopWindowProcessData();
                using (WebClient client = new WebClient())
                {
                    byte[] response = client.UploadValues("http://77.234.226.34:3000/log_software", new System.Collections.Specialized.NameValueCollection() { { "process_name", process.processName}, { "window_name", process.windowName}, { "description", process.description }, { "filepath", process.filePath} });
                }
                //LogSoftwareSQL.AddNewLog(process, get_user_id());

                System.Console.WriteLine("Lognute> " + process.filePath + " | " + process.processName + " | " + process.windowName + "\r\n");
                Debug.WriteLine("koniec");
            }
            catch (System.Exception ex)
            {
                //if (ex is System.Data.SqlClient.SqlException || ex is System.Data.Entity.Core.EntityException)
                //{
                //    pauseLog(300000);
                //}
                //else
                //{
                Error.Show(ex, Form1.get_user_id().ToString());
                //}
            }

            //Error.Show(GetActiveWindowTitle() + "  " + GetTopWindowName() + "\r\n");

        }


        private void button3_Click(object sender, EventArgs e)
        {
            OpenFileDialog of = new OpenFileDialog();
            if (of.ShowDialog() == DialogResult.OK)
            {
                //Bitmap b = new Bitmap(of.FileName);
                //OCR ocr = new OCR();
                //ocr.DoOCRNormal(b, "eng");

                //PDFParser pdfp = new PDFParser();
                //pdfp.ExtractText(of.FileName, "halo.txt");
                System.Console.WriteLine(PDFParser.pdfText(of.FileName));

            }
        }

        [DllImport("user32.dll")]
        static extern bool UnhookWinEvent(IntPtr hWinEventHook);

        const uint EVENT_OBJECT_NAMECHANGE = 0x800C;

        // Need to ensure delegate is not collected while we're using it,
        // storing it in a class field is simplest way to do this.

        static void WinEventProcTextChanges(IntPtr hWinEventHook, uint eventType, IntPtr hwnd, int idObject, int idChild, uint dwEventThread, uint dwmsEventTime)
        {
            // filter out non-HWND namechanges... (eg. items within a listbox)
            if (idObject != 0 || idChild != 0)
            {
                return;
            }

            uint lpdwProcessId;
            GetWindowThreadProcessId(hwnd, out lpdwProcessId);
            IntPtr hProcess = OpenProcess(0x0410, false, lpdwProcessId);
            StringBuilder text = new StringBuilder(1000);

            //GetModuleBaseName(hProcess, IntPtr.Zero, text, text.Capacity);

            CloseHandle(hProcess);

            ProcessData pd = new ProcessData("das", (int)lpdwProcessId);
            //Process p = Process.GetProcessById((int)lpdwProcessId);

            Console.WriteLine("Text of hwnd changed {0:x8} |" + pd.windowName + "|", hwnd.ToInt32());
        }

        public static int get_user_id()
        {
            if (Form1.user_ID == -1)
            {
                UserSQL u = new UserSQL();
                Form1.user_ID = u.user_id;
            }
            return Form1.user_ID;
        }

        private void button4_Click(object sender, EventArgs e)
        {
            //Error.Show(get_user_id());
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox1.Checked)
            {
                Form1.should_log = true;
            }
            else
            {
                Form1.should_log = false;
            }
        }

        String fix_path(String path)
        {
            if (path.IndexOf("?") >= 0)
            {
                String temp_path = path.Substring(0, path.IndexOf("?"));
                String temp_path2 = path.Substring(path.IndexOf("?"));

                if (temp_path2.IndexOf("\\") > -1)
                {
                    String temp_rest = temp_path2.Substring(temp_path2.IndexOf("\\"));
                    temp_path = temp_path + temp_path2.Substring(0, temp_path2.IndexOf("\\"));
                    temp_path2 = temp_path.Substring(temp_path.LastIndexOf("\\") + 1);
                    temp_path = temp_path.Substring(0, temp_path.LastIndexOf("\\"));

                    foreach (String directory_full in Directory.EnumerateDirectories(temp_path))
                    {
                        String directory = directory_full.Substring(directory_full.LastIndexOf("\\") + 1);
                        if (compare_fix_names(temp_path2, directory))
                        {
                            temp_path = directory_full + temp_rest;
                            return fix_path(temp_path);
                        }
                    }
                }
                else
                {
                    temp_path = temp_path + temp_path2;
                    temp_path2 = temp_path.Substring(temp_path.LastIndexOf("\\") + 1);
                    temp_path = temp_path.Substring(0, temp_path.LastIndexOf("\\"));
                    foreach (String file_full in Directory.EnumerateFiles(temp_path))
                    {
                        String file = file_full.Substring(file_full.LastIndexOf("\\") + 1);
                        if (compare_fix_names(temp_path2, file))
                        {
                            temp_path = file_full;
                            return fix_path(temp_path);
                        }
                    }
                }
            }
            return path;
        }


        public Boolean compare_fix_names(String orinal, String possiblities)
        {
            if (orinal.Length != possiblities.Length)
            {
                return false;
            }
            int i = 0;
            for (i = 0; i < orinal.Length; i++)
            {
                if (orinal[i] != possiblities[i] && orinal[i] != '?')
                {
                    return false;
                }
            }

            return true;
        }
    }

    public class ProcessData
    {
        public String filePath { get; set; }
        public int PID { get; set; }
        public String processName { set; get; }
        public String description { set; get; }
        public String windowName { set; get; }


        public ProcessData()
        {

        }
        public ProcessData(String filePath, int PID)
        {
            this.filePath = filePath;
            this.PID = PID;
            try
            {
                Process p = Process.GetProcessById(PID);
                this.windowName = p.MainWindowTitle;
                this.processName = p.MainModule.ModuleName;
                if (filePath == "")
                    this.filePath = p.MainModule.FileName;
            }
            catch (System.Exception ex)
            {
                processName = "";
            }
            try
            {
                if (File.Exists(filePath))
                {
                    this.description = FileVersionInfo.GetVersionInfo(filePath).FileDescription;
                }
            }
            catch (System.Exception ex)
            {
                this.description = "";
                this.filePath = "";

            }


            this.windowName = windowName;
        }
    }
}
