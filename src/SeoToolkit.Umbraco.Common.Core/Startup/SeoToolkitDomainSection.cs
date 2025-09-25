using SeoToolkit.Umbraco.Common.Core.Collections;
using System;

namespace SeoToolkit.Umbraco.Common.Core.Startup
{
    public class SeoToolkitDomainSection : ISeoTreeSection
    {
        public Guid Id => new("ab248b43-9757-432a-9821-22f9eeb513e7");

        public string Name => "Domains"; 
        public bool CanBeDomainSpecific => false;
    }
}
