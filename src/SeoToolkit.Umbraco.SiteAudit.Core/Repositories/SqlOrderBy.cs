#nullable enable
using System;

namespace SeoToolkit.Umbraco.SiteAudit.Core.Repositories
{
    /// <summary>
    /// Builds an ORDER BY clause as a plain string.
    /// <para>
    /// This exists because of a trap worth knowing about. The typed helper is
    /// <c>OrderBy&lt;TDto&gt;(x =&gt; x.Column)</c>, and it needs its type argument spelled out.
    /// Hand it an <see cref="System.Linq.Expressions.Expression"/> held in a variable instead and
    /// the call quietly binds to NPoco's <c>OrderBy(params object[])</c>, which calls
    /// <c>ToString()</c> on whatever it is given - so the lambda lands in the SQL verbatim as
    /// <c>ORDER BY it =&gt; Convert(it.Url, Object)</c>.
    /// </para>
    /// <para>
    /// It compiles, and nothing complains until the query is paged: the OFFSET/FETCH rewrite
    /// cannot find a usable ORDER BY, and the database reports "Incorrect syntax near '='" -
    /// the '=' being the arrow of the lambda - which points nowhere near the real mistake.
    /// Building the clause as a string here keeps that from being possible.
    /// </para>
    /// </summary>
    public static class SqlOrderBy
    {
        /// <summary>
        /// Quotes and joins column expressions. Each may carry a direction, e.g. "Severity DESC".
        /// </summary>
        public static string Clause(params string[] columns)
        {
            if (columns is null || columns.Length == 0)
                throw new ArgumentException("At least one column is required.", nameof(columns));

            var parts = new string[columns.Length];

            for (var i = 0; i < columns.Length; i++)
            {
                var column = columns[i]?.Trim();
                if (string.IsNullOrEmpty(column))
                    throw new ArgumentException("A column name cannot be empty.", nameof(columns));

                var space = column!.IndexOf(' ');

                parts[i] = space < 0
                    ? $"[{column}]"
                    : $"[{column[..space]}]{column[space..]}";
            }

            return string.Join(", ", parts);
        }
    }
}
