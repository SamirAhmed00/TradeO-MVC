using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using TradeO.DataAccess.Data;
using TradeO.DataAccess.Repository.IRepository;
using TradeO.Models;

namespace TradeO.DataAccess.Repository
{
    public class CategoryRepository : Repository<Category>, ICategoryRepository
    {
        private readonly ApplicationDbContext _db;
        public CategoryRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        

        public void Update(Category category)
        {
            _db.Categories.Update(category);
        }
    }
}
