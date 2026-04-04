using System.Reflection;
using Ariadna.Properties;

namespace Ariadna.Tests;

internal static class TestSettings
{
    public static string Get(string propertyName)
    {
        var settingsType = typeof(Resources).Assembly.GetType("Ariadna.Properties.Settings", true)!;
        var defaultProperty = settingsType.GetProperty("Default", BindingFlags.Public | BindingFlags.Static)!;
        var settingsInstance = defaultProperty.GetValue(null)!;
        var property = settingsType.GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public)!;
        return (string)property.GetValue(settingsInstance)!;
    }
}