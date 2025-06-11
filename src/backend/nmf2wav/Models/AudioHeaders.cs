namespace nmf2wav.Services.Models;

/// <summary>
/// Represents the header structure of an NMF audio file
/// </summary>
public class NmfHeader
{
    public string Magic { get; set; } = string.Empty;
    public int Version { get; set; }
    public int Channels { get; set; }
    public int SampleRate { get; set; }
    public int BitDepth { get; set; }
    public int NumSamples { get; set; }
}

/// <summary>
/// Represents the header structure of a WAV audio file
/// </summary>
public class WavHeader
{
    public int FileSize { get; set; }
    public short AudioFormat { get; set; }
    public short Channels { get; set; }
    public int SampleRate { get; set; }
    public int ByteRate { get; set; }
    public short BlockAlign { get; set; }
    public short BitsPerSample { get; set; }
    public int DataSize { get; set; }
    public int DataPosition { get; set; }
}