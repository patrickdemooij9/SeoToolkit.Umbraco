using System;

namespace SeoToolkit.Umbraco.Common.Core.Collections
{
    public interface ISeoTreeSection
    {
        public Guid Id { get; }
        public string Name { get; }
        public bool CanBeDomainSpecific { get; }
    }
}
