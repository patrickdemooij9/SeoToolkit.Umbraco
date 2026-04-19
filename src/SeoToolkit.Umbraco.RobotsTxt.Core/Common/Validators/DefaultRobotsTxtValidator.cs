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
            var warningShownForCurrentUserAgent = false;

            for (var i = 0; i < lines.Length; i++)
            {
                var line = lines[i].Trim().ToUpperInvariant();
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                var lineParts = line.Split(':', 2);
                var directive = lineParts[0].Trim();
                var value = lineParts.Length > 1 ? lineParts[1].Trim() : string.Empty;

                if (directive == "USER-AGENT")
                {
                    hasWildcardUserAgent = value == "*";
                    warningShownForCurrentUserAgent = false;
                }

                if (hasWildcardUserAgent && !warningShownForCurrentUserAgent && directive == "DISALLOW" && value == "/")
                {
                    yield return new RobotsTxtValidation(i + 1, DisallowAllWarning);
                    warningShownForCurrentUserAgent = true;
                }

                var valid = ValidLineStarts.Any(validLineStart => line.StartsWith(validLineStart));
                if (!valid)
                    yield return new RobotsTxtValidation(i + 1, "This line includes syntax that isn't supported by robots.txt");
            }
        }
    }
}
