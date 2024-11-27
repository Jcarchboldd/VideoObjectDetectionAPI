namespace Domain.ValueObjects;

public class VideoFrame
{
    public double FrameNumber { get; }
    public string FilePath { get; }

    public VideoFrame(double frameNumber, string filePath)
    {
        FrameNumber = frameNumber;
        FilePath = filePath;
    }
}