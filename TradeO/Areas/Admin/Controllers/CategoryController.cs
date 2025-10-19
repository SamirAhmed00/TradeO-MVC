using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using TradeO.DataAccess.Data;
using TradeO.DataAccess.Repository.IRepository;
using TradeO.Models;
using TradeO.Utility;

namespace TradeO.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class CategoryController : Controller
    {
        //private readonly ApplicationDbContext _db;
        //private readonly ICategoryRepository _categoryRepo;
        private readonly IUnitOfWork _unitOfWork;

        public CategoryController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        [HttpGet]
        public async Task<IActionResult> Index(string SortOrder)
        {
            var allCategories = await _unitOfWork.Category.GetAll();

            List<Category> categories = SortOrder switch
            {
                "displayOrderAsc" => allCategories.OrderBy(c => c.DisplayOrder).ToList(),
                "displayOrderDesc" => allCategories.OrderByDescending(c => c.DisplayOrder).ToList(),
                "nameAsc" => allCategories.OrderBy(c => c.Name).ToList(),
                _ => allCategories.ToList(), // default
            };

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
            var existingCategory = await _unitOfWork.Category.Get(c => c.Name == newCategory.Name);
            if (existingCategory != null)
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

            await _unitOfWork.Category.Add(newCategory);
            await _unitOfWork.Save();

            TempData["Success"] = "Category Created Successfully.";

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

            Category CurrentCategory = await _unitOfWork.Category.Get(c => c.Id == CategoryId);

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


            Category existingCategory = await _unitOfWork.Category.Get(c => c.Id == updatedCategory.Id);


            if (existingCategory == null)
            {
                ViewBag.Message = "Category not found!";
                TempData["Error"] = "Something went wrong while Updating the category!";
                return View(updatedCategory);
            }

            //bool isDuplicateName = await _db.Categories.AnyAsync(c => c.Name == updatedCategory.Name && c.Id != updatedCategory.Id);
            var duplicateCategory = await _unitOfWork.Category.Get( c => c.Name == updatedCategory.Name && c.Id != updatedCategory.Id );

            if (duplicateCategory != null)
            {
                ModelState.AddModelError("Name", "Category name already exists!");
                return View(updatedCategory);
            }

            existingCategory.Name = updatedCategory.Name;
            existingCategory.DisplayOrder = updatedCategory.DisplayOrder;

            _unitOfWork.Category.Update(existingCategory);
            await _unitOfWork.Save();
            TempData["Success"] = "Category Updated Successfully.";

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int CategoryId)
        {
            Category CategoryFromDb = await _unitOfWork.Category.Get(c => c.Id == CategoryId);
            if (CategoryFromDb == null )
            {
                TempData["Error"] = "Category not found or already deleted!";
                return RedirectToAction(nameof(Index));
            }

            _unitOfWork.Category.Remove(CategoryFromDb);
            await _unitOfWork.Save();
            TempData["Success"] = "Category Deleted Successfully!";

            return RedirectToAction(nameof(Index));
        }
    }
}
