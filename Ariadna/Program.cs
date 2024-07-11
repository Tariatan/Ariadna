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
        private static void Main()
        {
            Themes.Theme theme;
            AbstractDBStrategy strategy;
            var param = Environment.GetCommandLineArgs().Length > 1 ? Environment.GetCommandLineArgs()[1] : string.Empty;

            switch (param)
            {
                case "games":
                    theme = new Themes.ThemeGames();
                    strategy = new DBStrategies.GamesDBStrategy();
                    break;
                case "documentaries":
                    theme = new Themes.ThemeDocumentaries();
                    strategy = new DBStrategies.DocumentariesDBStrategy();
                    break;
                // if (param == "movies")
                default:
                    theme = new Themes.ThemeMovies();
                    strategy = new DBStrategies.MoviesDBStrategy();
                    break;
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
