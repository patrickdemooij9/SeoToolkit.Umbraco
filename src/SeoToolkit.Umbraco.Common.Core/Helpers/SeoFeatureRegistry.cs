using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace SeoToolkit.Umbraco.Common.Core.Helpers
{
    /// <summary>
    /// Which optional features are present in this installation.
    /// <para>
    /// Generalises the pattern already used for AI, where an add-on package flips a flag from
    /// its composer and the rest of the toolkit adapts. Presence of the package is the signal:
    /// there is deliberately no key validation here, because whether a feature should be
    /// available is the add-on's decision to make, not this package's.
    /// </para>
    /// <para>
    /// Features are registered during composition and read afterwards, so the cost of the
    /// copy-on-write set is paid once at startup and reads stay lock free.
    /// </para>
    /// </summary>
    public static class SeoFeatureRegistry
    {
        private static ImmutableHashSet<string> _features =
            ImmutableHashSet.Create<string>(StringComparer.OrdinalIgnoreCase);

        /// <summary>Marks a feature as available. Safe to call more than once.</summary>
        public static void Enable(string feature)
        {
            if (string.IsNullOrWhiteSpace(feature))
                throw new ArgumentException("A feature name is required.", nameof(feature));

            ImmutableInterlocked.Update(ref _features, (set, item) => set.Add(item), feature);
        }

        public static bool IsEnabled(string? feature)
            => !string.IsNullOrEmpty(feature) && _features.Contains(feature!);

        /// <summary>Every enabled feature, for surfacing to the backoffice.</summary>
        public static IReadOnlyCollection<string> Enabled => _features;
    }
}
