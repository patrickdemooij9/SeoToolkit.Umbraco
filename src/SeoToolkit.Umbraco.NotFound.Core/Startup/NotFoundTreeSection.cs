using SeoToolkit.Umbraco.Common.Core.Collections;
using System;

namespace SeoToolkit.Umbraco.NotFound.Core.Startup
{
    public class NotFoundTreeSection : ISeoTreeSection
    {
        public Guid Id => new("a9b6dec6-e045-476a-ba3f-742355e18e33");
        public string Name => "Not Found";
        public bool CanBeDomainSpecific => true;
    }
}
