using SeoToolkit.Umbraco.Common.Core.Collections;
using System;

namespace SeoToolkit.Umbraco.RobotsTxt.Core.Startup
{
    public class RobotsTxtTreeSection : ISeoTreeSection
    {
        public Guid Id => new("20A2086E-7D72-44BA-B97B-5836CAF6E28E");

        public string Name => "Robots.txt";

        public bool CanBeDomainSpecific => true;
    }
}
