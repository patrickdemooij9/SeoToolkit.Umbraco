using System;
using System.Collections.Generic;
using System.Linq;
using SeoToolkit.Umbraco.RobotsTxt.Core.Interfaces;
using SeoToolkit.Umbraco.RobotsTxt.Core.Models.Business;

namespace SeoToolkit.Umbraco.RobotsTxt.Core.Common.Validators
{
    public class DefaultRobotsTxtValidator : IRobotsTxtValidator
    {
        public const string DisallowAllWarning = "You are currently blocking all bots which will impact your SEO. Make sure to check if this is correct.";

        private readonly string[] ValidLineStarts =
        {
            "#", "USER-AGENT", "DISALLOW", "ALLOW", "SITEMAP", "CRAWL-DELAY", "HOST", "CLEAN-PARAM", "REQUEST-RATE"
        };

        public IEnumerable<RobotsTxtValidation> Validate(string content)
        {
            var lines = content.Split(Environment.NewLine.ToCharArray());
            var currentGroupHasWildcardUserAgent = false;
            var warningShownForCurrentGroup = false;
            var hasRulesForCurrentGroup = false;

            for (var i = 0; i < lines.Length; i++)
            {
                var line = lines[i].Trim().ToUpperInvariant();
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                var colonIndex = line.IndexOf(':');
                if (colonIndex >= 0)
                {
                    var directive = line[..colonIndex].Trim();
                    var value = line[(colonIndex + 1)..].Trim();

                    if (directive == "USER-AGENT")
                    {
                        if (hasRulesForCurrentGroup)
                        {
                            currentGroupHasWildcardUserAgent = false;
                            warningShownForCurrentGroup = false;
                            hasRulesForCurrentGroup = false;
                        }

                        currentGroupHasWildcardUserAgent = currentGroupHasWildcardUserAgent || value == "*";
                    }
                    else
                    {
                        hasRulesForCurrentGroup = true;
                    }

                    if (currentGroupHasWildcardUserAgent && !warningShownForCurrentGroup && directive == "DISALLOW" && value == "/")
                    {
                        yield return new RobotsTxtValidation(i + 1, DisallowAllWarning);
                        warningShownForCurrentGroup = true;
                    }
                }

                var valid = ValidLineStarts.Any(validLineStart => line.StartsWith(validLineStart));
                if (!valid)
                    yield return new RobotsTxtValidation(i + 1, "This line includes syntax that isn't supported by robots.txt");
            }
        }
    }
}
