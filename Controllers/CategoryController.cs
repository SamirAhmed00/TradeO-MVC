using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using TradeO.DataAccess.Data;
using TradeO.Models;

namespace TradeO.Controllers
{
    public class CategoryController : Controller
    {
        private readonly ApplicationDbContext _db;

        public CategoryController(ApplicationDbContext db)
        {
            _db = db;
        }
       

        public async Task<IActionResult> Index(string SortOrder)
        {
            List<Category> categories = new List<Category>();

            switch (SortOrder)
            {
                case "displayOrderAsc":
                    categories = await _db.Categories.AsNoTracking().OrderBy(c => c.DisplayOrder).ToListAsync();
                    break;
                case "displayOrderDesc":
                    categories= await _db.Categories.AsNoTracking().OrderByDescending(c => c.DisplayOrder).ToListAsync();
                    break;
                case "nameAsc":
                    categories = await _db.Categories.AsNoTracking().OrderBy(c => c.Name).ToListAsync();
                    break;
                default:
                    categories = await _db.Categories.AsNoTracking().ToListAsync();
                    break;
            }

            // DONT FORGET TO REMOVE THE TEST 
            List<Category> emptyForTestcategories = new List<Category>();
            // || emptyForTestcategories == null || !emptyForTestcategories.Any()


            if (categories == null || !categories.Any() )
            {
                ViewBag.Message = "No categories found.";
                return View(new List<Category>());
            }


            return View(categories);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(Category newCategory)
        {
            // Check Model State Valid Or Not
            if (!ModelState.IsValid)
            {
                ViewBag.Message = "Invalid request data!";
                TempData["Error"] = "Something went wrong while Creating the category!";
                return View(newCategory);
            }

            // Check If Category Name Is Exist ? - (For unique Category Name)
            if (_db.Categories.Any(c => c.Name == newCategory.Name))
            {
                ModelState.AddModelError("Name", "Category already exists!");
                return View(newCategory);
            }

            // Check If Displacy Order Is Negative
            if (newCategory.DisplayOrder < 0)
            {
                ModelState.AddModelError("DisplayOrder", "Display order cannot be negative.");
                return View(newCategory);
            }

            await _db.Categories.AddAsync(newCategory);
            await _db.SaveChangesAsync();

            TempData["Success"] = "Category created successfully.";

            // Back To Index View
            return RedirectToAction(nameof(Index), new { SortOrder = "" });
        }


        [HttpGet]
        public async Task<IActionResult> Edit(int CategoryId)
        {
            if (CategoryId <= 0)
            {
                ViewBag.Message = "Category not found. ID can't be negative or zero.";
                return View("NotFound"); // I Will Handle This In Future
            }

            Category CurrentCategory = await _db.Categories.FindAsync(CategoryId);

            if (CurrentCategory == null)
            {
                ViewBag.Message = "Category not found!";
                TempData["Error"] = "Something went wrong while Getting the category!";
                return View("NotFound"); // I Will Handle This In Future
            }
            return View(CurrentCategory);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Category updatedCategory)
        {
            if(!ModelState.IsValid)
            {
                return View(updatedCategory);
            }


            Category existingCategory = await _db.Categories.FindAsync(updatedCategory.Id);


            if (existingCategory == null)
            {
                ViewBag.Message = "Category not found!";
                TempData["Error"] = "Something went wrong while Updating the category!";
                return View(updatedCategory);
            }

            bool isDuplicateName = await _db.Categories.AnyAsync(c => c.Name == updatedCategory.Name && c.Id != updatedCategory.Id);

            if (isDuplicateName)
            {
                ModelState.AddModelError("Name", "Category name already exists!");
                return View(updatedCategory);
            }

            existingCategory.Name = updatedCategory.Name;
            existingCategory.DisplayOrder = updatedCategory.DisplayOrder;

            
            await _db.SaveChangesAsync();
            TempData["Success"] = "Category Updated successfully.";

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int CategoryId)
        {
            Category CategoryFromDb = await _db.Categories.FindAsync(CategoryId);
            if (CategoryFromDb == null )
            {
                TempData["Error"] = "Category not found or already deleted!";
                return RedirectToAction(nameof(Index));
            }
            
            _db.Categories.Remove(CategoryFromDb);
            await _db.SaveChangesAsync();
            TempData["Success"] = "Category deleted successfully!";

            return RedirectToAction(nameof(Index));
        }
    }
}
