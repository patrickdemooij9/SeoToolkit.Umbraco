using SeoToolkit.Umbraco.Common.Core.Collections;
using System;

namespace SeoToolkit.Umbraco.Common.Core.Startup
{
    public class SeoToolkitInfoSection : ISeoTreeSection
    {
        public Guid Id => new("CDF429D1-2380-4AC2-AC3E-22D619EE4529");
        public string Name => "Info"; 
        public bool CanBeDomainSpecific => false;
    }
}
