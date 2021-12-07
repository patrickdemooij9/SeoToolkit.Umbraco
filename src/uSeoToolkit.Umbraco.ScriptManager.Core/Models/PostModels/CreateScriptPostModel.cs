using System.Collections.Generic;
using Umbraco.Cms.Core.Models.ContentEditing;

namespace uSeoToolkit.Umbraco.ScriptManager.Core.Models.PostModels
{
    public class CreateScriptPostModel
    {
        public string Name { get; set; }
        public string DefinitionAlias { get; set; }
        public List<DataTypeConfigurationFieldSave> Fields { get; set; }
    }
}
