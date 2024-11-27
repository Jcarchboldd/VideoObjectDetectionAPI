using Domain.Entities;
using Domain.ValueObjects;

namespace Domain.Services;

public class FrameProcessor
{
    public VideoFrame SelectBestFrame(IEnumerable<DetectionResult> detectionResults)
    {
        var bestResult = detectionResults.OrderByDescending(r => r.ConfidenceScore).FirstOrDefault();
        if (bestResult == null)
        {
            throw new InvalidOperationException("No detection results provided.");
        }

        return new VideoFrame(0, bestResult.FrameFilePath);
    }
}