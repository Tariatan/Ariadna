using System;
using System.Collections.Generic;
using System.Drawing;
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
            Themes.Theme theme;
            AbstractDBStrategy strategy;
            if (Environment.GetCommandLineArgs()[1] == "games")
            {
                theme = new Themes.ThemeGames();
                strategy = new Ariadna.DBStrategies.GamesDBStrategy();
            }
            else// if (args[1] == "movies")
            {
                theme = new Themes.ThemeMovies();
                strategy = new Ariadna.DBStrategies.MoviesDBStrategy();
            }
            theme.Init();

            //	show the splash form
            Splasher.Show();

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);


            var panel = new MainPanel(strategy);
            Application.Run(panel);

            //	if the form is still shown...
            Splasher.Close();
        }
    }
}
