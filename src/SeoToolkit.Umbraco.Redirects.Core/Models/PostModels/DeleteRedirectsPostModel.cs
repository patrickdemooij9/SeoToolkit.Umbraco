using System;

namespace SeoToolkit.Umbraco.Redirects.Core.Models.PostModels
{
    public class DeleteRedirectsPostModel
    {
        public Guid[] Ids { get; set; }

        public DeleteRedirectsPostModel()
        {
            Ids = [];
        }
    }
}
