using System;
using System.ComponentModel.DataAnnotations;

namespace PMS.Application.DTOs.Guests
{
    public record CreateGuestDto
    {
        [Required(ErrorMessage = "الاسم بالكامل مطلوب")]
        public string FullName { get; init; } = string.Empty;

        [Required(ErrorMessage = "رقم الهاتف مطلوب")]
        [Phone(ErrorMessage = "رقم الهاتف غير صحيح")]
        public string PhoneNumber { get; init; } = string.Empty;

        [Required(ErrorMessage = "رقم الهوية مطلوب")]
        public string NationalId { get; init; } = string.Empty;

        public string? Nationality { get; init; }
        public DateTime? DateOfBirth { get; init; }

        [EmailAddress(ErrorMessage = "البريد الإلكتروني غير صحيح")]
        public string? Email { get; init; }

        public string? Address { get; init; }
        public string? City { get; init; }

        [MaxLength(20, ErrorMessage = "رقم السيارة لا يجب أن يتجاوز 20 حرفاً")]
        public string? CarNumber { get; init; }

        [MaxLength(50, ErrorMessage = "الرقم الضريبي لا يجب أن يتجاوز 50 حرفاً")]
        [RegularExpression(@"^[a-zA-Z0-9]*$", ErrorMessage = "الرقم الضريبي يجب أن يحتوي على أرقام وحروف إنجليزية فقط")]
        public string? VatNumber { get; init; }

        public string? Notes { get; init; }
    }
}
