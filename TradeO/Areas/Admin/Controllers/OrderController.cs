using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.Blazor;
using Stripe;
using Stripe.Checkout;
using Stripe.Climate;
using System.Security.Claims;
using System.Threading.Tasks;
using TradeO.DataAccess.Repository.IRepository;
using TradeO.Models;
using TradeO.Models.ViewModels;
using TradeO.Utility;

namespace TradeO.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize]
    public class OrderController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        [BindProperty]
        public OrderVM orderVM { get; set; }

        public OrderController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        [HttpGet]
        public async Task<IActionResult> Index(string? status)
        {
            IEnumerable<OrderHeader> orderHeaders;

            if (User.IsInRole(SD.Role_Admin) || User.IsInRole(SD.Role_Employee))
            {
                orderHeaders = await _unitOfWork.OrderHeader.GetAll(IncludeProperties: "ApplicationUser");
            }
            else
            {
                var ClaimsIdentity = (ClaimsIdentity)User.Identity;
                var UserId = ClaimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

                orderHeaders = await _unitOfWork.OrderHeader.GetAll(u => u.ApplicationUserId == UserId, IncludeProperties: "ApplicationUser");

            }


            // Filtering
            if (!string.IsNullOrEmpty(status) && status.ToLower() != "all")
            {
                orderHeaders = orderHeaders.Where(o =>
                    o.OrderStatus != null &&
                    o.OrderStatus.Equals(status, StringComparison.OrdinalIgnoreCase)
                );
            }


            ViewBag.CurrentStatus = status ?? "All";

            return View(orderHeaders);
        }

        [HttpGet]
        public async Task<IActionResult> Details(int orderId)
        {
            orderVM = new()
            {
                OrderHeader = await _unitOfWork.OrderHeader.Get(
                    o => o.Id == orderId,
                    IncludeProperties: "ApplicationUser"
                ),
                OrderDetail = await _unitOfWork.OrderDetail.GetAll(
                o => o.OrderHeaderId == orderId,
                IncludeProperties: "Product"
                )
            };



            // Ensure dates are set to current date if they are uninitialized
            if (orderVM.OrderHeader.ShippingDate.Year < 1900)
                orderVM.OrderHeader.ShippingDate = DateTime.Now;

            if (orderVM.OrderHeader.PaymentDate.Year < 1900)
                orderVM.OrderHeader.PaymentDate = DateTime.Now;


            if (orderVM.OrderHeader.PaymentDueDate.Year < 1900)
                orderVM.OrderHeader.PaymentDueDate = DateOnly.FromDateTime(DateTime.Now);


            return View(orderVM);
        }


        // This Action For Company Payment Only
        [HttpPost]
        public async Task<IActionResult> Details()
        {
            orderVM.OrderHeader = await _unitOfWork.OrderHeader.Get(
                   o => o.Id == orderVM.OrderHeader.Id,
                   IncludeProperties: "ApplicationUser"
               );
            orderVM.OrderDetail = await _unitOfWork.OrderDetail.GetAll(
                o => o.OrderHeaderId == orderVM.OrderHeader.Id,
                IncludeProperties: "Product"
                );


            // Stripe Logic Here 
            var DOMAIN = "http://192.168.1.7:5009/";
            var options = new Stripe.Checkout.SessionCreateOptions
            {
                SuccessUrl = DOMAIN + $"Admin/Order/PaymentConfirmation?OrderHeaderId={orderVM.OrderHeader.Id}",
                CancelUrl = DOMAIN + $"Admin/Order/Details?orderId={orderVM.OrderHeader.Id}",
                LineItems = new List<Stripe.Checkout.SessionLineItemOptions>(),
                Mode = "payment",
            };

            foreach (var item in orderVM.OrderDetail)
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
            _unitOfWork.OrderHeader.UpdateStripePaymentId(orderVM.OrderHeader.Id, session.Id, session.PaymentIntentId);
            await _unitOfWork.Save();
            Response.Headers.Add("Location", session.Url);
            return new StatusCodeResult(303);
        }


        // PaymentConformation For Company Pay
        [HttpGet]
        public async Task<IActionResult> PaymentConfirmation(int OrderHeaderId)
        {
            OrderHeader orderHeader = await _unitOfWork.OrderHeader.Get(u => u.Id == OrderHeaderId);
            if (orderHeader == null)
            {
                TempData["Error"] = "No Orders Found!";
                return RedirectToAction(nameof(Index));
            }
            if (orderHeader.PaymentStatus == SD.PaymentStatusDelayedPayment)
            {
                // Order For Company 
                var service = new SessionService();
                Session session = service.Get(orderHeader.SessionId);


                // Check The Payment Status
                if (session.PaymentStatus.ToLower() == "paid")
                {
                    _unitOfWork.OrderHeader.UpdateStripePaymentId(OrderHeaderId, session.Id, session.PaymentIntentId);
                    _unitOfWork.OrderHeader.UpdateStatus(OrderHeaderId, orderHeader.OrderStatus , SD.PaymentStatusApproved);
                    await _unitOfWork.Save();
                }

            }

            //List<ShoppingCart> shoppingCarts = (List<ShoppingCart>)await _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == orderHeader.ApplicationUserId);

            //// Clear The Shopping Cart
            //_unitOfWork.ShoppingCart.RemoveRange(shoppingCarts);
            //await _unitOfWork.Save();

            return View(OrderHeaderId);
        }

        [HttpPost]
        [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
        public async Task<IActionResult> UpdateOrderDetail()
        {
            var orderHeaderFromDb = await _unitOfWork.OrderHeader.Get(o => o.Id == orderVM.OrderHeader.Id);

            orderHeaderFromDb.Name = orderVM.OrderHeader.Name;
            orderHeaderFromDb.PhoneNumber = orderVM.OrderHeader.PhoneNumber;
            orderHeaderFromDb.StreetAddress = orderVM.OrderHeader.StreetAddress;
            orderHeaderFromDb.City = orderVM.OrderHeader.City;
            orderHeaderFromDb.State = orderVM.OrderHeader.State;
            orderHeaderFromDb.PostalCode = orderVM.OrderHeader.PostalCode;

            // Update Those If They Are Null Or Empty
            if (!string.IsNullOrEmpty(orderVM.OrderHeader.Carrier))
            {
                orderHeaderFromDb.Carrier = orderVM.OrderHeader.Carrier;
            }
            if (!string.IsNullOrEmpty(orderVM.OrderHeader.TrackingNumber))
            {
                orderHeaderFromDb.TrackingNumber = orderVM.OrderHeader.TrackingNumber;
            }
            _unitOfWork.OrderHeader.Update(orderHeaderFromDb);
            await _unitOfWork.Save();

            TempData["Success"] = "Order Details Updated Successfully.";

            return RedirectToAction(nameof(Details), new { orderId = orderHeaderFromDb.Id });
        }


        

        [HttpPost]
        [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
        public async Task<IActionResult> StartProcessing()
        {
            _unitOfWork.OrderHeader.UpdateStatus(orderVM.OrderHeader.Id, SD.StatusInProcess);
            await _unitOfWork.Save();
            TempData["Success"] = "Order Details Updated Successfully.";
            return RedirectToAction(nameof(Details), new { orderId = orderVM.OrderHeader.Id });
        }

        [HttpPost]
        [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
        public async Task<IActionResult> ShipOrder()
        {

            var orderHeader = await _unitOfWork.OrderHeader.Get(u => u.Id == orderVM.OrderHeader.Id);
            orderHeader.TrackingNumber = orderVM.OrderHeader.TrackingNumber;
            orderHeader.Carrier = orderVM.OrderHeader.Carrier;
            orderHeader.OrderStatus = SD.StatusShipped;
            orderHeader.ShippingDate = DateTime.Now;
            if (orderHeader.PaymentStatus == SD.PaymentStatusDelayedPayment)
            {
                orderHeader.PaymentDueDate = DateOnly.FromDateTime(DateTime.Now.AddDays(30));
            }

            _unitOfWork.OrderHeader.Update(orderHeader);
            await _unitOfWork.Save();
            TempData["Success"] = "Order Shipped Successfully.";
            return RedirectToAction(nameof(Details), new { orderId = orderVM.OrderHeader.Id });
        }

        [HttpPost]
        [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
        public async Task<IActionResult> CancelOrder()
        {

            var orderHeader = await _unitOfWork.OrderHeader.Get(u => u.Id == orderVM.OrderHeader.Id);

            if (orderHeader.PaymentStatus == SD.PaymentStatusApproved)
            {
                var options = new RefundCreateOptions
                {
                    Reason = RefundReasons.RequestedByCustomer,
                    PaymentIntent = orderHeader.PaymentIntentId
                };

                var service = new RefundService();
                Refund refund = service.Create(options);

                _unitOfWork.OrderHeader.UpdateStatus(orderHeader.Id, SD.StatusCancelled, SD.StatusRefunded);
            }
            else
            {
                _unitOfWork.OrderHeader.UpdateStatus(orderHeader.Id, SD.StatusCancelled, SD.StatusCancelled);
            }
            await _unitOfWork.Save();
            TempData["Success"] = "Order Cancelled Successfully.";
            return RedirectToAction(nameof(Details), new { orderId = orderVM.OrderHeader.Id });

        }


        [HttpPost]
        public async Task<IActionResult> Delete(int OrderId)
        {
            var OrderheaderFromDb = await _unitOfWork.OrderHeader.Get(o => o.Id == OrderId);

            if (OrderheaderFromDb != null)
            {
                _unitOfWork.OrderHeader.Remove(OrderheaderFromDb);
                await _unitOfWork.Save();
            }
            TempData["Success"] = "Order Deleted Successfully.";

            return RedirectToAction(nameof(Index));
        }
        // After Paying With Stripe rmemebr to تتابع الاورد عبر ده 
        // Admin/Order/Details? orderId = 5
    }
}
