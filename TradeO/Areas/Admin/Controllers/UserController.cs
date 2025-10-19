using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using TradeO.DataAccess.Data;
using TradeO.DataAccess.Repository.IRepository;
using TradeO.Models;
using TradeO.Models.ViewModels;
using TradeO.Utility;

namespace TradeO.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class UserController : Controller
    {
        

        // Changed field type to UserManager<IdentityUser> as requested.
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IUnitOfWork _unitOfWork;

        // Corrected constructor parameter to match the field type for compilation
        public UserController(UserManager<IdentityUser> userManager, IUnitOfWork unitOfWork, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _unitOfWork = unitOfWork;
            _roleManager = roleManager;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            // NEW: Fetch all users using UnitOfWork, including Company.
            // This returns IEnumerable<ApplicationUser>.
            var allUsers = await _unitOfWork.ApplicationUser.GetAll(IncludeProperties: "Company");

            // Loop through users to assign the Role property
            foreach (var user in allUsers)
            {
                // Casting ApplicationUser to IdentityUser to match the _userManager field type.
                // NOTE: ApplicationUser MUST inherit from IdentityUser for this to work.
                user.Role = (await _userManager.GetRolesAsync(user as IdentityUser)).FirstOrDefault();

                if (user.Company == null)
                {
                    user.Company = new Company() { Name = "" };
                }
            }

            if (allUsers == null || !allUsers.Any())
            {
                TempData["Error"] = "No Users Found.";
                return View(new List<ApplicationUser>());
            }

            return View(allUsers);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LockUnlock(string id)
        {
            var objFromDb = await _unitOfWork.ApplicationUser.Get(u => u.Id == id);

            if (objFromDb == null)
            {
                TempData["Error"] = "User not found.";
                return RedirectToAction(nameof(Index));
            }

            string message;

            // Check LockoutEnd to determine current status
            if (objFromDb.LockoutEnd != null && objFromDb.LockoutEnd > DateTime.Now)
            {
                // Unlock the user
                objFromDb.LockoutEnd = DateTime.UtcNow;
                message = "User Unlocked Successfully!";
            }
            else
            {
                // Lock the user
                objFromDb.LockoutEnd = DateTime.UtcNow.AddYears(1000);
                message = "User Locked Successfully!";
            }

            _unitOfWork.ApplicationUser.Update(objFromDb);
            await _unitOfWork.Save();

            TempData["Success"] = message;
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> RoleManagment(string userId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                TempData["Error"] = "User ID cannot be empty.";
                return RedirectToAction(nameof(Index));
            }

            var applicationUser = await _unitOfWork.ApplicationUser.Get(u => u.Id == userId, IncludeProperties: "Company");

            if (applicationUser == null)
            {
                TempData["Error"] = "User not found.";
                return RedirectToAction(nameof(Index));
            }

            RoleManagmentVM RoleVM = new RoleManagmentVM()
            {
                ApplicationUser = applicationUser,

                RoleList = _roleManager.Roles.Select(i => new SelectListItem
                {
                    Text = i.Name,
                    Value = i.Name
                }),

                // Await the GetAll() Task before applying the Select LINQ method
                CompanyList = (await _unitOfWork.Company.GetAll()).Select(i => new SelectListItem
                {
                    Text = i.Name,
                    Value = i.Id.ToString()
                }),
            };

            // Get the user's current role (Casting to IdentityUser)
            RoleVM.ApplicationUser.Role = (await _userManager.GetRolesAsync(applicationUser as IdentityUser)).FirstOrDefault();

            return View(RoleVM);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RoleManagment(RoleManagmentVM roleManagmentVM)
        {
            ApplicationUser applicationUser = await _unitOfWork.ApplicationUser.Get(u => u.Id == roleManagmentVM.ApplicationUser.Id);

            if (applicationUser == null)
            {
                TempData["Error"] = "User not found during role update.";
                return RedirectToAction(nameof(Index));
            }

            // Get old role (Casting to IdentityUser)
            string oldRole = (await _userManager.GetRolesAsync(applicationUser as IdentityUser)).FirstOrDefault();


            if (!(roleManagmentVM.ApplicationUser.Role == oldRole))
            {
                // Role was changed, update CompanyId based on the new role
                if (roleManagmentVM.ApplicationUser.Role == SD.Role_Company)
                {
                    applicationUser.CompanyId = roleManagmentVM.ApplicationUser.CompanyId;
                }

                // If old role was Company, clear CompanyId
                if (oldRole == SD.Role_Company)
                {
                    applicationUser.CompanyId = null;
                }

                // Update user details and save before changing Identity roles
                _unitOfWork.ApplicationUser.Update(applicationUser);
                await _unitOfWork.Save();

                // Update the roles in Identity (Casting to IdentityUser)
                await _userManager.RemoveFromRoleAsync(applicationUser as IdentityUser, oldRole);
                await _userManager.AddToRoleAsync(applicationUser as IdentityUser, roleManagmentVM.ApplicationUser.Role);

                TempData["Success"] = "User role updated successfully!";
            }
            else
            {
                // Role did not change, check if CompanyId changed for a Company user
                if (oldRole == SD.Role_Company && applicationUser.CompanyId != roleManagmentVM.ApplicationUser.CompanyId)
                {
                    applicationUser.CompanyId = roleManagmentVM.ApplicationUser.CompanyId;
                    _unitOfWork.ApplicationUser.Update(applicationUser);
                    await _unitOfWork.Save();
                    TempData["Success"] = "User Company updated successfully!";
                }
                else
                {
                    TempData["Error"] = "No changes were made to the user's role or company.";
                }
            }

            return RedirectToAction(nameof(Index));
        }
    }
}