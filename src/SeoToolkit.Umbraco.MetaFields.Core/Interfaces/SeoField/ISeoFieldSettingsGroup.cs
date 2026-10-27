namespace SeoToolkit.Umbraco.MetaFields.Core.Interfaces.SeoField;

/// <summary>
/// A group used to structure the SEO fields on the document type settings screen.
/// Fields join a group through <see cref="ISeoField.SettingsGroupAlias"/>.
/// </summary>
public interface ISeoFieldSettingsGroup
{
    string Alias { get; }
    string Name { get; }
    string Description { get; }
    string Icon { get; }
}
