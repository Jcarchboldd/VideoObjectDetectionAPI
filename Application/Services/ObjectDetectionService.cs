using Application.Interfaces;
using Domain.Entities;
using Infrastructure.ObjectDetection;
using Infrastructure.VideoProcessing;

namespace Application.Services;

public class ObjectDetectionService : IObjectDetectionService
{
    private readonly VideoFrameExtractor _frameExtractor;
    private readonly OnnxObjectDetector _objectDetector;

    public ObjectDetectionService(VideoFrameExtractor frameExtractor, OnnxObjectDetector objectDetector)
    {
        _frameExtractor = frameExtractor;
        _objectDetector = objectDetector;
    }

    public async Task<IEnumerable<DetectionResult>> ProcessVideoAsync(string videoPath)
    {
        // Use the system's temporary directory for frame extraction
        var outputDirectory = Path.Combine(Path.GetTempPath(), "ExtractedFrames");
        Directory.CreateDirectory(outputDirectory);

        var results = new List<DetectionResult>();

        try
        {
            // Extract frames to the temporary directory
            var frames = _frameExtractor.ExtractFrames(videoPath, outputDirectory);

            // Process each frame for object detection
            foreach (var frame in frames)
            {
                var detections = await _objectDetector.DetectObjectsAsync(frame.FilePath);
                results.AddRange(detections);
            }

            // Return the highest confidence result for each object type
            return results
                .GroupBy(r => r.ObjectName)
                .Select(g => g.OrderByDescending(r => r.ConfidenceScore).First());
        }
        finally
        {
            // Clean up temporary frames after processing
            if (Directory.Exists(outputDirectory))
            {
                Directory.Delete(outputDirectory, recursive: true);
            }
        }
    }
}