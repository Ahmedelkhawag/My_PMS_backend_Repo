using AutoMapper;
using Microsoft.EntityFrameworkCore;
using PMS.Application.DTOs.Common;
using PMS.Application.DTOs.Dashboard;
using PMS.Application.DTOs.Guests;
using PMS.Application.Interfaces.Services;
using PMS.Application.Interfaces.UOF;
using PMS.Domain.Entities;
using PMS.Domain.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.RegularExpressions;

namespace PMS.Infrastructure.Implmentations.Services
{
    public class GuestService : IGuestService
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly IMapper _mapper;

		public GuestService(IUnitOfWork unitOfWork, IMapper mapper)
		{
			_unitOfWork = unitOfWork;
			_mapper = mapper;
		}

		public async Task<ResponseObjectDto<GuestDto>> AddGuestAsync(CreateGuestDto dto)
		{
			var response = new ResponseObjectDto<GuestDto>();

			
			var existingPhone = await _unitOfWork.Guests.FindAsync(g => g.PhoneNumber == dto.PhoneNumber);
			if (existingPhone != null)
			{
				response.IsSuccess = false;
				response.Message = "رقم الهاتف مسجل بالفعل لنزيل آخر!";
				response.StatusCode = 400;
				return response;
			}

			
			var existingId = await _unitOfWork.Guests.FindAsync(g => g.NationalId == dto.NationalId);
			if (existingId != null)
			{
				response.IsSuccess = false;
				response.Message = "رقم الهوية مسجل بالفعل!";
				response.StatusCode = 400;
				return response;
			}


			if (!string.IsNullOrEmpty(dto.Email))
			{
				var existingEmail = await _unitOfWork.Guests.FindAsync(g => g.Email == dto.Email);
				if (existingEmail != null)
				{
					response.IsSuccess = false;
					response.Message = "البريد الإلكتروني مسجل لنزيل آخر!";
					response.StatusCode = 400;
					return response;
				}
			}

			var guest = new Guest
			{
				FullName = dto.FullName,
				PhoneNumber = dto.PhoneNumber,
				NationalId = dto.NationalId,
				Nationality = dto.Nationality ?? "Unknown",
				DateOfBirth = dto.DateOfBirth,
				Email = dto.Email,
				Address = dto.Address,
				City = dto.City,
				CarNumber = dto.CarNumber,
				VatNumber = dto.VatNumber,
				Notes = dto.Notes,
				LoyaltyLevel = LoyaltyLevel.Bronze, 
				IsActive = true,
				CreatedAt = DateTime.UtcNow
			};

			await _unitOfWork.Guests.AddAsync(guest);
			await _unitOfWork.CompleteAsync();

			
			response.IsSuccess = true;
			response.Message = "تم إضافة النزيل بنجاح";
			response.StatusCode = 201;
			response.Data = _mapper.Map<GuestDto>(guest);

			return response;
		}


		public async Task<ResponseObjectDto<PagedResult<GuestDto>>> GetAllGuestsAsync(string? search, int pageNumber, int pageSize)
		{
			var response = new ResponseObjectDto<PagedResult<GuestDto>>();

			var query = _unitOfWork.Guests.GetQueryable()
				.Where(g => g.IsActive)
				.AsQueryable();

			if (!string.IsNullOrEmpty(search))
			{
				query = query.Where(g =>
					g.FullName.Contains(search) ||
					g.PhoneNumber.Contains(search) ||
					g.NationalId.Contains(search));
			}

			var totalCount = await query.CountAsync();

			if (pageNumber < 1) pageNumber = 1;
			if (pageSize <= 0) pageSize = 10;

			var skip = (pageNumber - 1) * pageSize;

			var items = await query
				.OrderByDescending(g => g.Id)
				.Skip(skip)
				.Take(pageSize)
				.Select(g => new GuestDto
				{
					Id = g.Id,
					FullName = g.FullName,
					PhoneNumber = g.PhoneNumber,
					NationalId = g.NationalId,
					Nationality = g.Nationality,
					LoyaltyLevel = g.LoyaltyLevel.ToString(),
					DateOfBirth = g.DateOfBirth,
					Email = g.Email,
					CarNumber = g.CarNumber,
					Notes = g.Notes,
					IsActive = g.IsActive,
					CreatedBy = g.CreatedBy,
					CreatedAt = g.CreatedAt,
					UpdatedBy = g.LastModifiedBy,
					UpdatedAt = g.LastModifiedAt
				})
				.ToListAsync();

			var paged = new PagedResult<GuestDto>(items, totalCount, pageNumber, pageSize);

			response.IsSuccess = true;
			response.StatusCode = 200;
			response.Message = "تم استرجاع قائمة النزلاء بنجاح";
			response.Data = paged;

			return response;
		}

