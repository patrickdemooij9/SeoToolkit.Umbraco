using System;
using System.Collections.Generic;
using SeoToolkit.Umbraco.MetaFields.Core.Models.SchemaEntry.Business;

namespace SeoToolkit.Umbraco.MetaFields.Core.Services.SchemaEntryService
{
    public interface ISchemaEntryService
    {
        IEnumerable<SchemaEntryDto> GetAll(string ownerType, Guid ownerKey);
        SchemaEntryDto GetById(Guid id);
        IEnumerable<SchemaEntryDto> GetByIds(IEnumerable<Guid> ids);
        SchemaEntryDto Add(SchemaEntryDto model);
        SchemaEntryDto Update(SchemaEntryDto model);
        void Delete(Guid id);
    }
}
