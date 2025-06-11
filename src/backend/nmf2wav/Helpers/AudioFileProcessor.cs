using nmf2wav.Services.Models;
using System.Text;

namespace nmf2wav.Services.Helpers;

/// <summary>
/// Helper class for processing audio files
/// </summary>
public static class AudioFileProcessor
{
    /// <summary>
    /// Reads an NMF header from a binary reader
    /// </summary>
    public static NmfHeader ReadNmfHeader(BinaryReader reader)
    {
        var header = new NmfHeader();

        // Read magic number (should be "NMF1")
        byte[] magicBytes = reader.ReadBytes(4);
        header.Magic = Encoding.ASCII.GetString(magicBytes);
        
        if (header.Magic != "NMF1")
        {
            throw new InvalidDataException("Invalid NMF file format: incorrect magic number");
        }

        // Read version
        header.Version = reader.ReadInt32();
        
        // Read channels
        header.Channels = reader.ReadInt32();
        if (header.Channels <= 0 || header.Channels > 8)
        {
            throw new InvalidDataException($"Invalid number of channels: {header.Channels}");
        }
        
        // Read sample rate
        header.SampleRate = reader.ReadInt32();
        if (header.SampleRate <= 0 || header.SampleRate > 192000)
        {
            throw new InvalidDataException($"Invalid sample rate: {header.SampleRate}");
        }
        
        // Read bit depth
        header.BitDepth = reader.ReadInt32();
        if (header.BitDepth != 8 && header.BitDepth != 16 && header.BitDepth != 24 && header.BitDepth != 32)
        {
            throw new InvalidDataException($"Unsupported bit depth: {header.BitDepth}");
        }
        
        // Read number of samples
        header.NumSamples = reader.ReadInt32();
        if (header.NumSamples <= 0)
        {
            throw new InvalidDataException($"Invalid number of samples: {header.NumSamples}");
        }

        return header;
    }

    /// <summary>
    /// Reads a WAV header from a binary reader
    /// </summary>
    public static WavHeader ReadWavHeader(BinaryReader reader)
    {
        var header = new WavHeader();

        // Read RIFF header
        byte[] riffHeader = reader.ReadBytes(4);
        string riffId = Encoding.ASCII.GetString(riffHeader);
        if (riffId != "RIFF")
        {
            throw new InvalidDataException("Invalid WAV file format: missing RIFF header");
        }

        // Read file size (minus 8 bytes)
        header.FileSize = reader.ReadInt32() + 8;

        // Read WAVE identifier
        byte[] waveHeader = reader.ReadBytes(4);
        string waveId = Encoding.ASCII.GetString(waveHeader);
        if (waveId != "WAVE")
        {
            throw new InvalidDataException("Invalid WAV file format: missing WAVE identifier");
        }

        // Read format chunk
        bool foundFmt = false;
        bool foundData = false;

        while (!foundData && reader.BaseStream.Position < reader.BaseStream.Length)
        {
            // Read chunk ID
            byte[] chunkIdBytes = reader.ReadBytes(4);
            string chunkId = Encoding.ASCII.GetString(chunkIdBytes);
            int chunkSize = reader.ReadInt32();

            switch (chunkId)
            {
                case "fmt ":
                    foundFmt = true;
                    // Read format data
                    header.AudioFormat = reader.ReadInt16();
                    header.Channels = reader.ReadInt16();
                    header.SampleRate = reader.ReadInt32();
                    header.ByteRate = reader.ReadInt32();
                    header.BlockAlign = reader.ReadInt16();
                    header.BitsPerSample = reader.ReadInt16();

                    // Skip any extra format bytes
                    if (chunkSize > 16)
                    {
                        reader.BaseStream.Seek(chunkSize - 16, SeekOrigin.Current);
                    }
                    break;

                case "data":
                    foundData = true;
                    header.DataSize = chunkSize;
                    header.DataPosition = (int)reader.BaseStream.Position;
                    break;

                default:
                    // Skip unknown chunks
                    reader.BaseStream.Seek(chunkSize, SeekOrigin.Current);
                    break;
            }
        }

        if (!foundFmt || !foundData)
        {
            throw new InvalidDataException("Invalid WAV file format: missing required chunks");
        }

        return header;
    }

