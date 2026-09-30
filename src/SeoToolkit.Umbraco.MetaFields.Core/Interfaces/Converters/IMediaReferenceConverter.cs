using System;
using System.Collections.Generic;

namespace SeoToolkit.Umbraco.MetaFields.Core.Interfaces.Converters
{
    /// <summary>
    /// Implemented by <see cref="IEditorValueConverter"/>s whose value references media items
    /// by key. Lets Deploy emit media artifact dependencies for values that store a bare media
    /// GUID (database form) rather than an embedded <c>umb://media/...</c> UDI string.
    /// </summary>
    public interface IMediaReferenceConverter
    {
        /// <summary>
        /// Returns the keys of any media referenced by the given value. Accepts either the
        /// object form (e.g. an <c>IPublishedContent</c>) or the database form (a bare GUID),
        /// so it works whichever form the caller happens to hold.
        /// </summary>
        IEnumerable<Guid> GetReferencedMediaKeys(object value);
    }
}
