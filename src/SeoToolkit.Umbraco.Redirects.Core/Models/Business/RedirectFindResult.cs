using System;
using System.Text.RegularExpressions;
using Umbraco.Extensions;
using SeoToolkit.Umbraco.Redirects.Core.Extensions;

namespace SeoToolkit.Umbraco.Redirects.Core.Models.Business
{
    public class RedirectFindResult
    {
        /// <summary>
        /// Uri used to find the redirect
        /// </summary>
        public Uri Uri { get; }

        /// <summary>
        /// Redirect that was found
        /// </summary>
        public Redirect Redirect { get; }

        public RedirectFindResult(Uri uri, Redirect redirect)
        {
            Uri = uri;
            Redirect = redirect;
        }

        public string GetNewUrl()
        {
            if (Uri is null || !Redirect.IsRegex || string.IsNullOrWhiteSpace(Redirect.NewUrl) || !Redirect.NewUrl.Contains($"$"))
                return Redirect.NewUrl.IfNullOrWhiteSpace(Redirect.NewNode?.Url(Redirect.NewNodeCulture?.IsoCode?.ToLowerInvariant()));

            try
            {
                var regexNewUrl = Redirect.NewUrl;
                var match = Regex.Match(Uri.PathAndQuery.CleanUrl(), Redirect.OldUrl);

                for (var i = 1; i < match.Groups.Count; i++)
                {
                    regexNewUrl = regexNewUrl.Replace($"${i}", match.Groups[i].Value);
                }

                return regexNewUrl;
            }
            catch (Exception)
            {
                return Redirect.NewUrl;
            }
        }
    }
}
