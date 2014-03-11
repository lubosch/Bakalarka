using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Tabber
{
    static class Program
    {
        private static Mutex m_Mutex;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);


            bool createdNew;
            m_Mutex = new Mutex(true, "TabberMutex", out createdNew);
            if (createdNew)
                Application.Run(new Form1());
            else
                Environment.Exit(0);

            Application.Run(new Form1());
        }
    }
}
