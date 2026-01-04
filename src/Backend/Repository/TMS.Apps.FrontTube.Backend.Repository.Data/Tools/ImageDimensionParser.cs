namespace TMS.Apps.FrontTube.Backend.Repository.Data.Tools;

/// <summary>
/// Parses image dimensions from binary data by reading image format headers.
/// Supports JPEG, PNG, GIF, WebP, and BMP formats.
/// </summary>
internal static class ImageDimensionParser
{
    /// <summary>
    /// Extracts width and height from raw image data by parsing format headers.
    /// </summary>
    /// <param name="data">Raw image bytes.</param>
    /// <returns>Width and height tuple, or (0, 0) if parsing fails.</returns>
    public static (int Width, int Height) GetImageDimensions(byte[] data)
    {
        if (data is null || data.Length < 8)
        {
            return (0, 0);
        }

        // PNG: 89 50 4E 47 0D 0A 1A 0A
        if (data.Length > 24 && data[0] == 0x89 && data[1] == 0x50 && data[2] == 0x4E && data[3] == 0x47)
        {
            return ParsePng(data);
        }

        // JPEG: FF D8 FF
        if (data.Length > 2 && data[0] == 0xFF && data[1] == 0xD8 && data[2] == 0xFF)
        {
            return ParseJpeg(data);
        }

        // GIF: 47 49 46 38 (GIF8)
        if (data.Length > 10 && data[0] == 0x47 && data[1] == 0x49 && data[2] == 0x46 && data[3] == 0x38)
        {
            return ParseGif(data);
        }

        // WebP: 52 49 46 46 ... 57 45 42 50 (RIFF...WEBP)
        if (data.Length > 30 && data[0] == 0x52 && data[1] == 0x49 && data[2] == 0x46 && data[3] == 0x46
            && data[8] == 0x57 && data[9] == 0x45 && data[10] == 0x42 && data[11] == 0x50)
        {
            return ParseWebP(data);
        }

        // BMP: 42 4D (BM)
        if (data.Length > 26 && data[0] == 0x42 && data[1] == 0x4D)
        {
            return ParseBmp(data);
        }

        return (0, 0);
    }

    private static (int Width, int Height) ParsePng(byte[] data)
    {
        // IHDR chunk starts at byte 8, width at 16, height at 20 (big-endian)
        var width = (data[16] << 24) | (data[17] << 16) | (data[18] << 8) | data[19];
        var height = (data[20] << 24) | (data[21] << 16) | (data[22] << 8) | data[23];
        return (width, height);
    }

    private static (int Width, int Height) ParseJpeg(byte[] data)
    {
        // Scan for SOFn marker (Start of Frame)
        var offset = 2;
        while (offset < data.Length - 9)
        {
            if (data[offset] != 0xFF)
            {
                offset++;
                continue;
            }

            var marker = data[offset + 1];

            // SOF0, SOF1, SOF2 markers (Baseline, Extended, Progressive)
            if (marker is 0xC0 or 0xC1 or 0xC2)
            {
                var height = (data[offset + 5] << 8) | data[offset + 6];
                var width = (data[offset + 7] << 8) | data[offset + 8];
                return (width, height);
            }

            // Skip marker segment
            if (marker >= 0xC0 && marker <= 0xFE && marker != 0xD8 && marker != 0xD9)
            {
                var length = (data[offset + 2] << 8) | data[offset + 3];
                offset += 2 + length;
            }
            else
            {
                offset++;
            }
        }

        return (0, 0);
    }

    private static (int Width, int Height) ParseGif(byte[] data)
    {
        // Width at offset 6, height at offset 8 (little-endian)
        var width = data[6] | (data[7] << 8);
        var height = data[8] | (data[9] << 8);
        return (width, height);
    }

    private static (int Width, int Height) ParseWebP(byte[] data)
    {
        // Check for VP8 or VP8L chunk
        if (data.Length > 30 && data[12] == 0x56 && data[13] == 0x50 && data[14] == 0x38)
        {
            // VP8 lossy
            if (data[15] == 0x20) // Space character
            {
                if (data.Length > 26)
                {
                    var width = (data[26] | (data[27] << 8)) & 0x3FFF;
                    var height = (data[28] | (data[29] << 8)) & 0x3FFF;
                    return (width, height);
                }
            }
            // VP8L lossless
            else if (data[15] == 0x4C) // 'L'
            {
                if (data.Length > 25)
                {
                    var bits = data[21] | (data[22] << 8) | (data[23] << 16) | (data[24] << 24);
                    var width = (bits & 0x3FFF) + 1;
                    var height = ((bits >> 14) & 0x3FFF) + 1;
                    return (width, height);
                }
            }
            // VP8X extended
            else if (data[15] == 0x58) // 'X'
            {
                if (data.Length > 30)
                {
                    var width = 1 + (data[24] | (data[25] << 8) | (data[26] << 16));
                    var height = 1 + (data[27] | (data[28] << 8) | (data[29] << 16));
                    return (width, height);
                }
            }
        }

        return (0, 0);
    }

    private static (int Width, int Height) ParseBmp(byte[] data)
    {
        // Width at offset 18, height at offset 22 (little-endian, signed)
        var width = data[18] | (data[19] << 8) | (data[20] << 16) | (data[21] << 24);
        var height = data[22] | (data[23] << 8) | (data[24] << 16) | (data[25] << 24);

        // Height can be negative (top-down DIB)
        if (height < 0)
        {
            height = -height;
        }

        return (width, height);
    }
}
