using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace SeoToolkit.Umbraco.Common.Core.Helpers
{
    public static class AssemblyVersionHelper
    {
        public static string GetInformationalVersion(Assembly assembly)
        {
            var informationalVersion = assembly
                .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?
                .InformationalVersion;

            if (!string.IsNullOrWhiteSpace(informationalVersion))
            {
                var plusIndex = informationalVersion.IndexOf('+');
                return plusIndex >= 0
                    ? informationalVersion[..plusIndex]
                    : informationalVersion;
            }

            var assemblyVersion = assembly.GetName().Version?.ToString();
            return string.IsNullOrWhiteSpace(assemblyVersion) ? "0.0.0" : assemblyVersion;
        }
    }
}
