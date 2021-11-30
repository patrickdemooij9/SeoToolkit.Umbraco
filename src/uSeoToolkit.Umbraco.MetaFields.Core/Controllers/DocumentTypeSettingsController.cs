using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Web.BackOffice.Controllers;
using Umbraco.Cms.Web.Common.Attributes;
using uSeoToolkit.Umbraco.MetaFields.Core.Collections;
using uSeoToolkit.Umbraco.MetaFields.Core.Interfaces;
using uSeoToolkit.Umbraco.MetaFields.Core.Models.DocumentTypeSettings.Business;
using uSeoToolkit.Umbraco.MetaFields.Core.Models.DocumentTypeSettings.PostModels;
using uSeoToolkit.Umbraco.MetaFields.Core.Models.DocumentTypeSettings.ViewModels;
using uSeoToolkit.Umbraco.MetaFields.Core.Models.SeoField.ViewModels;
using uSeoToolkit.Umbraco.MetaFields.Core.Services.DocumentTypeSettings;

namespace uSeoToolkit.Umbraco.MetaFields.Core.Controllers
{
    [PluginController("uSeoToolkit")]
    public class DocumentTypeSettingsController : UmbracoAuthorizedApiController
    {
        private readonly IDocumentTypeSettingsService _documentTypeSettingsService;
        private readonly SeoFieldCollection _seoFieldCollection;
        private readonly IUmbracoMapper _umbracoMapper;

        public DocumentTypeSettingsController(IDocumentTypeSettingsService documentTypeSettingsService, SeoFieldCollection seoFieldCollection, IUmbracoMapper umbracoMapper)
        {
            _documentTypeSettingsService = documentTypeSettingsService;
            _seoFieldCollection = seoFieldCollection;
            _umbracoMapper = umbracoMapper;
        }

        [HttpGet]
        public IActionResult Get(int nodeId)
        {
            var model = _documentTypeSettingsService.Get(nodeId);
            var content = model != null ? 
                new DocumentTypeSettingsContentViewModel(model, _seoFieldCollection.GetAll().Select(it => new SeoFieldViewModel(it, model.Get(it.Alias))).ToArray()) :
                new DocumentTypeSettingsContentViewModel(_seoFieldCollection.GetAll().Select(it => new SeoFieldViewModel(it)).ToArray());
            return new JsonResult(new DocumentTypeSettingsViewModel
            {
                ContentModel = content
            });
        }

        [HttpPost]
        public IActionResult Save(DocumentTypeSettingsPostViewModel postModel)
        {
            _documentTypeSettingsService.Set(_umbracoMapper.Map<DocumentTypeSettingsPostViewModel, DocumentTypeSettingsDto>(postModel));
            return Ok();
        }
    }
}
