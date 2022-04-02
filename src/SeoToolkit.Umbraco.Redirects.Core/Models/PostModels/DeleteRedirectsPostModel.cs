using System;

namespace SeoToolkit.Umbraco.Redirects.Core.Models.PostModels
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
