namespace SeoToolkit.Umbraco.Redirects.Core.Models.Business;

public class ImportStatus
{
    public int StatusCode;
    public string Error;

    public ImportStatus(int statusCode, string error = "")
    {
        StatusCode = statusCode;
        Error = error;
    }
}