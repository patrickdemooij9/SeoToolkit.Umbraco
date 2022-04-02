using System;
using System.Collections.Generic;
using System.Linq;
using SeoToolkit.Umbraco.RobotsTxt.Core.Interfaces;
using SeoToolkit.Umbraco.RobotsTxt.Core.Models.Business;

namespace SeoToolkit.Umbraco.RobotsTxt.Core.Common.Validators
{
    public class DefaultRobotsTxtValidator : IRobotsTxtValidator
    {
        private readonly string[] ValidLineStarts =
        {
            "#", "USER-AGENT", "DISALLOW", "ALLOW", "SITEMAP", "CRAWL-DELAY", "HOST", "CLEAN-PARAM", "REQUEST-RATE"
        };

        public IEnumerable<RobotsTxtValidation> Validate(string content)
        {
            var lines = content.Split(Environment.NewLine.ToCharArray());
            for (var i = 0; i < lines.Length; i++)
            {
                var line = lines[i].Trim().ToUpperInvariant();
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                var valid = ValidLineStarts.Any(validLineStart => line.StartsWith(validLineStart));
                if (!valid)
                    yield return new RobotsTxtValidation(i + 1, "This line includes syntax that isn't supported by robots.txt");
            }
        }
    }
}
