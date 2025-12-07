using SeoToolkit.Umbraco.MetaFields.Core.Models.DocumentTypeSettings.Business;
using Umbraco.Cms.Core.Notifications;

namespace SeoToolkit.Umbraco.MetaFields.Core.Notifications
{
    public class MetaFieldSettingsSavedNotification : INotification
    {
        public DocumentTypeSettingsDto Model { get; }

        public MetaFieldSettingsSavedNotification(DocumentTypeSettingsDto model)
        {
            Model = model;
        }
    }
}
