namespace uSeoToolkit.Umbraco.MetaFields.Core.Common.FieldProviders
{
    public class FieldItemViewModel
    {
        public string Name { get; }
        public string Value { get; }
        public bool OnlyShowIfInherited { get; }

        public FieldItemViewModel(string name, string value, bool onlyShowIfInherited = false)
        {
            Name = name;
            Value = value;
            OnlyShowIfInherited = onlyShowIfInherited;
        }
    }
}