    /// <summary>
    /// Writes a WAV header to a binary writer
    /// </summary>
    public static void WriteWavHeader(BinaryWriter writer, NmfHeader nmfHeader)
    {
        // Calculate sizes
        int bytesPerSample = nmfHeader.BitDepth / 8;
        int dataSize = nmfHeader.NumSamples * nmfHeader.Channels * bytesPerSample;
        int fileSize = 36 + dataSize; // 36 bytes for header + data size
        
        // RIFF header
        writer.Write(Encoding.ASCII.GetBytes("RIFF"));
        writer.Write(fileSize);
        writer.Write(Encoding.ASCII.GetBytes("WAVE"));
        
        // Format chunk
        writer.Write(Encoding.ASCII.GetBytes("fmt "));
        writer.Write(16); // Chunk size
        writer.Write((short)1); // Audio format (PCM)
        writer.Write((short)nmfHeader.Channels);
        writer.Write(nmfHeader.SampleRate);
        writer.Write(nmfHeader.SampleRate * nmfHeader.Channels * bytesPerSample); // Byte rate
        writer.Write((short)(nmfHeader.Channels * bytesPerSample)); // Block align
        writer.Write((short)nmfHeader.BitDepth);
        
        // Data chunk
        writer.Write(Encoding.ASCII.GetBytes("data"));
        writer.Write(dataSize);
    }

    /// <summary>
    /// Writes an NMF header to a binary writer
    /// </summary>
    public static void WriteNmfHeader(BinaryWriter writer, NmfHeader header)
    {
        // Write magic number
        writer.Write(Encoding.ASCII.GetBytes(header.Magic));
        
        // Write version, channels, sample rate, bit depth, and number of samples
        writer.Write(header.Version);
        writer.Write(header.Channels);
        writer.Write(header.SampleRate);
        writer.Write(header.BitDepth);
        writer.Write(header.NumSamples);
    }

    /// <summary>
    /// Updates the WAV file size in the header
    /// </summary>
    public static void UpdateWavFileSize(BinaryWriter writer)
    {
        long fileSize = writer.BaseStream.Length;
        
        // Update RIFF chunk size (file size - 8 bytes)
        writer.BaseStream.Position = 4;
        writer.Write((int)(fileSize - 8));
        
        // Update data chunk size
        writer.BaseStream.Position = 40;
        writer.Write((int)(fileSize - 44));
    }

    /// <summary>
    /// Processes audio data from NMF to WAV
    /// </summary>
    public static async Task ProcessAudioData(BinaryReader reader, BinaryWriter writer, NmfHeader header)
    {
        int bytesPerSample = header.BitDepth / 8;
        int totalBytes = header.NumSamples * header.Channels * bytesPerSample;
        
        await CopyBytes(reader, writer, totalBytes);
    }

    /// <summary>
    /// Processes audio data from WAV to NMF
    /// </summary>
    public static async Task ProcessWavAudioData(BinaryReader reader, BinaryWriter writer, WavHeader header)
    {
        // Position reader at the start of audio data
        reader.BaseStream.Seek(header.DataPosition, SeekOrigin.Begin);
        
        await CopyBytes(reader, writer, header.DataSize);
    }

    /// <summary>
    /// Copies bytes from reader to writer with buffering
    /// </summary>
    private static async Task CopyBytes(BinaryReader reader, BinaryWriter writer, int totalBytes)
    {
        const int bufferSize = 4096;
        byte[] buffer = new byte[bufferSize];
        
        int bytesRemaining = totalBytes;
        while (bytesRemaining > 0)
        {
            int bytesToRead = Math.Min(bufferSize, bytesRemaining);
            int bytesRead = reader.Read(buffer, 0, bytesToRead);
            
            if (bytesRead == 0)
                break; // End of file
                
            writer.Write(buffer, 0, bytesRead);
            bytesRemaining -= bytesRead;
            
            // Allow other tasks to run
            if (bytesRemaining > 0 && bytesRemaining % (bufferSize * 10) == 0)
                await Task.Yield();
        }
    }
}