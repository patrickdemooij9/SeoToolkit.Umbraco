namespace SeoToolkit.Umbraco.Deploy.Configuration
{
    /// <summary>
    /// Bound from the "SeoToolkit:Deploy" configuration section.
    /// </summary>
    public class SeoToolkitDeploySettings
    {
        /// <summary>
        /// SeoToolkit Deploy UDI entity types (e.g. "seotoolkit-script") to exclude from
        /// deploy operations. Disabled connectors return no artifacts and skip processing.
        /// </summary>
        public string[] DisabledEntityTypes { get; set; } = [];
    }
}
