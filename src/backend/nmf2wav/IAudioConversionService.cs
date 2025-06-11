using nmf2wav.Services.DTOs;

namespace nmf2wav.Services;

public interface IAudioConversionService
{
    Task<ConversionResponseDto> ConvertNmfToWavAsync(ConversionRequestDto request);
    Task<ConversionResponseDto> ConvertWavToNmfAsync(WavToNmfRequestDto request);
}