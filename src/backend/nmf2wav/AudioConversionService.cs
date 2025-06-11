using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using nmf2wav.Services.DTOs;
using nmf2wav.Services.Helpers;
using nmf2wav.Services.Models;
using System.IO;

namespace nmf2wav.Services;

/// <summary>
/// Service for converting between NMF and WAV audio formats
/// </summary>
public class AudioConversionService : IAudioConversionService
{
    private readonly ILogger<AudioConversionService> _logger;
    private readonly IWebHostEnvironment _environment;
    private readonly string _outputDirectory;

    public AudioConversionService(ILogger<AudioConversionService> logger, IWebHostEnvironment environment)
    {
        _logger = logger;
        _environment = environment;
        _outputDirectory = Path.Combine(_environment.ContentRootPath, "OutputFiles");
        
        // Ensure output directory exists
        if (!Directory.Exists(_outputDirectory))
        {
            Directory.CreateDirectory(_outputDirectory);
        }
    }

    /// <summary>
    /// Converts an NMF file to WAV format
    /// </summary>
    public async Task<ConversionResponseDto> ConvertNmfToWavAsync(ConversionRequestDto request)
    {
        try
        {
            if (request.NmfFile == null || request.NmfFile.Length == 0)
            {
                return new ConversionResponseDto
                {
                    Success = false,
                    ErrorMessage = "No file was uploaded."
                };
            }

            // Validate file extension
            var fileExtension = Path.GetExtension(request.NmfFile.FileName).ToLowerInvariant();
            if (fileExtension != ".nmf")
            {
                return new ConversionResponseDto
                {
                    Success = false,
                    ErrorMessage = "Only .nmf files are supported."
                };
            }

            // Generate output filename
            var outputFileName = string.IsNullOrEmpty(request.OutputFileName)
                ? Path.GetFileNameWithoutExtension(request.NmfFile.FileName) + ".wav"
                : request.OutputFileName.EndsWith(".wav") 
                    ? request.OutputFileName 
                    : request.OutputFileName + ".wav";

            var outputPath = Path.Combine(_outputDirectory, outputFileName);

            // Save the uploaded NMF file to a temporary location
            var tempNmfPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName() + ".nmf");
            using (var fileStream = new FileStream(tempNmfPath, FileMode.Create))
            {
                await request.NmfFile.CopyToAsync(fileStream);
            }

            // Perform the NMF to WAV conversion
            await ConvertNmfToWavFileAsync(tempNmfPath, outputPath);

            // Clean up the temporary file
            if (File.Exists(tempNmfPath))
            {
                File.Delete(tempNmfPath);
            }

            // Get file info for response
            var fileInfo = new FileInfo(outputPath);
            
            return new ConversionResponseDto
            {
                FileName = outputFileName,
                FileUrl = $"/api/audioconversion/download/{outputFileName}",
                FileSize = fileInfo.Length,
                Success = true
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error converting NMF to WAV");
            return new ConversionResponseDto
            {
                Success = false,
                ErrorMessage = $"Conversion failed: {ex.Message}"
            };
        }
    }

