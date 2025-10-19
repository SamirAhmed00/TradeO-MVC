using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TradeO.DataAccess.Data;
using TradeO.Models;
using TradeO.Utility;

namespace TradeO.DataAccess.DbInitializer
{
    public class DbInitializer : IDbInitializer
    {

        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationDbContext _dbcontext;

        public DbInitializer(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager, ApplicationDbContext dbContext)
        {
            _roleManager = roleManager;
            _userManager = userManager;
            _dbcontext = dbContext;

        }


        public void Initialize()
        {
            try
            {
                if (_dbcontext.Database.GetPendingMigrations().Count() > 0)
                {
                    _dbcontext.Database.Migrate();
                }
            }
            catch (Exception ex)
            {
                throw;
            }


            // Create Roles if they are not created
            if (!_roleManager.RoleExistsAsync(SD.Role_Customer).GetAwaiter().GetResult())
            {
                _roleManager.CreateAsync(new IdentityRole(SD.Role_Customer)).GetAwaiter().GetResult();
                _roleManager.CreateAsync(new IdentityRole(SD.Role_Company)).GetAwaiter().GetResult();
                _roleManager.CreateAsync(new IdentityRole(SD.Role_Admin)).GetAwaiter().GetResult();
                _roleManager.CreateAsync(new IdentityRole(SD.Role_Employee)).GetAwaiter().GetResult();
                
                
                // Create Admin User if not created
                _userManager.CreateAsync(new ApplicationUser
                {
                    UserName = "Samir_Admin@Gmail.com",
                    Email = "Samir_Admin@Gmail.com",
                    Name = "Samir Ahmed",
                    PhoneNumber = "(+20)01277040276",
                    StreetAddress = "20 |St,",
                    State = "Alexandria",
                    PostalCode = "00000",
                    City = "Alexandria"
                }, "Admin_Samir123").GetAwaiter().GetResult();

                ApplicationUser user = _dbcontext.ApplicationUsers.FirstOrDefault(u => u.Email == "Samir_Admin@Gmail.com");
                _userManager.AddToRoleAsync(user, SD.Role_Admin).GetAwaiter().GetResult();
            }

            return;
        } }

}
