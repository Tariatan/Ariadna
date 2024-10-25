using System;
using System.Windows.Forms;
using Ariadna.DatabaseStrategies;
using Ariadna.SplashScreen;
using Ariadna.Themes;
using Microsoft.Extensions.Logging;

namespace Ariadna
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        private static void Main()
        {
            using var factory = LoggerFactory.Create(builder => builder.AddConsole());
            var logger = factory.CreateLogger("Ariadna");


            Theme theme;
            AbstractDbStrategy strategy;
            var param = Environment.GetCommandLineArgs().Length > 1 ? Environment.GetCommandLineArgs()[1] : string.Empty;

            switch (param)
            {
                case "games":
                    theme = new ThemeGames();
                    strategy = new GamesDbStrategy(logger);
                    break;
                case "documentaries":
                    theme = new ThemeDocumentaries();
                    strategy = new DocumentariesDbStrategy(logger);
                    break;
                case "library":
                    theme = new ThemeLibrary();
                    strategy = new LibraryDbStrategy(logger);
                    break;

                //case "movies":
                default:
                    theme = new ThemeMovies();
                    strategy = new MoviesDbStrategy(logger);
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
