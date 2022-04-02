using System;
using SeoToolkit.Umbraco.MetaFields.Core.Models.DocumentTypeSettings.Business;
using SeoToolkit.Umbraco.MetaFields.Core.Models.SeoField.ViewModels;

namespace SeoToolkit.Umbraco.MetaFields.Core.Models.DocumentTypeSettings.ViewModels
{
    public class DocumentTypeSettingsContentViewModel
    {
        public SeoFieldViewModel[] Fields { get; set; }
        public DocumentTypeSettingsInheritanceViewModel Inheritance { get; set; }

        public DocumentTypeSettingsContentViewModel()
        {
            Fields = Array.Empty<SeoFieldViewModel>();
        }

        public DocumentTypeSettingsContentViewModel(SeoFieldViewModel[] fields)
        {
            Fields = fields;
        }

        public DocumentTypeSettingsContentViewModel(DocumentTypeSettingsDto model, SeoFieldViewModel[] fields) : this(fields)
        {
            Inheritance = model.Inheritance is null ? null : new DocumentTypeSettingsInheritanceViewModel
            {
                Id = model.Inheritance.Id,
                Name = model.Inheritance.Name
            };
        }
    }
}
