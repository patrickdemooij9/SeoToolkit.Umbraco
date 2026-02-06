using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using ExcelDataReader;
using Microsoft.VisualBasic.FileIO;
using SeoToolkit.Umbraco.Redirects.Core.Enumerators;
using SeoToolkit.Umbraco.Redirects.Core.Interfaces;
using SeoToolkit.Umbraco.Redirects.Core.Models.Business;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Web;
using Umbraco.Extensions;

namespace SeoToolkit.Umbraco.Redirects.Core.Helpers;

public class RedirectsImportHelper
{
    private Domain _selectedDomain;
    private readonly IRedirectsService _redirectsService;
    private readonly IUmbracoContextFactory _umbracoContextFactory;

    public RedirectsImportHelper(IRedirectsService redirectsService, IUmbracoContextFactory umbracoContextFactory)
    {
        _redirectsService = redirectsService;
        _umbracoContextFactory = umbracoContextFactory;
    }

    public Attempt<List<Redirect>, string> Validate(ImportRedirectsFileExtension fileExtension,
        MemoryStream memoryStream, int domain)
    {
        SetDomain(domain);
        Attempt<List<Redirect>, string> validationResult;
        switch (fileExtension)
        {
            case ImportRedirectsFileExtension.Csv: validationResult = ValidateCsv(memoryStream); break;
            case ImportRedirectsFileExtension.Excel: validationResult = ValidateExcel(memoryStream); break;
            default:
                return Attempt<List<Redirect>, string>.Fail("Invalid filetype, you may only use .csv or .xls",
                    result: null);
        }

        if (validationResult.Success)
        {
            return Attempt<List<Redirect>, string>.Succeed(string.Empty, validationResult.Result);
        }

        return validationResult;
    }

    public Attempt<List<Redirect>, string> Import(ImportRedirectsFileExtension fileExtension, MemoryStream memoryStream,
        int domain)
    {
        SetDomain(domain);
        var validation = Validate(fileExtension, memoryStream, domain);
        if (validation is { Success: true, Result: not null } && validation.Result.Count != 0)
        {
            foreach (var redirect in validation.Result)
            {
                _redirectsService.Save(redirect);
            }
        }

        return validation;
    }

    private bool UrlExists(string oldUrl)
    {
        var existingRedirects = _redirectsService.GetAll(1, 10, null, null, oldUrl.TrimEnd('/'));
        if (existingRedirects.TotalItems > 0 && existingRedirects.Items is not null)
        {
            if (existingRedirects.Items.All(x => x.OldUrl != oldUrl.TrimEnd('/')))
            {
                // exact match not found
                return false;
            }

            if (existingRedirects.Items.Any(x => x.Domain is null || x.Domain.Id == 0))
            {
                // url exists without any domain set
                return true;
            }

            if (existingRedirects.Items.Any(x => x.Domain == _selectedDomain))
            {
                // url exists with specific domain set
                return true;
            }
        }

        return false;
    }

    private Attempt<List<Redirect>, string> ValidateCsv(Stream fileStream)
    {
        // This currently assumes no header, and only 2 columns, from and to url
        fileStream.Position = 0;
        using var reader = new StreamReader(fileStream);
        using (var parser = new TextFieldParser(reader))
        {
            parser.TextFieldType = FieldType.Delimited;
            parser.SetDelimiters(",");
            var parsedData = new List<Redirect>();
            while (!parser.EndOfData)
            {
                var fields = parser.ReadFields();
                if (fields?.Length is not (2 or 3))
                {
                    return Attempt<List<Redirect>, string>.Fail(
                        $"Validation Fail: only 2 columns allowed on line {parser.LineNumber}", result: null);
                }

                var fromUrl = CleanFromUrl(fields[0]);
                var toUrl = Uri.IsWellFormedUriString(fields[1], UriKind.Absolute)
                    ? fields[1]
                    : fields[1].EnsureEndsWith("/").ToLower();
                var redirectCode = HttpStatusCode.MovedPermanently;
                if (fields.Length is 3)
                {
                    redirectCode = GetRedirectCode(fields[2]);
                }
                if (!string.IsNullOrWhiteSpace(fromUrl) && !string.IsNullOrWhiteSpace(toUrl))
                {
                    var urlValidation = ValidateRedirectUrl(fromUrl, parsedData, parser.LineNumber);
                    if (!urlValidation.Success)
                    {
                        return urlValidation;
                    }

                    var codeValidation = ValidateRedirectCode(redirectCode, fromUrl);
                    if (!codeValidation.Success)
                    {
                        return codeValidation;
                    }


                    parsedData.Add(CreateRedirect(fromUrl, toUrl, redirectCode));
                }
                else
                {
                    return Attempt<List<Redirect>, string>.Fail($"line {parser.LineNumber}", result: null);
                }
            }

            return Attempt<List<Redirect>, string>.Succeed(string.Empty, parsedData);
        }
    }

