using System.Collections.Generic;

namespace SeoToolkit.Umbraco.Common.Core.Interfaces
{
    public interface IRepository<T>
    {
        IEnumerable<T> GetAll();
        T Get(int id);
        T Add(T model);
        T Update(T model);
        void Delete(int id);
    }
}
