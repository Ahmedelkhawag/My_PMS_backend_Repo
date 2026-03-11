using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PMS.Domain.Entities;
using PMS.Domain.Entities.BackOffice;
using PMS.Domain.Entities.BackOffice.AR;
	using PMS.Domain.Entities.BackOffice.AP;
	using PMS.Domain.Enums.BackOffice.AP;
using PMS.Domain.Entities.Configuration;
using PMS.Domain.Constants;
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
		private readonly IHttpContextAccessor _httpContextAccessor; 
		public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, IHttpContextAccessor httpContextAccessor)
		 : base(options)
		{
			_httpContextAccessor = httpContextAccessor;
		}

		protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
		{
			base.OnConfiguring(optionsBuilder);
			optionsBuilder.ConfigureWarnings(w =>
				w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning));
		}
		public DbSet<Status> Statuses { get; set; }
		public DbSet<Country> Countries { get; set; }
		public DbSet<Currency> Currencies { get; set; }
		public DbSet<EmployeeDocument> EmployeeDocuments { get; set; }

		public DbSet<RefreshToken> RefreshTokens { get; set; }
		public DbSet<RoomType> RoomTypes { get; set; }
		public DbSet<Room> Rooms { get; set; }
		public DbSet<Guest> Guests { get; set; }
		public DbSet<Reservation> Reservations { get; set; }
		public DbSet<ReservationService> ReservationServices { get; set; }

		public DbSet<GuestFolio> GuestFolios { get; set; }
		public DbSet<FolioTransaction> FolioTransactions { get; set; }
		public DbSet<EmployeeShift> EmployeeShifts { get; set; }
		public DbSet<BusinessDay> BusinessDays { get; set; }

		public DbSet<BookingSource> BookingSources { get; set; }
		public DbSet<MarketSegment> MarketSegments { get; set; }
		public DbSet<MealPlan> MealPlans { get; set; }
		public DbSet<RoomStatusLookup> RoomStatusLookups { get; set; }
		public DbSet<CompanyProfile> CompanyProfiles { get; set; }
		public DbSet<RatePlan> RatePlans { get; set; }

		public DbSet<Account> Accounts { get; set; }
		public DbSet<PaymentTerm> PaymentTerms { get; set; }
		public DbSet<JournalEntry> JournalEntries { get; set; }
		public DbSet<JournalEntryLine> JournalEntryLines { get; set; }
		public DbSet<JournalEntryMapping> JournalEntryMappings { get; set; }
		public DbSet<CostCenter> CostCenters { get; set; }

		public DbSet<FiscalYear> FiscalYears { get; set; }
		public DbSet<AccountingPeriod> AccountingPeriods { get; set; }

		public DbSet<ARInvoice> ARInvoices { get; set; }
		public DbSet<ARInvoiceLine> ARInvoiceLines { get; set; }
		public DbSet<ARPayment> ARPayments { get; set; }
		public DbSet<ARPaymentAllocation> ARPaymentAllocations { get; set; }
		public DbSet<ARAdjustment> ARAdjustments { get; set; }
		public DbSet<ARAllocation> ARAllocations { get; set; }
		public DbSet<TACommissionRecord> TACommissionRecords { get; set; }

		// AP Module
		public DbSet<Vendor> Vendors { get; set; }
		public DbSet<APInvoice> APInvoices { get; set; }
		public DbSet<APInvoiceLine> APInvoiceLines { get; set; }
		public DbSet<APPayment> APPayments { get; set; }
		public DbSet<APPaymentAllocation> APPaymentAllocations { get; set; }
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
			
			// ==========================================
			builder.Entity<Room>().HasData(
		
		new Room
		{
			Id = 1,
			RoomNumber = "101",
			FloorNumber = 1,
			RoomTypeId = 1,
			RoomStatusId = 1,
			IsActive = true,
			CreatedAt = new DateTime(2026, 1, 1) 
		},
		new Room
		{
			Id = 2,
			RoomNumber = "102",
			FloorNumber = 1,
			RoomTypeId = 2,
			RoomStatusId = 2,
			IsActive = true,
			CreatedAt = new DateTime(2026, 1, 1) 
		},
		new Room
		{
			Id = 3,
			RoomNumber = "103",
			FloorNumber = 1,
			RoomTypeId = 2,
			RoomStatusId = 1,
			IsActive = true,
			CreatedAt = new DateTime(2026, 1, 1) 
		},

		
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
		new BookingSource { Id = 1, Name = "Direct (Walk-in)", IsActive = true, RequiresExternalReference = false },
		new BookingSource { Id = 2, Name = "Phone", IsActive = true, RequiresExternalReference = false },
		new BookingSource { Id = 3, Name = "Booking.com", IsActive = true, RequiresExternalReference = true },
		new BookingSource { Id = 4, Name = "Expedia", IsActive = true, RequiresExternalReference = true },
		new BookingSource { Id = 5, Name = "Website", IsActive = true, RequiresExternalReference = false }
	);

			
			builder.Entity<MarketSegment>().HasData(
				new MarketSegment { Id = 1, Name = "Individual (أفراد)" },
				new MarketSegment { Id = 2, Name = "Corporate (شركات)" },
				new MarketSegment { Id = 3, Name = "Group (مجموعات)" },
				new MarketSegment { Id = 4, Name = "Government (حكومي)" }
			);

			
			builder.Entity<MealPlan>().HasData(
				new MealPlan { Id = 1, Name = "Room Only (بدون وجبات)", Price = 0 },
				new MealPlan { Id = 2, Name = "Bed & Breakfast (إفطار)", Price = 150 },
				new MealPlan { Id = 3, Name = "Half Board (إفطار وعشاء)", Price = 400 },
				new MealPlan { Id = 4, Name = "Full Board (إفطار وغداء وعشاء)", Price = 700 }
			);

            
            builder.Entity<RoomStatusLookup>().HasData(
        new RoomStatusLookup { Id = 1, Name = "Clean (نظيفة)", Color = StatusColorPalette.Success }, 
        new RoomStatusLookup { Id = 2, Name = "Dirty (متسخة)", Color = StatusColorPalette.Danger },  
        new RoomStatusLookup { Id = 3, Name = "Inspected (تم الفحص)", Color = "#2ecc71" },           
        new RoomStatusLookup { Id = 4, Name = "Out of Order (صيانة جسيمة)", Color = StatusColorPalette.Secondary }, 
        new RoomStatusLookup { Id = 5, Name = "Out of Service (صيانة بسيطة)", Color = StatusColorPalette.Warning }  
                                                                                                                    
			);


			builder.Entity<ExtraService>().HasData(
				new ExtraService { Id = 1, Name = "Airport Transfer (نقل مطار)", Price = 150, IsPerDay = false },
				new ExtraService { Id = 2, Name = "Parking (موقف سيارات)", Price = 30, IsPerDay = true },
				new ExtraService { Id = 3, Name = "VIP Service (خدمة VIP)", Price = 200, IsPerDay = true },
				new ExtraService { Id = 4, Name = "Spa (سبا)", Price = 300, IsPerDay = false },
				new ExtraService { Id = 5, Name = "Laundry (غسيل)", Price = 75, IsPerDay = false }
			);

			// Reservation configuration
			builder.Entity<Reservation>(entity =>
			{
				entity.Property(r => r.CheckInDate).HasColumnType("datetimeoffset");
				entity.Property(r => r.CheckOutDate).HasColumnType("datetimeoffset");
			});

			// Rate Plans configuration
			builder.Entity<RatePlan>(entity =>
			{
				entity.Property(r => r.RateValue).HasColumnType("decimal(18,2)");

				entity.HasIndex(r => r.Code)
					  .IsUnique();
			});

			builder.Entity<RatePlan>().HasData(
				new RatePlan
				{
					Id = 1,
					Code = "STANDARD",
					Name = "Standard Rate",
					Description = "Standard public rate plan (no discount).",
					RateType = global::PMS.Domain.Enums.RateType.PercentageDiscount,
					RateValue = 0m,
					IsPublic = true,
					IsActive = true,
					CreatedAt = new DateTime(2026, 1, 1),
					CreatedBy = "System"
				},
				new RatePlan
				{
					Id = 2,
					Code = "NONREF",
					Name = "Non-Refundable",
					Description = "Non-refundable rate with 10% discount.",
					RateType = global::PMS.Domain.Enums.RateType.PercentageDiscount,
					RateValue = 10m,
					IsPublic = true,
					IsActive = true,
					CreatedAt = new DateTime(2026, 1, 1),
					CreatedBy = "System"
				}
			);


			// Guest Folio configuration
			builder.Entity<GuestFolio>(entity =>
			{
				entity.Property(f => f.TotalCharges).HasColumnType("decimal(18,2)");
				entity.Property(f => f.TotalPayments).HasColumnType("decimal(18,2)");
				entity.Property(f => f.Balance).HasColumnType("decimal(18,2)");

				entity.Property(f => f.Currency)
					  .HasMaxLength(3)
					  .HasDefaultValue("EGP");

				entity.HasIndex(f => f.ReservationId)
					  .IsUnique();

				entity.HasOne(f => f.Reservation)
					  .WithOne(r => r.GuestFolio)
					  .HasForeignKey<GuestFolio>(f => f.ReservationId)
					  .OnDelete(DeleteBehavior.Restrict);
			});

			// Folio Transaction configuration
			builder.Entity<FolioTransaction>(entity =>
			{
				entity.Property(t => t.Amount).HasColumnType("decimal(18,2)");
				entity.Property(t => t.BusinessDate).HasColumnType("date");

				entity.HasOne(t => t.Folio)
					  .WithMany(f => f.Transactions)
					  .HasForeignKey(t => t.FolioId)
					  .IsRequired(false)
					  .OnDelete(DeleteBehavior.Cascade);

				entity.HasOne(t => t.Shift)
					  .WithMany()
					  .HasForeignKey(t => t.ShiftId)
					  .OnDelete(DeleteBehavior.Restrict);
			});

			// Business Day configuration
			builder.Entity<BusinessDay>(entity =>
			{
				entity.Property(b => b.Date).HasColumnType("date");

				// Ensure each calendar date is unique.
				entity.HasIndex(b => b.Date)
					  .IsUnique();

				// Ensure only one open business day exists at a time.
				// BusinessDayStatus.Open = 1
				entity.HasIndex(b => b.Status)
					  .IsUnique()
					  .HasFilter("[Status] = 1");

				entity.HasOne(b => b.ClosedBy)
					  .WithMany()
					  .HasForeignKey(b => b.ClosedById)
					  .OnDelete(DeleteBehavior.Restrict);
			});

			// Employee Shift configuration
			builder.Entity<EmployeeShift>(entity =>
			{
				entity.Property(s => s.StartingCash).HasColumnType("decimal(18,2)");
				entity.Property(s => s.SystemCalculatedCash).HasColumnType("decimal(18,2)");
				entity.Property(s => s.ActualCashHanded).HasColumnType("decimal(18,2)");
				entity.Property(s => s.Difference).HasColumnType("decimal(18,2)");

				entity.Property(s => s.StartedAt)
					  .HasDefaultValueSql("GETUTCDATE()");

				entity.Property(s => s.IsClosed)
					  .HasDefaultValue(false);

				// Enforce one active (open) shift per employee (filtered unique index).
				entity.HasIndex(s => new { s.EmployeeId, s.IsClosed })
					  .IsUnique()
					  .HasFilter("[IsClosed] = 0");

				entity.HasOne(s => s.Employee)
					  .WithMany()
					  .HasForeignKey(s => s.EmployeeId)
					  .OnDelete(DeleteBehavior.Restrict);

				entity.HasOne(s => s.ReconciliationTransaction)
					  .WithMany()
					  .HasForeignKey(s => s.ReconciliationTransactionId)
					  .OnDelete(DeleteBehavior.Restrict);
			});

			// Back-Office GL: FiscalYear configuration
			builder.Entity<FiscalYear>(entity =>
			{
				entity.HasMany(f => f.AccountingPeriods)
					  .WithOne(p => p.FiscalYear)
					  .HasForeignKey(p => p.FiscalYearId)
					  .OnDelete(DeleteBehavior.Restrict);
			});

			// Back-Office GL: AccountingPeriod configuration
			builder.Entity<AccountingPeriod>(entity =>
			{
				entity.HasOne(p => p.FiscalYear)
					  .WithMany(f => f.AccountingPeriods)
					  .HasForeignKey(p => p.FiscalYearId)
					  .OnDelete(DeleteBehavior.Restrict);
			});

			// Back-Office GL: Account configuration
			builder.Entity<Account>(entity =>
			{
				entity.Property(a => a.CurrentBalance).HasColumnType("decimal(18,2)");

				entity.HasIndex(a => a.Code)
					  .IsUnique()
					  .HasFilter("[IsDeleted] = 0");

				entity.HasOne(a => a.ParentAccount)
					  .WithMany(a => a.ChildAccounts)
					  .HasForeignKey(a => a.ParentAccountId)
					  .OnDelete(DeleteBehavior.Restrict);
			});

			// Back-Office GL: CostCenter configuration
			builder.Entity<CostCenter>(entity =>
			{
				entity.HasIndex(c => c.Code)
					  .IsUnique()
					  .HasFilter("[IsDeleted] = 0");

				entity.HasOne(c => c.ParentCostCenter)
					  .WithMany(c => c.ChildCostCenters)
					  .HasForeignKey(c => c.ParentCostCenterId)
					  .OnDelete(DeleteBehavior.Restrict);
			});

			// Back-Office GL: JournalEntry configuration
			builder.Entity<JournalEntry>(entity =>
			{
				entity.HasIndex(j => j.EntryNumber)
					  .IsUnique();

				entity.HasMany(j => j.Lines)
					  .WithOne(l => l.JournalEntry)
					  .HasForeignKey(l => l.JournalEntryId)
					  .OnDelete(DeleteBehavior.Cascade);

				entity.HasOne(j => j.BusinessDay)
					  .WithMany()
					  .HasForeignKey(j => j.BusinessDayId)
					  .OnDelete(DeleteBehavior.Restrict);
			});

			// Back-Office GL: JournalEntryLine configuration
			builder.Entity<JournalEntryLine>(entity =>
			{
				entity.Property(l => l.Debit).HasColumnType("decimal(18,2)");
				entity.Property(l => l.Credit).HasColumnType("decimal(18,2)");
				entity.Property(l => l.DebitForeign).HasColumnType("decimal(18,2)");
				entity.Property(l => l.CreditForeign).HasColumnType("decimal(18,2)");
				entity.Property(l => l.ExchangeRate).HasColumnType("decimal(18,4)");

				entity.HasOne(l => l.Account)
					  .WithMany()
					  .HasForeignKey(l => l.AccountId)
					  .OnDelete(DeleteBehavior.Restrict);

				entity.HasOne(l => l.Currency)
					  .WithMany()
					  .HasForeignKey(l => l.CurrencyId)
					  .IsRequired(false)
					  .OnDelete(DeleteBehavior.Restrict);

				entity.HasOne(l => l.CostCenter)
					  .WithMany(c => c.JournalEntryLines)
					  .HasForeignKey(l => l.CostCenterId)
					  .IsRequired(false)
					  .OnDelete(DeleteBehavior.Restrict);
			});

			// Back-Office GL: JournalEntryMapping configuration
			builder.Entity<JournalEntryMapping>(entity =>
			{
				entity.HasIndex(m => m.TransactionType).IsUnique(false);

				entity.HasOne(m => m.DebitAccount)
					  .WithMany()
					  .HasForeignKey(m => m.DebitAccountId)
					  .OnDelete(DeleteBehavior.Restrict);

				entity.HasOne(m => m.CreditAccount)
					  .WithMany()
					  .HasForeignKey(m => m.CreditAccountId)
					  .OnDelete(DeleteBehavior.Restrict);

				entity.Property(m => m.Percentage).HasColumnType("decimal(5,2)");
			});

			// AR Stage 2: CompanyProfile -> PaymentTerm
			builder.Entity<CompanyProfile>(entity =>
			{
				entity.HasOne(c => c.PaymentTerm)
					  .WithMany()
					  .HasForeignKey(c => c.PaymentTermId)
					  .IsRequired(false)
					  .OnDelete(DeleteBehavior.Restrict);
			});

			// Back-Office AR: ARInvoice configuration
			builder.Entity<ARInvoice>(entity =>
			{
				entity.HasIndex(i => i.InvoiceNumber)
					  .IsUnique();

				entity.Property(i => i.TotalAmount).HasColumnType("decimal(18,2)");
				entity.Property(i => i.PaidAmount).HasColumnType("decimal(18,2)");

				entity.HasOne(i => i.Company)
					  .WithMany()
					  .HasForeignKey(i => i.CompanyId)
					  .OnDelete(DeleteBehavior.Restrict);
			});

			// Back-Office AR: ARInvoiceLine configuration
			builder.Entity<ARInvoiceLine>(entity =>
			{
				entity.Property(l => l.Amount).HasColumnType("decimal(18,2)");

				entity.HasOne(l => l.ARInvoice)
					  .WithMany(i => i.Lines)
					  .HasForeignKey(l => l.ARInvoiceId)
					  .OnDelete(DeleteBehavior.Cascade);

				entity.HasOne(l => l.FolioTransaction)
					  .WithMany()
					  .HasForeignKey(l => l.FolioTransactionId)
					  .OnDelete(DeleteBehavior.Restrict);
			});

			// Back-Office AR: ARPayment configuration
			builder.Entity<ARPayment>(entity =>
			{
				entity.Property(p => p.Amount).HasColumnType("decimal(18,2)");
				entity.Property(p => p.UnallocatedAmount).HasColumnType("decimal(18,2)");

				entity.HasOne(p => p.Company)
					  .WithMany()
					  .HasForeignKey(p => p.CompanyId)
					  .OnDelete(DeleteBehavior.Restrict);

				entity.HasOne(p => p.Invoice)
					  .WithMany()
					  .HasForeignKey(p => p.InvoiceId)
					  .IsRequired(false)
					  .OnDelete(DeleteBehavior.Restrict);
			});

			// Back-Office AR: ARPaymentAllocation configuration
			builder.Entity<ARPaymentAllocation>(entity =>
			{
				entity.Property(a => a.AmountApplied).HasColumnType("decimal(18,2)");

				entity.HasOne(a => a.ARPayment)
					  .WithMany()
					  .HasForeignKey(a => a.ARPaymentId)
					  .OnDelete(DeleteBehavior.Cascade);

				entity.HasOne(a => a.ARInvoice)
					  .WithMany()
					  .HasForeignKey(a => a.ARInvoiceId)
					  .OnDelete(DeleteBehavior.Cascade);
			});

			// Back-Office AR: ARAllocation configuration
			builder.Entity<ARAllocation>(entity =>
			{
				entity.Property(a => a.Amount).HasColumnType("decimal(18,2)");

				entity.HasOne(a => a.Payment)
					  .WithMany()
					  .HasForeignKey(a => a.PaymentId)
					  .OnDelete(DeleteBehavior.Cascade);

				entity.HasOne(a => a.Invoice)
					  .WithMany()
					  .HasForeignKey(a => a.InvoiceId)
					  .OnDelete(DeleteBehavior.Restrict);
			});

			// Back-Office AR: ARAdjustment configuration
			builder.Entity<ARAdjustment>(entity =>
			{
				entity.Property(a => a.Amount).HasColumnType("decimal(18,2)");

				entity.HasOne(a => a.ARInvoice)
					  .WithMany()
					  .HasForeignKey(a => a.ARInvoiceId)
					  .OnDelete(DeleteBehavior.Restrict);
			});

			// Back-Office AR: TACommissionRecord configuration
			builder.Entity<TACommissionRecord>(entity =>
			{
				entity.Property(c => c.EligibleRevenue).HasColumnType("decimal(18,2)");
				entity.Property(c => c.CommissionRate).HasColumnType("decimal(18,2)");
				entity.Property(c => c.CommissionAmount).HasColumnType("decimal(18,2)");

				entity.HasOne(c => c.Company)
					  .WithMany()
					  .HasForeignKey(c => c.CompanyId)
					  .OnDelete(DeleteBehavior.Restrict);

				entity.HasOne(c => c.Reservation)
					  .WithMany()
					  .HasForeignKey(c => c.ReservationId)
					  .OnDelete(DeleteBehavior.Restrict);

				entity.HasOne(c => c.JournalEntry)
					  .WithMany()
					  .HasForeignKey(c => c.JournalEntryId)
					  .IsRequired(false)
					  .OnDelete(DeleteBehavior.Restrict);
			});

			// ─── AP Module Configuration ───────────────────────────

			// Vendor
			builder.Entity<Vendor>(entity =>
			{
				entity.HasIndex(v => v.TaxId)
					  .IsUnique()
					  .HasFilter("[IsDeleted] = 0");

				entity.HasOne(v => v.APAccount)
					  .WithMany()
					  .HasForeignKey(v => v.APAccountId)
					  .OnDelete(DeleteBehavior.Restrict);

				entity.HasOne(v => v.DefaultExpenseAccount)
					  .WithMany()
					  .HasForeignKey(v => v.DefaultExpenseAccountId)
					  .IsRequired(false)
					  .OnDelete(DeleteBehavior.Restrict);
			});

			// APInvoice
			builder.Entity<APInvoice>(entity =>
			{
				entity.Property(i => i.TotalAmount).HasColumnType("decimal(18,2)");
				entity.Property(i => i.AmountPaid).HasColumnType("decimal(18,2)");

				entity.HasIndex(i => new { i.VendorId, i.VendorInvoiceNo })
					  .IsUnique()
					  .HasFilter("[IsDeleted] = 0");

				entity.HasOne(i => i.Vendor)
					  .WithMany(v => v.Invoices)
					  .HasForeignKey(i => i.VendorId)
					  .OnDelete(DeleteBehavior.Restrict);
			});

			// APInvoiceLine
			builder.Entity<APInvoiceLine>(entity =>
			{
				entity.Property(l => l.Amount).HasColumnType("decimal(18,2)");

				entity.HasOne(l => l.APInvoice)
					  .WithMany(i => i.Lines)
					  .HasForeignKey(l => l.APInvoiceId)
					  .OnDelete(DeleteBehavior.Cascade);

				entity.HasOne(l => l.ExpenseAccount)
					  .WithMany()
					  .HasForeignKey(l => l.ExpenseAccountId)
					  .OnDelete(DeleteBehavior.Restrict);
			});

			// APPayment
			builder.Entity<APPayment>(entity =>
			{
				entity.Property(p => p.Amount).HasColumnType("decimal(18,2)");

				entity.HasOne(p => p.Vendor)
					  .WithMany(v => v.Payments)
					  .HasForeignKey(p => p.VendorId)
					  .OnDelete(DeleteBehavior.Restrict);
			});

			// APPaymentAllocation
			builder.Entity<APPaymentAllocation>(entity =>
			{
				entity.Property(a => a.AllocatedAmount).HasColumnType("decimal(18,2)");

				entity.HasOne(a => a.APPayment)
					  .WithMany(p => p.Allocations)
					  .HasForeignKey(a => a.APPaymentId)
					  .OnDelete(DeleteBehavior.Cascade);

				entity.HasOne(a => a.APInvoice)
					  .WithMany(i => i.Allocations)
					  .HasForeignKey(a => a.APInvoiceId)
					  .OnDelete(DeleteBehavior.Restrict);
			});

			foreach (var entityType in builder.Model.GetEntityTypes())
			{
				if (typeof(ISoftDeletable).IsAssignableFrom(entityType.ClrType))
				{
					
					
					var parameter = Expression.Parameter(entityType.ClrType, "e");
					var propertyMethodInfo = typeof(EF).GetMethod("Property")?.MakeGenericMethod(typeof(bool));
					var isDeletedProperty = Expression.Call(propertyMethodInfo, parameter, Expression.Constant("IsDeleted"));

					
					var compareExpression = Expression.MakeBinary(ExpressionType.Equal, isDeletedProperty, Expression.Constant(false));
					var lambda = Expression.Lambda(compareExpression, parameter);

					builder.Entity(entityType.ClrType).HasQueryFilter(lambda);
				}
			}
		}

		
		public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
		{
			
			var currentUserId = _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier) ?? "System";
			var currentDateTime = DateTime.UtcNow;

			foreach (var entry in ChangeTracker.Entries())
			{
				
				if (entry.Entity is IAuditable auditableEntity)
				{
					if (entry.State == EntityState.Added)
					{
						auditableEntity.CreatedBy = currentUserId; // Open By
						auditableEntity.CreatedAt = currentDateTime; // Date Open
					}
					else if (entry.State == EntityState.Modified)
					{
						
						entry.Property(nameof(IAuditable.CreatedBy)).IsModified = false;
						entry.Property(nameof(IAuditable.CreatedAt)).IsModified = false;

						auditableEntity.LastModifiedBy = currentUserId; // Updated By
						auditableEntity.LastModifiedAt = currentDateTime;
					}
				}

				
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
