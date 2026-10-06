namespace Ariadna.Wpf;
internal sealed record MediaMetric(string Value, string Icon)
{
    internal const string StorageIcon = "M3,5 C3,0 23,0 23,5 C23,10 3,10 3,5 M3,5 L3,21 C3,26 23,26 23,21 L23,5 M3,13 C3,18 23,18 23,13";
    internal const string ClockIcon = "M13,1 A12,12 0 1 1 12.99,1 M13,5 L13,13 L19,17";
    internal const string ScreenIcon = "M1,2 L25,2 L25,20 L1,20 Z M13,20 L13,26 M7,26 L19,26";
    internal const string BitrateIcon = "M3,26 L3,17 M11,26 L11,10 M19,26 L19,1";
}