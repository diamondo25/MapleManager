using System;
using System.Windows.Forms;

namespace MapleManager
{
    public static class Program
    {
        public static MainForm MainForm { get; private set; }

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(MainForm = new MainForm());
        }
    }
}