    private Attempt<List<Redirect>, string> ValidateExcel(Stream fileStream)
    {
        fileStream.Position = 0;
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        try
        {
            using var reader = ExcelReaderFactory.CreateReader(fileStream);
            var result = reader.AsDataSet();
            var dataTable = result.Tables[0];
            var parsedData = new List<Redirect>();
            for (var i = 0; i < dataTable.Rows.Count; i++)
            {
                var row = dataTable.Rows[i];
                if (row.ItemArray.Length is not (2 or 3))
                {
                    return Attempt<List<Redirect>, string>.Fail($"2 or 3 columns required on row {i + 1}");
                }

                var fromUrl = CleanFromUrl(row[0].ToString());
                var toUrl = Uri.IsWellFormedUriString(row[1].ToString(), UriKind.Absolute)
                    ? row[1].ToString()
                    : row[1].ToString()?.EnsureEndsWith("/").ToLower();
                var redirectCode = GetRedirectCode(row[2].ToString());
                if (!string.IsNullOrWhiteSpace(fromUrl) && !string.IsNullOrWhiteSpace(toUrl))
                {
                    var urlValidation = ValidateRedirectUrl(fromUrl, parsedData, i + 1);
                    if (!urlValidation.Success)
                    {
                        return urlValidation;
                    }

                    var codeValidation = ValidateRedirectCode(redirectCode, fromUrl);
                    if (!codeValidation.Success)
                    {
                        return codeValidation;
                    }

                    parsedData.Add(CreateRedirect(fromUrl, toUrl, redirectCode));
                }
                else
                {
                    return Attempt<List<Redirect>, string>.Fail($"row {i + 1}");
                }
            }

            return Attempt<List<Redirect>, string>.Succeed(string.Empty, parsedData);
        }
        catch
        {
            return Attempt<List<Redirect>, string>.Fail("Invalid file type");
        }
    }

    private HttpStatusCode GetRedirectCode(string redirectCode)
    {
        return redirectCode switch
        {
            "301" => HttpStatusCode.MovedPermanently,
            "302" => HttpStatusCode.Redirect,
            _ => HttpStatusCode.MovedPermanently
        };
    }

    private void SetDomain(int domain)
    {
        if (domain <= 0) return;

        using var ctx = _umbracoContextFactory.EnsureUmbracoContext();
        var foundDomain = ctx.UmbracoContext.Domains?.GetAll(false).FirstOrDefault(it => it.Id == domain);
        if (foundDomain is null)
        {
            return;
        }

        _selectedDomain = foundDomain;
    }

    private static string CleanFromUrl(string? url)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            return string.Empty;
        }

        var urlParts = url.ToLowerInvariant().Split('?');
        if (urlParts.Length == 0)
        {
            return string.Empty;
        }

        var fromUrl = urlParts[0].TrimEnd('/');
        if (urlParts.Length > 1)
        {
            fromUrl = $"{fromUrl}?{string.Join("?", urlParts.Skip(1))}";
        }

        fromUrl = fromUrl.EnsureStartsWith("/");
        return fromUrl;
    }

    private Redirect CreateRedirect(string fromUrl, string toUrl, HttpStatusCode redirectCode)
    {
        return new Redirect
        {
            Domain = _selectedDomain, CustomDomain = null, Id = 0, Key = Guid.NewGuid(), IsEnabled = true, IsRegex = false,
            NewNodeCulture = null, NewNode = null, NewUrl = toUrl, OldUrl = fromUrl, RedirectCode = (int)redirectCode
        };
    }

    private Attempt<List<Redirect>, string> ValidateRedirectUrl(string fromUrl, IEnumerable<Redirect> parsedData, long? rowNumber = null)
    {
        if (!Uri.IsWellFormedUriString(fromUrl, UriKind.Relative))
        {
            return Attempt<List<Redirect>, string>.Fail($"row {rowNumber ?? 0 + 1}", result: null);
        }

        if (UrlExists(fromUrl))
        {
            return Attempt<List<Redirect>, string>.Fail(
                $"Redirect already exists for 'from' URL: {fromUrl} validation aborted.");
        }

        if (parsedData.Any(x => x.OldUrl == fromUrl.TrimEnd('/')))
        {
            return Attempt<List<Redirect>, string>.Fail(
                $"Url appears more then one time in import file: {fromUrl}", result: null);
        }

        return Attempt<List<Redirect>, string>.Succeed(string.Empty, null);
    }

    private Attempt<List<Redirect>, string> ValidateRedirectCode(HttpStatusCode redirectCode, string fromUrl)
    {
        if (redirectCode is not (HttpStatusCode.Redirect or HttpStatusCode.MovedPermanently))
        {
            return Attempt<List<Redirect>, string>.Fail($"Only 301 and 302 are allowed: {fromUrl}", result: null);
        }

        return Attempt<List<Redirect>, string>.Succeed(string.Empty, null);
    }

}