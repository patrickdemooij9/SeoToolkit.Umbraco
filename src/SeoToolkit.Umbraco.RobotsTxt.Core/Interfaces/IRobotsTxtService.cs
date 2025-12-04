using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using SeoToolkit.Umbraco.RobotsTxt.Core.Models.Business;

namespace SeoToolkit.Umbraco.RobotsTxt.Core.Interfaces
{
    public interface IRobotsTxtService
    {
        RobotsTxtModel? Get(Guid id);
        RobotsTxtModel[] GetAll();
        void Save(RobotsTxtModel model);

        string GetContent(Guid? domainId = null);
        string GetContentWithSitemaps(HttpRequest request);
        void SetContent(string content, Guid? domainId = null);

        IEnumerable<RobotsTxtValidation> Validate(string content);
    }
}
