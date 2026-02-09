namespace SeoToolkit.Umbraco.Common.Core.Models.ViewModels
{
    public class SeoKeyValueSettingViewModel
    {
        public required string Key { get; set; }
        public required string Title { get; set; }
        public required string Description { get; set; }
        public required string PropertyAlias { get; set; }

        public object? Value { get; set; }
        public bool IsRoot { get; set; }
    }
}
