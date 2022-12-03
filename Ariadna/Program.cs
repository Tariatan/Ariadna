using System;
using System.Windows.Forms;

namespace Ariadna
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            //	show the splash form
            Splasher.Show();

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            var panel = new MainPanel();

            Application.Run(panel);

            //	if the form is still shown...
            Splasher.Close();
        }
    }
}
