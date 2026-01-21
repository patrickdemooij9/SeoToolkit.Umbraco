namespace SeoToolkit.Umbraco.Common.Core.Interfaces
{
    public interface ISeoKeyValueSetting
    {
        public string Key { get; }
        public string Title { get; }
        public string Description { get; }
        public string PropertyAlias { get; }
    }
}
