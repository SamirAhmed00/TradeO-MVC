using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using System.Threading.Tasks;
using TradeO.DataAccess.Repository.IRepository;
using TradeO.Models;
using TradeO.Models.ViewModels;
using TradeO.Utility;

namespace TradeO.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class ProductController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        // IWebHostEnviroment For Get And Access wwwroot Folder
        private readonly IWebHostEnvironment _webHostEnviroment; 

        public ProductController(IUnitOfWork unitOfWork , IWebHostEnvironment webHostEnviroment)
        {
            _unitOfWork = unitOfWork;
            _webHostEnviroment = webHostEnviroment;
        }

        [HttpGet]
        public async Task<IActionResult> Index(string sortOrder, int? categoryId)
        {            
            var allProducts = await _unitOfWork.Product.GetAll();

            if (allProducts == null || !allProducts.Any())
            {
                ViewBag.Message = "No Products Found.";
                return View(new List<Product>());
            }

            if (categoryId.HasValue && categoryId.Value > 0)
            {
                allProducts = allProducts.Where(p => p.CategoryId == categoryId.Value).ToList();
            }

            IEnumerable<Product> products = sortOrder switch
            {
                "nameAsc" => allProducts.OrderBy(p => p.Name),
                "nameDesc" => allProducts.OrderByDescending(p => p.Name),
                "priceAsc" => allProducts.OrderBy(p => p.Price),
                "priceDesc" => allProducts.OrderByDescending(p => p.Price),
                "discountAsc" => allProducts.OrderBy(p => p.DiscountPrice ?? p.Price),
                "discountDesc" => allProducts.OrderByDescending(p => p.DiscountPrice ?? p.Price),
                "sellerAsc" => allProducts.OrderBy(p => p.Seller),
                "sellerDesc" => allProducts.OrderByDescending(p => p.Seller),
                "newest" => allProducts.OrderByDescending(p => p.Id),
                "oldest" => allProducts.OrderBy(p => p.Id),

                _ => allProducts, // Default (no sorting)
            };

            var allCategories = await _unitOfWork.Category.GetAll();
            ViewBag.Categories = allCategories.Select(c => new SelectListItem
            {
                Text = c.Name,
                Value = c.Id.ToString(),
                Selected = categoryId.HasValue && c.Id == categoryId.Value
            });

           


            return View(products);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {

            //// All Categories List
            //var allCategories = await _unitOfWork.Category.GetAll();
            //IEnumerable<SelectListItem> Categories = allCategories.Select(c => new SelectListItem
            //{
            //    Text = c.Name,
            //    Value = c.Id.ToString()
            //});

            ProductVM productVM= new ProductVM() 
            { 
                CategoryList = await GetCategoriesListAsync(),
                Product = new Product()
            };
           
            return View(productVM);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProductVM newproductVM, IFormFile imgFile)
        {
            // Check Model State Valid Or Not
            ModelState.Remove("ImageUrl");
            ModelState.Remove("Product.Category");
            ModelState.Remove("CategoryList");
            ModelState.Remove("Product.ImageUrl");
            ModelState.Remove("Product.Category");

            if (!ModelState.IsValid)
            {
                
                newproductVM.CategoryList = await GetCategoriesListAsync();

                ViewBag.Message = "Invalid request data!";
                TempData["Error"] = "Something went wrong while Creating the product!";
                return View(newproductVM);
            }

            // Check If Product Name Is Exist ? - (For unique Product Name)
            var existingProduct = await _unitOfWork.Product.Get(p => p.Name == newproductVM.Product.Name && p.Seller == newproductVM.Product.Seller);
            if (existingProduct != null)
            {
                newproductVM.CategoryList = await GetCategoriesListAsync();

                ModelState.AddModelError("Name", "Product with the same name already exists for this seller!");
                return View(newproductVM);
            }

            if (newproductVM.Product.DiscountPrice.HasValue && newproductVM.Product.DiscountPrice > newproductVM.Product.Price)
            {
                newproductVM.CategoryList = await GetCategoriesListAsync();

                ModelState.AddModelError("DiscountPrice", "Discount price cannot be higher than the original price.");
                return View(newproductVM);
            }

            // Save Image To wwwroot/images/products Folder -- Only If All Data Is Valid
            newproductVM.Product.ImageUrl = await SaveImageAsync(imgFile);

            await _unitOfWork.Product.Add(newproductVM.Product);
            await _unitOfWork.Save();

            TempData["Success"] = "Product created successfully!";

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int productId)
        {
            if (productId <= 0)
            {
                ViewBag.Message = "Product not found. ID can't be negative or zero.";
                return View("NotFound"); // I Will Handle This In Future
            }

            var currentProduct = await _unitOfWork.Product.Get(p => p.Id == productId);

            if (currentProduct == null)
            {
                ViewBag.Message = "Product not found!";
                TempData["Error"] = "Something went wrong while retrieving the product!";
                return View("NotFound"); // I Will Handle This In Future
            }

            var categoriesList = await GetCategoriesListAsync();

            ProductVM productVM = new ProductVM
            {
                Product = currentProduct,
                CategoryList = categoriesList
            };

            return View(productVM);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ProductVM updatedProductVM, IFormFile? imageFile)
        {
            ModelState.Remove("Product.ImageUrl");
            ModelState.Remove("Product.Category");
            if (!ModelState.IsValid)
            {
                updatedProductVM.CategoryList = await GetCategoriesListAsync();                
                return View(updatedProductVM);
            }

            var existingProduct = await _unitOfWork.Product.Get(p => p.Id == updatedProductVM.Product.Id);

            if (existingProduct == null)
            {
                updatedProductVM.CategoryList = await GetCategoriesListAsync();
                ViewBag.Message = "Product not found!";
                TempData["Error"] = "Something went wrong while updating the product!";
                return View(updatedProductVM);
            }

            // Check if another product by same seller has the same name
            var duplicateProduct = await _unitOfWork.Product.Get(
                p => p.Name == updatedProductVM.Product.Name
                  && p.Seller == updatedProductVM.Product.Seller
                  && p.Id != updatedProductVM.Product.Id
            );

            if (duplicateProduct != null)
            {
                updatedProductVM.CategoryList = await GetCategoriesListAsync();
                ModelState.AddModelError("Name", "A product with the same name already exists for this seller!");
                return View(updatedProductVM);
            }

            // Logical validation: Discount can't exceed Price
            if (updatedProductVM.Product.DiscountPrice.HasValue && updatedProductVM.Product.DiscountPrice > updatedProductVM.Product.Price)
            {
                updatedProductVM.CategoryList = await GetCategoriesListAsync();
                ModelState.AddModelError("DiscountPrice", "Discount price cannot be higher than the original price.");
                return View(updatedProductVM);
            }

            // Update only editable fields
            existingProduct.Name = updatedProductVM.Product.Name;
            existingProduct.Description = updatedProductVM.Product.Description;
            existingProduct.Seller = updatedProductVM.Product.Seller;
            existingProduct.Price = updatedProductVM.Product.Price;
            existingProduct.DiscountPrice = updatedProductVM.Product.DiscountPrice;
            existingProduct.CategoryId = updatedProductVM.Product.CategoryId;
            // To Check If User Will Update The Image Or Not (If Not We Will Send The Old Image )
            if (imageFile != null && imageFile.Length > 0)
            {
                string wwwRootPath = _webHostEnviroment.WebRootPath;
                string fileName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);
                string uploadPath = Path.Combine(wwwRootPath, @"images\products");

                // Delete old image if exists
                if (!string.IsNullOrEmpty(existingProduct.ImageUrl))
                {
                    var oldImagePath = Path.Combine(wwwRootPath, existingProduct.ImageUrl.TrimStart('/'));
                    if (System.IO.File.Exists(oldImagePath))
                        System.IO.File.Delete(oldImagePath);
                }

                // Save new image
                using (var fileStream = new FileStream(Path.Combine(uploadPath, fileName), FileMode.Create))
                {
                    await imageFile.CopyToAsync(fileStream);
                }

                existingProduct.ImageUrl = "/images/products/" + fileName;
            }
            else
            {
                // Keep old image if no new file uploaded
                existingProduct.ImageUrl = existingProduct.ImageUrl;
            }


            _unitOfWork.Product.Update(existingProduct);
            await _unitOfWork.Save();

            TempData["Success"] = "Product updated successfully!";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int productId)
        {
            var productFromDb = await _unitOfWork.Product.Get(p => p.Id == productId);

            if (productFromDb == null)
            {
                TempData["Error"] = "Product not found or already deleted!";
                return RedirectToAction(nameof(Index));
            }

            // Delete image from wwwroot if exists
            if (!string.IsNullOrEmpty(productFromDb.ImageUrl))
            {
                string wwwRootPath = _webHostEnviroment.WebRootPath;
                var imagePath = Path.Combine(wwwRootPath, productFromDb.ImageUrl.TrimStart('/').Replace("/", "\\"));

                if (System.IO.File.Exists(imagePath))
                {
                    System.IO.File.Delete(imagePath);
                }
            }

            _unitOfWork.Product.Remove(productFromDb);
            await _unitOfWork.Save();

            TempData["Success"] = "Product deleted successfully!";
            return RedirectToAction(nameof(Index));
        }


        [HttpGet]
        public async Task<IActionResult> Details(int productId)
        {
            if (productId <= 0)
            {
                ViewBag.Message = "Product not found. ID can't be negative or zero.";
                return View("NotFound"); // I Will Handle This In Future
            }
            var currentProduct = await _unitOfWork.Product.Get(p => p.Id == productId);
            if (currentProduct == null)
            {
                ViewBag.Message = "Product not found!";
                TempData["Error"] = "Something went wrong while retrieving the product!";
                return View("NotFound"); // I Will Handle This In Future
            }
            
            var category = await _unitOfWork.Category.Get(c => c.Id == currentProduct.CategoryId);
            currentProduct.Category = category;

            return View(currentProduct);
        }



        // ==================================================================
        // Private Helper Methods
        // ==================================================================
        // These methods are declared as 'private' because they are only used internally
        // within this controller and should not be exposed as endpoints to the web.
        //
        // Benefits of using private helper methods:
        // 1. Reusability: We can call the same code from multiple actions without repeating it.
        // 2. Readability: Separating logic into small, descriptive methods makes the controller easier to understand.
        // 3. Maintainability: Any future changes need to be done in one place, reducing the risk of bugs.
        // 4. Clean Code: Keeps the main actions concise and focused on their main responsibility.

        private async Task<IEnumerable<SelectListItem>> GetCategoriesListAsync()
        {
            // All Categories List
            var allCategories = await _unitOfWork.Category.GetAll();
            return allCategories.Select(c => new SelectListItem
            {
                Text = c.Name,
                Value = c.Id.ToString()
            });
        }

        private async Task<string> SaveImageAsync(IFormFile imgFile)
        {
            if (imgFile == null) return null;

            string wwwRootPath = _webHostEnviroment.WebRootPath;
            string imgFileName = Guid.NewGuid().ToString() + Path.GetExtension(imgFile.FileName);
            string productPath = Path.Combine(wwwRootPath, @"images\products");

            if (!Directory.Exists(productPath))
                Directory.CreateDirectory(productPath);

            string finalPath = Path.Combine(productPath, imgFileName);
            using (var fileStream = new FileStream(finalPath, FileMode.Create))
            {
                await imgFile.CopyToAsync(fileStream);
            }

            return "/images/products/" + imgFileName;

        }

    }
}
