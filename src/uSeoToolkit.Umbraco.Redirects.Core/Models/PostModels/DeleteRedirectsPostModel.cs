using System;

namespace uSeoToolkit.Umbraco.Redirects.Core.Models.PostModels
{
    public class DeleteRedirectsPostModel
    {
        public int[] Ids { get; set; }

        public DeleteRedirectsPostModel()
        {
            Ids = Array.Empty<int>();
        }
    }
}
