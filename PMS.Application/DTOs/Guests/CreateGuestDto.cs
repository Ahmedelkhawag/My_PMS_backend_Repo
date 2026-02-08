using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace PMS.Application.DTOs.Guests
{
    public class CreateGuestDto
    {
		[Required(ErrorMessage = "الاسم بالكامل مطلوب")]
		public string FullName { get; set; }

		[Required(ErrorMessage = "رقم الهاتف مطلوب")]
		[Phone(ErrorMessage = "رقم الهاتف غير صحيح")]
		public string PhoneNumber { get; set; }

		[Required(ErrorMessage = "رقم الهوية مطلوب")]
		public string NationalId { get; set; }

		public string? Nationality { get; set; }
		public DateTime? DateOfBirth { get; set; }

		[EmailAddress(ErrorMessage = "البريد الإلكتروني غير صحيح")]
		public string? Email { get; set; }

		public string? Address { get; set; }
		public string? City { get; set; }
		[MaxLength(20, ErrorMessage = "رقم السيارة لا يجب أن يتجاوز 20 حرفاً")]
		public string? CarNumber { get; set; }
		[MaxLength(50, ErrorMessage = "الرقم الضريبي لا يجب أن يتجاوز 50 حرفاً")]
		[RegularExpression(@"^[a-zA-Z0-9]*$", ErrorMessage = "الرقم الضريبي يجب أن يحتوي على أرقام وحروف إنجليزية فقط")]
		public string? VatNumber { get; set; }
		
		public string? Notes { get; set; }
	}
}
