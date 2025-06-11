# NMF to WAV Conversion API Testing

This directory contains scripts for testing the NMF to WAV conversion API.

## Files

- `download-test-nmf.sh`: Creates a mock NMF file for testing
- `test-api.sh`: Tests the API by sending a request to convert the mock NMF file
- `test-files/`: Directory containing test files

## How to Test

1. Start the API:
   ```
   cd /Users/alexk/Projects/nmf2wav/src/backend/nmf2wav
   dotnet run
   ```

2. In a new terminal, run the test script:
   ```
   cd /Users/alexk/Projects/nmf2wav/src/scripts
   ./test-api.sh
   ```

3. Check the API response. If successful, you should see a JSON response with details about the converted file.

4. Download the converted WAV file using the URL from the response:
   ```
   curl -o converted_sample.wav http://localhost:5180/api/audioconversion/download/converted_sample.wav
   ```

5. Verify the WAV file plays correctly using an audio player.

## Notes

- The mock NMF file is not a real audio file but serves as a placeholder for testing the API
- In a real-world scenario, you would use actual NMF files for testing