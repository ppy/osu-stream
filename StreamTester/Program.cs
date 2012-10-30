using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace StreamTester
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
#if !DIST
            if (DateTime.Now > new DateTime(2012, 12, 30))
                Environment.Exit(-1);
#endif

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }
    }
}
