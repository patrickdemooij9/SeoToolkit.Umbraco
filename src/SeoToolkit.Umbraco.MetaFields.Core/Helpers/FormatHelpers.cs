namespace SeoToolkit.Umbraco.MetaFields.Core.Helpers
{
    internal static class FormatHelpers
    {
        internal static string Format(string format, string value)
        {
            return format.Replace("%Value%", value);
        }
    }
}