    /// <summary>
    /// Converts a WAV file to NMF format
    /// </summary>
    public async Task<ConversionResponseDto> ConvertWavToNmfAsync(WavToNmfRequestDto request)
    {
        try
        {
            if (request.WavFile == null || request.WavFile.Length == 0)
            {
                return new ConversionResponseDto
                {
                    Success = false,
                    ErrorMessage = "No file was uploaded."
                };
            }

            // Validate file extension
            var fileExtension = Path.GetExtension(request.WavFile.FileName).ToLowerInvariant();
            if (fileExtension != ".wav")
            {
                return new ConversionResponseDto
                {
                    Success = false,
                    ErrorMessage = "Only .wav files are supported."
                };
            }

            // Generate output filename
            var outputFileName = string.IsNullOrEmpty(request.OutputFileName)
                ? Path.GetFileNameWithoutExtension(request.WavFile.FileName) + ".nmf"
                : request.OutputFileName.EndsWith(".nmf") 
                    ? request.OutputFileName 
                    : request.OutputFileName + ".nmf";

            var outputPath = Path.Combine(_outputDirectory, outputFileName);

            // Save the uploaded WAV file to a temporary location
            var tempWavPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName() + ".wav");
            using (var fileStream = new FileStream(tempWavPath, FileMode.Create))
            {
                await request.WavFile.CopyToAsync(fileStream);
            }

            // Perform the WAV to NMF conversion
            await ConvertWavToNmfFileAsync(tempWavPath, outputPath);

            // Clean up the temporary file
            if (File.Exists(tempWavPath))
            {
                File.Delete(tempWavPath);
            }

            // Get file info for response
            var fileInfo = new FileInfo(outputPath);
            
            return new ConversionResponseDto
            {
                FileName = outputFileName,
                FileUrl = $"/api/audioconversion/download/{outputFileName}",
                FileSize = fileInfo.Length,
                Success = true
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error converting WAV to NMF");
            return new ConversionResponseDto
            {
                Success = false,
                ErrorMessage = $"Conversion failed: {ex.Message}"
            };
        }
    }

    /// <summary>
    /// Converts an NMF file to WAV format
    /// </summary>
    private async Task ConvertNmfToWavFileAsync(string nmfFilePath, string wavFilePath)
    {
        using (var fileStream = new FileStream(nmfFilePath, FileMode.Open, FileAccess.Read))
        using (var reader = new BinaryReader(fileStream))
        using (var outputStream = new FileStream(wavFilePath, FileMode.Create))
        using (var writer = new BinaryWriter(outputStream))
        {
            // Read NMF header
            var header = AudioFileProcessor.ReadNmfHeader(reader);
            _logger.LogInformation($"NMF Header: Magic={header.Magic}, Version={header.Version}, Channels={header.Channels}, " +
                                  $"SampleRate={header.SampleRate}, BitDepth={header.BitDepth}, NumSamples={header.NumSamples}");

            // Write WAV header
            AudioFileProcessor.WriteWavHeader(writer, header);

            // Process audio data
            await AudioFileProcessor.ProcessAudioData(reader, writer, header);

            // Update file size in WAV header
            AudioFileProcessor.UpdateWavFileSize(writer);
            
            _logger.LogInformation($"Converted {nmfFilePath} to {wavFilePath}");
        }
    }

    /// <summary>
    /// Converts a WAV file to NMF format
    /// </summary>
    private async Task ConvertWavToNmfFileAsync(string wavFilePath, string nmfFilePath)
    {
        using (var fileStream = new FileStream(wavFilePath, FileMode.Open, FileAccess.Read))
        using (var reader = new BinaryReader(fileStream))
        using (var outputStream = new FileStream(nmfFilePath, FileMode.Create))
        using (var writer = new BinaryWriter(outputStream))
        {
            // Read WAV header
            var wavHeader = AudioFileProcessor.ReadWavHeader(reader);
            _logger.LogInformation($"WAV Header: Format={wavHeader.AudioFormat}, Channels={wavHeader.Channels}, " +
                                  $"SampleRate={wavHeader.SampleRate}, BitDepth={wavHeader.BitsPerSample}, DataSize={wavHeader.DataSize}");

            // Create NMF header from WAV header
            var nmfHeader = new NmfHeader
            {
                Magic = "NMF1",
                Version = 1,
                Channels = wavHeader.Channels,
                SampleRate = wavHeader.SampleRate,
                BitDepth = wavHeader.BitsPerSample,
                NumSamples = wavHeader.DataSize / (wavHeader.Channels * (wavHeader.BitsPerSample / 8))
            };

            // Write NMF header
            AudioFileProcessor.WriteNmfHeader(writer, nmfHeader);

            // Process audio data
            await AudioFileProcessor.ProcessWavAudioData(reader, writer, wavHeader);
            
            _logger.LogInformation($"Converted {wavFilePath} to {nmfFilePath}");
        }
    }
}