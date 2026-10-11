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
                case ["reset-portraits", var catalog, var backup]:
                    if (System.Diagnostics.Process.GetProcessesByName("Ariadna").Length != 0 || System.Diagnostics.Process.GetProcessesByName("Ariadna.Wpf").Length != 0)
                    {
                        throw new InvalidOperationException("Close Ariadna before changing the portrait schema.");
                    }
                    new Ariadna.Storage.CatalogDatabase(catalog).ResetPortraitStorage(backup);
                    logger.LogInformation("Portrait reset finished; catalog schema is now 2 and database portrait BLOBs are removed");
                    break;
                case ["compare", var source, var catalog, var report]:
                    new CatalogComparison(logger).Run(source, catalog, report);
                    break;
                default:
                    logger.LogInformation("Usage: export <SQL connection string> <asset root> <new package directory>; import <package> <new absolute SQLite path>; verify <package> <SQLite path>; snapshot <SQLite path> <asset root> <new package directory>; reset-portraits <SQLite path> <verified backup SQLite path>; compare <SQL connection string> <SQLite path> <new report path>");
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
