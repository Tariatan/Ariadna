using System;
using Microsoft.Extensions.Logging;

namespace Ariadna;

internal static class Program
{
    [STAThread]
    private static void Main(string[] args)
    {
        AppContext.SetSwitch("System.Runtime.Serialization.EnableUnsafeBinaryFormatterSerialization", true);
        using var factory = LoggerFactory.Create(builder => builder.AddConsole());
        var application = new CatalogApplication(factory.CreateLogger("Ariadna"));
        application.Run(args);
    }
}
