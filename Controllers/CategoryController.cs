using TradeO.ViewModels.Categories;

namespace TradeO.Controllers
{
    public class CategoryController : Controller
    {
        private readonly ApplicationDbContext _db;

        public CategoryController(ApplicationDbContext db)
        {
            _db = db;
        }
        // Category/Index
        public IActionResult Index(string SortOrder)
        {
            List<Category> categories = new List<Category>();

            switch (SortOrder)
            {
                case "displayOrderAsc":
                    categories = _db.Categories.OrderBy(c => c.DisplayOrder).ToList();
                    break;
                case "displayOrderDesc":
                    categories= _db.Categories.OrderByDescending(c => c.DisplayOrder).ToList();
                    break;
                case "nameAsc":
                    categories = _db.Categories.OrderBy(c => c.Name).ToList();
                    break;
                default:
                    categories = _db.Categories.ToList();
                    break;
            }



            if (categories == null || !categories.Any())
            {
                ViewBag.Message = "No categories found.";
                return View(new List<Category>());
            }
            return View(categories);
        }
    }
}
