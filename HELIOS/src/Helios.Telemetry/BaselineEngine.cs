namespace Helios.Telemetry;

public enum BaselineConfidence { Unknown, Low, Medium, High }

public sealed class MetricBaseline
{
    public double Mean { get; set; }
    public double StandardDeviation { get; set; }
    public int SampleCount { get; set; }
    public BaselineConfidence Confidence { get; set; } = BaselineConfidence.Unknown;
}

public sealed class DeviationResult
{
    public bool IsAnomalous { get; set; }
    public double ZScore { get; set; }
    public BaselineConfidence Confidence { get; set; }
}

/// <summary>
/// Builds a rolling statistical baseline (mean/stddev) per metric per node from
/// historical samples, then reports deviation as a z-score. Confidence is
/// explicitly Low/Unknown until enough samples exist - HELIOS never silently
/// pretends to have a confident baseline from sparse data.
/// </summary>
public sealed class BaselineEngine
{
    private const int MinSamplesForMedium = 30;
    private const int MinSamplesForHigh = 200;

    public MetricBaseline BuildBaseline(IReadOnlyList<double> samples)
    {
        if (samples.Count == 0)
            return new MetricBaseline { Confidence = BaselineConfidence.Unknown };

        double mean = samples.Average();
        double variance = samples.Count > 1
            ? samples.Sum(s => (s - mean) * (s - mean)) / (samples.Count - 1)
            : 0;

        var confidence = samples.Count switch
        {
            >= MinSamplesForHigh => BaselineConfidence.High,
            >= MinSamplesForMedium => BaselineConfidence.Medium,
            > 0 => BaselineConfidence.Low,
            _ => BaselineConfidence.Unknown
        };

        return new MetricBaseline
        {
            Mean = mean,
            StandardDeviation = Math.Sqrt(variance),
            SampleCount = samples.Count,
            Confidence = confidence
        };
    }

    public DeviationResult EvaluateDeviation(MetricBaseline baseline, double currentValue, double zThreshold = 3.0)
    {
        if (baseline.Confidence == BaselineConfidence.Unknown || baseline.StandardDeviation == 0)
            return new DeviationResult { IsAnomalous = false, Confidence = baseline.Confidence };

        double z = (currentValue - baseline.Mean) / baseline.StandardDeviation;
        return new DeviationResult
        {
            IsAnomalous = Math.Abs(z) >= zThreshold,
            ZScore = z,
            Confidence = baseline.Confidence
        };
    }
}
