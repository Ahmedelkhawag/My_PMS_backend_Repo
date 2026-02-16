using PMS.Application.Interfaces.Repositories;
using PMS.Application.Interfaces.UOF;
using PMS.Domain.Entities;
using PMS.Domain.Entities.Configuration;
using PMS.Infrastructure.Context;
using System;
using System.Collections.Generic;
using System.Text;

namespace PMS.Infrastructure.Implmentations
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;

        public IBaseRepository<Country> Countries { get; private set; }
        public IBaseRepository<Status> Statuses { get; private set; }
        public IBaseRepository<EmployeeDocument> EmployeeDocuments { get; private set; }
        public IBaseRepository<RefreshToken> RefreshTokens { get; private set; }
        public IBaseRepository<Room> Rooms { get; private set; }
        public IBaseRepository<RoomType> RoomTypes { get; private set; }
		public IBaseRepository<Guest> Guests { get; private set; }
		public IBaseRepository<Reservation> Reservations { get; private set; }
		public IBaseRepository<GuestFolio> GuestFolios { get; private set; }
		public IBaseRepository<FolioTransaction> FolioTransactions { get; private set; }
        public IBaseRepository<ReservationService> ReservationServices { get; private set; }
		public IBaseRepository<BookingSource> BookingSources { get; private set; }
		public IBaseRepository<MarketSegment> MarketSegments { get; private set; }
		public IBaseRepository<MealPlan> MealPlans { get; private set; }
		public IBaseRepository<RoomStatusLookup> RoomStatuses { get; private set; }
		public IBaseRepository<ExtraService> ExtraServices { get; private set; }
		public IBaseRepository<CompanyProfile> CompanyProfiles { get; private set; }
		public UnitOfWork(ApplicationDbContext context)
        {
            _context = context;

            Countries = new BaseRepository<Country>(_context);
            Statuses = new BaseRepository<Status>(_context);
            EmployeeDocuments = new BaseRepository<EmployeeDocument>(_context);
            RefreshTokens = new BaseRepository<RefreshToken>(_context);

            Rooms = new BaseRepository<Room>(_context);
            RoomTypes = new BaseRepository<RoomType>(_context);
            Guests = new BaseRepository<Guest>(_context);
            Reservations = new BaseRepository<Reservation>(_context);
			GuestFolios = new BaseRepository<GuestFolio>(_context);
			FolioTransactions = new BaseRepository<FolioTransaction>(_context);
            ReservationServices = new BaseRepository<ReservationService>(_context);
			BookingSources = new BaseRepository<BookingSource>(_context);
			MarketSegments = new BaseRepository<MarketSegment>(_context);
			MealPlans = new BaseRepository<MealPlan>(_context);
			RoomStatuses = new BaseRepository<RoomStatusLookup>(_context);
			ExtraServices = new BaseRepository<ExtraService>(_context);
			CompanyProfiles = new BaseRepository<CompanyProfile>(_context);
		}

        public async Task<int> CompleteAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
