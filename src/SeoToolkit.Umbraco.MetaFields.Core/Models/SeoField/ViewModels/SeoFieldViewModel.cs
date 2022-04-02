using SeoToolkit.Umbraco.MetaFields.Core.Interfaces.SeoField;
using SeoToolkit.Umbraco.MetaFields.Core.Models.DocumentTypeSettings.Business;
using SeoToolkit.Umbraco.MetaFields.Core.Models.SeoFieldEditors.ViewModels;

namespace SeoToolkit.Umbraco.MetaFields.Core.Models.SeoField.ViewModels
{
    public class SeoFieldViewModel
    {
        public string Alias { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public bool UseInheritedValue { get; set; }
        public object Value { get; set; }
        public SeoFieldEditorViewModel Editor { get; set; }

        public SeoFieldViewModel(ISeoField field)
        {
            Alias = field.Alias;
            Title = field.Title;
            Description = field.Description;
            Editor = new SeoFieldEditorViewModel(field.Editor);
        }

        public SeoFieldViewModel(ISeoField field, object value) : this(field)
        {
            Value = value;
        }

        public SeoFieldViewModel(ISeoField field, DocumentTypeValueDto value) : this(field)
        {
            Value = field.Editor.ValueConverter.ConvertObjectToEditorValue(value.Value);
            UseInheritedValue = value.UseInheritedValue;
        }
    }
}
