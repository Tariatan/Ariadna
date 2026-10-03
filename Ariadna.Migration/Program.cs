using Microsoft.Extensions.Logging;

namespace Ariadna.Migration;

internal static class Program
{
    private static int Main(string[] args)
    {
        using var factory = LoggerFactory.Create(builder => builder.AddSimpleConsole(options => options.SingleLine = true));
        var logger = factory.CreateLogger("Ariadna.Migration");
        try
        {
            var transfer = new CatalogTransfer(logger);
            switch (args)
            {
                case ["export", var connection, var assets, var package]:
                    transfer.Export(connection, assets, package);
                    break;
                case ["import", var package, var destination]:
                    transfer.Import(package, destination);
                    break;
                case ["verify", var package, var destination]:
                    transfer.Verify(package, destination);
                    break;
                case ["snapshot", var catalog, var assets, var package]:
                    transfer.Snapshot(catalog, assets, package);
                    break;
                case ["compare", var source, var catalog, var report]:
                    new CatalogComparison(logger).Run(source, catalog, report);
                    break;
                default:
                    logger.LogInformation("Usage: export <SQL connection string> <asset root> <new package directory>; import <package> <new absolute SQLite path>; verify <package> <SQLite path>; snapshot <SQLite path> <asset root> <new package directory>; compare <SQL connection string> <SQLite path> <new report path>");
                    return 2;
            }
            return 0;
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Catalog transfer failed; the source remains unchanged");
            return 1;
        }
    }
}
