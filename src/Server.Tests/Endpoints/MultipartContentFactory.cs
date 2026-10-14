using System.Text;

namespace Docentric.EInvoice.Validator.RestServer.Tests.Endpoints;

internal static class MultipartContentFactory
{
    /// <summary>
    /// Builds a multipart/form-data body with a single "file" part.
    /// </summary>
    public static MultipartFormDataContent CreateFile(string content, string fileName)
    {
        MultipartFormDataContent form = new();
        form.Add(new ByteArrayContent(Encoding.UTF8.GetBytes(content)), "file", fileName);
        return form;
    }

    /// <summary>
    /// Builds a multipart/form-data body with a single "file" part containing the bytes of an existing file.
    /// </summary>
    public static MultipartFormDataContent CreateFromPath(string path)
    {
        MultipartFormDataContent form = new();
        form.Add(new ByteArrayContent(File.ReadAllBytes(path)), "file", Path.GetFileName(path));
        return form;
    }
}
