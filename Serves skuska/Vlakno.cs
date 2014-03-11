using MSMQ_Class;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Serves_skuska
{

    class Vlakno
    {
        //   public delegate void delegate(); 
        Labl stav;
        private volatile bool done;
        Labl sprava;
        Labl odoslane;
        Labl chyby;


        [DllImport("user32.dll", SetLastError = true)]
        private static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, IntPtr dwExtraInfo);

        private void CtrlKeyEvent(int znamienko)
        {
            keybd_event(0x11, 0x1d, 1, IntPtr.Zero);
            MouseOperations.MouseEvent(MouseOperations.MouseEventFlags.WHEEL, (int)(150 * znamienko));
            keybd_event(0x11, 0x1d, 3, IntPtr.Zero);
        }

        public Vlakno(Labl stav, Labl sprava, Labl chyby, Labl odoslane)
        {
            this.sprava = sprava;
            this.stav = stav;
            this.chyby = chyby;
            this.odoslane = odoslane;
        }


        IPAddress DisplayDhcpServerAddresses()
        {

            IPAddress ipcka = default(IPAddress);

            NetworkInterface[] adapters = NetworkInterface.GetAllNetworkInterfaces();
            System.Net.NetworkInformation.Ping ping = new System.Net.NetworkInformation.Ping();

            foreach (NetworkInterface adapter in adapters)
            {



                IPInterfaceProperties adapterProperties = adapter.GetIPProperties();

                //            IPAddressCollection addresses = adapterProperties.DhcpServerAddresses;
                //IPAddressInformation[] addresses = adapterProperties.UnicastAddresses();

                //  if     (addresses.Count > 0)
                {
                    //Error.Show(adapter.Description.ToLower() + "  " + adapter.Description.ToLower().IndexOf("virutal"));
                    if (adapter.Description.ToLower().IndexOf("virtual") > -1 || adapter.Description.ToLower().IndexOf("wireles") > -1) { }
                    else
                    {
                        continue;

                    }
                    if (adapter.OperationalStatus.ToString() == "Down") continue;

                    foreach (IPAddressInformation addresses in adapterProperties.UnicastAddresses)
                    {
                        if (addresses.Address.ToString().Length > 15) continue;
                        //++chyba


                        //Error.Show(addresses.Address.ToString());

                        PingReply pr = default(PingReply);
                        pr = ping.Send(addresses.Address);

                        Console.WriteLine(addresses.Address.ToString());
                        Console.WriteLine(pr.Status);
                        if (pr.Status == IPStatus.Success)
                        {
                            ipcka = addresses.Address;

                        }

                    }

                }

            }
            return ipcka;
        }
        public void RequestStop()
        {
            done = true;
        }

        private void ProcessRunner()
        {


            //Create our arguments
            //      string finalArgs = @"/env /user:Administrator """ + subCommandFinal + @"""";
            //        procStartInfo.Arguments = finalArgs;


            ProcessStartInfo processStartInfo = new ProcessStartInfo("cmd");

            processStartInfo.RedirectStandardInput = true;
            processStartInfo.RedirectStandardOutput = true;
            processStartInfo.UseShellExecute = false;
            //   processStartInfo.UserName = @"NT AUTHORITY\SYSTEM";
            //  processStartInfo.FileName = "runas";
            //        processStartInfo.Arguments = "/env /user:" + "Fred" + " cmd";
            Process process = Process.Start(processStartInfo);

            if (process != null)
            {
                //!++ Treba ako administrátor!!

                process.StandardInput.WriteLine("netsh wlan set hostednetwork mode=allow ssid=Andro key=aaaaaaaa keyUsage=persistent");
                process.StandardInput.WriteLine("netsh wlan start hostednetwork");

                process.StandardInput.Close(); // line added to stop process from hanging on ReadToEnd()
            }

        }

        public void DoWork()
        {
            try
            {
            }
            catch (Exception ex)
            {
                chyby.Text = ex.ToString();
                // Error.Show();
            }
        }

        public class MouseOperations
        {
            [Flags]
            public enum MouseEventFlags
            {
                MOUSEEVENTF_HWHEEL = 0x01000,
                LeftDown = 0x00000002,
                LeftUp = 0x00000004,
                MiddleDown = 0x00000020,
                MiddleUp = 0x00000040,
                Move = 0x00000001,
                Absolute = 0x00008000,
                RightDown = 0x00000008,
                RightUp = 0x00000010,
                MIDDLEDOWN = 0x20, /*middle button down */
                MIDDLEUP = 0x40, /* middle button up */
                WHEEL = 0x800, /*wheel button rolled */
            }

            [DllImport("user32.dll", EntryPoint = "SetCursorPos")]
            [return: MarshalAs(UnmanagedType.Bool)]
            private static extern bool SetCursorPos(int X, int Y);

            [DllImport("user32.dll")]
            [return: MarshalAs(UnmanagedType.Bool)]
            private static extern bool GetCursorPos(out MousePoint lpMousePoint);

            [DllImport("user32.dll")]
            private static extern void mouse_event(int dwFlags, int dx, int dy, int dwData, int dwExtraInfo);

            public static void SetCursorPosition(int X, int Y)
            {
                SetCursorPos(X, Y);
            }

            public static MousePoint GetCursorPosition()
            {
                MousePoint currentMousePoint;
                var gotPoint = GetCursorPos(out currentMousePoint);
                if (!gotPoint) { currentMousePoint = new MousePoint(0, 0); }
                return currentMousePoint;
            }

            public static void MouseEvent(MouseEventFlags value, int muchness)
            {
                MousePoint position = GetCursorPosition();

                mouse_event
                    ((int)value,
                     position.X,
                     position.Y,
                     muchness,
                     0)
                    ;
            }

            [StructLayout(LayoutKind.Sequential)]
            public struct MousePoint
            {
                public int X;
                public int Y;

                public MousePoint(int x, int y)
                {
                    X = x;
                    Y = x;
                }

            }

        }


        public class handleClinet
        {
            TcpClient clientSocket;
            string clNo;


            Labl stav;
            private volatile bool done;
            Labl sprava;
            Labl odoslane;
            Labl chyby;

            public handleClinet(Labl stav, Labl sprava, Labl odoslane, Labl chyby)
            {

            }

            public void startClient(TcpClient inClientSocket, string clineNo)
            {
                this.clientSocket = inClientSocket;
                this.clNo = clineNo;

                Thread ctThread = new Thread(doChat);
                ctThread.Start();
            }
            [DllImport("user32.dll", SetLastError = true)]
            private static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, IntPtr dwExtraInfo);

            private void CtrlKeyEvent(int znamienko)
            {
                keybd_event(0x11, 0x1d, 1, IntPtr.Zero);
                MouseOperations.MouseEvent(MouseOperations.MouseEventFlags.WHEEL, (int)(150 * znamienko));
                keybd_event(0x11, 0x1d, 3, IntPtr.Zero);
            }
            private void doChat()
            {
                //Byte[] sendBytes = null;
                //string serverResponse = null;
                //string rCount = null;
                //requestCount = 0;
                NetworkStream networkStream = clientSocket.GetStream();
                int mode = 0;

                while (!done)
                {
                    if (networkStream.CanRead)
                    {
                        byte[] myReadBuffer = new byte[10024];
                        StringBuilder myCompleteMessage = new StringBuilder();
                        int numberOfBytesRead = 0;

                        // Incoming message may be larger than the buffer size. 
                        do
                        {
                            numberOfBytesRead = networkStream.Read(myReadBuffer, 0, myReadBuffer.Length);
                            myCompleteMessage.AppendFormat("{0}", Encoding.ASCII.GetString(myReadBuffer, 0, numberOfBytesRead));
                        }

                        while (networkStream.DataAvailable);
                        if (myCompleteMessage.Length != 0)
                        {
                            myCompleteMessage.Remove(myCompleteMessage.Length - 1, 1);
                        }
                        // Print out the received message to the console.
                        sprava.Text = ("You received the following message : ||" + myCompleteMessage + "||");


                        switch (myCompleteMessage.ToString())
                        {


                        }


                    }
                    else
                    {
                        chyby.Text = ("Sorry.  You cannot read from this NetworkStream.");
                    }
                }
            }
        }
    }

}
