using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PMS.Domain.Entities;
using PMS.Domain.Entities.Configuration;
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
		public DbSet<Reservation> Reservations { get; set; }
		public DbSet<ReservationService> ReservationServices { get; set; }

		public DbSet<BookingSource> BookingSources { get; set; }
		public DbSet<MarketSegment> MarketSegments { get; set; }
		public DbSet<MealPlan> MealPlans { get; set; }
		public DbSet<RoomStatusLookup> RoomStatusLookups { get; set; }
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
		new Room
		{
			Id = 1,
			RoomNumber = "101",
			FloorNumber = 1,
			RoomTypeId = 1,
			RoomStatusId = 1,
			IsActive = true,
			CreatedAt = new DateTime(2026, 1, 1) // 👈 التعديل هنا (تاريخ ثابت)
		},
		new Room
		{
			Id = 2,
			RoomNumber = "102",
			FloorNumber = 1,
			RoomTypeId = 2,
			RoomStatusId = 2,
			IsActive = true,
			CreatedAt = new DateTime(2026, 1, 1) // 👈 وهنا
		},
		new Room
		{
			Id = 3,
			RoomNumber = "103",
			FloorNumber = 1,
			RoomTypeId = 2,
			RoomStatusId = 1,
			IsActive = true,
			CreatedAt = new DateTime(2026, 1, 1) // 👈 وهنا
		},

		// الدور الثاني
		new Room
		{
			Id = 4,
			RoomNumber = "201",
			FloorNumber = 2,
			RoomTypeId = 3,
			RoomStatusId = 1,
			IsActive = true,
			CreatedAt = new DateTime(2026, 1, 1)
		},
		new Room
		{
			Id = 5,
			RoomNumber = "202",
			FloorNumber = 2,
			RoomTypeId = 4,
			RoomStatusId = 3,
			IsActive = true,
			CreatedAt = new DateTime(2026, 1, 1)
		},
		new Room
		{
			Id = 6,
			RoomNumber = "203",
			FloorNumber = 2,
			RoomTypeId = 2,
			RoomStatusId = 4,
			IsActive = true,
			CreatedAt = new DateTime(2026, 1, 1)
		}
	);


			builder.Entity<BookingSource>().HasData(
		new BookingSource { Id = 1, Name = "Direct (Walk-in)" },
		new BookingSource { Id = 2, Name = "Phone" },
		new BookingSource { Id = 3, Name = "Booking.com" },
		new BookingSource { Id = 4, Name = "Expedia" },
		new BookingSource { Id = 5, Name = "Website" }
	);

			// قطاعات السوق
			builder.Entity<MarketSegment>().HasData(
				new MarketSegment { Id = 1, Name = "Individual (أفراد)" },
				new MarketSegment { Id = 2, Name = "Corporate (شركات)" },
				new MarketSegment { Id = 3, Name = "Group (مجموعات)" },
				new MarketSegment { Id = 4, Name = "Government (حكومي)" }
			);

			// الوجبات
			builder.Entity<MealPlan>().HasData(
				new MealPlan { Id = 1, Name = "Room Only (بدون وجبات)", Price = 0 },
				new MealPlan { Id = 2, Name = "Bed & Breakfast (إفطار)", Price = 150 },
				new MealPlan { Id = 3, Name = "Half Board (إفطار وعشاء)", Price = 400 },
				new MealPlan { Id = 4, Name = "Full Board (إفطار وغداء وعشاء)", Price = 700 }
			);

			// حالات الغرف
			builder.Entity<RoomStatusLookup>().HasData(
				new RoomStatusLookup { Id = 1, Name = "Clean (نظيفة)", Color = "green" },
				new RoomStatusLookup { Id = 2, Name = "Dirty (متسخة)", Color = "red" },
				new RoomStatusLookup { Id = 3, Name = "Maintenance (صيانة)", Color = "orange" },
				new RoomStatusLookup { Id = 4, Name = "Out of Order (خارج الخدمة)", Color = "gray" }
			);


			builder.Entity<ExtraService>().HasData(
	new ExtraService { Id = 1, Name = "Airport Transfer (نقل مطار)", Price = 150, IsPerDay = false },
	new ExtraService { Id = 2, Name = "Parking (موقف سيارات)", Price = 30, IsPerDay = true },
	new ExtraService { Id = 3, Name = "VIP Service (خدمة VIP)", Price = 200, IsPerDay = true },
	new ExtraService { Id = 4, Name = "Spa (سبا)", Price = 300, IsPerDay = false },
	new ExtraService { Id = 5, Name = "Laundry (غسيل)", Price = 75, IsPerDay = false }
);


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
			// بنجيب اليوزر الحالي (لو مفيش يوزر بنكتب System)
			var currentUserId = _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier) ?? "System";
			var currentDateTime = DateTime.UtcNow;

			foreach (var entry in ChangeTracker.Entries())
			{
				// 1. معالجة الـ Auditing (الإنشاء والتعديل) 🆕
				if (entry.Entity is IAuditable auditableEntity)
				{
					if (entry.State == EntityState.Added)
					{
						auditableEntity.CreatedBy = currentUserId; // Open By
						auditableEntity.CreatedAt = currentDateTime; // Date Open
					}
					else if (entry.State == EntityState.Modified)
					{
						// بنمنع تعديل بيانات الإنشاء بالغلط
						entry.Property(nameof(IAuditable.CreatedBy)).IsModified = false;
						entry.Property(nameof(IAuditable.CreatedAt)).IsModified = false;

						auditableEntity.LastModifiedBy = currentUserId; // Updated By
						auditableEntity.LastModifiedAt = currentDateTime;
					}
				}

				// 2. معالجة الـ Soft Delete (زي ما كانت)
				if (entry.Entity is ISoftDeletable softDeletableEntity && entry.State == EntityState.Deleted)
				{
					entry.State = EntityState.Modified;
					softDeletableEntity.IsDeleted = true;
					softDeletableEntity.DeletedAt = currentDateTime;
					softDeletableEntity.DeletedBy = currentUserId; // Closed By (Delete)
				}
			}

			return base.SaveChangesAsync(cancellationToken);
		}
	}
}
