﻿using SeoToolkit.Umbraco.MetaFields.Core.Enums;

namespace SeoToolkit.Umbraco.MetaFields.Core.Models.Converters
{
    public class FieldsItem
    {
        public string Name { get; set; }
        public string Value { get; set; }
        public FieldSourceType Source { get; set; }
    }
}
