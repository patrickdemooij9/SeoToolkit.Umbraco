using System;
using System.Collections.Generic;

namespace SeoToolkit.Umbraco.Common.Core.Interfaces
{
    public interface IRepository<T>
    {
        IEnumerable<T> GetAll();
        T Get(int id);
        T Get(Guid key);
        T Add(T model);
        T Update(T model);
        void Delete(int id);
    }
}
