using System;

namespace uSeoToolkit.Umbraco.ScriptManager.Core.Models.PostModels
{
    public class DeleteScriptPostModel
    {
        public int[] Ids { get; set; }

        public DeleteScriptPostModel()
        {
            Ids = Array.Empty<int>();
        }
    }
}
