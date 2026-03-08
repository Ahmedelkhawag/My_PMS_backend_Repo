using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PMS.Domain.Constants;
using PMS.Domain.Entities;
using PMS.Domain.Entities.BackOffice;
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
                    Id = SystemUserId, 
                    UserName = "system_auto",
                    Email = "system@pms.com",
                    FullName = "System Automator",
                    Nationality = "System",
                    NationalId = "00000000000000",
                    EmailConfirmed = true,
                    StatusID = await context.Statuses.Where(s => s.Name == "Active").Select(s => s.StatusID).FirstOrDefaultAsync()
                };
                await userManager.CreateAsync(systemUser, "System@Password123"); 
            }


            
            if (!await context.Statuses.AnyAsync())
            {
                await context.Statuses.AddRangeAsync(
                    new Status { StatusID = Guid.NewGuid(), Name = "Active" },
                    new Status { StatusID = Guid.NewGuid(), Name = "Inactive" },
                    new Status { StatusID = Guid.NewGuid(), Name = "Suspended" }
                );
                await context.SaveChangesAsync();
            }

            
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

            
            var existingRoomNumbers = await context.Rooms.Select(r => r.RoomNumber).ToListAsync();

            var roomsToAdd = new List<Room>();
            int typesCount = roomTypes.Count;
            int typeIndex = 0;

            
            int floorNumber = 1;
            int roomsPerFloor = 10;
            int totalTarget = 50;
            int createdCount = 0;

            while (createdCount < totalTarget)
            {
                for (int i = 1; i <= roomsPerFloor && createdCount < totalTarget; i++)
                {
                    var roomNum = $"{floorNumber}{i:D2}";

                    
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
                            
                        };

                        
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

                await context.SaveChangesAsync();
            }
        }

        public static async Task SeedAccountsAsync(ApplicationDbContext context)
        {
            if (await context.Accounts.AnyAsync())
            {
                return;
            }

            using var transaction = await context.Database.BeginTransactionAsync();
            try
            {
                await context.Database.ExecuteSqlRawAsync("SET IDENTITY_INSERT Accounts ON;");

                var now = DateTime.UtcNow;

                var accounts = new List<Account>
                {
                    // 1. ASSETS
                    new Account { Id = 1, Code = "1", NameAr = "الأصول", NameEn = "Assets", Type = Domain.Enums.BackOffice.AccountType.Asset, IsGroup = true, CurrentBalance = 0m, IsActive = true, CreatedAt = now, CreatedBy = "System" },
                    new Account { Id = 11, Code = "11", NameAr = "الأصول المتداولة", NameEn = "Current Assets", Type = Domain.Enums.BackOffice.AccountType.Asset, ParentAccountId = 1, IsGroup = true, CurrentBalance = 0m, IsActive = true, CreatedAt = now, CreatedBy = "System" },
                    new Account { Id = 111, Code = "111", NameAr = "النقدية", NameEn = "Cash", Type = Domain.Enums.BackOffice.AccountType.Asset, ParentAccountId = 11, IsGroup = true, CurrentBalance = 0m, IsActive = true, CreatedAt = now, CreatedBy = "System" },
                    new Account { Id = 1111, Code = "1111", NameAr = "صندوق الكاشير الأمامي", NameEn = "Front Office Cashier", Type = Domain.Enums.BackOffice.AccountType.Asset, ParentAccountId = 111, IsGroup = false, CurrentBalance = 0m, IsActive = true, CreatedAt = now, CreatedBy = "System" },
                    new Account { Id = 1112, Code = "1112", NameAr = "الخزنة العامة", NameEn = "General Safe", Type = Domain.Enums.BackOffice.AccountType.Asset, ParentAccountId = 111, IsGroup = false, CurrentBalance = 0m, IsActive = true, CreatedAt = now, CreatedBy = "System" },
                    new Account { Id = 112, Code = "112", NameAr = "البنوك", NameEn = "Banks", Type = Domain.Enums.BackOffice.AccountType.Asset, ParentAccountId = 11, IsGroup = true, CurrentBalance = 0m, IsActive = true, CreatedAt = now, CreatedBy = "System" },
                    new Account { Id = 1121, Code = "1121", NameAr = "الحساب البنكي الرئيسي - محلي", NameEn = "Main Bank Account - Local", Type = Domain.Enums.BackOffice.AccountType.Asset, ParentAccountId = 112, IsGroup = false, CurrentBalance = 0m, IsActive = true, CreatedAt = now, CreatedBy = "System" },
                    new Account { Id = 113, Code = "113", NameAr = "المدينون", NameEn = "Accounts Receivable", Type = Domain.Enums.BackOffice.AccountType.Asset, ParentAccountId = 11, IsGroup = true, CurrentBalance = 0m, IsActive = true, CreatedAt = now, CreatedBy = "System" },
                    new Account { Id = 1131, Code = "1131", NameAr = "المدينون - شركات ووكلاء سياحة", NameEn = "City Ledger (Companies/Travel Agents)", Type = Domain.Enums.BackOffice.AccountType.Asset, ParentAccountId = 113, IsGroup = false, CurrentBalance = 0m, IsActive = true, CreatedAt = now, CreatedBy = "System" },
                    new Account { Id = 1132, Code = "1132", NameAr = "المدينون - النزلاء المقيمون", NameEn = "Guest Ledger (In-house Guests)", Type = Domain.Enums.BackOffice.AccountType.Asset, ParentAccountId = 113, IsGroup = false, CurrentBalance = 0m, IsActive = true, CreatedAt = now, CreatedBy = "System" },

                    // 2. LIABILITIES
                    new Account { Id = 2, Code = "2", NameAr = "الالتزامات", NameEn = "Liabilities", Type = Domain.Enums.BackOffice.AccountType.Liability, IsGroup = true, CurrentBalance = 0m, IsActive = true, CreatedAt = now, CreatedBy = "System" },
                    new Account { Id = 21, Code = "21", NameAr = "الالتزامات المتداولة", NameEn = "Current Liabilities", Type = Domain.Enums.BackOffice.AccountType.Liability, ParentAccountId = 2, IsGroup = true, CurrentBalance = 0m, IsActive = true, CreatedAt = now, CreatedBy = "System" },
                    new Account { Id = 211, Code = "211", NameAr = "الدائنون", NameEn = "Accounts Payable", Type = Domain.Enums.BackOffice.AccountType.Liability, ParentAccountId = 21, IsGroup = true, CurrentBalance = 0m, IsActive = true, CreatedAt = now, CreatedBy = "System" },
                    new Account { Id = 2111, Code = "2111", NameAr = "الدائنون التجاريون (الموردون)", NameEn = "Trade Creditors (Suppliers)", Type = Domain.Enums.BackOffice.AccountType.Liability, ParentAccountId = 211, IsGroup = false, CurrentBalance = 0m, IsActive = true, CreatedAt = now, CreatedBy = "System" },
                    new Account { Id = 212, Code = "212", NameAr = "الضرائب", NameEn = "Taxes", Type = Domain.Enums.BackOffice.AccountType.Liability, ParentAccountId = 21, IsGroup = true, CurrentBalance = 0m, IsActive = true, CreatedAt = now, CreatedBy = "System" },
                    new Account { Id = 2121, Code = "2121", NameAr = "ضريبة القيمة المضافة مستحقة السداد", NameEn = "VAT Payable", Type = Domain.Enums.BackOffice.AccountType.Liability, ParentAccountId = 212, IsGroup = false, CurrentBalance = 0m, IsActive = true, CreatedAt = now, CreatedBy = "System" },

                    // 3. EQUITY
                    new Account { Id = 3, Code = "3", NameAr = "حقوق الملكية", NameEn = "Equity", Type = Domain.Enums.BackOffice.AccountType.Equity, IsGroup = true, CurrentBalance = 0m, IsActive = true, CreatedAt = now, CreatedBy = "System" },
                    new Account { Id = 31, Code = "31", NameAr = "رأس المال", NameEn = "Capital", Type = Domain.Enums.BackOffice.AccountType.Equity, ParentAccountId = 3, IsGroup = true, CurrentBalance = 0m, IsActive = true, CreatedAt = now, CreatedBy = "System" },
                    new Account { Id = 311, Code = "311", NameAr = "حقوق الملكية للمالك", NameEn = "Owner's Equity", Type = Domain.Enums.BackOffice.AccountType.Equity, ParentAccountId = 31, IsGroup = false, CurrentBalance = 0m, IsActive = true, CreatedAt = now, CreatedBy = "System" },

                    // 4. REVENUE
                    new Account { Id = 4, Code = "4", NameAr = "الإيرادات", NameEn = "Revenue", Type = Domain.Enums.BackOffice.AccountType.Revenue, IsGroup = true, CurrentBalance = 0m, IsActive = true, CreatedAt = now, CreatedBy = "System" },
                    new Account { Id = 41, Code = "41", NameAr = "الإيرادات التشغيلية", NameEn = "Operational Revenue", Type = Domain.Enums.BackOffice.AccountType.Revenue, ParentAccountId = 4, IsGroup = true, CurrentBalance = 0m, IsActive = true, CreatedAt = now, CreatedBy = "System" },
                    new Account { Id = 411, Code = "411", NameAr = "إيرادات الغرف", NameEn = "Room Revenue", Type = Domain.Enums.BackOffice.AccountType.Revenue, ParentAccountId = 41, IsGroup = false, CurrentBalance = 0m, IsActive = true, CreatedAt = now, CreatedBy = "System" },
                    new Account { Id = 412, Code = "412", NameAr = "إيرادات المأكولات والمشروبات", NameEn = "Food & Beverage Revenue", Type = Domain.Enums.BackOffice.AccountType.Revenue, ParentAccountId = 41, IsGroup = false, CurrentBalance = 0m, IsActive = true, CreatedAt = now, CreatedBy = "System" },
                    new Account { Id = 413, Code = "413", NameAr = "إيرادات خدمات أخرى", NameEn = "Other Services Revenue", Type = Domain.Enums.BackOffice.AccountType.Revenue, ParentAccountId = 41, IsGroup = false, CurrentBalance = 0m, IsActive = true, CreatedAt = now, CreatedBy = "System" },

                    // 5. EXPENSES
                    new Account { Id = 5, Code = "5", NameAr = "المصروفات", NameEn = "Expenses", Type = Domain.Enums.BackOffice.AccountType.Expense, IsGroup = true, CurrentBalance = 0m, IsActive = true, CreatedAt = now, CreatedBy = "System" },
                    new Account { Id = 51, Code = "51", NameAr = "المصروفات التشغيلية", NameEn = "Operating Expenses", Type = Domain.Enums.BackOffice.AccountType.Expense, ParentAccountId = 5, IsGroup = true, CurrentBalance = 0m, IsActive = true, CreatedAt = now, CreatedBy = "System" },
                    new Account { Id = 511, Code = "511", NameAr = "الرواتب والأجور", NameEn = "Salaries and Wages", Type = Domain.Enums.BackOffice.AccountType.Expense, ParentAccountId = 51, IsGroup = false, CurrentBalance = 0m, IsActive = true, CreatedAt = now, CreatedBy = "System" },
                    new Account { Id = 512, Code = "512", NameAr = "الكهرباء والمياه", NameEn = "Electricity & Water", Type = Domain.Enums.BackOffice.AccountType.Expense, ParentAccountId = 51, IsGroup = false, CurrentBalance = 0m, IsActive = true, CreatedAt = now, CreatedBy = "System" },
                    new Account { Id = 513, Code = "513", NameAr = "تكلفة البضائع المباعة", NameEn = "Cost of Goods Sold", Type = Domain.Enums.BackOffice.AccountType.Expense, ParentAccountId = 51, IsGroup = false, CurrentBalance = 0m, IsActive = true, CreatedAt = now, CreatedBy = "System" }
                };

                await context.Accounts.AddRangeAsync(accounts);
                await context.SaveChangesAsync();

                await context.Database.ExecuteSqlRawAsync("SET IDENTITY_INSERT Accounts OFF;");
                await transaction.CommitAsync();
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public static async Task SeedJournalEntryMappingsAsync(ApplicationDbContext context)
        {
            var now = DateTime.UtcNow;
            var hasAny = await context.JournalEntryMappings.AnyAsync();

            if (!hasAny)
            {
                var mappings = new List<JournalEntryMapping>
                {
                    new JournalEntryMapping
                    {
                        TransactionType = TransactionType.RoomCharge,
                        DebitAccountId = 1132, // Guest Ledger (In-house Guests)
                        CreditAccountId = 411, // Room Revenue
                        IsActive = true,
                        CreatedAt = now,
                        CreatedBy = "System"
                    },
                    new JournalEntryMapping
                    {
                        TransactionType = TransactionType.CashPayment,
                        DebitAccountId = 1111, // Front Office Cashier
                        CreditAccountId = 1132, // Guest Ledger (In-house Guests)
                        IsActive = true,
                        CreatedAt = now,
                        CreatedBy = "System"
                    },
                    new JournalEntryMapping
                    {
                        TransactionType = TransactionType.Refund,
                        DebitAccountId = 1132, // Guest Ledger (In-house Guests)
                        CreditAccountId = 1111, // Front Office Cashier
                        IsActive = true,
                        CreatedAt = now,
                        CreatedBy = "System"
                    },
                    new JournalEntryMapping
                    {
                        TransactionType = TransactionType.CityLedgerPayment,
                        DebitAccountId = 1131, // City Ledger (Companies/Travel Agents)
                        CreditAccountId = 1132, // Guest Ledger (In-house Guests)
                        IsActive = true,
                        CreatedAt = now,
                        CreatedBy = "System"
                    }
                };

                await context.JournalEntryMappings.AddRangeAsync(mappings);
                await context.SaveChangesAsync();
                return;
            }

            bool added = false;

            if (!await context.JournalEntryMappings.AnyAsync(m => m.TransactionType == TransactionType.RoomCharge))
            {
                await context.JournalEntryMappings.AddAsync(new JournalEntryMapping
                {
                    TransactionType = TransactionType.RoomCharge,
                    DebitAccountId = 1132,
                    CreditAccountId = 411,
                    IsActive = true,
                    CreatedAt = now,
                    CreatedBy = "System"
                });
                added = true;
            }

            if (!await context.JournalEntryMappings.AnyAsync(m => m.TransactionType == TransactionType.CashPayment))
            {
                await context.JournalEntryMappings.AddAsync(new JournalEntryMapping
                {
                    TransactionType = TransactionType.CashPayment,
                    DebitAccountId = 1111,
                    CreditAccountId = 1132,
                    IsActive = true,
                    CreatedAt = now,
                    CreatedBy = "System"
                });
                added = true;
            }

            if (!await context.JournalEntryMappings.AnyAsync(m => m.TransactionType == TransactionType.Refund))
            {
                await context.JournalEntryMappings.AddAsync(new JournalEntryMapping
                {
                    TransactionType = TransactionType.Refund,
                    DebitAccountId = 1132,
                    CreditAccountId = 1111,
                    IsActive = true,
                    CreatedAt = now,
                    CreatedBy = "System"
                });
                added = true;
            }

            if (!await context.JournalEntryMappings.AnyAsync(m => m.TransactionType == TransactionType.CityLedgerPayment))
            {
                await context.JournalEntryMappings.AddAsync(new JournalEntryMapping
                {
                    TransactionType = TransactionType.CityLedgerPayment,
                    DebitAccountId = 1131,
                    CreditAccountId = 1132,
                    IsActive = true,
                    CreatedAt = now,
                    CreatedBy = "System"
                });
                added = true;
            }

            if (added)
            {
                await context.SaveChangesAsync();
            }
        }
    }
}

