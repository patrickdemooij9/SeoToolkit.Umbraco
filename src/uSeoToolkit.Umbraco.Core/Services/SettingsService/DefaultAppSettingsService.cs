using System;

namespace uSeoToolkit.Umbraco.Core.Services.SettingsService
{
    public abstract class DefaultAppSettingsService<T> : ISettingsService<T> where T : class, new()
    {
        public abstract T GetSettings();
    }
}
