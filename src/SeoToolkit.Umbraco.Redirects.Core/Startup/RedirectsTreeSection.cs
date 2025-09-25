using SeoToolkit.Umbraco.Common.Core.Collections;
using System;

namespace SeoToolkit.Umbraco.Redirects.Core.Startup
{
    public class RedirectsTreeSection : ISeoTreeSection
    {
        public Guid Id => new("1147F58D-D2D5-425B-AEDE-DB537BDAC9EF");
        public string Name => "Redirects";
        public bool CanBeDomainSpecific => false;
    }
}
