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
        /// Scripts (e.g. analytics or tag manager ids) usually differ per environment, so the
        /// script connector is opt-in. Set to true to include "seotoolkit-script" in deploys.
        /// </summary>
        public bool EnableScripts { get; set; }
    }
}
