using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace TradeO.DataAccess.Repository.IRepository
{
    public interface IRepository<T> where T : class
    {
        // Genereic repository 
        Task<IEnumerable<T>> GetAll(Expression<Func<T, bool>>? filter = null , string? IncludeProperties = null);
        Task<T> Get(Expression<Func<T, bool>> filter, string? IncludeProperties = null); 
        Task Add(T entity);
        
        void Remove(T entity);
        void RemoveRange(IEnumerable<T> entity);
    }
}
