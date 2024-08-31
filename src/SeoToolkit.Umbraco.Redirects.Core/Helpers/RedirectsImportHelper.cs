using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

    public Attempt<Dictionary<string,string>?, string> Validate(ImportRedirectsFileExtension fileExtension, MemoryStream memoryStream, string domain)
    {
        SetDomain(domain);
        Attempt<Dictionary<string,string>, string> validationResult;
        switch (fileExtension)
        {
            case ImportRedirectsFileExtension.Csv:
                validationResult = ValidateCsv(memoryStream);
                break;
            case ImportRedirectsFileExtension.Excel:
                validationResult = ValidateExcel(memoryStream);
                break;
            default:
                return Attempt<Dictionary<string,string>?, string>.Fail("Invalid filetype, you may only use .csv or .xls", result: null);
        }

        if (validationResult.Success)
        {
            return Attempt<Dictionary<string,string>?, string>.Succeed(string.Empty, validationResult.Result);
        }

        return validationResult;
    }

    public Attempt<Dictionary<string,string>?, string> Import(ImportRedirectsFileExtension fileExtension, MemoryStream memoryStream, string domain)
    {
        SetDomain(domain);
        var validation = Validate(fileExtension, memoryStream, domain);
        if (validation is { Success: true, Result: not null } && validation.Result.Count != 0)
        {
            foreach (var entry in validation.Result)
            {
                SaveRedirect(entry);
            }
        }

        return validation;
    }

    private bool UrlExists(string oldUrl)
    {
        var existingRedirects = _redirectsService.GetAll(1, 10, null, null, oldUrl.TrimEnd('/'));
        if (existingRedirects.TotalItems > 0 && existingRedirects.Items is not null)
        {
            if (existingRedirects.Items.Count(x => x.Domain is null || x.Domain.Id == 0) > 0)
            {
                //url exists without any domain set
                return true;
            }
            if (existingRedirects.Items.Count(x => x.Domain == _selectedDomain) > 0)
            {
                //url exists with specific domain set
                return true;
            }
        }
        return false;
    }

    private Attempt<Dictionary<string,string>?, string> ValidateCsv(Stream fileStream)
    {
        //This currently assumes no header, and only 2 columns, from and to url
        fileStream.Position = 0;
        using (var reader = new StreamReader(fileStream))
        using (var parser = new TextFieldParser(reader))
        {
            parser.TextFieldType = FieldType.Delimited;
            parser.SetDelimiters(",");
            var parsedData = new Dictionary<string,string>();

            while (!parser.EndOfData)
            {
                var fields = parser.ReadFields();

                if (fields?.Length != 2)
                {
                    return Attempt<Dictionary<string,string>?, string>.Fail($"Validation Fail: only 2 columns allowed on line {parser.LineNumber}", result: null);
                }
                var fromUrl = CleanFromUrl(fields[0]);
                var toUrl = Uri.IsWellFormedUriString(fields[1], UriKind.Absolute) ?
                    fields[1] :
                    fields[1].EnsureEndsWith("/").ToLower();

                if(!string.IsNullOrWhiteSpace(fromUrl) && !string.IsNullOrWhiteSpace(toUrl)){

                    if (!Uri.IsWellFormedUriString(fromUrl, UriKind.Relative))
                    {
                        return Attempt<Dictionary<string,string>?, string>.Fail($"line {parser.LineNumber}", result: null);
                    }
                    if(UrlExists(fromUrl))
                    {
                        return Attempt<Dictionary<string,string>?, string>.Fail($"Redirect already exists for 'from' URL: {fromUrl} validation aborted.", result: null);
                    }
                    if (parsedData.ContainsKey(fromUrl.TrimEnd("/")))
                    {
                        return Attempt<Dictionary<string,string>?, string>.Fail($"Url appears more then one time in import file: {fromUrl}", result: null);
                    }
                    parsedData.Add(fromUrl, toUrl);

                }
                else
                {
                    return Attempt<Dictionary<string,string>?, string>.Fail($"line {parser.LineNumber}", result: null);
                }
            }
            return Attempt<Dictionary<string,string>?, string>.Succeed(string.Empty, parsedData);
        }


    }
     private Attempt<Dictionary<string,string>?, string> ValidateExcel(Stream fileStream)
    {
        fileStream.Position = 0;
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

        try
        {
            using var reader = ExcelReaderFactory.CreateReader(fileStream);
            var result = reader.AsDataSet();
            var dataTable = result.Tables[0];

            var parsedData = new Dictionary<string, string>();

            for (var i = 0; i < dataTable.Rows.Count; i++)
            {
                var row = dataTable.Rows[i];
                if (row.ItemArray.Length != 2)
                {
                    return Attempt<Dictionary<string, string>?, string>.Fail($"only 2 columns allowed on row {i + 1}");
                }

                var fromUrl = CleanFromUrl(row[0].ToString());
                var toUrl = Uri.IsWellFormedUriString(row[1].ToString(), UriKind.Absolute)
                    ? row[1].ToString()
                    : row[1].ToString()?.EnsureEndsWith("/").ToLower();

                if (!string.IsNullOrWhiteSpace(fromUrl) && !string.IsNullOrWhiteSpace(toUrl))
                {
                    if (!Uri.IsWellFormedUriString(fromUrl, UriKind.Relative))
                    {
                        return Attempt<Dictionary<string, string>?, string>.Fail($"row {i + 1}", result: null);
                    }

                    if (UrlExists(fromUrl))
                    {
                        return Attempt<Dictionary<string, string>?, string>.Fail(
                            $"Redirect already exists for 'from' URL: {fromUrl} validation aborted.");
                    }
                    if (parsedData.ContainsKey(fromUrl.TrimEnd("/")))
                    {
                        return Attempt<Dictionary<string,string>?, string>.Fail($"Url appears more then one time in import file: {fromUrl}", result: null);
                    }
                    parsedData.Add(fromUrl, toUrl);
                }
                else
                {
                    return Attempt<Dictionary<string, string>?, string>.Fail($"row {i + 1}");
                }
            }

            return Attempt<Dictionary<string, string>?, string>.Succeed(string.Empty, parsedData);
        }
        catch
        {
            return Attempt<Dictionary<string, string>?, string>.Fail("Invalid file type");
        }
    }

    private void SetDomain(string domain)
    {
        var parseSuccess = int.TryParse(domain, out var domainId);
        if (!parseSuccess)
        {
            domainId = 0;
        }

        using var ctx = _umbracoContextFactory.EnsureUmbracoContext();
        var foundDomain = ctx.UmbracoContext.Domains?.GetAll(false).FirstOrDefault(it => it.Id == domainId);
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

    private void SaveRedirect(KeyValuePair<string, string> entry)
    {
        var redirect = new Redirect
        {
            Domain = _selectedDomain,
            CustomDomain = null,
            Id = 0,
            IsEnabled = true,
            IsRegex = false,
            NewNodeCulture = null,
            NewNode = null,
            NewUrl = entry.Value,
            OldUrl = entry.Key,
            RedirectCode = 301
        };

        _redirectsService.Save(redirect);
    }
}
