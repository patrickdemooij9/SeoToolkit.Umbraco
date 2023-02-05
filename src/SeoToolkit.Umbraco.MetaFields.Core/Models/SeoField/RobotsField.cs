using Microsoft.AspNetCore.Html;
using SeoToolkit.Umbraco.MetaFields.Core.Common.Converters.EditorConverters;
using SeoToolkit.Umbraco.MetaFields.Core.Common.SeoFieldEditEditors;
using SeoToolkit.Umbraco.MetaFields.Core.Common.SeoFieldEditors;
using SeoToolkit.Umbraco.MetaFields.Core.Constants;
using SeoToolkit.Umbraco.MetaFields.Core.Interfaces.SeoField;
using SeoToolkit.Umbraco.MetaFields.Core.Models.Converters;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Cms.Core.Composing;

namespace SeoToolkit.Umbraco.MetaFields.Core.Models.SeoField
{
    [Weight(600)]
    public class RobotsField : SeoField<string[]>
    {
        public override string Title => "Robots";
        public override string Alias => SeoFieldAliasConstants.Robots;
        public override string Description => "Robot tags for your page";

        public override string GroupAlias => SeoFieldGroupConstants.Others;

        public override ISeoFieldEditor Editor { get; }
        public override ISeoFieldEditEditor EditEditor { get; }

        public RobotsField()
        {
            var items = new CheckboxItem[]
            {
                new CheckboxItem("No index", "noindex"),
                new CheckboxItem("No follow", "nofollow"),
                new CheckboxItem("No archive", "noarchive"),
                new CheckboxItem("No search box", "nositelinkssearchbox"),
                new CheckboxItem("No snippet", "nosnippet"),
                new CheckboxItem("Index if embedded", "indexifembedded"),
                new CheckboxItem("No translate", "notranslate"),
                new CheckboxItem("No image index", "noimageindex")
            };

            Editor = new CheckboxListFieldPropertyEditor(items);

            EditEditor = new SeoCheckboxlistEditEditor(items);
        }

        protected override HtmlString Render(string[] value)
        {
            if (value is null || value.Length == 0) return HtmlString.Empty;
            if (value.Length == 1 && value[0].Equals("none")) return HtmlString.Empty;

            return new HtmlString($"<meta name=\"robots\" content=\"{string.Join(',', value)}\">");
        }
    }
}
