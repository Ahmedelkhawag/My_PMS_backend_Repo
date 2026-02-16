using PMS.Application.Interfaces.Repositories;
using PMS.Domain.Entities;
using PMS.Domain.Entities.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

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

		IBaseRepository<ReservationService> ReservationServices { get; }
		IBaseRepository<BookingSource> BookingSources { get; }
		IBaseRepository<MarketSegment> MarketSegments { get; }
		IBaseRepository<MealPlan> MealPlans { get; }
		IBaseRepository<RoomStatusLookup> RoomStatuses { get; }
		IBaseRepository<ExtraService> ExtraServices { get; }
		IBaseRepository<CompanyProfile> CompanyProfiles { get; }

		//  SaveChanges
		Task<int> CompleteAsync();
    }
}
