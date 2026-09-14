using Schema.NET;
using SeoToolkit.Umbraco.MetaFields.Core.Common.Converters.EditorConverters;
using SeoToolkit.Umbraco.MetaFields.Core.Constants;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace SeoToolkit.Umbraco.MetaFields.Core.Common.SchemaResolvers
{
    /// <summary>
    /// Helpers to read the resolved property values that are passed to <see cref="ISchemaResolver.ToSchema"/>.
    /// </summary>
    public static class SchemaValueHelper
    {
        private static readonly char[] LineSeparators = ['\r', '\n'];

        public static string GetString(this Dictionary<string, object> values, string alias)
        {
            var value = values.GetValueOrDefault(alias)?.ToString();
            return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }

        public static Uri GetUri(this Dictionary<string, object> values, string alias)
            => Uri.TryCreate(values.GetString(alias), UriKind.Absolute, out var uri) ? uri : null;

        /// <summary>
        /// Reads a value where each line is a separate absolute URL, such as a list of sameAs links.
        /// </summary>
        public static Uri[] GetUris(this Dictionary<string, object> values, string alias)
            => (values.GetString(alias) ?? string.Empty)
                .Split(LineSeparators, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(line => Uri.TryCreate(line, UriKind.Absolute, out var uri) ? uri : null)
                .Where(uri => uri != null)
                .ToArray();

        public static DateTimeOffset? GetDate(this Dictionary<string, object> values, string alias)
        {
            var value = values.GetValueOrDefault(alias);
            switch (value)
            {
                case DateTimeOffset dateTimeOffset:
                    return dateTimeOffset;
                case DateTime dateTime:
                    return dateTime == DateTime.MinValue ? null : new DateTimeOffset(dateTime);
            }

            return DateTimeOffset.TryParse(values.GetString(alias), CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out var result)
                ? result
                : null;
        }

        public static TimeSpan? GetTime(this Dictionary<string, object> values, string alias)
            => TimeSpan.TryParse(values.GetString(alias), CultureInfo.InvariantCulture, out var result) ? result : null;

        public static double? GetDouble(this Dictionary<string, object> values, string alias)
            => double.TryParse(values.GetString(alias), NumberStyles.Float, CultureInfo.InvariantCulture, out var result) ? result : null;

        /// <summary>
        /// Reads the values selected in a checkbox list or dropdown.
        /// </summary>
        public static string[] GetStrings(this Dictionary<string, object> values, string alias)
        {
            return values.GetValueOrDefault(alias) switch
            {
                IEnumerable<string> items => items.Where(it => !string.IsNullOrWhiteSpace(it)).Select(it => it.Trim()).ToArray(),
                string text => text.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries),
                _ => []
            };
        }

        /// <summary>
        /// Reads the schemas of a nested schema editor property that implement <typeparamref name="T"/>.
        /// </summary>
        public static T[] GetSchemas<T>(this Dictionary<string, object> values, string alias)
            => (values.GetValueOrDefault(alias) as IEnumerable<IThing>)?.OfType<T>().ToArray() ?? [];

        /// <summary>
        /// Reads a nested schema editor property that can contain both organizations and persons.
        /// </summary>
        public static Values<IOrganization, IPerson> GetOrganizationsOrPersons(this Dictionary<string, object> values, string alias)
            => new(values.GetSchemas<IThing>(alias).Where(schema => schema is IOrganization or IPerson));

        /// <summary>
        /// Creates a property that uses the nested schema editor, limited to the given schema aliases.
        /// </summary>
        public static SchemaProperty NestedSchemaProperty(string alias, string displayName, params string[] allowedSchemas)
            => new(alias, displayName, "SeoToolkit.SchemaEditor", allowReference: false, valueConverter: new SchemaEditorValueConverter(), config: new Dictionary<string, object>
            {
                ["ownerType"] = SchemaOwnerTypeConstants.SchemaEntry,
                ["allowedSchemas"] = allowedSchemas
            });
    }
}
