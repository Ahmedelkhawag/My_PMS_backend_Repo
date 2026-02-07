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

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

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
