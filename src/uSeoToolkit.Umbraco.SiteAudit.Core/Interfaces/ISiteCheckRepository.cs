namespace uSeoToolkit.Umbraco.SiteAudit.Core.Interfaces
{
    public interface ISiteCheckRepository
    {
        int Get(string alias);
        int RegisterCheck(ISiteCheck check);
    }
}
