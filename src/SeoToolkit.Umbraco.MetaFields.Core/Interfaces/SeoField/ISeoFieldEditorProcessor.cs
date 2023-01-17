namespace SeoToolkit.Umbraco.MetaFields.Core.Interfaces.SeoField
{
    public interface ISeoFieldEditorProcessor
    {
        /// <summary>
        /// Handle the value before sending it through the converter
        /// </summary>
        /// <param name="value"></param>
        object HandleValue(object value);
    }
}
