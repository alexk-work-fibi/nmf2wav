#!/bin/bash

# Test script for converting The Beatles - Help!.wav to NMF and back

# Configuration
API_URL="http://localhost:5180/api/audioconversion"
INPUT_WAV="/Users/alexk/Projects/nmf2wav/src/scripts/test-files/The Beatles - Help!.wav"
TEST_FILES_DIR="/Users/alexk/Projects/nmf2wav/src/scripts/test-files"
OUTPUT_DIR="/Users/alexk/Projects/nmf2wav/src/scripts/test-results"

# Create directories if they don't exist
mkdir -p "$TEST_FILES_DIR"
mkdir -p "$OUTPUT_DIR"

# Colors for output
GREEN='\033[0;32m'
RED='\033[0;31m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

echo -e "${YELLOW}Starting NMF2WAV Conversion Test${NC}"
echo "----------------------------------------"

# Check if the API is running
echo -e "${YELLOW}Step 0: Checking if API is running...${NC}"
if curl -s "$API_URL/status" > /dev/null; then
  echo -e "${GREEN}✓ API is running${NC}"
else
  echo -e "${RED}✗ API is not running. Please start it with 'dotnet run --project src/backend/nmf2wav.api/nmf2wav.api.csproj --urls=\"http://localhost:5180\"'${NC}"
  exit 1
fi

# Check if the input file exists
echo -e "${YELLOW}Step 1: Checking if input file exists...${NC}"
if [ ! -f "$INPUT_WAV" ]; then
  echo -e "${RED}✗ Input file not found at $INPUT_WAV${NC}"
  exit 1
else
  echo -e "${GREEN}✓ Input file found: $(basename "$INPUT_WAV")${NC}"
  echo "  File size: $(du -h "$INPUT_WAV" | cut -f1)"
fi

# Step 1: Convert WAV to NMF
echo -e "${YELLOW}Step 2: Converting WAV to NMF...${NC}"
RESPONSE=$(curl -s -X POST "$API_URL/convert-wav-to-nmf" \
  -H "accept: */*" \
  -H "Content-Type: multipart/form-data" \
  -F "WavFile=@$INPUT_WAV" \
  -F "OutputFileName=beatles_help.nmf")

# Check if conversion was successful
if echo "$RESPONSE" | grep -q "\"success\":true"; then
  echo -e "${GREEN}✓ WAV to NMF conversion successful${NC}"
  FILE_URL=$(echo "$RESPONSE" | grep -o '"fileUrl":"[^"]*"' | cut -d'"' -f4)
  echo "  File URL: $FILE_URL"
else
  echo -e "${RED}✗ WAV to NMF conversion failed${NC}"
  echo "  Error: $RESPONSE"
  exit 1
fi

# Step 2: Download the converted NMF file
echo -e "${YELLOW}Step 3: Downloading the converted NMF file...${NC}"
NMF_FILE="$OUTPUT_DIR/beatles_help.nmf"
curl -s -o "$NMF_FILE" "$API_URL/download/beatles_help.nmf"

if [ -f "$NMF_FILE" ]; then
  echo -e "${GREEN}✓ NMF file downloaded successfully${NC}"
  echo "  File size: $(du -h "$NMF_FILE" | cut -f1)"
  echo "  Saved to: $NMF_FILE"
else
  echo -e "${RED}✗ Failed to download NMF file${NC}"
  exit 1
fi

# Step 3: Convert NMF back to WAV
echo -e "${YELLOW}Step 4: Converting NMF back to WAV...${NC}"
RESPONSE=$(curl -s -X POST "$API_URL/convert" \
  -H "accept: */*" \
  -H "Content-Type: multipart/form-data" \
  -F "NmfFile=@$NMF_FILE" \
  -F "OutputFileName=beatles_help_reconverted.wav")

# Check if conversion was successful
if echo "$RESPONSE" | grep -q "\"success\":true"; then
  echo -e "${GREEN}✓ NMF to WAV conversion successful${NC}"
  FILE_URL=$(echo "$RESPONSE" | grep -o '"fileUrl":"[^"]*"' | cut -d'"' -f4)
  echo "  File URL: $FILE_URL"
else
  echo -e "${RED}✗ NMF to WAV conversion failed${NC}"
  echo "  Error: $RESPONSE"
  exit 1
fi

# Step 4: Download the reconverted WAV file
echo -e "${YELLOW}Step 5: Downloading the reconverted WAV file...${NC}"
WAV_FILE="$OUTPUT_DIR/beatles_help_reconverted.wav"
curl -s -o "$WAV_FILE" "$API_URL/download/beatles_help_reconverted.wav"

if [ -f "$WAV_FILE" ]; then
  echo -e "${GREEN}✓ WAV file downloaded successfully${NC}"
  echo "  File size: $(du -h "$WAV_FILE" | cut -f1)"
  echo "  Saved to: $WAV_FILE"
else
  echo -e "${RED}✗ Failed to download WAV file${NC}"
  exit 1
fi

echo "----------------------------------------"
echo -e "${GREEN}Conversion test complete!${NC}"
echo "Original WAV: $INPUT_WAV"
echo "NMF file: $NMF_FILE"
echo "Reconverted WAV: $WAV_FILE"
echo ""
echo "You can now compare the original and reconverted WAV files to check audio quality."