namespace SeoToolkit.Umbraco.RobotsTxt.Core.Models.Business
{
    public class RobotsTxtValidation
    {
        public int LineNumber { get; }
        public string Error { get; }

        public RobotsTxtValidation(int lineNumber, string error)
        {
            LineNumber = lineNumber;
            Error = error;
        }
    }
}
