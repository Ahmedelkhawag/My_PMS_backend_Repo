using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PMS.Domain.Constants;
using PMS.Domain.Entities;
using PMS.Domain.Enums;
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

            // 3. زراعة / تحديث الـ SuperAdmin
            const string superAdminPassword = "123";

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
                var result = await userManager.CreateAsync(defaultUser, superAdminPassword);

                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(defaultUser, Roles.SuperAdmin);
                }
                else
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    throw new Exception($"فشل إنشاء الـ SuperAdmin: {errors}");
                }
            }
            else
            {
                // تأكد من أن باسورد الـ SuperAdmin الحالي هو 123 في كل بيئة
                var resetToken = await userManager.GeneratePasswordResetTokenAsync(user);
                var resetResult = await userManager.ResetPasswordAsync(user, resetToken, superAdminPassword);

                if (!resetResult.Succeeded)
                {
                    var errors = string.Join(", ", resetResult.Errors.Select(e => e.Description));
                    throw new Exception($"فشل تحديث باسورد الـ SuperAdmin: {errors}");
                }
            }

            // 4. Seed initial BusinessDay if none exists
            if (!await context.BusinessDays.AnyAsync())
            {
                var today = DateTime.UtcNow.Date;

                await context.BusinessDays.AddAsync(new BusinessDay
                {
                    Date = today,
                    Status = BusinessDayStatus.Open,
                    StartedAt = DateTime.UtcNow,
                    EndedAt = null,
                    ClosedById = null
                });

                await context.SaveChangesAsync();
            }
        }
    }
}

