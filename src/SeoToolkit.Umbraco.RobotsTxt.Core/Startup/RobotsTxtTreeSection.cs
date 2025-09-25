using SeoToolkit.Umbraco.Common.Core.Collections;
using System;

namespace SeoToolkit.Umbraco.RobotsTxt.Core.Startup
{
    public class RobotsTxtTreeSection : ISeoTreeSection
    {
        public static readonly Guid RobotsTxtSectionGuid = new("20A2086E-7D72-44BA-B97B-5836CAF6E28E");

        public Guid Id => RobotsTxtSectionGuid;

        public string Name => "Robots.txt";

        public bool CanBeDomainSpecific => true;
    }
}
