using System;

namespace SeoToolkit.Umbraco.ScriptManager.Core.Models.PostModels
{
    public class DeleteScriptPostModel
    {
        public Guid[] Ids { get; set; }

        public DeleteScriptPostModel()
        {
            Ids = Array.Empty<Guid>();
        }
    }
}
