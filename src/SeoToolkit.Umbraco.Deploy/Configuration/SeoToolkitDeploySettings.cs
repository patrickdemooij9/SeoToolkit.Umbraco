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

        /// <summary>
        /// When <c>true</c>, restores are convergent: target data that is absent from the
        /// incoming artifact (removed key/values, cleared meta field values, dropped settings)
        /// is deleted so the target mirrors the source. When <c>false</c> (the default),
        /// deploys are overwrite-only and never delete target-only data.
        /// </summary>
        public bool PruneMissing { get; set; }
    }
}
