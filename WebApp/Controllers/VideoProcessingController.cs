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

    /// <summary>
    /// Processes a video file and returns detected objects or animals with metadata.
    /// </summary>
    /// <param name="description" example="Wildlife footage">Optional description of the uploaded video.</param>
    /// <param name="videoFile">The video file to process (e.g., MP4, maximum size: 500 MB).</param>
    /// <returns>
    /// A JSON object containing detected objects, confidence scores, and frame paths.
    /// </returns>
    /// <remarks>
    /// Example request:
    /// 
    /// POST: /api/videoprocessing/process
    /// 
    /// The uploaded video is processed using a pre-trained ONNX model (e.g., GoogLeNet) to detect objects or animals
    /// in individual frames. The output includes the top confidence result for each detected object type.
    /// </remarks>
    /// <response code="200">Returns detected objects with metadata.</response>
    /// <response code="400">If no video is uploaded or the file is invalid.</response>
    /// <response code="500">If an internal error occurs during processing.</response>
    [HttpPost("process")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ProcessVideo(string description, IFormFile? videoFile)
    {
        if (videoFile == null || videoFile.Length == 0)
        {
            return BadRequest(new { Message = "No video file uploaded." });
        }
        
        try
        {
            var detectionResults = await _objectDetectionService.ProcessVideoAsync(description, videoFile);

            return Ok(new
            {
                Message = "Video processed successfully.",
                Description = description,
                Results = detectionResults
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Message = "An error occurred while processing the video.", Details = ex.Message });
        }
    }
}