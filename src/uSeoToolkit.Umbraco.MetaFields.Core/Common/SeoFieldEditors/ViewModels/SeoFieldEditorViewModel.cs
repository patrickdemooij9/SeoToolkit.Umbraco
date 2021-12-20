using System.Collections.Generic;
using Newtonsoft.Json;
using uSeoToolkit.Umbraco.MetaFields.Core.Interfaces.SeoField;

namespace uSeoToolkit.Umbraco.MetaFields.Core.Models.SeoFieldEditors.ViewModels
{
    public class SeoFieldEditorViewModel
    {
        [JsonProperty("view")]
        public string View { get; set; }
        
        [JsonProperty("config")]
        public Dictionary<string, object> Config { get; set; }

        public SeoFieldEditorViewModel(ISeoFieldEditor editor)
        {
            View = editor.View;
            Config = editor.Config;
        }
    }
}
