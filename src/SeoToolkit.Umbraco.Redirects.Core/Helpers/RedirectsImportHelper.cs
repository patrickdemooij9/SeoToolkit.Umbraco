using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using Microsoft.VisualBasic.FileIO;
using SeoToolkit.Umbraco.Redirects.Core.Interfaces;
using SeoToolkit.Umbraco.Redirects.Core.Models.Business;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Web;

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

    public  HttpResponseMessage ImportCsv(Stream fileStream, bool importFile, string domain)
    {
        SetDomain(domain);
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
                    return new HttpResponseMessage { StatusCode = HttpStatusCode.BadRequest, ReasonPhrase = $"Validation failed: only 2 columns allowed on line {parser.LineNumber}" };
                }
                
                if(!string.IsNullOrWhiteSpace(fields[0]) && !string.IsNullOrWhiteSpace(fields[1])){
                    
                    if(UrlExists(fields[0]))
                    {
                        return new HttpResponseMessage { StatusCode = HttpStatusCode.BadRequest, ReasonPhrase = $"Redirect already exists for 'from' URL: {fields[0]} validation aborted." };
                    }
                    parsedData.Add(fields[0], fields[1]);
                    
                }
                else
                {
                    return new HttpResponseMessage { StatusCode = HttpStatusCode.BadRequest, ReasonPhrase = $"line {parser.LineNumber}" };
                }
            }

            if (importFile)
            {
                foreach (var entry in parsedData)
                {
                    this.SaveRedirect(entry);
                }
            }
        }

        return new HttpResponseMessage { StatusCode = HttpStatusCode.OK };
    }
    
    private bool UrlExists(string oldUrl)
    {
        var existingRedirects = _redirectsService.GetAll(1, 10, null, null, oldUrl.TrimEnd('/'));
        return existingRedirects.TotalItems > 0;
    }
    
    public HttpResponseMessage ImportExcel(Stream fileStream, bool importFile, string domain)
    {
        SetDomain(domain);
        fileStream.Position = 0;
        System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

        using var reader = ExcelReaderFactory.CreateReader(fileStream);
        var result = reader.AsDataSet();
        var dataTable = result.Tables[0]; 
            
        var parsedData = new Dictionary<string, string>();
            
        for (var i = 0; i < dataTable.Rows.Count; i++)
        {
            var row = dataTable.Rows[i];
            if (row.ItemArray.Length != 2)
            {
                return new HttpResponseMessage { StatusCode = HttpStatusCode.BadRequest, ReasonPhrase = $"only 2 columns allowed on row {i + 1}" };
            }
                
            var fromUrl = row[0].ToString();
            var toUrl = row[1].ToString();
                
            if (!string.IsNullOrWhiteSpace(fromUrl) && !string.IsNullOrWhiteSpace(toUrl))
            {
                if (UrlExists(fromUrl))
                {
                    return new HttpResponseMessage { StatusCode = HttpStatusCode.BadRequest, ReasonPhrase = $"Redirect already exists for 'from' URL: {fromUrl} validation aborted." };
                }
                parsedData.Add(fromUrl, toUrl);
            }
            else
            {
                return new HttpResponseMessage { StatusCode = HttpStatusCode.BadRequest, ReasonPhrase = $"row {i + 1}" };
            }
        }

        if (importFile)
        {
            foreach (var entry in parsedData)
            {
                this.SaveRedirect(entry);
            }
        }

        return new HttpResponseMessage { StatusCode = HttpStatusCode.OK };
    }

    private void SetDomain(string domain)
    {
        int.TryParse(domain, out var domainId);
        using var ctx = _umbracoContextFactory.EnsureUmbracoContext();

        var foundDomain = ctx.UmbracoContext.Domains?.GetAll(false).FirstOrDefault(it => it.Id == domainId);
        if (foundDomain is not null)
        {
            _selectedDomain = foundDomain;;
        }
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
