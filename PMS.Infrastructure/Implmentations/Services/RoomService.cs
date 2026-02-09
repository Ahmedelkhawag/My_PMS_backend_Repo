using Microsoft.EntityFrameworkCore;
using PMS.Application.DTOs.Common;
using PMS.Application.DTOs.Rooms;
using PMS.Application.Interfaces.Services;
using PMS.Application.Interfaces.UOF;
using PMS.Domain.Entities;
// using PMS.Domain.Enums; // مش محتاجينه

namespace PMS.Infrastructure.Implmentations.Services
{
	public class RoomService : IRoomService
	{
		private readonly IUnitOfWork _unitOfWork;

		public RoomService(IUnitOfWork unitOfWork)
		{
			_unitOfWork = unitOfWork;
		}

		// 1. استرجاع كل الغرف (مع الألوان)
		public async Task<IEnumerable<RoomDto>> GetAllRoomsAsync(int? floor, int? roomTypeId, string? status)
		{
			var query = _unitOfWork.Rooms.GetQueryable()
				.Include(r => r.RoomType)
				.Include(r => r.RoomStatus) // ضروري
				.AsQueryable();

			if (floor.HasValue)
				query = query.Where(r => r.FloorNumber == floor);

			if (roomTypeId.HasValue)
				query = query.Where(r => r.RoomTypeId == roomTypeId);

			if (!string.IsNullOrEmpty(status))
			{
				query = query.Where(r => r.RoomStatus.Name == status);
			}

			var rooms = await query.ToListAsync();

			return rooms.Select(r => new RoomDto
			{
				Id = r.Id,
				RoomNumber = r.RoomNumber,
				FloorNumber = r.FloorNumber,

				// 👇 البيانات المحسنة
				Status = r.RoomStatus?.Name ?? "Unknown",
				StatusColor = r.RoomStatus?.Color ?? "#808080", // 👈 دي اللي كانت ناقصة

				RoomType = r.RoomType?.Name ?? "N/A",
				Price = r.RoomType?.BasePrice ?? 0,
				MaxAdults = r.RoomType?.MaxAdults ?? 0
			});
		}

		// 2. استرجاع غرفة واحدة (كانت ناقصة)
		public async Task<ResponseObjectDto<RoomDto>> GetRoomByIdAsync(int id)
		{
			var response = new ResponseObjectDto<RoomDto>();

			var room = await _unitOfWork.Rooms.GetQueryable()
				.Include(r => r.RoomType)
				.Include(r => r.RoomStatus)
				.FirstOrDefaultAsync(r => r.Id == id);

			if (room == null)
			{
				response.IsSuccess = false;
				response.Message = "الغرفة غير موجودة";
				response.StatusCode = 404;
				return response;
			}

			response.IsSuccess = true;
			response.Data = new RoomDto
			{
				Id = room.Id,
				RoomNumber = room.RoomNumber,
				FloorNumber = room.FloorNumber,
				Status = room.RoomStatus?.Name ?? "Unknown",
				StatusColor = room.RoomStatus?.Color ?? "#808080", // 👈 اللون
				RoomType = room.RoomType?.Name ?? "N/A",
				Price = room.RoomType?.BasePrice ?? 0,
				MaxAdults = room.RoomType?.MaxAdults ?? 0
			};
			response.StatusCode = 200;

			return response;
		}

		// 3. إنشاء غرفة
		public async Task<ResponseObjectDto<RoomDto>> CreateRoomAsync(CreateRoomDto dto)
		{
			var response = new ResponseObjectDto<RoomDto>();

			var existingRoom = await _unitOfWork.Rooms.FindAsync(r => r.RoomNumber == dto.RoomNumber);
			if (existingRoom != null)
			{
				response.IsSuccess = false;
				response.Message = $"الغرفة رقم {dto.RoomNumber} موجودة بالفعل!";
				response.StatusCode = 400;
				return response;
			}

			var roomType = await _unitOfWork.RoomTypes.GetByIdAsync(dto.RoomTypeId);
			if (roomType == null)
			{
				response.IsSuccess = false;
				response.Message = "نوع الغرفة غير صحيح!";
				response.StatusCode = 404;
				return response;
			}

			var room = new Room
			{
				RoomNumber = dto.RoomNumber,
				FloorNumber = dto.FloorNumber,
				RoomTypeId = dto.RoomTypeId,
				Notes = dto.Notes,
				RoomStatusId = 1, // Default Clean
				IsActive = true
			};

			await _unitOfWork.Rooms.AddAsync(room);
			await _unitOfWork.CompleteAsync();

			var status = await _unitOfWork.RoomStatuses.GetByIdAsync(room.RoomStatusId);

			response.IsSuccess = true;
			response.Message = "تم إضافة الغرفة بنجاح";
			response.Data = new RoomDto
			{
				Id = room.Id,
				RoomNumber = room.RoomNumber,
				FloorNumber = room.FloorNumber,
				Status = status?.Name ?? "Clean",
				StatusColor = status?.Color ?? "#008000", // 👈
				RoomType = roomType.Name,
				Price = roomType.BasePrice,
				MaxAdults = roomType.MaxAdults
			};
			response.StatusCode = 201;

			return response;
		}

