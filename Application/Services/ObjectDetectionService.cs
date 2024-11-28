using Application.Interfaces;
using Domain.Entities;
using Infrastructure.ObjectDetection;
using Infrastructure.VideoProcessing;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Application.Services;

public class ObjectDetectionService : IObjectDetectionService
{
    private readonly VideoFrameExtractor _frameExtractor;
    private readonly OnnxObjectDetector _objectDetector;
    private readonly ILogger<ObjectDetectionService> _logger;

    public ObjectDetectionService(VideoFrameExtractor frameExtractor, OnnxObjectDetector objectDetector, ILogger<ObjectDetectionService> logger)
    {
        _frameExtractor = frameExtractor;
        _objectDetector = objectDetector;
        _logger = logger;
    }

    public async Task<IEnumerable<DetectionResult>> ProcessVideoAsync(string description, IFormFile videoFile)
    {
        const long maxFileSize = 500_000_000; // 500 MB

        if (videoFile.Length > maxFileSize)
        {
            throw new InvalidOperationException($"File size exceeds the limit of {maxFileSize / (1024 * 1024)} MB.");
        }

        _logger.LogInformation("Processing video: {FileName} with description: {Description}", videoFile.FileName, description);

        // Save the uploaded file to a temporary directory
        var tempDir = Path.Combine("UploadedVideos");
        Directory.CreateDirectory(tempDir);
        var filePath = Path.Combine(tempDir, videoFile.FileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await videoFile.CopyToAsync(stream);
        }

        // Use the system's temporary directory for frame extraction
        var outputDirectory = Path.Combine(Path.GetTempPath(), "ExtractedFrames", Guid.NewGuid().ToString());
        Directory.CreateDirectory(outputDirectory);

        var results = new List<DetectionResult>();

        try
        {
            _logger.LogInformation("Extracting frames from video: {FilePath}", filePath);

            // Extract frames to the temporary directory
            var frames = _frameExtractor.ExtractFrames(filePath, outputDirectory);

            foreach (var frame in frames)
            {
                try
                {
                    _logger.LogInformation("Detecting objects in frame: {FramePath}", frame.FilePath);
                    var detections = await _objectDetector.DetectObjectsAsync(frame.FilePath);
                    results.AddRange(detections);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error during object detection for frame: {FramePath}", frame.FilePath);
                    throw;
                }
            }

            _logger.LogInformation("Finished processing video: {FilePath}", filePath);

            // Return the highest confidence result for each object type
            return results
                .GroupBy(r => r.ObjectName)
                .Select(g => g.OrderByDescending(r => r.ConfidenceScore).First());
        }
        finally
        {
            // Optionally delete the video after processing
            if (!string.IsNullOrEmpty(filePath) && File.Exists(filePath))
            {
                File.Delete(filePath);
                _logger.LogInformation("Deleted uploaded video file: {FilePath}", filePath);
            }

            // Clean up temporary frames after processing
            if (Directory.Exists(outputDirectory))
            {
                Directory.Delete(outputDirectory, recursive: true);
                _logger.LogInformation("Deleted temporary frame directory: {OutputDirectory}", outputDirectory);
            }
        }
    }
}
