#nullable enable
using System.Text;

namespace SeoToolkit.Umbraco.SiteAudit.Core.Services
{
    /// <summary>
    /// Writes csv rows.
    /// <para>
    /// Everything an audit exports - urls, page titles, evidence - comes from a crawled site, so
    /// it is untrusted as far as the export is concerned. That makes the formula guard below the
    /// point of this type rather than an afterthought.
    /// </para>
    /// </summary>
    public sealed class CsvWriter
    {
        private readonly StringBuilder _builder;

        public CsvWriter(int capacity = 4096)
        {
            _builder = new StringBuilder(capacity);
        }

        public CsvWriter Row(params string?[] values)
        {
            for (var i = 0; i < values.Length; i++)
            {
                if (i > 0) _builder.Append(',');
                _builder.Append(Escape(values[i]));
            }

            _builder.Append("\r\n");
            return this;
        }

        public override string ToString() => _builder.ToString();

        public byte[] ToBytes() => Encoding.UTF8.GetBytes(_builder.ToString());

        /// <summary>
        /// Quotes a value, and neutralises anything a spreadsheet would treat as a formula.
        /// A cell starting with =, +, - or @ is executed on open by Excel and others, so a
        /// crawled page title could otherwise become code running on someone's machine.
        /// </summary>
        public static string Escape(string? value)
        {
            if (string.IsNullOrEmpty(value)) return "\"\"";

            var safe = value![0] is '=' or '+' or '-' or '@' or '\t' or '\r'
                ? "'" + value
                : value;

            return "\"" + safe.Replace("\"", "\"\"") + "\"";
        }
    }
}
