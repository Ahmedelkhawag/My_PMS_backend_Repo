using PMS.Application.Interfaces.Repositories;
using PMS.Domain.Entities;
using PMS.Domain.Entities.Configuration;
using System;
using System.Collections.Generic;
using System.Text;
	using System.Threading.Tasks;

namespace PMS.Application.Interfaces.UOF
{
    public interface IUnitOfWork : IDisposable
    {
        IBaseRepository<Country> Countries { get; }
        IBaseRepository<Status> Statuses { get; }
        IBaseRepository<EmployeeDocument> EmployeeDocuments { get; }
        IBaseRepository<RefreshToken> RefreshTokens { get; }
        IBaseRepository<Room> Rooms { get; }
        IBaseRepository<RoomType> RoomTypes { get; }
		IBaseRepository<Guest> Guests { get; }
		IBaseRepository<Reservation> Reservations { get; }

		IBaseRepository<GuestFolio> GuestFolios { get; }
		IBaseRepository<FolioTransaction> FolioTransactions { get; }
		IBaseRepository<EmployeeShift> EmployeeShifts { get; }
		IBaseRepository<BusinessDay> BusinessDays { get; }

		IBaseRepository<ReservationService> ReservationServices { get; }
		IBaseRepository<BookingSource> BookingSources { get; }
		IBaseRepository<MarketSegment> MarketSegments { get; }
		IBaseRepository<MealPlan> MealPlans { get; }
		IBaseRepository<RoomStatusLookup> RoomStatuses { get; }
		IBaseRepository<ExtraService> ExtraServices { get; }
		IBaseRepository<CompanyProfile> CompanyProfiles { get; }
		IBaseRepository<RatePlan> RatePlans { get; }

        IBaseRepository<PMS.Domain.Entities.BackOffice.Account> Accounts { get; }
        IBaseRepository<PMS.Domain.Entities.BackOffice.JournalEntry> JournalEntries { get; }
        IBaseRepository<PMS.Domain.Entities.BackOffice.JournalEntryLine> JournalEntryLines { get; }
        IBaseRepository<PMS.Domain.Entities.BackOffice.JournalEntryMapping> JournalEntryMappings { get; }

        IBaseRepository<PMS.Domain.Entities.BackOffice.AR.ARInvoice> ARInvoices { get; }
        IBaseRepository<PMS.Domain.Entities.BackOffice.AR.ARInvoiceLine> ARInvoiceLines { get; }
        IBaseRepository<PMS.Domain.Entities.BackOffice.AR.ARPayment> ARPayments { get; }
        IBaseRepository<PMS.Domain.Entities.BackOffice.AR.ARPaymentAllocation> ARPaymentAllocations { get; }
        IBaseRepository<PMS.Domain.Entities.BackOffice.AR.ARAdjustment> ARAdjustments { get; }

        IBaseRepository<PMS.Domain.Entities.BackOffice.AP.Vendor> Vendors { get; }
        IBaseRepository<PMS.Domain.Entities.BackOffice.AP.APInvoice> APInvoices { get; }
        IBaseRepository<PMS.Domain.Entities.BackOffice.AP.APInvoiceLine> APInvoiceLines { get; }
        IBaseRepository<PMS.Domain.Entities.BackOffice.AP.APPayment> APPayments { get; }
        IBaseRepository<PMS.Domain.Entities.BackOffice.AP.APPaymentAllocation> APPaymentAllocations { get; }

		//  SaveChanges
		Task<int> CompleteAsync();

        Task BeginTransactionAsync();
        Task CommitTransactionAsync();
        Task RollbackTransactionAsync();

        /// <summary>
        /// Returns the current business (financial) date based on the open BusinessDay.
        /// </summary>
        Task<DateTime> GetCurrentBusinessDateAsync();
    }
}
