# NMF2WAV Converter

A .NET Core application for converting between WAV and NMF audio formats.

## Project Structure

- `src/backend/nmf2wav.api`: API project with controllers and endpoints
- `src/backend/nmf2wav`: Core business logic and services
- `src/scripts`: Test scripts for the application

## Features

- Convert WAV files to NMF format
- Convert NMF files back to WAV format
- RESTful API for audio conversion
- Test scripts in both Bash and PowerShell

## Getting Started

### Prerequisites

- .NET 8.0 SDK
- PowerShell (optional, for running PowerShell test scripts)

### Running the API

```bash
cd src/backend/nmf2wav.api
dotnet run --urls="http://localhost:5180"
```

### Testing the API

Using Bash:
```bash
bash src/scripts/test-beatles-conversion.sh
```

Using PowerShell:
```powershell
pwsh src/scripts/Test-BeatlesConversion.ps1
```

## API Endpoints

- `POST /api/audioconversion/convert-wav-to-nmf`: Convert WAV to NMF
- `POST /api/audioconversion/convert`: Convert NMF to WAV
- `GET /api/audioconversion/download/{fileName}`: Download converted files
- `GET /api/audioconversion/status`: Check API status