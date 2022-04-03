namespace SeoToolkit.Umbraco.Common.Core.Models
{
    /// <summary>
    /// Umbraco cache doesn't cache NULL, but in some cases we would want it to
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class CachedNullableModel<T>
    {
        public T Model { get; set; }

        public CachedNullableModel(T model)
        {
            Model = model;
        }
    }
}
