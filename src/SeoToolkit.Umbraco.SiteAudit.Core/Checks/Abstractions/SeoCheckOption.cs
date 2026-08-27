#nullable enable
using System;

namespace SeoToolkit.Umbraco.SiteAudit.Core.Checks.Abstractions
{
    public enum SeoCheckOptionType
    {
        Integer = 0,
        Decimal = 1,
        Boolean = 2,
        Text = 3,
        TextList = 4
    }

    /// <summary>
    /// A knob a check exposes, declared rather than read ad hoc from configuration.
    /// <para>
    /// Declaring options is what lets the backoffice render a settings form for any check -
    /// including ones from add-on packages - without either side knowing about the other.
    /// It also means thresholds stop being compile-time constants, which is the main reason
    /// the old checks could not be tuned per site.
    /// </para>
    /// </summary>
    public sealed class SeoCheckOption
    {
        public required string Key { get; init; }
        public required string Name { get; init; }
        public required SeoCheckOptionType Type { get; init; }
        public required object DefaultValue { get; init; }

        public string? Description { get; init; }

        /// <summary>Lower bound for numeric options, used for validation and by the settings UI.</summary>
        public double? Minimum { get; init; }

        /// <summary>Upper bound for numeric options.</summary>
        public double? Maximum { get; init; }

        public static SeoCheckOption Int(string key, string name, int defaultValue, int? min = null, int? max = null, string? description = null)
            => new SeoCheckOption
            {
                Key = key,
                Name = name,
                Type = SeoCheckOptionType.Integer,
                DefaultValue = defaultValue,
                Minimum = min,
                Maximum = max,
                Description = description
            };

        public static SeoCheckOption Bool(string key, string name, bool defaultValue, string? description = null)
            => new SeoCheckOption
            {
                Key = key,
                Name = name,
                Type = SeoCheckOptionType.Boolean,
                DefaultValue = defaultValue,
                Description = description
            };

        public static SeoCheckOption Text(string key, string name, string defaultValue, string? description = null)
            => new SeoCheckOption
            {
                Key = key,
                Name = name,
                Type = SeoCheckOptionType.Text,
                DefaultValue = defaultValue,
                Description = description
            };

        public static SeoCheckOption Decimal(string key, string name, double defaultValue, double? min = null, double? max = null, string? description = null)
            => new SeoCheckOption
            {
                Key = key,
                Name = name,
                Type = SeoCheckOptionType.Decimal,
                DefaultValue = defaultValue,
                Minimum = min,
                Maximum = max,
                Description = description
            };

        public static SeoCheckOption TextList(string key, string name, string[] defaultValue, string? description = null)
            => new SeoCheckOption
            {
                Key = key,
                Name = name,
                Type = SeoCheckOptionType.TextList,
                DefaultValue = defaultValue ?? Array.Empty<string>(),
                Description = description
            };
    }
}
