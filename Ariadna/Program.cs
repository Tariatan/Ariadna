using Microsoft.Extensions.Logging;
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
            using var factory = LoggerFactory.Create(builder => builder.AddConsole());
            var logger = factory.CreateLogger("Ariadna");


            Themes.Theme theme;
            AbstractDBStrategy strategy;
            var param = Environment.GetCommandLineArgs().Length > 1 ? Environment.GetCommandLineArgs()[1] : string.Empty;

            switch (param)
            {
                case "games":
                    theme = new Themes.ThemeGames();
                    strategy = new DBStrategies.GamesDBStrategy(logger);
                    break;
                case "documentaries":
                    theme = new Themes.ThemeDocumentaries();
                    strategy = new DBStrategies.DocumentariesDBStrategy(logger);
                    break;
                // if (param == "movies")
                default:
                    theme = new Themes.ThemeMovies();
                    strategy = new DBStrategies.MoviesDBStrategy(logger);
                    break;
            }
            
            theme.Init();

            //	show the splash form
            Splasher.Show();

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            var panel = new MainPanel(strategy, logger);
            Application.Run(panel);

            //	if the form is still shown...
            Splasher.Close();
        }
    }
}