		public async Task<ResponseObjectDto<GuestDto>> GetGuestByIdAsync(int id)
		{
			var response = new ResponseObjectDto<GuestDto>();

			var guest = await _unitOfWork.Guests.GetByIdAsync(id);
			if (guest == null)
			{
				response.IsSuccess = false;
				response.Message = "النزيل غير موجود";
				response.StatusCode = 404;
				return response;
			}

			response.IsSuccess = true;
			response.StatusCode = 200;
			response.Message = "تم استرجاع بيانات النزيل بنجاح";
			response.Data = _mapper.Map<GuestDto>(guest);

			return response;
		}



		public async Task<ResponseObjectDto<GuestDto>> UpdateGuestAsync(int id, UpdateGuestDto dto)
		{
			var response = new ResponseObjectDto<GuestDto>();

			if (dto == null || !HasAnyUpdateField(dto))
			{
				response.IsSuccess = false;
				response.Message = "يجب إرسال حقل واحد على الأقل للتحديث";
				response.StatusCode = 400;
				return response;
			}

			var guest = await _unitOfWork.Guests.GetByIdAsync(id);
			if (guest == null)
			{
				response.IsSuccess = false;
				response.Message = "النزيل غير موجود!";
				response.StatusCode = 404;
				return response;
			}

			// Validate format only when a value is actually provided (non-empty)
			if (dto.PhoneNumber != null && dto.PhoneNumber.Length > 0 && !new PhoneAttribute().IsValid(dto.PhoneNumber))
			{
				response.IsSuccess = false;
				response.Message = "رقم الهاتف غير صحيح";
				response.StatusCode = 400;
				return response;
			}
			if (dto.Email != null && dto.Email.Length > 0 && !new EmailAddressAttribute().IsValid(dto.Email))
			{
				response.IsSuccess = false;
				response.Message = "البريد الإلكتروني غير صحيح";
				response.StatusCode = 400;
				return response;
			}
			if (dto.VatNumber != null && dto.VatNumber.Length > 0 && !Regex.IsMatch(dto.VatNumber, @"^[a-zA-Z0-9]*$"))
			{
				response.IsSuccess = false;
				response.Message = "الرقم الضريبي يجب أن يحتوي على أرقام وحروف إنجليزية فقط";
				response.StatusCode = 400;
				return response;
			}

			// Update only provided fields (partial update)
			if (dto.PhoneNumber != null)
			{
				var duplicatePhone = await _unitOfWork.Guests.FindAsync(g => g.PhoneNumber == dto.PhoneNumber && g.Id != id);
				if (duplicatePhone != null)
				{
					response.IsSuccess = false;
					response.Message = "رقم الهاتف مستخدم بالفعل لنزيل آخر!";
					response.StatusCode = 400;
					return response;
				}
				guest.PhoneNumber = dto.PhoneNumber;
			}

			if (dto.NationalId != null)
			{
				var duplicateId = await _unitOfWork.Guests.FindAsync(g => g.NationalId == dto.NationalId && g.Id != id);
				if (duplicateId != null)
				{
					response.IsSuccess = false;
					response.Message = "رقم الهوية مستخدم بالفعل لنزيل آخر!";
					response.StatusCode = 400;
					return response;
				}
				guest.NationalId = dto.NationalId;
			}

			if (dto.Email != null)
			{
				var duplicateEmail = await _unitOfWork.Guests.FindAsync(g => g.Email == dto.Email && g.Id != id);
				if (duplicateEmail != null)
				{
					response.IsSuccess = false;
					response.Message = "هذا الايميل مستخدم بالفعل لنزيل آخر!";
					response.StatusCode = 400;
					return response;
				}
				guest.Email = dto.Email;
			}

			if (dto.FullName != null) guest.FullName = dto.FullName;
			if (dto.Nationality != null) guest.Nationality = dto.Nationality;
			if (dto.DateOfBirth.HasValue) guest.DateOfBirth = dto.DateOfBirth;
			if (dto.Address != null) guest.Address = dto.Address;
			if (dto.City != null) guest.City = dto.City;
			if (dto.CarNumber != null) guest.CarNumber = dto.CarNumber;
			if (dto.VatNumber != null) guest.VatNumber = dto.VatNumber;
			if (dto.Notes != null) guest.Notes = dto.Notes;
			if (dto.LoyaltyLevel.HasValue) guest.LoyaltyLevel = dto.LoyaltyLevel.Value;

			_unitOfWork.Guests.Update(guest);
			await _unitOfWork.CompleteAsync();

			// و) إرجاع النتيجة
			response.IsSuccess = true;
			response.Message = "تم تحديث بيانات النزيل بنجاح";
			response.Data = _mapper.Map<GuestDto>(guest);

			return response;
		}



