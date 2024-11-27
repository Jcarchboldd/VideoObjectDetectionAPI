namespace Domain.Entities;

public class DetectionResult
{
    public string ObjectName { get; init; } = string.Empty;
    public float ConfidenceScore { get; init; }
    public string FrameFilePath { get; init; } = string.Empty;
}