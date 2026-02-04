using PMS.Application.Interfaces.Repositories;
using PMS.Domain.Entities;
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

        //  SaveChanges
        Task<int> CompleteAsync();
    }
}
