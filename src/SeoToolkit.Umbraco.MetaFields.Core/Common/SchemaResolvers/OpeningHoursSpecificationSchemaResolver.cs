using Schema.NET;
using SeoToolkit.Umbraco.MetaFields.Core.Common.Converters.EditorConverters;
using System;
using System.Collections.Generic;
using System.Linq;
using DayOfWeek = Schema.NET.DayOfWeek;

namespace SeoToolkit.Umbraco.MetaFields.Core.Common.SchemaResolvers
{
    public class OpeningHoursSpecificationSchemaResolver : ISchemaResolver
    {
        private static readonly string[] Days = Enum.GetNames<DayOfWeek>();

        public string Name => "Opening Hours Specification";
        public string Alias => "openingHoursSpecification";

        public SchemaProperty[] Properties =>
            [
                new("dayOfWeek", "Days", "Umb.PropertyEditorUi.CheckBoxList", allowReference: false, valueConverter: new CheckboxlistConverter(), config: new Dictionary<string, object>
                {
                    ["items"] = Days
                }),
                new("opens", "Opens (e.g. 09:00)", "Umb.PropertyEditorUi.TextBox"),
                new("closes", "Closes (e.g. 17:30)", "Umb.PropertyEditorUi.TextBox")
            ];

        public IThing ToSchema(Dictionary<string, object> values)
        {
            var days = values.GetStrings("dayOfWeek")
                .Select(day => Enum.TryParse<DayOfWeek>(day, true, out var dayOfWeek) ? dayOfWeek : (DayOfWeek?)null)
                .Where(day => day != null)
                .ToArray();

            return new OpeningHoursSpecification
            {
                DayOfWeek = days,
                Opens = values.GetTime("opens"),
                Closes = values.GetTime("closes")
            };
        }
    }
}
