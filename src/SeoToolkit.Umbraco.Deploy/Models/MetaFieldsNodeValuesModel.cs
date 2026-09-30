namespace SeoToolkit.Umbraco.Deploy.Models
{
    /// <param name="Values">Culture (empty string = invariant) → field alias → raw value.</param>
    public record MetaFieldsNodeValuesModel(Guid NodeKey, string NodeName, Dictionary<string, Dictionary<string, object>> Values);
}
