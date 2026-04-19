using System;
using System.Collections.Generic;
using System.Linq;
using SeoToolkit.Umbraco.RobotsTxt.Core.Interfaces;
using SeoToolkit.Umbraco.RobotsTxt.Core.Models.Business;

namespace SeoToolkit.Umbraco.RobotsTxt.Core.Common.Validators
{
    public class DefaultRobotsTxtValidator : IRobotsTxtValidator
    {
        private const string DisallowAllWarning = "You are currently blocking all bots which will impact your SEO. Make sure to check if this is correct.";

        private readonly string[] ValidLineStarts =
        {
            "#", "USER-AGENT", "DISALLOW", "ALLOW", "SITEMAP", "CRAWL-DELAY", "HOST", "CLEAN-PARAM", "REQUEST-RATE"
        };

        public IEnumerable<RobotsTxtValidation> Validate(string content)
        {
            var lines = content.Split(Environment.NewLine.ToCharArray());
            var hasWildcardUserAgent = false;

            for (var i = 0; i < lines.Length; i++)
            {
                var line = lines[i].Trim().ToUpperInvariant();
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                if (line.StartsWith("USER-AGENT"))
                {
                    hasWildcardUserAgent = line == "USER-AGENT:*" || line == "USER-AGENT: *";
                }

                if (hasWildcardUserAgent && (line == "DISALLOW:/" || line == "DISALLOW: /"))
                {
                    yield return new RobotsTxtValidation(i + 1, DisallowAllWarning);
                }

                var valid = ValidLineStarts.Any(validLineStart => line.StartsWith(validLineStart));
                if (!valid)
                    yield return new RobotsTxtValidation(i + 1, "This line includes syntax that isn't supported by robots.txt");
            }
        }
    }
}
