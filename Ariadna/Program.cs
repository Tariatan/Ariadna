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
            Themes.Theme theme;
            AbstractDBStrategy strategy;
            string param = Environment.GetCommandLineArgs().Length > 1 ? Environment.GetCommandLineArgs()[1] : string.Empty;

            if (param == "games")
            {
                theme = new Themes.ThemeGames();
                strategy = new DBStrategies.GamesDBStrategy();
            }
            else if (param == "documentaries")
            {
                theme = new Themes.ThemeDocumentaries();
                strategy = new DBStrategies.DocumentariesDBStrategy();
            }
            else// if (param == "movies")
            {
                theme = new Themes.ThemeMovies();
                strategy = new DBStrategies.MoviesDBStrategy();
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