		public async Task<ResponseObjectDto<bool>> DeleteGuestAsync(int id)
		{
			var response = new ResponseObjectDto<bool>();

			var guest = await _unitOfWork.Guests.GetByIdAsync(id);
			if (guest == null)
			{
				response.IsSuccess = false;
				response.Message = "النزيل غير موجود";
				response.StatusCode = 404;
				return response;
			}

			// Soft Delete (إخفاء فقط)
			guest.IsActive = false;
			_unitOfWork.Guests.Update(guest);
			await _unitOfWork.CompleteAsync();

			response.IsSuccess = true;
			response.Message = "تم حذف النزيل (أرشفة) بنجاح";
			response.Data = true;

			return response;
		}


		// استرجاع نزيل تم أرشفته (Soft-Delete)
		public async Task<ResponseObjectDto<bool>> RestoreGuestAsync(int id)
		{
			var response = new ResponseObjectDto<bool>();

			// نستخدم IgnoreQueryFilters عشان نلاقي النزيل حتى لو IsDeleted = true
			var guest = await _unitOfWork.Guests.GetQueryable()
				.IgnoreQueryFilters()
				.FirstOrDefaultAsync(g => g.Id == id);

			if (guest == null)
			{
				response.IsSuccess = false;
				response.Message = "النزيل غير موجود";
				response.StatusCode = 404;
				return response;
			}

			if (guest.IsActive && !guest.IsDeleted)
			{
				response.IsSuccess = false;
				response.Message = "النزيل نشط بالفعل";
				response.StatusCode = 400;
				return response;
			}

			guest.IsActive = true;
			guest.IsDeleted = false;
			guest.DeletedAt = null;
			guest.DeletedBy = null;

			_unitOfWork.Guests.Update(guest);
			await _unitOfWork.CompleteAsync();

			response.IsSuccess = true;
			response.Message = "تم استرجاع النزيل بنجاح";
			response.StatusCode = 200;
			response.Data = true;

			return response;
		}


		//public async Task<ResponseObjectDto<IEnumerable<GuestSearchDto>>> SearchGuestsAsync(string searchTerm)
		//{
		//	// لو البحث فاضي نرجع قائمة فاضية أو أول 10 نزلاء (حسب الرغبة)
		//	if (string.IsNullOrWhiteSpace(searchTerm))
		//	{
		//		return new ResponseObjectDto<IEnumerable<GuestSearchDto>>
		//		{
		//			IsSuccess = true,
		//			Data = new List<GuestSearchDto>(),
		//			Message = "يرجى إدخال كلمة للبحث"
		//		};
		//	}

		//	var guests = await _unitOfWork.Guests.GetQueryable()
		//		.Where(g => g.FullName.Contains(searchTerm) ||
		//					g.PhoneNumber.Contains(searchTerm) ||
		//					g.NationalId.Contains(searchTerm))
		//		.Take(20) // بنحدد العدد عشان الأداء (Autocomplete)
		//		.Select(g => new GuestSearchDto
		//		{
		//			Id = g.Id,
		//			FullName = g.FullName,
		//			PhoneNumber = g.PhoneNumber,
		//			NationalId = g.NationalId
		//		})
		//		.ToListAsync();

		//	return new ResponseObjectDto<IEnumerable<GuestSearchDto>>
		//	{
		//		IsSuccess = true,
		//		Data = guests,
		//		Message = $"تم العثور على {guests.Count} نتيجة",
		//		StatusCode = 200
		//	};
		//}

		// إحصائيات النزلاء
		public async Task<ResponseObjectDto<GuestStatsDto>> GetGuestStatsAsync()
		{
			var response = new ResponseObjectDto<GuestStatsDto>();

			var today = DateTime.UtcNow.Date;
			var firstDayOfMonth = new DateTime(today.Year, today.Month, 1);
			var firstDayOfNextMonth = firstDayOfMonth.AddMonths(1);

			var guestsQuery = _unitOfWork.Guests
				.GetQueryable()
				.Where(g => !g.IsDeleted);

			var totalGuests = await guestsQuery.CountAsync();
			var newGuestsThisMonth = await guestsQuery.CountAsync(g =>
				g.CreatedAt >= firstDayOfMonth && g.CreatedAt < firstDayOfNextMonth);

			var stats = new GuestStatsDto
			{
				TotalGuests = totalGuests,
				NewGuestsThisMonth = newGuestsThisMonth
			};

			response.IsSuccess = true;
			response.StatusCode = 200;
			response.Message = "Guest statistics retrieved successfully";
			response.Data = stats;

			return response;
		}

		private static bool HasAnyUpdateField(UpdateGuestDto dto)
		{
			return dto.FullName != null || dto.PhoneNumber != null || dto.NationalId != null
				|| dto.Nationality != null || dto.DateOfBirth.HasValue || dto.Email != null
				|| dto.Address != null || dto.City != null || dto.CarNumber != null
				|| dto.VatNumber != null || dto.Notes != null || dto.LoyaltyLevel.HasValue;
		}
	}
}


