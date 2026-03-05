using PMS.Domain.Interfaces;
using System;

namespace PMS.Domain.Entities
{
    /// <summary>
    /// Base entity that provides Id, auditing, and soft-delete fields.
    /// Back-Office entities should inherit from this class to participate
    /// in the global auditing and soft-delete behavior configured in ApplicationDbContext.
    /// </summary>
    public abstract class BaseAuditableEntity : ISoftDeletable, IAuditable
    {
        public int Id { get; set; }

        // Soft delete
        public bool IsDeleted { get; set; } = false;
        public DateTime? DeletedAt { get; set; }
        public string? DeletedBy { get; set; }

        // Auditing
        public string? CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? LastModifiedBy { get; set; }
        public DateTime? LastModifiedAt { get; set; }
    }
}

