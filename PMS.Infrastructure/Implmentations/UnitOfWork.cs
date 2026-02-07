using PMS.Application.Interfaces.Repositories;
using PMS.Application.Interfaces.UOF;
using PMS.Domain.Entities;
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
        public UnitOfWork(ApplicationDbContext context)
        {
            _context = context;

            Countries = new BaseRepository<Country>(_context);
            Statuses = new BaseRepository<Status>(_context);
            EmployeeDocuments = new BaseRepository<EmployeeDocument>(_context);
            RefreshTokens = new BaseRepository<RefreshToken>(_context);

            Rooms = new BaseRepository<Room>(_context);
            RoomTypes = new BaseRepository<RoomType>(_context);
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
