using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Diagnostics;
using System.Security.Claims;
using TradeO.DataAccess.Repository.IRepository;
using TradeO.Models;
using TradeO.Utility;

namespace TradeO.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IUnitOfWork _unitOfWork;

        public HomeController(ILogger<HomeController> logger, IUnitOfWork unitOfWork)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
        }

        [HttpGet]
        public async Task<IActionResult> Index(string sortOrder, int? categoryId)
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            if(claim != null)
            {
                var allCarts = await _unitOfWork.ShoppingCart.GetAll(c => c.ApplicationUserId == claim.Value);
                HttpContext.Session.SetInt32(SD.SessionCart,
                   allCarts.Count());
            }


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
        public async Task<IActionResult> Details(int productId)
        {
            if (productId <= 0)
            {
                ViewBag.Message = "Product not found. ID can't be negative or zero.";
                return RedirectToAction(nameof(Index)); // I Will Handle This In Future
            }


            var currentProduct = await _unitOfWork.Product.Get(p => p.Id == productId);
            if (currentProduct == null)
            {
                ViewBag.Message = "Product not found!";
                TempData["Error"] = "Something went wrong while retrieving the product!";
                return RedirectToAction(nameof(Index)); // I Will Handle This In Future
            }

            var category = await _unitOfWork.Category.Get(c => c.Id == currentProduct.CategoryId);
            currentProduct.Category = category;

            ShoppingCart shoppingCart = new()
            {
                Product = currentProduct,
                Count = 1,
                ProductId = productId
            };

            return View(shoppingCart);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Details(ShoppingCart shoppingCart)
        {
            //  Check if the product exists
            var product = await _unitOfWork.Product.Get(p => p.Id == shoppingCart.ProductId);
            if (product == null)
            {
                TempData["Error"] = "Invalid product!";
                return RedirectToAction(nameof(Index));
            }

            //  Get the currently logged-in user ID
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;
            shoppingCart.ApplicationUserId = userId;

            //  Check if this product already exists in the user's cart
            var existingCart = await _unitOfWork.ShoppingCart.Get(
                c => c.ApplicationUserId == userId && c.ProductId == shoppingCart.ProductId);

            if (existingCart != null)
            {
                // Product already exists → increase quantity
                existingCart.Count += shoppingCart.Count;
                _unitOfWork.ShoppingCart.Update(existingCart);
                TempData["Success"] = "Cart Updated Successfully!";
                await _unitOfWork.Save();

            }
            else
            {
                // Product not in cart → add a new one
                await _unitOfWork.ShoppingCart.Add(shoppingCart);
                await _unitOfWork.Save();

                // Update Session Cart Count
                var allCarts = await _unitOfWork.ShoppingCart.GetAll(c => c.ApplicationUserId == userId);
                HttpContext.Session.SetInt32(SD.SessionCart,
                   allCarts.Count());
            }
            TempData["Success"] = "Product added to cart successfully!";



            return RedirectToAction(nameof(Index));
        }



        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
