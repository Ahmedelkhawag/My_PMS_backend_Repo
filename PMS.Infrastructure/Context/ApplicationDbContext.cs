using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PMS.Domain.Entities;
using PMS.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Security.Claims;
using System.Text;

namespace PMS.Infrastructure.Context
{
    public class ApplicationDbContext : IdentityDbContext<AppUser>
    {
        private readonly IHttpContextAccessor _httpContextAccessor; // عشان نجيب مين اللي مسح
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, IHttpContextAccessor httpContextAccessor)
         : base(options)
        {
            _httpContextAccessor = httpContextAccessor;
        }
        public DbSet<Status> Statuses { get; set; }
        public DbSet<Country> Countries { get; set; }
        public DbSet<EmployeeDocument> EmployeeDocuments { get; set; }

        public DbSet<RefreshToken> RefreshTokens { get; set; }
        public DbSet<RoomType> RoomTypes { get; set; }
        public DbSet<Room> Rooms { get; set; }
		public DbSet<Guest> Guests { get; set; }
		protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<RoomType>().HasData(
        new RoomType { Id = 1, Name = "فردية", BasePrice = 250, MaxAdults = 1, MaxChildren = 0, Description = "غرفة لشخص واحد" },
        new RoomType { Id = 2, Name = "مزدوجة", BasePrice = 350, MaxAdults = 2, MaxChildren = 1, Description = "غرفة لشخصين" },
        new RoomType { Id = 3, Name = "جناح", BasePrice = 540, MaxAdults = 2, MaxChildren = 2, Description = "جناح فاخر" },
        new RoomType { Id = 4, Name = "ديلوكس", BasePrice = 500, MaxAdults = 2, MaxChildren = 1, Description = "غرفة مميزة بإطلالة" }
    );

            // ==========================================
            // 2. زراعة الغرف (Rooms)
            // ==========================================
            builder.Entity<Room>().HasData(
                // الدور الأول
                new Room { Id = 1, RoomNumber = "101", FloorNumber = 1, RoomTypeId = 1, Status = PMS.Domain.Enums.RoomStatus.Available, IsActive = true },
                new Room { Id = 2, RoomNumber = "102", FloorNumber = 1, RoomTypeId = 2, Status = PMS.Domain.Enums.RoomStatus.Occupied, IsActive = true },
                new Room { Id = 3, RoomNumber = "103", FloorNumber = 1, RoomTypeId = 2, Status = PMS.Domain.Enums.RoomStatus.Cleaning, IsActive = true },

                // الدور الثاني
                new Room { Id = 4, RoomNumber = "201", FloorNumber = 2, RoomTypeId = 3, Status = PMS.Domain.Enums.RoomStatus.Available, IsActive = true },
                new Room { Id = 5, RoomNumber = "202", FloorNumber = 2, RoomTypeId = 4, Status = PMS.Domain.Enums.RoomStatus.Maintenance, IsActive = true },
                new Room { Id = 6, RoomNumber = "203", FloorNumber = 2, RoomTypeId = 2, Status = PMS.Domain.Enums.RoomStatus.Maintenance, IsActive = true });


            // 1. تطبيق الفلتر السحري (Global Query Filter) 🧹
            // اللفة دي عشان نطبق الفلتر على كل الـ Entities اللي واخدة ISoftDeletable مرة واحدة
            foreach (var entityType in builder.Model.GetEntityTypes())
            {
                if (typeof(ISoftDeletable).IsAssignableFrom(entityType.ClrType))
                {
                    // دي كأننا كتبنا: builder.Entity<AppUser>().HasQueryFilter(e => !e.IsDeleted);
                    // بس معمولة بشكل Generic عشان تشتغل للكل
                    var parameter = Expression.Parameter(entityType.ClrType, "e");
                    var propertyMethodInfo = typeof(EF).GetMethod("Property")?.MakeGenericMethod(typeof(bool));
                    var isDeletedProperty = Expression.Call(propertyMethodInfo, parameter, Expression.Constant("IsDeleted"));

                    // الشرط: IsDeleted == false
                    var compareExpression = Expression.MakeBinary(ExpressionType.Equal, isDeletedProperty, Expression.Constant(false));
                    var lambda = Expression.Lambda(compareExpression, parameter);

                    builder.Entity(entityType.ClrType).HasQueryFilter(lambda);
                }
            }
        }

        // 2. تحويل الحذف الحقيقي لحذف ناعم 🔄
        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            // بنشوف أي حد حالته "Deleted"
            foreach (var entry in ChangeTracker.Entries<ISoftDeletable>())
            {
                if (entry.State == EntityState.Deleted)
                {
                    // بدل ما نمسحه، نخليه Modified (تعديل)
                    entry.State = EntityState.Modified;

                    // نحدث البيانات
                    entry.Entity.IsDeleted = true;
                    entry.Entity.DeletedAt = DateTime.UtcNow;
                    entry.Entity.DeletedBy = _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
                }
            }

            return base.SaveChangesAsync(cancellationToken);
        }
    }
}
