using AutoMapper;
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
		private readonly IMapper _mapper;

		public RoomService(IUnitOfWork unitOfWork, IMapper mapper)
		{
			_unitOfWork = unitOfWork;
			_mapper = mapper;
		}

		
		public async Task<ResponseObjectDto<PagedResult<RoomDto>>> GetAllRoomsAsync(RoomFilterDto filter)
		{
			var response = new ResponseObjectDto<PagedResult<RoomDto>>();

			var query = _unitOfWork.Rooms.GetQueryable()
				.Include(r => r.RoomType)
				.Include(r => r.RoomStatus)
				.Where(r => r.IsActive)
				.AsQueryable();

			if (filter.Floor.HasValue)
				query = query.Where(r => r.FloorNumber == filter.Floor);

			if (filter.Type.HasValue)
				query = query.Where(r => r.RoomTypeId == filter.Type);

			if (!string.IsNullOrEmpty(filter.Status))
				query = query.Where(r => r.RoomStatus.Name == filter.Status);

			if (filter.FromDate.HasValue && filter.ToDate.HasValue)
			{
				// Assuming standard server time offset for simplicity, EF will handle the comparison.
				var fromOffset = new DateTimeOffset(filter.FromDate.Value, TimeSpan.Zero);
				var toOffset = new DateTimeOffset(filter.ToDate.Value, TimeSpan.Zero);

                // Exclude rooms with conflicting reservations

                query = query.Where(r => !_unitOfWork.Reservations.GetQueryable().Any(res =>
                        res.RoomId == r.Id &&
                        !res.IsDeleted &&
                        res.Status != ReservationStatus.Cancelled &&
                        res.Status != ReservationStatus.NoShow && 
                        res.CheckInDate < toOffset &&
                        res.CheckOutDate > fromOffset));
	                   
                query = query.Where(r =>
                    !(r.MaintenanceStartDate != null && r.MaintenanceEndDate != null && 
                      r.MaintenanceStartDate < filter.ToDate &&
                      r.MaintenanceEndDate > filter.FromDate));


                //query = query.Where(r => !_unitOfWork.Reservations.GetQueryable().Any(res =>
                //	res.RoomId == r.Id &&
                //	res.Status != ReservationStatus.Cancelled &&
                //	!res.IsDeleted &&
                //	res.CheckInDate < toOffset &&
                //	res.CheckOutDate > fromOffset));

                //// Exclude rooms marked for maintenance during the range
                //query = query.Where(r =>
                //	!(r.MaintenanceStartDate < filter.ToDate && r.MaintenanceEndDate > filter.FromDate));
            }

			var totalCount = await query.CountAsync();

			if (filter.PageNumber < 1) filter.PageNumber = 1;
			if (filter.PageSize <= 0) filter.PageSize = 10;

			var skip = (filter.PageNumber - 1) * filter.PageSize;

			var rooms = await query
				.OrderBy(r => r.FloorNumber)
				.ThenBy(r => r.RoomNumber)
				.Skip(skip)
				.Take(filter.PageSize)
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

			var paged = new PagedResult<RoomDto>(items, totalCount, filter.PageNumber, filter.PageSize);

			response.IsSuccess = true;
			response.StatusCode = 200;
			response.Message = "تم استرجاع قائمة الغرف بنجاح";
			response.Data = paged;

			return response;
		}

		
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

			
			if (dto.Notes != null)
			{
				room.Notes = dto.Notes;
			}

			
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

				
				if (statusObj.Id == 5)
				{
					response.IsSuccess = false;
					response.Message = "Cannot set room status to Occupied manually. FO Occupied is controlled by Reservations (Check-In).";
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

		
		public async Task<ResponseObjectDto<bool>> RestoreRoomAsync(int id)
		{
			var response = new ResponseObjectDto<bool>();

			
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

        
        public async Task<ResponseObjectDto<bool>> ChangeRoomStatusAsync(int roomId, ChangeRoomStatusDto dto)
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

            
            var hasActiveReservation = await _unitOfWork.Reservations.GetQueryable()
                .AnyAsync(r => r.RoomId == roomId && r.Status == ReservationStatus.CheckIn && !r.IsDeleted);

            
            if (!Enum.IsDefined(typeof(RoomStatusType), dto.StatusType))
            {
                response.IsSuccess = false;
                response.Message = "نوع العملية (StatusType) غير صحيح";
                response.StatusCode = 400;
                return response;
            }

            // =========================================================
            // Scenario 1: House Keeping Change
            // =========================================================
            if (dto.StatusType == RoomStatusType.HouseKeeping)
            {
                
                //if (dto.StatusId == 5)
                //{
                //    response.IsSuccess = false;
                //    response.Message = "Cannot set room status to Occupied manually. FO Occupied is controlled by Reservations (Check-In).";
                //    response.StatusCode = 400;
                //    response.Data = false;
                //    return response;
                //}

                
                if (hasActiveReservation && dto.StatusId == 1)
                {
                    response.IsSuccess = false;
                    response.Message = "Cannot change an occupied room to Vacant Clean while a reservation is checked in.";
                    response.StatusCode = 400;
                    response.Data = false;
                    return response;
                }

                
                if (!Enum.IsDefined(typeof(HKStatus), dto.StatusId))
                {
                    response.IsSuccess = false;
                    response.Message = "حالة الغرفة غير صحيحة";
                    response.StatusCode = 400;
                    return response;
                }

                
                room.RoomStatusId = dto.StatusId;

                
                room.HKStatus = (HKStatus)dto.StatusId;

                if (!string.IsNullOrEmpty(dto.Notes))
                {
                    room.Notes = (room.Notes ?? "") + $" | {DateTime.Now:dd/MM} [HK]: {dto.Notes}";
                }
            }
            // =========================================================
            // Scenario 2: Front Office Change
            // =========================================================
            else if (dto.StatusType == RoomStatusType.FrontOffice)
            {
                
                if (hasActiveReservation)
                {
                    response.IsSuccess = false;
                    response.Message = "Cannot change Front Office status for an occupied room. FO Occupied is owned by the Reservation module.";
                    response.StatusCode = 400;
                    response.Data = false;
                    return response;
                }

                
                if (!Enum.IsDefined(typeof(FOStatus), dto.StatusId))
                {
                    response.IsSuccess = false;
                    response.Message = "حالة الاستقبال (FO Status) غير صحيحة";
                    response.StatusCode = 400;
                    return response;
                }

                var newFoStatus = (FOStatus)dto.StatusId;

                
                if (newFoStatus == FOStatus.Occupied)
                {
                    response.IsSuccess = false;
                    response.Message = "Cannot set Front Office status to Occupied manually. Use Reservation Check-In.";
                    response.StatusCode = 400;
                    response.Data = false;
                    return response;
                }

                room.FOStatus = newFoStatus;

                
                
                

                if (!string.IsNullOrEmpty(dto.Notes))
                {
                    room.Notes = (room.Notes ?? "") + $" | {DateTime.Now:dd/MM} [FO]: {dto.Notes}";
                }
            }

            
            
            _unitOfWork.Rooms.Update(room);
            await _unitOfWork.CompleteAsync();

            response.IsSuccess = true;
            response.Message = "تم تحديث حالة الغرفة بنجاح";
            response.Data = true;
            response.StatusCode = 200;

            return response;
        }

        public async Task<ResponseObjectDto<bool>> StartMaintenanceAsync(int roomId, RoomMaintenanceDto dto)
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

			var occupiedReservation = await _unitOfWork.Reservations.GetQueryable()
				.FirstOrDefaultAsync(r => r.RoomId == roomId && r.Status == ReservationStatus.CheckIn && !r.IsDeleted);
			if (occupiedReservation != null)
			{
				response.IsSuccess = false;
				response.Message = "Cannot place an occupied room Out of Order.";
				response.StatusCode = 400;
				return response;
			}

			room.HKStatus = HKStatus.OOO;
			room.MaintenanceReason = dto.Reason;
			room.MaintenanceStartDate = dto.StartDate;
			room.MaintenanceEndDate = dto.EndDate;
			room.MaintenanceRemarks = dto.Remarks;

			_unitOfWork.Rooms.Update(room);
			await _unitOfWork.CompleteAsync();

			response.IsSuccess = true;
			response.Message = "تم وضع الغرفة في حالة صيانة (خارج الخدمة) بنجاح";
			response.Data = true;
			response.StatusCode = 200;
			return response;
		}

		public async Task<ResponseObjectDto<bool>> FinishMaintenanceAsync(int roomId)
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

			if (room.HKStatus != HKStatus.OOO)
			{
				response.IsSuccess = false;
				response.Message = "Room is not in Out-of-Order status.";
				response.StatusCode = 400;
				return response;
			}

			room.HKStatus = HKStatus.Dirty;
			room.MaintenanceReason = null;
			room.MaintenanceStartDate = null;
			room.MaintenanceEndDate = null;
			room.MaintenanceRemarks = null;

			_unitOfWork.Rooms.Update(room);
			await _unitOfWork.CompleteAsync();

			response.IsSuccess = true;
			response.Message = "تم إنهاء الصيانة؛ الغرفة بحالة متسخة وتحتاج تنظيفاً.";
			response.Data = true;
			response.StatusCode = 200;
			return response;
		}

		
		public async Task<ResponseObjectDto<RoomStatsDto>> GetRoomStatsAsync()
		{
			var response = new ResponseObjectDto<RoomStatsDto>();

			var roomsQuery = _unitOfWork.Rooms
				.GetQueryable()
				.Where(r => !r.IsDeleted && r.IsActive);

			var totalRooms = await roomsQuery.CountAsync();
			var availableRooms = await roomsQuery.CountAsync(r => r.HKStatus == HKStatus.Clean);
			var dirtyRooms = await roomsQuery.CountAsync(r => r.HKStatus == HKStatus.Dirty);
			var outOfServiceRooms = await roomsQuery.CountAsync(r =>
				r.HKStatus == HKStatus.OOO || r.HKStatus == HKStatus.OOS);
			var occupiedRooms = await _unitOfWork.Reservations.GetQueryable()
				.CountAsync(r => r.Status == ReservationStatus.CheckIn && !r.IsDeleted);

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


        private RoomDto MapRoomToDto(Room room, Dictionary<int, Reservation> activeReservations)
        {
            
            Reservation? reservation = null;
            if (activeReservations.TryGetValue(room.Id, out var foundRes))
            {
                reservation = foundRes;
            }

            
            var dto = _mapper.Map<RoomDto>(room);

            
            if (reservation != null)
            {
                
                dto.FoStatus = FOStatus.Occupied.ToString();

                
                dto.CurrentReservationId = reservation.Id;
                dto.GuestName = reservation.Guest?.FullName;

                
                dto.CurrentReservation = new CurrentReservationDto
                {
                    Id = reservation.Id,
                    GuestName = reservation.Guest?.FullName ?? "",
                    ArrivalDate = reservation.CheckInDate.ToString("yyyy-MM-dd"),
                    DepartureDate = reservation.CheckOutDate.ToString("yyyy-MM-dd"),
                    Balance = reservation.GrandTotal
                };
            }

            return dto;
        }


        //private static RoomDto MapRoomToDto(Room room, Dictionary<int, Reservation> reservationByRoomId)
        //{
        //	var isOccupied = reservationByRoomId.TryGetValue(room.Id, out var res);
        //	var maxAdults = room.MaxAdults > 0 ? room.MaxAdults : (room.RoomType?.MaxAdults ?? 0);
        //	var basePrice = room.BasePrice > 0 ? room.BasePrice : (room.RoomType?.BasePrice ?? 0);

        //	var dto = new RoomDto
        //	{
        //		Id = room.Id,
        //		RoomNumber = room.RoomNumber,
        //		FloorNumber = room.FloorNumber,
        //		RoomTypeName = room.RoomType?.Name ?? "N/A",
        //		RoomTypeCode = room.RoomType?.Name ?? "N/A",
        //		FoStatus = isOccupied ? "OCCUPIED" : "VACANT",
        //		HkStatus = room.HKStatus.ToString().ToUpperInvariant(),
        //		BedType = room.BedType.ToString().ToUpperInvariant(),
        //		MaxAdults = maxAdults,
        //		BasePrice = basePrice,
        //		Notes = room.Notes,
        //		CurrentReservation = null
        //	};

        //	if (isOccupied && res != null)
        //	{
        //		dto.CurrentReservation = new CurrentReservationDto
        //		{
        //			Id = res.Id,
        //			GuestName = res.Guest?.FullName ?? "",
        //			ArrivalDate = res.CheckInDate.ToString("yyyy-MM-dd"),
        //			DepartureDate = res.CheckOutDate.ToString("yyyy-MM-dd"),
        //			Balance = res.GrandTotal
        //		};
        //	}

        //	return dto;
        //}



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