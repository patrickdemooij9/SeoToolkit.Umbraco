using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SeoToolkit.Umbraco.ScriptManager.Core.Models.PostModels
{
    public class CreateScriptPostModel
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public string DefinitionAlias { get; set; }

        public Dictionary<string, string> Fields { get; set; }
    }
}
