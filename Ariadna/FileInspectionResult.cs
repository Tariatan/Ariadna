namespace Ariadna;
internal sealed record FileInspectionResult(IReadOnlyCollection<AudioLanguage> Languages, IReadOnlyCollection<MediaMetric> Metrics);