		// 4. تحديث غرفة
		public async Task<ResponseObjectDto<RoomDto>> UpdateRoomAsync(UpdateRoomDto dto)
		{
			var response = new ResponseObjectDto<RoomDto>();

			var room = await _unitOfWork.Rooms.GetQueryable()
				.Include(r => r.RoomType)
				.Include(r => r.RoomStatus)
				.FirstOrDefaultAsync(r => r.Id == dto.Id);

			if (room == null)
			{
				response.IsSuccess = false;
				response.Message = "الغرفة غير موجودة!";
				response.StatusCode = 404;
				return response;
			}

			// التحقق من التكرار
			var duplicateRoom = await _unitOfWork.Rooms.FindAsync(r => r.RoomNumber == dto.RoomNumber && r.Id != dto.Id);
			if (duplicateRoom != null)
			{
				response.IsSuccess = false;
				response.Message = $"رقم الغرفة {dto.RoomNumber} مستخدم بالفعل!";
				response.StatusCode = 400;
				return response;
			}

			room.RoomNumber = dto.RoomNumber;
			room.FloorNumber = dto.FloorNumber;
			room.RoomTypeId = dto.RoomTypeId;
			room.Notes = dto.Notes;

			if (!string.IsNullOrEmpty(dto.Status))
			{
				var statusObj = await _unitOfWork.RoomStatuses.FindAsync(s => s.Name == dto.Status);
				if (statusObj != null)
				{
					room.RoomStatusId = statusObj.Id;
				}
			}

			_unitOfWork.Rooms.Update(room);
			await _unitOfWork.CompleteAsync();

			response.IsSuccess = true;
			response.Message = "تم تحديث بيانات الغرفة بنجاح";
			response.Data = new RoomDto
			{
				Id = room.Id,
				RoomNumber = room.RoomNumber,
				FloorNumber = room.FloorNumber,
				Status = room.RoomStatus?.Name ?? dto.Status,
				StatusColor = room.RoomStatus?.Color ?? "#808080", // 👈
				RoomType = room.RoomType?.Name ?? "",
				Price = room.RoomType?.BasePrice ?? 0,
				MaxAdults = room.RoomType?.MaxAdults ?? 0
			};

			return response;
		}

		// 5. حذف
		public async Task<ResponseObjectDto<bool>> DeleteRoomAsync(int id)
		{
			var response = new ResponseObjectDto<bool>();
			var room = await _unitOfWork.Rooms.GetByIdAsync(id);
			if (room == null)
			{
				response.IsSuccess = false;
				response.Message = "الغرفة غير موجودة";
				response.StatusCode = 404;
				return response;
			}
			room.IsActive = false;
			_unitOfWork.Rooms.Update(room);
			await _unitOfWork.CompleteAsync();

			response.IsSuccess = true;
			response.Message = "تم حذف الغرفة (أرشفة) بنجاح";
			response.Data = true;
			return response;
		}

		// 6. 👇👇 دالة تغيير الحالة (Housekeeping) - دي الجديدة 👇👇
		public async Task<ResponseObjectDto<bool>> ChangeRoomStatusAsync(int roomId, int statusId, string? notes)
		{
			var response = new ResponseObjectDto<bool>();

			var room = await _unitOfWork.Rooms.GetByIdAsync(roomId);
			if (room == null)
			{
				response.IsSuccess = false;
				response.Message = "الغرفة غير موجودة";
				response.StatusCode = 404;
				return response;
			}

			var statusObj = await _unitOfWork.RoomStatuses.GetByIdAsync(statusId);
			if (statusObj == null)
			{
				response.IsSuccess = false;
				response.Message = "حالة الغرفة غير صحيحة";
				response.StatusCode = 400;
				return response;
			}

			room.RoomStatusId = statusId;

			if (!string.IsNullOrEmpty(notes))
			{
				room.Notes = (room.Notes ?? "") + $" | {DateTime.Now:dd/MM}: {notes}";
			}

			_unitOfWork.Rooms.Update(room);
			await _unitOfWork.CompleteAsync();

			response.IsSuccess = true;
			response.Message = "تم تغيير حالة الغرفة بنجاح";
			response.Data = true;
			response.StatusCode = 200;

			return response;
		}
	}
}