using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using System.ComponentModel.Design;
using System.Threading.Tasks;
using TradeO.DataAccess.Repository.IRepository;
using TradeO.Models;
using TradeO.Models.ViewModels;
using TradeO.Utility;

namespace TradeO.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class CompanyController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

       
       

        public CompanyController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {            
            var allCompanys = await _unitOfWork.Company.GetAll();

            if (allCompanys == null || !allCompanys.Any())
            {
                ViewBag.Message = "No Companys Found.";
                return View(new List<Company>());
            }
            return View(allCompanys);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {         
            return View(new Company());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Company newCompany)
        {
            // Check Model State Valid Or Not
            if (!ModelState.IsValid)
            {
                ViewBag.Message = "Invalid request data!";
                TempData["Error"] = "Something went wrong while Creating the Company!";
                return View(newCompany);
            }

            // Check If Company Name Is Exist ? - (For unique Company Name)
            var existingCompany = await _unitOfWork.Company.Get(p => p.Name == newCompany.Name);
            if (existingCompany != null)
            {
                ModelState.AddModelError("Name", "Company with the same name already exists!");
                return View(newCompany);
            }

            await _unitOfWork.Company.Add(newCompany);
            await _unitOfWork.Save();

            TempData["Success"] = "Company created successfully!";

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int CompanyId)
        {
            if (CompanyId <= 0)
            {
                ViewBag.Message = "Company not found. ID can't be negative or zero.";
                return View("NotFound"); // I Will Handle This In Future
            }

            var currentCompany = await _unitOfWork.Company.Get(p => p.Id == CompanyId);

            if (currentCompany == null)
            {
                ViewBag.Message = "Company not found!";
                TempData["Error"] = "Something went wrong while retrieving the Company!";
                return View("NotFound"); // I Will Handle This In Future
            }

            
            return View(currentCompany);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Company updatedCompany)
        {

            if (!ModelState.IsValid)
            {
                return View(updatedCompany);
            }

            var existingCompany = await _unitOfWork.Company.Get(c => c.Id == updatedCompany.Id);

            if (existingCompany == null)
            {                
                ViewBag.Message = "Company not found!";
                TempData["Error"] = "Something went wrong while updating the Company!";
                return View(updatedCompany);
            }

            // Check if another Company has the same name
            var duplicateCompany = await _unitOfWork.Company.Get(
                c => c.Name == updatedCompany.Name
                  && c.Id != updatedCompany.Id
            );

            if (duplicateCompany != null)
            {
                ModelState.AddModelError("Name", "A Company with the same name already exists for this seller!");
                return View(updatedCompany);
            }


            // Update only editable fields
            existingCompany.Name = updatedCompany.Name;
            existingCompany.StreetAddress = updatedCompany.StreetAddress;
            existingCompany.City = updatedCompany.City;
            existingCompany.State = updatedCompany.State;
            existingCompany.PostalCode = updatedCompany.PostalCode;
            existingCompany.PhoneNumber = updatedCompany.PhoneNumber;


            
            await _unitOfWork.Save();

            TempData["Success"] = "Company updated successfully!";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int CompanyId)
        {
            var CompanyFromDb = await _unitOfWork.Company.Get(p => p.Id == CompanyId);

            if (CompanyFromDb == null)
            {
                TempData["Error"] = "Company not found or already deleted!";
                return RedirectToAction(nameof(Index));
            }


            _unitOfWork.Company.Remove(CompanyFromDb);
            await _unitOfWork.Save();

            TempData["Success"] = "Company deleted successfully!";
            return RedirectToAction(nameof(Index));
        }


        [HttpGet]
        public async Task<IActionResult> Details(int CompanyId)
        {
            if (CompanyId <= 0)
            {
                ViewBag.Message = "Company not found. ID can't be negative or zero.";
                return View(nameof(Index)); // I Will Handle This In Future
            }
            var currentCompany = await _unitOfWork.Company.Get(c => c.Id == CompanyId);
            if (currentCompany == null)
            {
                ViewBag.Message = "Company not found!";
                TempData["Error"] = "Something went wrong while retrieving the Company!";
                return View("NotFound"); // I Will Handle This In Future
            }
            return View(currentCompany);
        }



    }
}
