using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stripe.Checkout;
using System.Security.Claims;
using System.Threading.Tasks;
using TradeO.DataAccess.Repository;
using TradeO.DataAccess.Repository.IRepository;
using TradeO.Models;
using TradeO.Models.ViewModels;
using TradeO.Utility;

namespace TradeO.Areas.Customer.Controllers
{
    [Area("Customer")]
    [Authorize]
    public class CartController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        [BindProperty]
        public ShoppingCartVM ShoppingCartVM { get; set; }
        public CartController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var ClaimsIdentity = (ClaimsIdentity)User.Identity;
            var UserId = ClaimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            var allShoppingCart = await _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == UserId, IncludeProperties: "Product");

            ShoppingCartVM = new ShoppingCartVM()
            {
                ShoppingCartList = allShoppingCart,
                OrderHeader = new OrderHeader()
            };

            //  Calculate total price properly
            foreach (var cart in ShoppingCartVM.ShoppingCartList)
            {
                cart.Price = cart.Product.DiscountPrice ?? cart.Product.Price; // Prefer discount if available
                ShoppingCartVM.OrderHeader.OrderTotal+= cart.Price * cart.Count;
            }

            return View(ShoppingCartVM);
        }

        [HttpGet]
        public async Task<IActionResult> Summary()
        {

            var ClaimsIdentity = (ClaimsIdentity)User.Identity;
            var UserId = ClaimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            var allShoppingCart = await _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == UserId, IncludeProperties: "Product");

            ShoppingCartVM = new ShoppingCartVM()
            {
                ShoppingCartList = allShoppingCart,
                OrderHeader = new OrderHeader()
            };

            ShoppingCartVM.OrderHeader.ApplicationUser = await _unitOfWork.ApplicationUser.Get(u => u.Id == UserId);
            ShoppingCartVM.OrderHeader.Name = ShoppingCartVM.OrderHeader.ApplicationUser.Name;
            ShoppingCartVM.OrderHeader.PhoneNumber = ShoppingCartVM.OrderHeader.ApplicationUser.PhoneNumber;
            ShoppingCartVM.OrderHeader.StreetAddress = ShoppingCartVM.OrderHeader.ApplicationUser.StreetAddress;
            ShoppingCartVM.OrderHeader.City = ShoppingCartVM.OrderHeader.ApplicationUser.City;
            ShoppingCartVM.OrderHeader.State = ShoppingCartVM.OrderHeader.ApplicationUser.State;
            ShoppingCartVM.OrderHeader.PostalCode = ShoppingCartVM.OrderHeader.ApplicationUser.PostalCode;



            //  Calculate total price properly
            foreach (var cart in ShoppingCartVM.ShoppingCartList)
            {
                cart.Price = cart.Product.DiscountPrice ?? cart.Product.Price; // Prefer discount if available
                ShoppingCartVM.OrderHeader.OrderTotal += cart.Price * cart.Count;
            }
            return View(ShoppingCartVM);
        }


        [HttpPost]
        public async Task<IActionResult> Summary(ShoppingCartVM shoppingCartVM)
        {
            var ClaimsIdentity = (ClaimsIdentity)User.Identity;
            var UserId = ClaimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            var allShoppingCart = await _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == UserId, IncludeProperties: "Product");

            ShoppingCartVM.ShoppingCartList = allShoppingCart;
            ShoppingCartVM.OrderHeader.OrderDate = System.DateTime.Now;
            ShoppingCartVM.OrderHeader.ApplicationUserId = UserId;
            ApplicationUser applicationUser = await _unitOfWork.ApplicationUser.Get(u => u.Id == UserId);

            //  Calculate total price properly
            foreach (var cart in ShoppingCartVM.ShoppingCartList)
            {
                cart.Price = cart.Product.DiscountPrice ?? cart.Product.Price; // Prefer discount if available
                ShoppingCartVM.OrderHeader.OrderTotal += cart.Price * cart.Count;
            }

            // Check if the ShoppingCart For Company
            if (applicationUser.CompanyId.GetValueOrDefault() == 0)
            {
                // Individual User
                ShoppingCartVM.OrderHeader.PaymentStatus = SD.StatusPending;
                ShoppingCartVM.OrderHeader.OrderStatus = SD.StatusPending;
            }
            else
            {
                // Company User
                ShoppingCartVM.OrderHeader.PaymentStatus = SD.PaymentStatusDelayedPayment;
                ShoppingCartVM.OrderHeader.OrderStatus = SD.StatusApproved;
            }

            // Create And Save OrderHeader
            await _unitOfWork.OrderHeader.Add(ShoppingCartVM.OrderHeader);
            await _unitOfWork.Save();

            // Create And Save OrderDetails For Each Product
            foreach (var cart in ShoppingCartVM.ShoppingCartList)
            {
                OrderDetail orderDetail = new()
                {
                    ProductId = cart.ProductId,
                    OrderHeaderId = ShoppingCartVM.OrderHeader.Id,
                    Price = cart.Price,
                    Count = cart.Count
                };
                await _unitOfWork.OrderDetail.Add(orderDetail);
                await _unitOfWork.Save();
            }


            // Adding Stripe Logic 

            if (applicationUser.CompanyId.GetValueOrDefault() == 0)
            {
                // Individual User need To Capture Payment

                // Stripe Logic Here 
                var DOMAIN = "http://192.168.1.7:5009/";
                var options = new Stripe.Checkout.SessionCreateOptions
                {
                    SuccessUrl = DOMAIN+ $"Customer/Cart/OrderConfirmation?id={ShoppingCartVM.OrderHeader.Id}",
                    CancelUrl = DOMAIN + "Customer/Cart/Index",
                    LineItems = new List<Stripe.Checkout.SessionLineItemOptions>(),
                    Mode = "payment",
                };

                foreach (var item in ShoppingCartVM.ShoppingCartList)
                {
                    var sessionLineItem = new Stripe.Checkout.SessionLineItemOptions
                    {
                        PriceData = new Stripe.Checkout.SessionLineItemPriceDataOptions
                        {
                            UnitAmount = (long)(item.Price * 100), // Convert to cents
                            Currency = "usd",
                            ProductData = new Stripe.Checkout.SessionLineItemPriceDataProductDataOptions
                            {
                                Name = item.Product.Name,
                            },
                        },
                        Quantity = item.Count,
                    };
                    options.LineItems.Add(sessionLineItem);
                }

                var service = new Stripe.Checkout.SessionService();
                Stripe.Checkout.Session session = service.Create(options);
                _unitOfWork.OrderHeader.UpdateStripePaymentId(ShoppingCartVM.OrderHeader.Id, session.Id, session.PaymentIntentId);
                await _unitOfWork.Save();
                Response.Headers.Add("Location", session.Url);
                return new StatusCodeResult(303);
            }

            

            return RedirectToAction(nameof(OrderConfirmation), new { id = ShoppingCartVM.OrderHeader.Id});
        }


        [HttpGet]
        public async Task<IActionResult> OrderConfirmation(int id)
        {
            OrderHeader orderHeader = await _unitOfWork.OrderHeader.Get(u => u.Id == id, IncludeProperties: "ApplicationUser");
            if (orderHeader == null)
            {
                TempData["Error"] = "No Orders Found!";
                return RedirectToAction(nameof(Index));
            }
            if (orderHeader.PaymentStatus != SD.PaymentStatusDelayedPayment)
            {
                // Order For Customer , Not For Company 
                var service = new SessionService();
                //var service = new Stripe.Checkout.SessionService();
                Session session = service.Get(orderHeader.SessionId);
                // Check The Payment Status
                if (session.PaymentStatus.ToLower() == "paid")
                {
                    _unitOfWork.OrderHeader.UpdateStripePaymentId(id, session.Id, session.PaymentIntentId);
                    _unitOfWork.OrderHeader.UpdateStatus(id, SD.StatusApproved, SD.PaymentStatusApproved);
                    await _unitOfWork.Save();
                }
                HttpContext.Session.Clear();

            }
            List<ShoppingCart> shoppingCarts = (List<ShoppingCart>)await _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == orderHeader.ApplicationUserId);

            // Clear The Shopping Cart
            _unitOfWork.ShoppingCart.RemoveRange(shoppingCarts);
            await _unitOfWork.Save();

            return View(id);
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int cartId)
        {

            var productInShoppingCart = await _unitOfWork.ShoppingCart.Get(p => p.Id == cartId);

            if (productInShoppingCart != null)
            {
                HttpContext.Session.SetInt32(SD.SessionCart,
                    (await _unitOfWork.ShoppingCart.GetAll(
                        c => c.ApplicationUserId == productInShoppingCart.ApplicationUserId)).Count() - 1 );
                _unitOfWork.ShoppingCart.Remove(productInShoppingCart);
                await _unitOfWork.Save();
            }

            return RedirectToAction(nameof(Index));
        }

        // Increase Quantity
        [HttpPost]
        [ValidateAntiForgeryToken]

        public async Task<IActionResult> Increase(int id)
        {
            var cartItem = await _unitOfWork.ShoppingCart.Get(u => u.Id == id);
            if (cartItem != null)
            {
                cartItem.Count += 1;
                _unitOfWork.ShoppingCart.Update(cartItem);
                await _unitOfWork.Save();
            }

            return RedirectToAction(nameof(Index));
        }

        //  Decrease Quantity
        [HttpPost]
        [ValidateAntiForgeryToken]

        public async Task<IActionResult> Decrease(int id)
        {
            var cartItem = await _unitOfWork.ShoppingCart.Get(u => u.Id == id);
            if (cartItem != null)
            {
                if (cartItem.Count > 1)
                {
                    cartItem.Count -= 1;
                    _unitOfWork.ShoppingCart.Update(cartItem);
                }
                else
                {
                    HttpContext.Session.SetInt32(SD.SessionCart,
                    (await _unitOfWork.ShoppingCart.GetAll(
                        c => c.ApplicationUserId == cartItem.ApplicationUserId)).Count() - 1);
                    _unitOfWork.ShoppingCart.Remove(cartItem);
                }

                await _unitOfWork.Save();
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
