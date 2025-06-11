using Microsoft.AspNetCore.Http;

namespace nmf2wav.Services.DTOs;

public class ConversionRequestDto
{
    public IFormFile NmfFile { get; set; } = null!;
    public string? OutputFileName { get; set; }
}

public class WavToNmfRequestDto
{
    public IFormFile WavFile { get; set; } = null!;
    public string? OutputFileName { get; set; }
}

public class ConversionResponseDto
{
    public string FileName { get; set; } = string.Empty;
    public string FileUrl { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
}