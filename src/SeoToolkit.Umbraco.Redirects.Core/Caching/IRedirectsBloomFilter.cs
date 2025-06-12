namespace SeoToolkit.Umbraco.Redirects.Core.Caching
{
    public interface IRedirectsBloomFilter
    {
        bool ShouldRebuild { get; }

        bool Contains(string url);
        void Add(string url);
        void Remove(string url);

        void Rebuild();
    }
}
