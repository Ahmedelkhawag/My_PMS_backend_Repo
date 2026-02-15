using Microsoft.EntityFrameworkCore;
using PMS.Application.DTOs.Common;
using PMS.Application.DTOs.Dashboard;
using PMS.Application.DTOs.Rooms;
using PMS.Application.Interfaces.Services;
using PMS.Application.Interfaces.UOF;
using PMS.Domain.Entities;
using PMS.Domain.Constants;
using PMS.Domain.Enums;
using PMS.Application.Validation;

namespace PMS.Infrastructure.Implmentations.Services
{
	public class RoomService : IRoomService
	{
		private readonly IUnitOfWork _unitOfWork;

		public RoomService(IUnitOfWork unitOfWork)
		{
			_unitOfWork = unitOfWork;
		}

		// 1. استرجاع كل الغرف (Dashboard: FO/HK status + current reservation) + Pagination
		public async Task<ResponseObjectDto<PagedResult<RoomDto>>> GetAllRoomsAsync(int? floor, int? roomTypeId, string? status, int pageNumber, int pageSize)
		{
			var response = new ResponseObjectDto<PagedResult<RoomDto>>();

			var query = _unitOfWork.Rooms.GetQueryable()
				.Include(r => r.RoomType)
				.Include(r => r.RoomStatus)
				.Where(r => r.IsActive)
				.AsQueryable();

			if (floor.HasValue)
				query = query.Where(r => r.FloorNumber == floor);

			if (roomTypeId.HasValue)
				query = query.Where(r => r.RoomTypeId == roomTypeId);

			if (!string.IsNullOrEmpty(status))
				query = query.Where(r => r.RoomStatus.Name == status);

			var totalCount = await query.CountAsync();

			if (pageNumber < 1) pageNumber = 1;
			if (pageSize <= 0) pageSize = 10;

			var skip = (pageNumber - 1) * pageSize;

			var rooms = await query
				.OrderBy(r => r.FloorNumber)
				.ThenBy(r => r.RoomNumber)
				.Skip(skip)
				.Take(pageSize)
				.ToListAsync();

			// Single batch: all active (CheckedIn) reservations with RoomId, with Guest
			var activeReservations = await _unitOfWork.Reservations.GetQueryable()
				.Include(r => r.Guest)
				.Where(r => r.Status == ReservationStatus.CheckIn && r.RoomId != null && !r.IsDeleted)
				.ToListAsync();

			var reservationByRoomId = activeReservations
				.Where(r => r.RoomId.HasValue)
				.ToDictionary(r => r.RoomId!.Value);

			var items = rooms.Select(r => MapRoomToDto(r, reservationByRoomId)).ToList();

			var paged = new PagedResult<RoomDto>(items, totalCount, pageNumber, pageSize);

			response.IsSuccess = true;
			response.StatusCode = 200;
			response.Message = "تم استرجاع قائمة الغرف بنجاح";
			response.Data = paged;

			return response;
		}

		// 2. استرجاع غرفة واحدة (Dashboard: FO/HK + current reservation)
		public async Task<ResponseObjectDto<RoomDto>> GetRoomByIdAsync(int id)
		{
			var response = new ResponseObjectDto<RoomDto>();

			var room = await _unitOfWork.Rooms.GetQueryable()
				.Include(r => r.RoomType)
				.Include(r => r.RoomStatus)
				.FirstOrDefaultAsync(r => r.Id == id && r.IsActive);

			if (room == null)
			{
				response.IsSuccess = false;
				response.Message = "الغرفة غير موجودة";
				response.StatusCode = 404;
				return response;
			}

			Reservation? currentReservation = null;
			var activeReservation = await _unitOfWork.Reservations.GetQueryable()
				.Include(r => r.Guest)
				.FirstOrDefaultAsync(r => r.RoomId == id && r.Status == ReservationStatus.CheckIn && !r.IsDeleted);
			if (activeReservation != null)
				currentReservation = activeReservation;

			var dto = MapRoomToDto(room, currentReservation != null ? new Dictionary<int, Reservation> { { id, currentReservation } } : new Dictionary<int, Reservation>());
			response.IsSuccess = true;
			response.Data = dto;
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
				RoomStatusId = 1,
				HKStatus = HKStatus.Clean,
				IsActive = true,
				MaxAdults = roomType.MaxAdults,
				BasePrice = roomType.BasePrice
			};

			await _unitOfWork.Rooms.AddAsync(room);
			await _unitOfWork.CompleteAsync();

			var reloaded = await _unitOfWork.Rooms.GetQueryable()
				.Include(r => r.RoomType)
				.FirstOrDefaultAsync(r => r.Id == room.Id);

			response.IsSuccess = true;
			response.Message = "تم إضافة الغرفة بنجاح";
			response.Data = MapRoomToDto(reloaded ?? room, new Dictionary<int, Reservation>());
			response.StatusCode = 201;

