#nullable enable
using System;
using System.Collections.Generic;

namespace SeoToolkit.Umbraco.SiteAudit.Core.Checks.Abstractions
{
    /// <summary>
    /// Builds a check's effective options by layering overrides on top of the values it declared.
    /// Later sources win, so callers pass them from least to most specific:
    /// appsettings, then stored per-site settings, then anything set on the run itself.
    /// </summary>
    public static class SeoCheckOptionsResolver
    {
        public static SeoCheckOptions Resolve(SeoCheckDescriptor descriptor,
            params IReadOnlyDictionary<string, object>?[] overridesInPriorityOrder)
        {
            if (descriptor is null) throw new ArgumentNullException(nameof(descriptor));

            if (descriptor.Options.Count == 0)
                return SeoCheckOptions.Empty;

            var values = new Dictionary<string, object>(descriptor.Options.Count, StringComparer.OrdinalIgnoreCase);

            //Start from what the check says it wants, so an unknown or missing override
            //can never leave an option unset.
            foreach (var option in descriptor.Options.Values)
                values[option.Key] = option.DefaultValue;

            if (overridesInPriorityOrder is null)
                return new SeoCheckOptions(values);

            foreach (var layer in overridesInPriorityOrder)
            {
                if (layer is null) continue;
                foreach (var pair in layer)
                {
                    //Ignore anything the check did not declare: an option it does not read is
                    //almost always a typo, and silently accepting it hides the mistake.
                    if (!descriptor.Options.ContainsKey(pair.Key)) continue;
                    if (pair.Value is null) continue;
                    values[pair.Key] = pair.Value;
                }
            }

            return new SeoCheckOptions(values);
        }
    }
}
