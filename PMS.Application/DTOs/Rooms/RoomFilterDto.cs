using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PMS.Application.DTOs.Rooms
{
    public class RoomFilterDto : IValidatableObject
    {
        public int? Floor { get; set; }
        public int? Type { get; set; }
        public string? Status { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;

        [DataType(DataType.Date)]
        public DateTime? FromDate { get; set; }

        [DataType(DataType.Date)]
        public DateTime? ToDate { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (FromDate.HasValue || ToDate.HasValue)
            {
                if (!FromDate.HasValue)
                {
                    yield return new ValidationResult("يجب تحديد تاريخ البداية (FromDate) عند تحديد تاريخ النهاية (ToDate).", new[] { nameof(FromDate) });
                }

                if (!ToDate.HasValue)
                {
                    yield return new ValidationResult("يجب تحديد تاريخ النهاية (ToDate) عند تحديد تاريخ البداية (FromDate).", new[] { nameof(ToDate) });
                }

                if (FromDate.HasValue && ToDate.HasValue && FromDate.Value >= ToDate.Value)
                {
                    yield return new ValidationResult("تاريخ النهاية يجب أن يكون بعد تاريخ البداية.", new[] { nameof(ToDate) });
                }
            }
        }
    }
}
