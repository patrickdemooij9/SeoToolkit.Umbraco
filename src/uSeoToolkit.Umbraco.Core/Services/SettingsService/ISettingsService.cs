namespace uSeoToolkit.Umbraco.Common.Core.Services.SettingsService
{
    public interface ISettingsService<T> where T : class, new()
    {
        T GetSettings();
    }
}
