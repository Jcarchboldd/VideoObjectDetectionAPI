using Domain.Entities;
using Microsoft.AspNetCore.Http;

namespace Application.Interfaces;

public interface IObjectDetectionService
{
    Task<IEnumerable<DetectionResult>> ProcessVideoAsync(string description, IFormFile videoFile);
}