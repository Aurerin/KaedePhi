namespace KaedePhi.Tool.Common;

public readonly struct ToolProgress
{
    public double Percentage { get; }
    public double OverallPercentage { get; }
    public string? Detail { get; }

    public ToolProgress(double percentage, string? detail = null)
        : this(percentage, -1, detail)
    {
    }

    public ToolProgress(double percentage, double overallPercentage, string? detail = null)
    {
        Percentage = Math.Clamp(percentage, 0.0, 1.0);
        OverallPercentage = overallPercentage < 0 ? -1 : Math.Clamp(overallPercentage, 0.0, 1.0);
        Detail = detail;
    }
}
