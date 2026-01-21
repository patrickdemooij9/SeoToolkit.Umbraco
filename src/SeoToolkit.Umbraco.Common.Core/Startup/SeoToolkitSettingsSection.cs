using SeoToolkit.Umbraco.Common.Core.Collections;
using System;
using System.Collections.Generic;
using System.Text;

namespace SeoToolkit.Umbraco.Common.Core.Startup
{
    public class SeoToolkitSettingsSection : ISeoTreeSection
    {
        public Guid Id => new Guid("5ed58cb7-2ec2-4c97-be5b-506d6189086f");

        public string Name => "Settings";

        public bool CanBeDomainSpecific => true;
    }
}
