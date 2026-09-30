namespace SeoToolkit.Umbraco.Deploy.Models
{
    /// <param name="ArtifactGuid">RootKeyValuesGuid for the no-domain set, else the domain collection id.</param>
    public record KeyValuesModel(Guid ArtifactGuid, Guid? DomainCollectionId, Dictionary<string, string> Values, string Name);
}
