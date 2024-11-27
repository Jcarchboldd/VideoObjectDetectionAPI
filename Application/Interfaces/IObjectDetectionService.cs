using Domain.Entities;

namespace Application.Interfaces;

public interface IObjectDetectionService
{
    Task<IEnumerable<DetectionResult>> ProcessVideoAsync(string videoPath);
}