using Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace WebApp.Controllers;

[ApiController]
[Route("api/[controller]")]
public class VideoProcessingController : ControllerBase
{
    private readonly IObjectDetectionService _objectDetectionService;

    public VideoProcessingController(IObjectDetectionService objectDetectionService)
    {
        _objectDetectionService = objectDetectionService;
    }

    [HttpPost("process")]
    public async Task<IActionResult> ProcessVideo(string description, IFormFile? videoFile)
    {
        const long maxFileSize = 500_000_000; // 500 MB
        
        if (videoFile == null || videoFile.Length == 0)
        {
            return BadRequest(new { Message = "No video file uploaded." });
        }
        
        if (videoFile.Length > maxFileSize)
        {
            return BadRequest(new { Message = $"File size exceeds the limit of {maxFileSize / (1024 * 1024)} MB." });
        }

        // Save the uploaded file to a temporary directory
        var tempDir = Path.Combine("UploadedVideos");
        Directory.CreateDirectory(tempDir);
        var filePath = Path.Combine(tempDir, videoFile.FileName);

        try
        {
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await videoFile.CopyToAsync(stream);
            }

            // Pass the file path to the processing service
            var detectionResults = await _objectDetectionService.ProcessVideoAsync(filePath);

            return Ok(new
            {
                Message = "Video processed successfully.",
                Description = description,
                Results = detectionResults
            });
        }
        finally
        {
            // Optionally delete the video after processing
            if (!string.IsNullOrEmpty(filePath) && System.IO.File.Exists(filePath))
            {
                System.IO.File.Delete(filePath);
            }
        }
    }
}