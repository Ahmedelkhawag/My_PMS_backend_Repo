using PMS.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace PMS.Domain.Entities
{

    public class RefreshToken:ISoftDeletable, IAuditable
	{
        [Key]
        public int Id { get; set; }

        public string Token { get; set; }
        public DateTime ExpiresOn { get; set; }
        public bool IsExpired => DateTime.UtcNow >= ExpiresOn;

        public DateTime CreatedOn { get; set; }
        public DateTime? RevokedOn { get; set; }

        public bool IsActive => RevokedOn == null && !IsExpired;


        public string AppUserId { get; set; }

        [ForeignKey(nameof(AppUserId))]
        public AppUser AppUser { get; set; }
		public string? CreatedBy { get; set; }
		public DateTime CreatedAt { get; set; }

		public string? LastModifiedBy { get; set; }
		public DateTime? LastModifiedAt { get; set; }
		public bool IsDeleted { get; set; } = false;
		public DateTime? DeletedAt { get; set; }
		public string? DeletedBy { get; set; }
	}
}
