using Microsoft.AspNetCore.Http;

namespace DentalClinic.API.Common;

/// <summary>
/// Server-side attachment validation (size + magic-byte content checks).
/// </summary>
public static class AttachmentFileValidator
{
    public const long MaxBytes = 10 * 1024 * 1024;

    public static void Validate(IFormFile file)
    {
        if (file is null || file.Length == 0)
        {
            throw new BusinessRuleException("File is required.");
        }

        if (file.Length > MaxBytes)
        {
            throw new BusinessRuleException("File exceeds the maximum allowed size (10 MB).");
        }

        using var stream = file.OpenReadStream();
        if (!IsAllowedContent(stream))
        {
            throw new BusinessRuleException("File type not allowed.");
        }
    }

    public static void ValidateContent(Stream content)
    {
        if (!content.CanSeek)
        {
            throw new BusinessRuleException("Unable to validate the uploaded file.");
        }

        var originalPosition = content.Position;
        try
        {
            if (!IsAllowedContent(content))
            {
                throw new BusinessRuleException("File type not allowed.");
            }
        }
        finally
        {
            content.Position = originalPosition;
        }
    }

    private static bool IsAllowedContent(Stream stream)
    {
        Span<byte> header = stackalloc byte[12];
        var read = stream.Read(header);

        if (read >= 4 && header[0] == 0x25 && header[1] == 0x50 && header[2] == 0x44 && header[3] == 0x46)
        {
            return true; // PDF
        }

        if (read >= 3 && header[0] == 0xFF && header[1] == 0xD8 && header[2] == 0xFF)
        {
            return true; // JPEG
        }

        if (read >= 8
            && header[0] == 0x89 && header[1] == 0x50 && header[2] == 0x4E && header[3] == 0x47
            && header[4] == 0x0D && header[5] == 0x0A && header[6] == 0x1A && header[7] == 0x0A)
        {
            return true; // PNG
        }

        if (read >= 6
            && header[0] == 0x47 && header[1] == 0x49 && header[2] == 0x46 && header[3] == 0x38
            && (header[4] == 0x37 || header[4] == 0x39) && header[5] == 0x61)
        {
            return true; // GIF
        }

        if (read >= 12
            && header[0] == 0x52 && header[1] == 0x49 && header[2] == 0x46 && header[3] == 0x46
            && header[8] == 0x57 && header[9] == 0x45 && header[10] == 0x42 && header[11] == 0x50)
        {
            return true; // WebP
        }

        if (read >= 2 && header[0] == 0x42 && header[1] == 0x4D)
        {
            return true; // BMP
        }

        return false;
    }
}