			return response;
		}

		// 4. تحديث غرفة (تحديث جزئي)
		public async Task<ResponseObjectDto<RoomDto>> UpdateRoomAsync(int id, UpdateRoomDto dto)
		{
			var response = new ResponseObjectDto<RoomDto>();

			if (dto == null || !HasAnyUpdateField(dto))
			{
				response.IsSuccess = false;
				response.Message = "يجب إرسال حقل واحد على الأقل للتحديث";
				response.StatusCode = 400;
				return response;
			}

			var room = await _unitOfWork.Rooms.GetQueryable()
				.Include(r => r.RoomType)
				.Include(r => r.RoomStatus)
				.FirstOrDefaultAsync(r => r.Id == id);

			if (room == null)
			{
				response.IsSuccess = false;
				response.Message = "الغرفة غير موجودة!";
				response.StatusCode = 404;
				return response;
			}

			// التحقق من تكرار رقم الغرفة فقط إذا تم إرساله
			if (!string.IsNullOrWhiteSpace(dto.RoomNumber))
			{
				var duplicateRoom = await _unitOfWork.Rooms.FindAsync(r => r.RoomNumber == dto.RoomNumber && r.Id != id);
				if (duplicateRoom != null)
				{
					response.IsSuccess = false;
					response.Message = $"رقم الغرفة {dto.RoomNumber} مستخدم بالفعل!";
					response.StatusCode = 400;
					return response;
				}

				room.RoomNumber = dto.RoomNumber;
			}

			// تحديث رقم الطابق إذا تم إرساله
			if (dto.FloorNumber.HasValue)
			{
				if (dto.FloorNumber.Value < 1 || dto.FloorNumber.Value > 100)
				{
					response.IsSuccess = false;
					response.Message = "رقم الطابق غير صحيح";
					response.StatusCode = 400;
					return response;
				}

				room.FloorNumber = dto.FloorNumber.Value;
			}

			// تحديث نوع الغرفة إذا تم إرساله
			if (dto.RoomTypeId.HasValue)
			{
				var roomType = await _unitOfWork.RoomTypes.GetByIdAsync(dto.RoomTypeId.Value);
				if (roomType == null)
				{
					response.IsSuccess = false;
					response.Message = "نوع الغرفة غير صحيح!";
					response.StatusCode = 404;
					return response;
				}

				room.RoomTypeId = dto.RoomTypeId.Value;
			}

			// تحديث الملاحظات إذا تم إرسالها (حتى لو كانت فارغة لمسحها)
			if (dto.Notes != null)
			{
				room.Notes = dto.Notes;
			}

			// تحديث حالة الغرفة إذا تم إرسالها (مع مزامنة HKStatus)
			if (!string.IsNullOrWhiteSpace(dto.Status))
			{
				var statusObj = await _unitOfWork.RoomStatuses.FindAsync(s => s.Name == dto.Status);
				if (statusObj == null)
				{
					response.IsSuccess = false;
					response.Message = "حالة الغرفة غير صحيحة";
					response.StatusCode = 400;
					return response;
				}

				room.RoomStatusId = statusObj.Id;
				room.HKStatus = MapRoomStatusIdToHKStatus(statusObj.Id);
			}

			_unitOfWork.Rooms.Update(room);
			await _unitOfWork.CompleteAsync();

			room = await _unitOfWork.Rooms.GetQueryable()
				.Include(r => r.RoomType)
				.FirstOrDefaultAsync(r => r.Id == id);

			response.IsSuccess = true;
			response.Message = "تم تحديث بيانات الغرفة بنجاح";
			response.StatusCode = 200;
			response.Data = MapRoomToDto(room!, new Dictionary<int, Reservation>());

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

		// استرجاع غرفة تم أرشفتها (Soft-Delete)
		public async Task<ResponseObjectDto<bool>> RestoreRoomAsync(int id)
		{
			var response = new ResponseObjectDto<bool>();

			// نستخدم IgnoreQueryFilters عشان نلاقي الغرفة حتى لو IsDeleted = true
			var room = await _unitOfWork.Rooms.GetQueryable()
				.IgnoreQueryFilters()
				.FirstOrDefaultAsync(r => r.Id == id);

			if (room == null)
			{
				response.IsSuccess = false;
				response.Message = "الغرفة غير موجودة";
				response.StatusCode = 404;
				return response;
			}

			if (room.IsActive && !room.IsDeleted)
			{
				response.IsSuccess = false;
				response.Message = "الغرفة نشطة بالفعل";
				response.StatusCode = 400;
				return response;
			}

			room.IsActive = true;
			room.IsDeleted = false;
			room.DeletedAt = null;
			room.DeletedBy = null;

			_unitOfWork.Rooms.Update(room);
			await _unitOfWork.CompleteAsync();

			response.IsSuccess = true;
			response.Message = "تم استرجاع الغرفة بنجاح";
			response.StatusCode = 200;
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
			room.HKStatus = MapRoomStatusIdToHKStatus(statusId);

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

		// 7. إحصائيات الغرف
		public async Task<ResponseObjectDto<RoomStatsDto>> GetRoomStatsAsync()
		{
			var response = new ResponseObjectDto<RoomStatsDto>();

			const int ROOM_STATUS_CLEAN = 1;
			const int ROOM_STATUS_DIRTY = 2;
			const int ROOM_STATUS_MAINTENANCE = 3;
			const int ROOM_STATUS_OUT_OF_ORDER = 4;
			const int ROOM_STATUS_OCCUPIED = 5;

			var roomsQuery = _unitOfWork.Rooms
				.GetQueryable()
				.Where(r => !r.IsDeleted && r.IsActive);

			var totalRooms = await roomsQuery.CountAsync();
			var availableRooms = await roomsQuery.CountAsync(r => r.RoomStatusId == ROOM_STATUS_CLEAN);
			var occupiedRooms = await roomsQuery.CountAsync(r => r.RoomStatusId == ROOM_STATUS_OCCUPIED);
			var dirtyRooms = await roomsQuery.CountAsync(r => r.RoomStatusId == ROOM_STATUS_DIRTY);
			var outOfServiceRooms = await roomsQuery.CountAsync(r =>
				r.RoomStatusId == ROOM_STATUS_MAINTENANCE || r.RoomStatusId == ROOM_STATUS_OUT_OF_ORDER);

			decimal occupancyPercentage = 0;
			if (totalRooms > 0 && occupiedRooms > 0)
			{
				occupancyPercentage = (decimal)occupiedRooms / totalRooms * 100m;
			}

			var stats = new RoomStatsDto
			{
				TotalRooms = totalRooms,
				AvailableRooms = availableRooms,
				OccupiedRooms = occupiedRooms,
				DirtyRooms = dirtyRooms,
				OutOfService = outOfServiceRooms,
				OccupancyPercentage = occupancyPercentage
			};

			response.IsSuccess = true;
			response.StatusCode = 200;
			response.Message = "Room statistics retrieved successfully";
			response.Data = stats;

			return response;
		}

		private static bool HasAnyUpdateField(UpdateRoomDto dto)
		{
			return !string.IsNullOrWhiteSpace(dto.RoomNumber)
			       || dto.FloorNumber.HasValue
			       || dto.RoomTypeId.HasValue
			       || dto.Notes != null
			       || !string.IsNullOrWhiteSpace(dto.Status);
		}

		private static RoomDto MapRoomToDto(Room room, Dictionary<int, Reservation> reservationByRoomId)
		{
			var isOccupied = reservationByRoomId.TryGetValue(room.Id, out var res);
			var maxAdults = room.MaxAdults > 0 ? room.MaxAdults : (room.RoomType?.MaxAdults ?? 0);
			var basePrice = room.BasePrice > 0 ? room.BasePrice : (room.RoomType?.BasePrice ?? 0);

			var dto = new RoomDto
			{
				Id = room.Id,
				RoomNumber = room.RoomNumber,
				FloorNumber = room.FloorNumber,
				RoomTypeName = room.RoomType?.Name ?? "N/A",
				RoomTypeCode = room.RoomType?.Name ?? "N/A",
				FoStatus = isOccupied ? "OCCUPIED" : "VACANT",
				HkStatus = room.HKStatus.ToString().ToUpperInvariant(),
				BedType = room.BedType.ToString().ToUpperInvariant(),
				MaxAdults = maxAdults,
				BasePrice = basePrice,
				CurrentReservation = null
			};

			if (isOccupied && res != null)
			{
				dto.CurrentReservation = new CurrentReservationDto
				{
					Id = res.Id,
					GuestName = res.Guest?.FullName ?? "",
					ArrivalDate = res.CheckInDate.ToString("yyyy-MM-dd"),
					DepartureDate = res.CheckOutDate.ToString("yyyy-MM-dd"),
					Balance = res.GrandTotal
				};
			}

			return dto;
		}

		/// <summary>Maps legacy RoomStatusLookup Id to HKStatus (1=Clean, 2=Dirty, 3/4=OOO, 5=Dirty).</summary>
		private static HKStatus MapRoomStatusIdToHKStatus(int roomStatusId)
		{
			return roomStatusId switch
			{
				1 => HKStatus.Clean,
				2 => HKStatus.Dirty,
				3 => HKStatus.OOO,
				4 => HKStatus.OOO,
				5 => HKStatus.Dirty,
				_ => HKStatus.Dirty
			};
		}
	}
}