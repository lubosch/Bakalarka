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
using Microsoft.Win32.TaskScheduler;

namespace Tabber
{
    [DefaultEvent("ClipboardChanged")]
    public partial class Form1 : Form
    {
        private NotifyIcon trayIcon;
        private ContextMenu trayMenu;

        private static WinEventDelegate winEventProc;
        private static WinEventDelegate winEventProcTextChange;
        public static int user_ID = -1;

        private Boolean ctrl;

        ArrayList myProcessArray = new ArrayList();
        private Process myProcess;

        public static Boolean should_log = true;
        public InterceptKeys logger = new InterceptKeys();
        private IntPtr nextClipboardViewer;
        //globalKeyboardHook gkh = new globalKeyboardHook();
        //private string queueName = @".\private$\server_skuska";

        public CookieContainer cookieContainer;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            Updates updates = new Updates(0);
            AddFirewall();

            //Visible = false;
            ShowInTaskbar = false;
            SysTrayApp();


            Boolean isElevated = UacHelper.IsProcessElevated;
            cookieContainer = new CookieContainer();
            //Error.Message(isElevated);


            try
            {
                updates.aktualizacie("Bakalarka", "{8766CBFD-294B-446B-A02A-E9498BA98B04}", Form1.get_user_id().ToString());
            }
            catch (System.Exception ex)
            {
                Error.Message("Nepodarilo sa pripojiť k internetu. Prosím, pripojte sa k internetu alebo povoľte firewall pre aplikáciu Tabber. Ďakujem");
                Error.Show(ex, "");
                //pauseLog(300000);
            }


            //authorize();
            poSpusteni();

            //Thread t2;
            winEventProc = new WinEventDelegate(WinEventProc);
            winEventProcTextChange = new WinEventDelegate(WinEventProcTextChanges);
            //t2 = new Thread(() => SetWinEventHook(EVENT_SYSTEM_FOREGROUND, EVENT_SYSTEM_FOREGROUND, IntPtr.Zero, dele, 0, 0, WINEVENT_OUTOFCONTEXT));
            //t2.Start();

            Thread t = new Thread(() => getFileProcesses());
            t.Start();

            SetWinEventHook(EVENT_SYSTEM_FOREGROUND, EVENT_SYSTEM_FOREGROUND, IntPtr.Zero, winEventProc, 0, 0, WINEVENT_OUTOFCONTEXT);
            //SetWinEventHook(EVENT_OBJECT_NAMECHANGE, EVENT_OBJECT_NAMECHANGE, IntPtr.Zero, winEventProcTextChange, 0, 0, WINEVENT_OUTOFCONTEXT);

            nextClipboardViewer = (IntPtr)SetClipboardViewer((int)this.Handle);


            InterceptKeys.KeyDown += new KeyEventHandler(gkh_KeyDown);
            InterceptKeys.KeyUp += new KeyEventHandler(gkh_KeyUp);


        }

