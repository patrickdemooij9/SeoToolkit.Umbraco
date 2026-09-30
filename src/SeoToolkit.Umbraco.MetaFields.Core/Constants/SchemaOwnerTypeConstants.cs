using System;

namespace SeoToolkit.Umbraco.MetaFields.Core.Constants
{
    public static class SchemaOwnerTypeConstants
    {
        public const string Content = "content";
        public const string DocumentType = "documentType";
        public const string SchemaEntry = "schemaEntry";
        public const string Website = "website";

        /// <summary>
        /// The owner key used for website-wide (site level) schema entries. There is a single
        /// global collection, so a fixed sentinel key is used.
        /// </summary>
        public static readonly Guid WebsiteOwnerKey = Guid.Empty;
    }
}
