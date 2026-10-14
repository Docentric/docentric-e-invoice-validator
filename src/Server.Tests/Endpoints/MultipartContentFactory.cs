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
}
