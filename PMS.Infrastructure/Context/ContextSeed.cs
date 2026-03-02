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

            const string SystemUserId = "b74ddd14-6340-4840-95c2-db12554843e5"; // Fixed GUID
            var systemUser = await userManager.FindByIdAsync(SystemUserId);

            if (systemUser == null)
            {
                systemUser = new AppUser
                {
                    Id = SystemUserId, // بنثبت الـ Id عشان نستخدمه في الكود
                    UserName = "system_auto",
                    Email = "system@pms.com",
                    FullName = "System Automator",
                    Nationality = "System",
                    NationalId = "00000000000000",
                    EmailConfirmed = true,
                    StatusID = await context.Statuses.Where(s => s.Name == "Active").Select(s => s.StatusID).FirstOrDefaultAsync()
                };
                await userManager.CreateAsync(systemUser, "System@Password123"); // باسورد معقد بس مش هنستخدمه
            }


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
        public static async Task SeedRoomsAsync(ApplicationDbContext context)
        {

            const string SystemUserId = "b74ddd14-6340-4840-95c2-db12554843e5";

            var roomTypes = await context.RoomTypes.ToListAsync();
            if (roomTypes.Count == 0) return;

            // 2. هنجيب كل أرقام الغرف اللي موجودة حالياً عشان مكررش حاجة
            var existingRoomNumbers = await context.Rooms.Select(r => r.RoomNumber).ToListAsync();

            var roomsToAdd = new List<Room>();
            int typesCount = roomTypes.Count;
            int typeIndex = 0;

            // إعدادات الأدوار (5 أدوار وكل دور فيه 10 غرف)
            int floorNumber = 1;
            int roomsPerFloor = 10;
            int totalTarget = 50;
            int createdCount = 0;

            while (createdCount < totalTarget)
            {
                for (int i = 1; i <= roomsPerFloor && createdCount < totalTarget; i++)
                {
                    var roomNum = $"{floorNumber}{i:D2}";

                    // 👇 الـ Check السحري: لو الغرفة مش موجودة ضيفها
                    if (!existingRoomNumbers.Contains(roomNum))
                    {
                        var roomType = roomTypes[typeIndex % typesCount];

                        var room = new Room
                        {
                            RoomNumber = roomNum,
                            FloorNumber = floorNumber,
                            RoomTypeId = roomType.Id,
                            HKStatus = HKStatus.Clean,
                            FOStatus = FOStatus.Vacant,
                            RoomStatusId = 1, // Clean
                            IsActive = true,
                            MaxAdults = roomType.MaxAdults,
                            BasePrice = roomType.BasePrice,
                            Notes = "Seeded by System",
                            // سيتم تعيين CreatedBy و CreatedAt أوتوماتيكياً في الـ DbContext
                        };

                        // حالة خاصة لتيست الـ OOO (آخر غرفتين في الـ Seed)
                        if (createdCount >= 48)
                        {
                            room.HKStatus = HKStatus.OOO;
                            room.RoomStatusId = 3;
                            room.MaintenanceReason = "Periodic Check";
                            room.MaintenanceStartDate = DateTime.UtcNow.AddDays(-1);
                            room.MaintenanceEndDate = DateTime.UtcNow.AddDays(2);
                        }

                        roomsToAdd.Add(room);
                    }

                    createdCount++;
                    typeIndex++;
                }
                floorNumber++;
            }

            if (roomsToAdd.Any())
            {
                await context.Rooms.AddRangeAsync(roomsToAdd);

                // 💡 ملحوظة "سنيور": بما إن SaveChangesAsync بتمسح الـ CreatedBy وتكتب "System"
                // إحنا هنعتمد إن الـ DbContext هيقوم بمهمته، 
                // ولو عاوز تغير "System" لـ GUID ثابت، لازم نعدل الـ DbContext نفسه.
                await context.SaveChangesAsync();
            }
        }
    }
}

