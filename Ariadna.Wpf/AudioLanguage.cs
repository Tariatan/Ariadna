namespace Ariadna.Wpf;
internal sealed record AudioLanguage(string Name)
{
    public string Icon => $"pack://application:,,,/Ariadna.Wpf;component/Resources/lang/{Flag}.png";

    private string Flag => Name.Trim().ToLowerInvariant() switch
    {
        "russian" or "русский" or "ru" or "rus" => "ru_flag",
        "english" or "en" or "eng" => "en_flag",
        "french" or "fr" or "fra" or "fre" => "fr_flag",
        "german" or "de" or "deu" or "ger" => "de_flag",
        "ukrainian" or "uk" or "ukr" => "ua_flag",
        _ => "unknown",
    };
}