        void gkh_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.LControlKey)
                ctrl = false;
            e.Handled = true;
        }

        void gkh_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.LControlKey)
                ctrl = true;
            if (e.KeyCode == Keys.V && ctrl)
            {
                PastedClipboardData();
                ctrl = false;
            }
            e.Handled = true;
        }

        private void poSpusteni()
        {

            String exePath = AppDomain.CurrentDomain.BaseDirectory;
            //string exePath = getInstallationFolder();
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
        private void continueLog(object sender, EventArgs e)
        {
            should_log = true;
            trayMenu.MenuItems.RemoveAt(2);
            trayMenu.MenuItems.RemoveAt(1);
            trayMenu.MenuItems.Add("Stop logovaniu na 1 hodinu", pauseLog);
            trayMenu.MenuItems.Add("Stop logovaniu", quitLog);
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
            trayMenu.MenuItems.RemoveAt(2);
            trayMenu.MenuItems.RemoveAt(1);
            trayMenu.MenuItems.Add("Zapnúť logovanie, prosím", continueLog);
            trayMenu.MenuItems.Add("Stop logovaniu", quitLog);


            Thread t = new Thread(() => pause(time));
            t.Start();
        }


        private void pause(int time)
        {
            System.Console.WriteLine("Pauzujem " + time / 1000);
            Form1.should_log = false;
            Thread.Sleep(3600000);
            continueLog(null, null);
            //Form1.should_log = true;

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
            List<String> videos_buffer = new List<String>();
            List<String> music_buffer = new List<String>();

            while (true)
            {
                if (!should_log)
                {
                    Thread.Sleep(50000);
                    continue;
                }
                else
                {
                    Thread.Sleep(30000);
                }

                try
                {
                    List<String> watching_videos = new List<String>();
                    List<String> listening_music = new List<String>();

                    Process analyzator = new Process();
                    String path = AppDomain.CurrentDomain.BaseDirectory;

                    analyzator.StartInfo = new System.Diagnostics.ProcessStartInfo(path);
                    analyzator.StartInfo.WorkingDirectory = path;
                    analyzator.StartInfo.FileName = "handle.exe";
                    analyzator.StartInfo.Arguments = "-a";
                    analyzator.StartInfo.CreateNoWindow = true;
                    analyzator.StartInfo.UseShellExecute = false;
                    analyzator.StartInfo.RedirectStandardOutput = true;


                    myProcessArray.Clear();
                    int i = 0;

                    Dictionary<string, Regex> programs = programs_regex_init();

                    string program;
                    StringBuilder sb = new StringBuilder();
                    analyzator.Start();
                    foreach (string line in analyzator.StandardOutput.ReadToEnd().Split('\n'))
                    {
                        //sb.AppendLine(line);
                        String anal_result = anal_file(line.ToString());
                        if ((anal_result == "audio" || anal_result == "video") && line.IndexOf(":\\") > 1)
                        {

                            string subor = line.Substring(line.IndexOf(":\\") - 1);
                            subor = subor.Substring(0, subor.Length - 1);
                            subor = fix_path(subor);
                            if (subor == "")
                            {
                                continue;
                            }
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
                                        if (!listening_music.Contains(subor))
                                            listening_music.Add(subor);
                                    }
                                    catch (System.Net.WebException ex)
                                    {
                                        pauseLog(300000);
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
                                        if (!watching_videos.Contains(subor))
                                        {
                                            //MessageBox.Show(watching_videos.Contains(subor).ToString());
                                            watching_videos.Add(subor);
                                        }
                                    }
                                    catch (System.Net.WebException ex)
                                    {
                                        pauseLog(300000);
                                    }
                                    catch (System.Exception ex)
                                    {
                                        Error.Show(ex, Form1.get_user_id().ToString());
                                    }
                                    break;
                            }
                        }


                        //Debug.WriteLine(line + "  " + line.IndexOf(".mp3") + "  " + line.Length);

                        //Error.Show(line + "  " + line.IndexOf(".mp3") + "  " + line.Length);
                    }

                    List<string> music = listening_music.Except(music_buffer, StringComparer.OrdinalIgnoreCase).ToList<String>();
                    foreach (String subor_music in music)
                    {
                        spracuj_audio(subor_music);
                    }

                    List<string> video = watching_videos.Except(videos_buffer, StringComparer.OrdinalIgnoreCase).ToList<String>();
                    foreach (String subor_video in video)
                    {
                        spracuj_video(subor_video);
                    }
                    music_buffer = listening_music;
                    videos_buffer = watching_videos;
                    analyzator.WaitForExit();
                    analyzator.Close();
                    //Console.Out.Write("FINITO");

                }
                catch (Exception exception)
                {
                    String path = AppDomain.CurrentDomain.BaseDirectory + "handle.exe";
                    Error.Show(exception.ToString() + " \n " + path + File.Exists(path), get_user_id().ToString());
                }
            }
        }

        public void spracuj_audio(String subor)
        {
            ProcessData prc = getProcessOfFile(subor);

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
                if (name.LastIndexOf(".") > -1)
                    name = name.Substring(0, name.LastIndexOf("."));
            }
            //MessageBox.Show("|" + name + "|" + subor);

            using (var client = new CookieAwareWebClient(cookieContainer))
            {
                byte[] response = client.UploadValues("http://77.234.226.34:3000/song", new System.Collections.Specialized.NameValueCollection() { { "filename", subor }, { "name", name }, { "length", f.Properties.Duration.TotalSeconds.ToString() }, { "artist", artist }, { "genre", genre }, { "software_name", prc.processName }, { "user_id", get_user_id().ToString() } });
            }

        }
        public void spracuj_video(String subor)
        {
            ProcessData prc = getProcessOfFile(subor);
            Console.WriteLine("Video | " + subor + " |");
            int video_length = getMovieLength(subor);
            using (var client = new CookieAwareWebClient(cookieContainer))
            {
                byte[] response = client.UploadValues("http://77.234.226.34:3000/video", new System.Collections.Specialized.NameValueCollection() { { "filename", subor }, { "name", getNameByFilename(subor) }, { "length", video_length.ToString() }, { "software_name", prc.processName }, { "user_id", get_user_id().ToString() } });
            }
        }

        public int getMovieLength(String file_name)
        {
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
            if (name.LastIndexOf(".") > -1)
                name = name.Substring(0, name.LastIndexOf("."));
            return name;
        }

        public ProcessData getProcessOfFile(String filename)
        {
            String path = AppDomain.CurrentDomain.BaseDirectory;
            Process tool = new Process();
            ProcessData prc = null;
            tool.StartInfo.WorkingDirectory = path;
            tool.StartInfo.FileName = "handle.exe";
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

        [DllImport("User32.dll")]
        protected static extern int SetClipboardViewer(int hWndNewViewer);

        [DllImport("User32.dll", CharSet = CharSet.Auto)]
        public static extern bool ChangeClipboardChain(IntPtr hWndRemove, IntPtr hWndNewNext);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern int SendMessage(IntPtr hwnd, int wMsg, IntPtr wParam, IntPtr lParam);

        //[DllImport("user32", SetLastError = true)]
        //private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

        //[DllImport("user32", SetLastError = true)]
        //private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        //public static void UnregisterHotKey(int id)
        //{
        //    _wnd.Invoke(new UnRegisterHotKeyDelegate(UnRegisterHotKeyInternal), _hwnd, id);
        //}

        //delegate void RegisterHotKeyDelegate(IntPtr hwnd, int id, uint modifiers, uint key);
        //delegate void UnRegisterHotKeyDelegate(IntPtr hwnd, int id);

        //private static void RegisterHotKeyInternal(IntPtr hwnd, int id, uint modifiers, uint key)
        //{
        //    RegisterHotKey(hwnd, id, modifiers, key);
        //}

        //private static void UnRegisterHotKeyInternal(IntPtr hwnd, int id)
        //{
        //    UnregisterHotKey(IntPtr.Zero, id);
        //}


        //public void HotKeyManager_HotKeyPressed(object sender, HotKeyEventArgs e)
        //{
        //    //DisplayClipboardData();
        //    //MessageBox.Show("kliknute");
        //}


        protected override void WndProc(ref System.Windows.Forms.Message m)
        {
            //defined in winuser.h
            const int WM_DRAWCLIPBOARD = 0x308;
            const int WM_CHANGECBCHAIN = 0x030D;

            const int WM_HOTKEY = 0x312;

            switch (m.Msg)
            {
                case WM_DRAWCLIPBOARD: CopiedClipboardData();
                    SendMessage(nextClipboardViewer, m.Msg, m.WParam, m.LParam);
                    break;

                case WM_CHANGECBCHAIN:
                    if (m.WParam == nextClipboardViewer)
                        nextClipboardViewer = m.LParam;
                    else
                        SendMessage(nextClipboardViewer, m.Msg, m.WParam, m.LParam);
                    break;
                //case WM_PASTE:
                //    //DisplayClipboardData();
                //    MessageBox.Show("asda");
                //    break;

                default:
                    base.WndProc(ref m);
                    break;
            }
            //base.WndProc(ref m);

        }

        public void PastedClipboardData()
        {
            string clipboard = DisplayClipboardData();
            if (clipboard != "" && Form1.should_log)
            {
                try
                {
                    System.Console.WriteLine(clipboard);
                    ProcessData process = GetTopWindowProcessData();
                    using (var client = new CookieAwareWebClient(cookieContainer))
                    {
                        client.UploadValuesAsync(new Uri("http://77.234.226.34:3000/pasted"), new System.Collections.Specialized.NameValueCollection() { { "process_name", process.processName }, { "clipboard", clipboard }, { "user_id", get_user_id().ToString() } });
                    }
                }
                catch (System.Net.WebException ex)
                {
                    //Error.Message(ex);
                    //authorize();
                    //pauseLog(300000);
                }
                catch (System.Exception ex)
                {
                    Error.Show(ex, Form1.get_user_id().ToString());
                }
            }
        }
        public void CopiedClipboardData()
        {
            string clipboard = DisplayClipboardData();
            if (clipboard != "" && Form1.should_log)
            {
                try
                {
                    System.Console.WriteLine(clipboard);

                    ProcessData process = GetTopWindowProcessData();
                    using (var client = new CookieAwareWebClient(cookieContainer))
                    {
                        byte[] response = client.UploadValues("http://77.234.226.34:3000/copied", new System.Collections.Specialized.NameValueCollection() { { "process_name", process.processName }, { "clipboard", clipboard }, { "user_id", get_user_id().ToString() } });
                    }
                }
                catch (System.Net.WebException ex)
                {
                    //Error.Message(ex);
                    //authorize();
                    //pauseLog(300000);
                }
                catch (System.Exception ex)
                {
                    Error.Show(ex, Form1.get_user_id().ToString());
                }
            }

        }
        public String DisplayClipboardData()
        {
            try
            {
                if (Clipboard.ContainsText())
                {
                    return Clipboard.GetText();
                }

            }
            catch (Exception e)
            {
                // Swallow or pop-up, not sure 
                // Trace.Write(e.ToString()); 
                MessageBox.Show(e.ToString());
            }
            return "";
        }


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
                using (var client = new CookieAwareWebClient(cookieContainer))
                {
                    //authorize();
                    //byte[] response = client.UploadValues("http://77.234.226.34:3000/user_sessions", new System.Collections.Specialized.NameValueCollection() { { "uniq_pc", UserSQL.get_my_uniq_id() } });
                    byte[] response = client.UploadValues("http://77.234.226.34:3000/log_software", new System.Collections.Specialized.NameValueCollection() { { "process_name", process.processName }, { "window_name", process.windowName }, { "description", process.description }, { "filepath", process.filePath }, { "user_id", get_user_id().ToString() } });
                }
                //LogSoftwareSQL.AddNewLog(process, get_user_id());

                System.Console.WriteLine("Lognute> " + process.filePath + " | " + process.processName + " | " + process.windowName + "\r\n");
                Debug.WriteLine("koniec");
            }
            catch (System.Net.WebException ex)
            {
                //Error.Message(ex);
                //authorize();
                //pauseLog(300000);
            }
            catch (System.Exception ex)
            {
                Error.Show(ex, Form1.get_user_id().ToString());
            }

            //Error.Show(GetActiveWindowTitle() + "  " + GetTopWindowName() + "\r\n");

        }

        //public void authorize()
        //{
        //    try
        //    {
        //        CookieContainer coocks;
        //        using (var client = new CookieAwareWebClient(cookieContainer))
        //        {
        //            byte[] response = client.UploadValues("http://77.234.226.34:3000/user_sessions", new System.Collections.Specialized.NameValueCollection() { { "uniq_pc", UserSQL.get_my_uniq_id() } });
        //            //Error.Message(client.container.GetCookieHeader());
        //            coocks = client.container;
        //        }
        //        cookieContainer = coocks;
        //    }
        //    catch (System.Net.WebException ex)
        //    {
        //        pauseLog(300000);
        //    }
        //    catch (System.Exception ex)
        //    {
        //        Error.Show(ex, get_user_id().ToString());
        //    }
        //}


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
                //System.Console.WriteLine(PDFParser.pdfText(of.FileName));

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

        String fix_path(String path2)
        {
            string path = path2;
            try
            {
                if (path.IndexOf("?") >= 0)
                {
                    String temp_path = path.Substring(0, path.IndexOf("?"));
                    String temp_path2 = path.Substring(path.IndexOf("?"));

                    if (temp_path2.IndexOf("\\") > 0)
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
                    else if (temp_path.LastIndexOf("\\") > -1)
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
            }
            catch (System.Exception ex)
            {
                return path2;
            }
            //MessageBox.Show(path);
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

    public class HotKeyEventArgs : EventArgs
    {
        public readonly Keys Key;
        public readonly KeyModifiers Modifiers;

        public HotKeyEventArgs(Keys key, KeyModifiers modifiers)
        {
            this.Key = key;
            this.Modifiers = modifiers;
        }

        public HotKeyEventArgs(IntPtr hotKeyParam)
        {
            uint param = (uint)hotKeyParam.ToInt64();
            Key = (Keys)((param & 0xffff0000) >> 16);
            Modifiers = (KeyModifiers)(param & 0x0000ffff);
        }
    }

    [Flags]
    public enum KeyModifiers
    {
        Alt = 1,
        Control = 2,
        Shift = 4,
        Windows = 8,
        NoRepeat = 0x4000
    }

}
