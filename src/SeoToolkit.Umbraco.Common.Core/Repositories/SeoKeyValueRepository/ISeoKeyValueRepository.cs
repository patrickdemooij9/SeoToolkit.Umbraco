namespace SeoToolkit.Umbraco.Common.Core.Repositories.SeoKeyValueRepository
{
    public interface ISeoKeyValueRepository
    {
        void Set(string key, string value, int? domainId);
        string? Get(string key, int? domainId);
        void Delete(string key, int? domainId);
    }
}
