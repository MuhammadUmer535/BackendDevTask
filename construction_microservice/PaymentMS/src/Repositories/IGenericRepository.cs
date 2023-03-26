using System.Collections.Generic;
using System.Threading.Tasks;

namespace PaymentMS.src.Repositories
{
    public interface IGenericRepository<T> where T : class
    {
        Task<int> AddAsync(T entity);
    }
}