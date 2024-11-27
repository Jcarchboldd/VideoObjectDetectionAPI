using Domain.ValueObjects;
using OpenCvSharp;

namespace Infrastructure.VideoProcessing;

public class VideoFrameExtractor
{
    public IEnumerable<VideoFrame> ExtractFrames(string videoPath, string outputDirectory)
    {
        const int frameStep = 30;

        // Ensure the output directory exists
        Directory.CreateDirectory(outputDirectory);

        // Initialize VideoCapture
        using var capture = new VideoCapture(videoPath);
        
        if (!capture.IsOpened())
        {
            throw new InvalidOperationException("Failed to open the video file.");
        }

        int totalFrames = (int)capture.Get(VideoCaptureProperties.FrameCount);
        double frameRate = capture.Fps;

        var extractedFrames = new List<VideoFrame>();

        for (int i = 0; i < totalFrames; i += frameStep)
        {
            // Set the position of the next frame
            capture.Set(VideoCaptureProperties.PosFrames, i);

            using var frame = new Mat();
            if (!capture.Read(frame) || frame.Empty())
                continue;

            // Resize frame to 224x224
            var resizedFrame = new Mat();
            Cv2.Resize(frame, resizedFrame, new OpenCvSharp.Size(224, 224));

            // Save frame to output directory
            string frameFileName = $"frame_{i}.jpg";
            string frameFilePath = Path.Combine(outputDirectory, frameFileName);
            Cv2.ImWrite(frameFilePath, resizedFrame);

            // Add metadata to the result list
            extractedFrames.Add(new VideoFrame(i / frameRate, frameFilePath));
        }

        return extractedFrames;
    }
}