#nullable enable
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace SeoToolkit.Umbraco.SiteAudit.Core.Checks.Abstractions
{
    /// <summary>
    /// A check's options, already resolved for the current run.
    /// <para>
    /// Resolution happens once per check per run rather than per page: with tens of checks
    /// across thousands of resources, re-reading configuration on every call would be the
    /// single hottest thing in the pipeline for no reason.
    /// </para>
    /// </summary>
    public sealed class SeoCheckOptions
    {
        public static readonly SeoCheckOptions Empty = new SeoCheckOptions(new Dictionary<string, object>(0));

        private readonly IReadOnlyDictionary<string, object> _values;

        public SeoCheckOptions(IReadOnlyDictionary<string, object> values)
        {
            _values = values ?? throw new ArgumentNullException(nameof(values));
        }

        public int Get(string key, int fallback)
            => TryGet(key, out var value) && TryToInt(value, out var result) ? result : fallback;

        public double Get(string key, double fallback)
            => TryGet(key, out var value) && TryToDouble(value, out var result) ? result : fallback;

        public bool Get(string key, bool fallback)
            => TryGet(key, out var value) && TryToBool(value, out var result) ? result : fallback;

        public string Get(string key, string fallback)
            => TryGet(key, out var value) && value is not null ? Convert.ToString(value, CultureInfo.InvariantCulture) ?? fallback : fallback;

        public IReadOnlyList<string> Get(string key, IReadOnlyList<string> fallback)
        {
            if (!TryGet(key, out var value)) return fallback;
            return value switch
            {
                string[] array => array,
                IReadOnlyList<string> list => list,
                IEnumerable<string> enumerable => enumerable.ToArray(),
                string single => single.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries),
                _ => fallback
            };
        }

        private bool TryGet(string key, out object value) => _values.TryGetValue(key, out value!);

        private static bool TryToInt(object value, out int result)
        {
            switch (value)
            {
                case int i: result = i; return true;
                case long l: result = (int)l; return true;
                case double d: result = (int)d; return true;
                case string s when int.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed):
                    result = parsed; return true;
                default: result = 0; return false;
            }
        }

        private static bool TryToDouble(object value, out double result)
        {
            switch (value)
            {
                case double d: result = d; return true;
                case int i: result = i; return true;
                case long l: result = l; return true;
                case string s when double.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out var parsed):
                    result = parsed; return true;
                default: result = 0; return false;
            }
        }

        private static bool TryToBool(object value, out bool result)
        {
            switch (value)
            {
                case bool b: result = b; return true;
                case string s when bool.TryParse(s, out var parsed): result = parsed; return true;
                default: result = false; return false;
            }
        }
    }
}
