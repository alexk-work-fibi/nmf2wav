using Microsoft.AspNetCore.Mvc;
using nmf2wav.Services;
using nmf2wav.Services.DTOs;
using System.IO;

namespace nmf2wav.api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AudioConversionController : ControllerBase
{
    private readonly IAudioConversionService _conversionService;
    private readonly ILogger<AudioConversionController> _logger;
    private readonly IWebHostEnvironment _environment;

    public AudioConversionController(
        IAudioConversionService conversionService,
        ILogger<AudioConversionController> logger,
        IWebHostEnvironment environment)
    {
        _conversionService = conversionService;
        _logger = logger;
        _environment = environment;
    }

    [HttpPost("convert")]
    [ProducesResponseType(typeof(ConversionResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [RequestFormLimits(MultipartBodyLengthLimit = 104857600)] // 100 MB limit
    [RequestSizeLimit(104857600)] // 100 MB limit
    public async Task<IActionResult> ConvertNmfToWav([FromForm] ConversionRequestDto request)
    {
        if (request.NmfFile == null || request.NmfFile.Length == 0)
        {
            return BadRequest("No file was uploaded.");
        }

        var result = await _conversionService.ConvertNmfToWavAsync(request);
        
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    [HttpPost("convert-wav-to-nmf")]
    [ProducesResponseType(typeof(ConversionResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [RequestFormLimits(MultipartBodyLengthLimit = 104857600)] // 100 MB limit
    [RequestSizeLimit(104857600)] // 100 MB limit
    public async Task<IActionResult> ConvertWavToNmf([FromForm] WavToNmfRequestDto request)
    {
        if (request.WavFile == null || request.WavFile.Length == 0)
        {
            return BadRequest("No file was uploaded.");
        }

        var result = await _conversionService.ConvertWavToNmfAsync(request);
        
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    [HttpGet("download/{fileName}")]
    public IActionResult DownloadFile(string fileName)
    {
        var outputDirectory = Path.Combine(_environment.ContentRootPath, "OutputFiles");
        var filePath = Path.Combine(outputDirectory, fileName);

        if (!System.IO.File.Exists(filePath))
        {
            return NotFound($"File {fileName} not found.");
        }

        // Determine content type based on file extension
        string contentType = "application/octet-stream";
        string extension = Path.GetExtension(fileName).ToLowerInvariant();
        
        if (extension == ".wav")
        {
            contentType = "audio/wav";
        }
        else if (extension == ".nmf")
        {
            contentType = "application/octet-stream"; // Custom MIME type for NMF
        }

        var fileBytes = System.IO.File.ReadAllBytes(filePath);
        return File(fileBytes, contentType, fileName);
    }

    [HttpGet("status")]
    public IActionResult GetStatus()
    {
        return Ok(new { Status = "API is running" });
    }
}