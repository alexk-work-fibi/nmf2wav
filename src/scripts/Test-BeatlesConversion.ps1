# Test script for converting The Beatles - Help!.wav to NMF and back

# Configuration
$ApiUrl = "http://localhost:5180/api/audioconversion"
$InputWav = Join-Path -Path $PSScriptRoot -ChildPath "test-files/The Beatles - Help!.wav"
$TestFilesDir = Join-Path -Path $PSScriptRoot -ChildPath "test-files"
$OutputDir = Join-Path -Path $PSScriptRoot -ChildPath "test-results"

# Create directories if they don't exist
if (-not (Test-Path -Path $TestFilesDir)) {
    New-Item -Path $TestFilesDir -ItemType Directory | Out-Null
}
if (-not (Test-Path -Path $OutputDir)) {
    New-Item -Path $OutputDir -ItemType Directory | Out-Null
}

# Colors for output
function Write-ColorOutput($ForegroundColor) {
    $fc = $host.UI.RawUI.ForegroundColor
    $host.UI.RawUI.ForegroundColor = $ForegroundColor
    if ($args) {
        Write-Output $args
    }
    else {
        $input | Write-Output
    }
    $host.UI.RawUI.ForegroundColor = $fc
}

Write-ColorOutput Yellow "Starting NMF2WAV Conversion Test"
Write-Output "----------------------------------------"

# Check if the API is running
Write-ColorOutput Yellow "Step 0: Checking if API is running..."
try {
    $statusResponse = Invoke-RestMethod -Uri "$ApiUrl/status" -Method Get -ErrorAction Stop
    Write-ColorOutput Green "✓ API is running"
}
catch {
    Write-ColorOutput Red "✗ API is not running. Please start it with 'dotnet run --project src/backend/nmf2wav.api/nmf2wav.api.csproj --urls=`"http://localhost:5180`"'"
    exit 1
}

# Check if the input file exists
Write-ColorOutput Yellow "Step 1: Checking if input file exists..."
if (-not (Test-Path -Path $InputWav)) {
    Write-ColorOutput Red "✗ Input file not found at $InputWav"
    exit 1
}
else {
    $fileName = Split-Path -Path $InputWav -Leaf
    $fileSize = "{0:N2} MB" -f ((Get-Item $InputWav).Length / 1MB)
    Write-ColorOutput Green "✓ Input file found: $fileName"
    Write-Output "  File size: $fileSize"
}

# Step 1: Convert WAV to NMF
Write-ColorOutput Yellow "Step 2: Converting WAV to NMF..."
$form = @{
    WavFile = Get-Item -Path $InputWav
    OutputFileName = "beatles_help.nmf"
}

try {
    $response = Invoke-RestMethod -Uri "$ApiUrl/convert-wav-to-nmf" -Method Post -Form $form -ErrorAction Stop
    
    if ($response.success -eq $true) {
        Write-ColorOutput Green "✓ WAV to NMF conversion successful"
        Write-Output "  File URL: $($response.fileUrl)"
    }
    else {
        Write-ColorOutput Red "✗ WAV to NMF conversion failed"
        Write-Output "  Error: $($response.errorMessage)"
        exit 1
    }
}
catch {
    Write-ColorOutput Red "✗ WAV to NMF conversion failed"
    Write-Output "  Error: $_"
    exit 1
}

# Step 2: Download the converted NMF file
Write-ColorOutput Yellow "Step 3: Downloading the converted NMF file..."
$nmfFile = Join-Path -Path $OutputDir -ChildPath "beatles_help.nmf"
try {
    Invoke-WebRequest -Uri "$ApiUrl/download/beatles_help.nmf" -OutFile $nmfFile -ErrorAction Stop
    
    if (Test-Path -Path $nmfFile) {
        $fileSize = "{0:N2} MB" -f ((Get-Item $nmfFile).Length / 1MB)
        Write-ColorOutput Green "✓ NMF file downloaded successfully"
        Write-Output "  File size: $fileSize"
        Write-Output "  Saved to: $nmfFile"
    }
    else {
        Write-ColorOutput Red "✗ Failed to download NMF file"
        exit 1
    }
}
catch {
    Write-ColorOutput Red "✗ Failed to download NMF file"
    Write-Output "  Error: $_"
    exit 1
}

# Step 3: Convert NMF back to WAV
Write-ColorOutput Yellow "Step 4: Converting NMF back to WAV..."
$form = @{
    NmfFile = Get-Item -Path $nmfFile
    OutputFileName = "beatles_help_reconverted.wav"
}

try {
    $response = Invoke-RestMethod -Uri "$ApiUrl/convert" -Method Post -Form $form -ErrorAction Stop
    
    if ($response.success -eq $true) {
        Write-ColorOutput Green "✓ NMF to WAV conversion successful"
        Write-Output "  File URL: $($response.fileUrl)"
    }
    else {
        Write-ColorOutput Red "✗ NMF to WAV conversion failed"
        Write-Output "  Error: $($response.errorMessage)"
        exit 1
    }
}
catch {
    Write-ColorOutput Red "✗ NMF to WAV conversion failed"
    Write-Output "  Error: $_"
    exit 1
}

# Step 4: Download the reconverted WAV file
Write-ColorOutput Yellow "Step 5: Downloading the reconverted WAV file..."
$wavFile = Join-Path -Path $OutputDir -ChildPath "beatles_help_reconverted.wav"
try {
    Invoke-WebRequest -Uri "$ApiUrl/download/beatles_help_reconverted.wav" -OutFile $wavFile -ErrorAction Stop
    
    if (Test-Path -Path $wavFile) {
        $fileSize = "{0:N2} MB" -f ((Get-Item $wavFile).Length / 1MB)
        Write-ColorOutput Green "✓ WAV file downloaded successfully"
        Write-Output "  File size: $fileSize"
        Write-Output "  Saved to: $wavFile"
    }
    else {
        Write-ColorOutput Red "✗ Failed to download WAV file"
        exit 1
    }
}
catch {
    Write-ColorOutput Red "✗ Failed to download WAV file"
    Write-Output "  Error: $_"
    exit 1
}

Write-Output "----------------------------------------"
Write-ColorOutput Green "Conversion test complete!"
Write-Output "Original WAV: $InputWav"
Write-Output "NMF file: $nmfFile"
Write-Output "Reconverted WAV: $wavFile"
Write-Output ""
Write-Output "You can now compare the original and reconverted WAV files to check audio quality."