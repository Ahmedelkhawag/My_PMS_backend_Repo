using PMS.Application.DTOs.Common;
using PMS.Application.DTOs.Guests;
using PMS.Application.Interfaces.Services;
using PMS.Application.Interfaces.UOF;
using PMS.Domain.Entities;
using PMS.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace PMS.Infrastructure.Implmentations.Services
{
    public class GuestService : IGuestService
	{
		private readonly IUnitOfWork _unitOfWork;

		public GuestService(IUnitOfWork unitOfWork)
		{
			_unitOfWork = unitOfWork;
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
			response.Data = new GuestDto
			{
				Id = guest.Id,
				FullName = guest.FullName,
				PhoneNumber = guest.PhoneNumber,
				NationalId = guest.NationalId,
				Nationality = guest.Nationality,
				LoyaltyLevel = guest.LoyaltyLevel.ToString(), // بنرجعها نص
				DateOfBirth = guest.DateOfBirth,
				Email = guest.Email,
				CarNumber = guest.CarNumber,
				Notes = guest.Notes,
				IsActive = guest.IsActive
			};

			return response;
		}


		public async Task<IEnumerable<GuestDto>> GetAllGuestsAsync(string? search)
		{
			
			var guests = await _unitOfWork.Guests.FindAllAsync(g =>
				g.IsActive && 
				(string.IsNullOrEmpty(search) ||
				 g.FullName.Contains(search) ||
				 g.PhoneNumber.Contains(search) ||
				 g.NationalId.Contains(search))
			);

	
			return guests.Select(g => new GuestDto
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
				IsActive = g.IsActive
			}).OrderByDescending(g => g.Id); 

		}



		public async Task<ResponseObjectDto<GuestDto>> UpdateGuestAsync(UpdateGuestDto dto)
		{
			var response = new ResponseObjectDto<GuestDto>();

			// أ) التأكد من وجود النزيل
			var guest = await _unitOfWork.Guests.GetByIdAsync(dto.Id);
			if (guest == null)
			{
				response.IsSuccess = false;
				response.Message = "النزيل غير موجود!";
				response.StatusCode = 404;
				return response;
			}

			// ب) التحقق من عدم تكرار رقم الهاتف (مع استثناء النزيل الحالي)
			// الشرط: الرقم موجود && الـ Id لا يساوي الـ Id الحالي
			var duplicatePhone = await _unitOfWork.Guests.FindAsync(g => g.PhoneNumber == dto.PhoneNumber && g.Id != dto.Id);
			if (duplicatePhone != null)
			{
				response.IsSuccess = false;
				response.Message = "رقم الهاتف مستخدم بالفعل لنزيل آخر!";
				response.StatusCode = 400;
				return response;
			}

			// ج) التحقق من عدم تكرار الهوية (مع استثناء الحالي)
			var duplicateId = await _unitOfWork.Guests.FindAsync(g => g.NationalId == dto.NationalId && g.Id != dto.Id);
			if (duplicateId != null)
			{
				response.IsSuccess = false;
				response.Message = "رقم الهوية مستخدم بالفعل لنزيل آخر!";
				response.StatusCode = 400;
				return response;
			}


			var duplicateEmail = await _unitOfWork.Guests.FindAsync(g => g.Email == dto.Email && g.Id != dto.Id);
			if (duplicateId != null)
			{
				response.IsSuccess = false;
				response.Message = "هذا الايميل مستخدم بالفعل لنزيل آخر!";
				response.StatusCode = 400;
				return response;
			}
			// د) تحديث البيانات
			guest.FullName = dto.FullName;
			guest.PhoneNumber = dto.PhoneNumber;
			guest.NationalId = dto.NationalId;
			guest.Nationality = dto.Nationality ?? "Unknown";
			guest.DateOfBirth = dto.DateOfBirth;
			guest.Email = dto.Email;
			guest.Address = dto.Address;
			guest.City = dto.City;
			guest.CarNumber = dto.CarNumber;
			guest.VatNumber = dto.VatNumber;
			guest.Notes = dto.Notes;
			guest.LoyaltyLevel = dto.LoyaltyLevel; // تحديث المستوى

			// هـ) الحفظ
			_unitOfWork.Guests.Update(guest);
			await _unitOfWork.CompleteAsync();

			// و) إرجاع النتيجة
			response.IsSuccess = true;
			response.Message = "تم تحديث بيانات النزيل بنجاح";
			response.Data = new GuestDto
			{
				Id = guest.Id,
				FullName = guest.FullName,
				PhoneNumber = guest.PhoneNumber,
				NationalId = guest.NationalId,
				Nationality = guest.Nationality,
				LoyaltyLevel = guest.LoyaltyLevel.ToString(),
				DateOfBirth = guest.DateOfBirth,
				Email = guest.Email,
				CarNumber = guest.CarNumber,
				Notes = guest.Notes,
				IsActive = guest.IsActive
			};

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
	}
}


