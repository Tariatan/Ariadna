namespace Ariadna.Wpf;
internal sealed record FileInspectionResult(IReadOnlyCollection<AudioLanguage> Languages, IReadOnlyCollection<MediaMetric> Metrics);