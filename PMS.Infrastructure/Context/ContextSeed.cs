using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PMS.Domain.Constants;
using PMS.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace PMS.Infrastructure.Context
{
    public static class ContextSeed
    {
        public static async Task SeedEssentialsAsync(UserManager<AppUser> userManager, RoleManager<IdentityRole> roleManager, ApplicationDbContext context)
        {
            // 1. زراعة الـ Statuses
            if (!await context.Statuses.AnyAsync())
            {
                await context.Statuses.AddRangeAsync(
                    new Status { StatusID = Guid.NewGuid(), Name = "Active" },
                    new Status { StatusID = Guid.NewGuid(), Name = "Inactive" },
                    new Status { StatusID = Guid.NewGuid(), Name = "Suspended" }
                );
                await context.SaveChangesAsync();
            }

            // 2. زراعة الرولز
            string[] roles = {
                Roles.SuperAdmin,
                Roles.IT,
                Roles.HotelManager,
                Roles.Receptionist,
                Roles.Accountant,
                Roles.HouseKeeping
            };

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            // 3. زراعة الـ SuperAdmin
            var defaultUser = new AppUser
            {
                UserName = "admin",
                Email = "admin@pms.com",
                FullName = "Super Admin User",
                Nationality = "Egyptian",
                EmailConfirmed = true,
                PhoneNumberConfirmed = true,
                NationalId = "12345678901234",
                PhoneNumber = "1234567890",
                CountryID = null,
                StatusID = await context.Statuses.Where(s => s.Name == "Active").Select(s => s.StatusID).FirstOrDefaultAsync()
            };

            var user = await userManager.FindByNameAsync(defaultUser.UserName);
            if (user == null)
            {
                var result = await userManager.CreateAsync(defaultUser, "P@ssword123");

                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(defaultUser, Roles.SuperAdmin);
                }
                else
                {
                    // --- هنا التعديل المهم ---
                    // هنجمع كل الأخطاء ونرميها في وشنا عشان نعرف السبب
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    throw new Exception($"فشل إنشاء الـ SuperAdmin: {errors}");
                }
            }
        }
    }
}

