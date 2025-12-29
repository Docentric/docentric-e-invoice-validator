namespace Docentric.EInvoice.Validator.RestServer.IO;

/// <summary>
/// Provides methods for validating file types based on their magic bytes (file signatures).
/// </summary>
public static class FileTypeValidator
{
    // PDF files start with "%PDF-" (hex: 25 50 44 46 2D)
    private static readonly byte[] _pdfSignature = [0x25, 0x50, 0x44, 0x46, 0x2D];

    // XML files can start with:
    // - UTF-8 BOM + "<?xml" (hex: EF BB BF 3C 3F 78 6D 6C)
    // - "<?xml" without BOM (hex: 3C 3F 78 6D 6C)
    // - "<" for XML without declaration (hex: 3C)
    private static readonly byte[] _xmlSignatureWithBom = [0xEF, 0xBB, 0xBF, 0x3C, 0x3F, 0x78, 0x6D, 0x6C];
    private static readonly byte[] _xmlSignature = [0x3C, 0x3F, 0x78, 0x6D, 0x6C]; // "<?xml"
    private static readonly byte[] _xmlRootElementSignature = [0x3C]; // "<"

    /// <summary>
    /// Validates that the uploaded file is a PDF by checking its magic bytes.
    /// </summary>
    /// <param name="file">The uploaded file to validate.</param>
    /// <returns>True if the file is a valid PDF, false otherwise.</returns>
    public static async Task<bool> IsPdfAsync(IFormFile file)
    {
        if (file.Length < _pdfSignature.Length)
            return false;

        await using Stream stream = file.OpenReadStream();
        byte[] buffer = new byte[_pdfSignature.Length];
        int bytesRead = await stream.ReadAsync(buffer);

        if (bytesRead < _pdfSignature.Length)
            return false;

        return buffer.AsSpan().SequenceEqual(_pdfSignature);
    }

    /// <summary>
    /// Validates that the uploaded file is an XML file by checking its magic bytes.
    /// </summary>
    /// <param name="file">The uploaded file to validate.</param>
    /// <returns>True if the file is a valid XML file, false otherwise.</returns>
    /// <remarks>
    /// This method checks for XML files with or without UTF-8 BOM, with or without XML declaration.
    /// It supports files starting with "&lt;?xml" or directly with a root element "&lt;".
    /// </remarks>
    public static async Task<bool> IsXmlAsync(IFormFile file)
    {
        if (file.Length == 0)
            return false;

        await using Stream stream = file.OpenReadStream();
        
        // Read enough bytes to check for XML signatures
        int maxSignatureLength = Math.Max(_xmlSignatureWithBom.Length, _xmlSignature.Length);
        byte[] buffer = new byte[maxSignatureLength];
        int bytesRead = await stream.ReadAsync(buffer);

        if (bytesRead == 0)
            return false;

        // Check for UTF-8 BOM + "<?xml"
        if (bytesRead >= _xmlSignatureWithBom.Length &&
            buffer.AsSpan(0, _xmlSignatureWithBom.Length).SequenceEqual(_xmlSignatureWithBom))
        {
            return true;
        }

        // Check for "<?xml" without BOM
        if (bytesRead >= _xmlSignature.Length &&
            buffer.AsSpan(0, _xmlSignature.Length).SequenceEqual(_xmlSignature))
        {
            return true;
        }

        // Check for XML without declaration (starts with "<")
        if (bytesRead >= 1 && buffer[0] == _xmlRootElementSignature[0])
        {
            // Additional validation: check if it looks like an XML tag
            // Skip whitespace after "<"
            int i = 1;
            while (i < bytesRead && (buffer[i] == 0x20 || buffer[i] == 0x09 || buffer[i] == 0x0A || buffer[i] == 0x0D))
                i++;

            // Check if followed by a valid XML name character (letter, underscore, or colon)
            if (i < bytesRead)
            {
                byte nextChar = buffer[i];
                return (nextChar >= 0x41 && nextChar <= 0x5A) || // A-Z
                       (nextChar >= 0x61 && nextChar <= 0x7A) || // a-z
                       nextChar == 0x5F ||                        // underscore
                       nextChar == 0x3A;                          // colon
            }
        }

        return false;
    }
}
