using Schema.NET;

namespace SeoToolkit.Umbraco.MetaFields.Core.Common.SchemaResolvers
{
    /// <summary>
    /// A thin wrapper around a raw JSON-LD string that satisfies the <see cref="IThing"/> contract.
    /// <see cref="ToString"/> returns the raw JSON as-is so it can be placed inside a
    /// <c>&lt;script type="application/ld+json"&gt;</c> tag without further transformation.
    /// </summary>
    internal sealed class RawJsonThing : Thing
    {
        private readonly string _rawJson;

        public RawJsonThing(string rawJson)
        {
            _rawJson = rawJson ?? "{}";
        }

        public override string ToString() => _rawJson;
    }
}
