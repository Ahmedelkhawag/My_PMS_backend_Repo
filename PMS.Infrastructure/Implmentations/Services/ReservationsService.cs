using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using PMS.Application.DTOs.Common;
using PMS.Application.DTOs.Dashboard;
using PMS.Application.DTOs.Reservations;
using PMS.Application.Interfaces.Services;
using PMS.Application.Interfaces.UOF;
using PMS.Domain.Entities;
using PMS.Domain.Entities.Configuration;
using PMS.Domain.Enums;
using PMS.Domain.Constants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace PMS.Infrastructure.Implmentations.Services
{
    public class ReservationsService : IReservationService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;

        private const string TaxPercentageConfigKey = "FinancialSettings:TaxPercentage";
        private const decimal DefaultTaxPercentage = 0.15m;

        private const int RoomStatusClean = 1;
        private const int RoomStatusDirty = 2;
        private const int RoomStatusMaintenance = 3;
        private const int RoomStatusOccupied = 5;

        public ReservationsService(IUnitOfWork unitOfWork, IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
        {
            _unitOfWork = unitOfWork;
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<ResponseObjectDto<ReservationDto>> CreateReservationAsync(CreateReservationDto dto)
        {
            if (dto.MealPlanId <= 0) dto.MealPlanId = 1;
            if (dto.MarketSegmentId <= 0) dto.MarketSegmentId = 1;

            var validationResult = await ValidateReservationAsync(dto);
            if (!validationResult.IsSuccess)
            {
                return new ResponseObjectDto<ReservationDto>
                {
                    IsSuccess = false,
                    Message = validationResult.Message,
                    StatusCode = validationResult.StatusCode
                };
            }

            List<ExtraService>? fetchedExtraServices = null;
            if (dto.Services != null && dto.Services.Any())
            {
                var requestedIds = dto.Services.Select(s => s.ExtraServiceId).Distinct().ToList();
                fetchedExtraServices = await _unitOfWork.ExtraServices.GetQueryable()
                    .Where(s => requestedIds.Contains(s.Id) && s.IsActive)
                    .ToListAsync();
                var foundIds = fetchedExtraServices.Select(e => e.Id).ToHashSet();
                var invalidId = requestedIds.FirstOrDefault(id => !foundIds.Contains(id));
                if (invalidId != 0)
                {
                    return new ResponseObjectDto<ReservationDto>
                    {
                        IsSuccess = false,
                        Message = $"Invalid Service ID: {invalidId}",
                        StatusCode = 400
                    };
                }
            }
            fetchedExtraServices ??= new List<ExtraService>();

            var checkInDate = dto.IsWalkIn ? DateTime.UtcNow : dto.CheckInDate;
            var nights = CalculateNights(checkInDate, dto.CheckOutDate);
            var financials = CalculateFinancials(dto.NightlyRate, nights, fetchedExtraServices, dto.Services, dto.DiscountAmount);

            var reservationNumber = await GenerateReservationNumberAsync();

            var reservation = new Reservation
            {
                ReservationNumber = reservationNumber,
                GuestId = dto.GuestId,
                RoomTypeId = dto.RoomTypeId,
                RoomId = dto.RoomId,
                CompanyId = dto.CompanyId,
                CheckInDate = checkInDate,
                CheckOutDate = dto.CheckOutDate,
                NightlyRate = dto.NightlyRate,
                TotalAmount = financials.RoomTotal,
                ServicesAmount = financials.ServicesTotal,
                DiscountAmount = dto.DiscountAmount,
                TaxAmount = financials.TaxAmount,
                GrandTotal = financials.GrandTotal,
                RateCode = dto.RateCode,
                MealPlanId = dto.MealPlanId,
                MarketSegmentId = dto.MarketSegmentId,
                BookingSourceId = dto.BookingSourceId,
                IsPostMaster = dto.IsPostMaster,
                IsGuestPay = dto.IsGuestPay,
                IsNoExtend = dto.IsNoExtend,
                IsConfidentialRate = dto.IsConfidentialRate,
                Status = dto.IsWalkIn ? ReservationStatus.CheckIn : ReservationStatus.Pending,
                Services = financials.ServiceEntities,
                Adults = dto.Adults,
                Children = dto.Children,
                Notes = dto.Notes,
                PurposeOfVisit = dto.PurposeOfVisit,
                ExternalReference = dto.ExternalReference,
                CarPlate = dto.CarPlate
            };

            reservation.CreatedBy = _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? _httpContextAccessor.HttpContext?.User?.Identity?.Name
                ?? "System";

            if (dto.IsWalkIn && dto.RoomId.HasValue)
            {
                var roomToOccupy = await _unitOfWork.Rooms.GetByIdAsync(dto.RoomId.Value);
                if (roomToOccupy != null)
                    roomToOccupy.RoomStatusId = RoomStatusOccupied;
            }

            await _unitOfWork.Reservations.AddAsync(reservation);
            await _unitOfWork.CompleteAsync();

            var guest = await _unitOfWork.Guests.GetByIdAsync(dto.GuestId);
            var roomType = await _unitOfWork.RoomTypes.GetByIdAsync(dto.RoomTypeId);
            var room = dto.RoomId.HasValue ? await _unitOfWork.Rooms.GetByIdAsync(dto.RoomId.Value) : null;

            var responseDto = MapToReservationDto(reservation, guest, roomType, room);

            return new ResponseObjectDto<ReservationDto>
            {
                IsSuccess = true,
                Message = "Reservation created successfully",
                StatusCode = 201,
                Data = responseDto
            };
        }

        public async Task<ResponseObjectDto<PagedResult<ReservationListDto>>> GetAllReservationsAsync(string? search, string? status, int pageNumber, int pageSize)
        {
            var query = _unitOfWork.Reservations.GetQueryable()
                .Include(r => r.Guest)
                .Include(r => r.Room)
                .Include(r => r.RoomType)
                .Include(r => r.BookingSource)
                .Include(r => r.MealPlan)
                .AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(r =>
                    r.ReservationNumber.Contains(search) ||
                    r.Guest.FullName.Contains(search) ||
                    r.Guest.PhoneNumber.Contains(search));
            }

            if (!string.IsNullOrEmpty(status) && Enum.TryParse<ReservationStatus>(status, true, out var statusEnum))
            {
                query = query.Where(r => r.Status == statusEnum);
            }

            var totalCount = await query.CountAsync();

            if (pageNumber < 1) pageNumber = 1;
            if (pageSize <= 0) pageSize = 10;

            var items = await query
                .OrderByDescending(r => r.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(r => new ReservationListDto
                {
                    Id = r.Id,
                    ReservationNumber = r.ReservationNumber,
                    GuestName = r.Guest.FullName,
                    GuestPhone = r.Guest.PhoneNumber,
                    RoomNumber = r.Room != null ? r.Room.RoomNumber : "Unassigned",
                    RoomTypeName = r.RoomType.Name,
                    CheckInDate = r.CheckInDate.ToString("yyyy-MM-dd"),
                    CheckOutDate = r.CheckOutDate.ToString("yyyy-MM-dd"),
                    Nights = (r.CheckOutDate - r.CheckInDate).Days,
                    GrandTotal = r.GrandTotal,
                    Status = r.Status.ToString(),
                    StatusColor = GetStatusColor(r.Status)
                })
                .ToListAsync();

            return new ResponseObjectDto<PagedResult<ReservationListDto>>
            {
                IsSuccess = true,
                Message = "Reservations retrieved successfully",
                StatusCode = 200,
                Data = new PagedResult<ReservationListDto>(items, totalCount, pageNumber, pageSize)
            };
        }

        public async Task<ResponseObjectDto<bool>> ChangeStatusAsync(ChangeReservationStatusDto dto)
        {
            // Load reservation with room, because we must update both atomically
            var reservation = await _unitOfWork.Reservations.GetQueryable()
                .Include(r => r.Room)
                .FirstOrDefaultAsync(r => r.Id == dto.ReservationId);

            if (reservation == null)
            {
                return NotFoundResponse<bool>("Reservation not found");
            }

            var currentStatus = reservation.Status;
            var newStatus = dto.NewStatus;

            // 1) Validate transition according to the state machine
            var transitionValidation = ValidateTransition(currentStatus, newStatus);
            if (!transitionValidation.IsSuccess)
            {
                return transitionValidation;
            }

            // 2) Additional business validations (room presence etc.)
            var businessValidation = ValidateStatusChange(reservation, dto);
            if (!businessValidation.IsSuccess)
            {
                return businessValidation;
            }

            // 3) Apply side effects (Reservation + Room) in one logical unit
            // NOTE: We don't have explicit transaction support in IUnitOfWork,
            // so we rely on a single SaveChanges call to keep changes consistent.

            // Attach / update room assignment if provided
            if (dto.RoomId.HasValue)
            {
                reservation.RoomId = dto.RoomId.Value;
            }

            // Load the room if we need to touch its status
            Room? room = null;
            if (reservation.RoomId.HasValue)
            {
                room = reservation.Room ?? await _unitOfWork.Rooms.GetByIdAsync(reservation.RoomId.Value);
            }

            const int ROOM_STATUS_CLEAN = 1;
            const int ROOM_STATUS_DIRTY = 2;
            const int ROOM_STATUS_OCCUPIED = 5; // new Occupied status

            // A. Check-In
            if (newStatus == ReservationStatus.CheckIn)
            {
                if (room == null)
                {
                    return new ResponseObjectDto<bool>
                    {
                        IsSuccess = false,
                        StatusCode = 400,
                        Message = "Cannot Check-In without a valid room."
                    };
                }

                // Room must be Clean before Check-In
                if (room.RoomStatusId != ROOM_STATUS_CLEAN)
                {
                    return new ResponseObjectDto<bool>
                    {
                        IsSuccess = false,
                        StatusCode = 400,
                        Message = "Room must be Clean before Check-In."
                    };
                }

                room.RoomStatusId = ROOM_STATUS_OCCUPIED;
                reservation.CheckInDate = DateTime.UtcNow;
            }

            // B. Check-Out
            if (newStatus == ReservationStatus.CheckOut)
            {
                if (room != null)
                {
                    room.RoomStatusId = ROOM_STATUS_DIRTY;
                }

                reservation.CheckOutDate = DateTime.UtcNow;
            }

            // C. Cancel or NoShow → release room to Clean
            if (newStatus == ReservationStatus.Cancelled || newStatus == ReservationStatus.NoShow)
            {
                if (room != null)
                {
                    room.RoomStatusId = ROOM_STATUS_CLEAN;
                }
            }

            // D. Undo Check-In (back to Confirmed)
            if (currentStatus == ReservationStatus.CheckIn && newStatus == ReservationStatus.Confirmed)
            {
                if (room != null)
                {
                    room.RoomStatusId = ROOM_STATUS_CLEAN;
                }
            }

            // E. Undo Check-Out (back to CheckIn)
            if (currentStatus == ReservationStatus.CheckOut && newStatus == ReservationStatus.CheckIn)
            {
                if (room != null)
                {
                    room.RoomStatusId = ROOM_STATUS_OCCUPIED;
                }
            }

            // Finally, update reservation core status and note
            reservation.Status = newStatus;
            if (!string.IsNullOrEmpty(dto.Note))
            {
                reservation.Notes = (reservation.Notes ?? string.Empty) +
                                    $" | Status Update: {dto.Note}";
            }

            _unitOfWork.Reservations.Update(reservation);
            if (room != null)
            {
                _unitOfWork.Rooms.Update(room);
            }

            await _unitOfWork.CompleteAsync();

            return new ResponseObjectDto<bool>
            {
                IsSuccess = true,
                Message = $"Reservation status changed from {currentStatus} to {newStatus} successfully",
                StatusCode = 200,
                Data = true
            };
        }

        public async Task<ResponseObjectDto<ReservationDto>> GetReservationByIdAsync(int id)
        {
            var reservation = await GetReservationWithDetailsAsync(id);

            if (reservation == null) return NotFoundResponse<ReservationDto>("Reservation not found");

            return new ResponseObjectDto<ReservationDto>
            {
                IsSuccess = true,
                Data = MapToReservationDto(reservation, reservation.Guest, reservation.RoomType, reservation.Room),
                StatusCode = 200
            };
        }

        public async Task<ResponseObjectDto<bool>> DeleteReservationAsync(int id)
        {
            var reservation = await _unitOfWork.Reservations.GetByIdAsync(id);

            if (reservation == null) return NotFoundResponse<bool>("Reservation not found");

            if (reservation.Status != ReservationStatus.Pending)
            {
                return new ResponseObjectDto<bool>
                {
                    IsSuccess = false,
                    Message = "Only pending reservations can be deleted. Please use Cancel instead.",
                    StatusCode = 400
                };
            }

            _unitOfWork.Reservations.Delete(reservation);
            await _unitOfWork.CompleteAsync();

            return new ResponseObjectDto<bool>
            {
                IsSuccess = true,
                Message = "Reservation deleted successfully",
                Data = true,
                StatusCode = 200
            };
        }

        public async Task<ResponseObjectDto<bool>> RestoreReservationAsync(int id)
        {
            var reservation = await _unitOfWork.Reservations.GetQueryable()
                .IgnoreQueryFilters()
                .Include(r => r.Room)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (reservation == null) return NotFoundResponse<bool>("Reservation not found");

            if (!reservation.IsDeleted)
            {
                return new ResponseObjectDto<bool>
                {
                    IsSuccess = false,
                    Message = "Reservation is already active",
                    StatusCode = 400
                };
            }

            if (reservation.RoomId.HasValue)
            {
                bool isRoomTaken = await CheckRoomConflictAsync(reservation.RoomId.Value, reservation.CheckInDate, reservation.CheckOutDate, reservation.Id);
                if (isRoomTaken)
                {
                    return new ResponseObjectDto<bool>
                    {
                        IsSuccess = false,
                        Message = $"Cannot restore reservation because Room {reservation.Room?.RoomNumber} is occupied during this period.",
                        StatusCode = 409
                    };
                }
            }

            reservation.IsDeleted = false;
            reservation.DeletedAt = null;
            reservation.DeletedBy = null;
            reservation.Notes = (reservation.Notes ?? "") + $" | Restored at {DateTime.Now:yyyy-MM-dd HH:mm}";

            _unitOfWork.Reservations.Update(reservation);
            await _unitOfWork.CompleteAsync();

            return new ResponseObjectDto<bool>
            {
                IsSuccess = true,
                Message = "Reservation restored successfully",
                Data = true,
                StatusCode = 200
            };
        }

        public async Task<ResponseObjectDto<ReservationDto>> UpdateReservationAsync(UpdateReservationDto dto)
        {
            var reservation = await GetReservationWithDetailsAsync(dto.Id);
            if (reservation == null) return NotFoundResponse<ReservationDto>("Reservation not found");

            if (reservation.IsNoExtend && dto.CheckOutDate.Date > reservation.CheckOutDate.Date)
            {
                return new ResponseObjectDto<ReservationDto>
                {
                    IsSuccess = false,
                    Message = "Cannot extend stay. Reservation is marked as No-Extend.",
                    StatusCode = 400
                };
            }

            if (dto.CheckOutDate.Date <= dto.CheckInDate.Date)
            {
                return new ResponseObjectDto<ReservationDto>
                {
                    IsSuccess = false,
                    Message = "Check-out date must be after check-in date",
                    StatusCode = 400
                };
            }

            if (dto.RoomId.HasValue)
            {
                bool isRoomTaken = await CheckRoomConflictAsync(dto.RoomId.Value, dto.CheckInDate, dto.CheckOutDate, dto.Id);
                if (isRoomTaken)
                {
                    return new ResponseObjectDto<ReservationDto>
                    {
                        IsSuccess = false,
                        Message = "Selected room is already booked for the chosen dates",
                        StatusCode = 409
                    };
                }
            }

            List<ExtraService> updateFetchedExtraServices;
            if (dto.Services != null && dto.Services.Any())
            {
                var requestedIds = dto.Services.Select(s => s.ExtraServiceId).Distinct().ToList();
                var fetched = await _unitOfWork.ExtraServices.GetQueryable()
                    .Where(s => requestedIds.Contains(s.Id) && s.IsActive)
                    .ToListAsync();
                var foundIds = fetched.Select(e => e.Id).ToHashSet();
                var invalidId = requestedIds.FirstOrDefault(id => !foundIds.Contains(id));
                if (invalidId != 0)
                {
                    return new ResponseObjectDto<ReservationDto>
                    {
                        IsSuccess = false,
                        Message = $"Invalid Service ID: {invalidId}",
                        StatusCode = 400
                    };
                }
                updateFetchedExtraServices = fetched;
            }
            else
            {
                updateFetchedExtraServices = new List<ExtraService>();
            }

            var nights = CalculateNights(dto.CheckInDate, dto.CheckOutDate);
            var financials = CalculateFinancials(dto.NightlyRate, nights, updateFetchedExtraServices, dto.Services, dto.DiscountAmount);

            // Update Fields
            reservation.GuestId = dto.GuestId;
            reservation.RoomTypeId = dto.RoomTypeId;
            reservation.RoomId = dto.RoomId;
            reservation.CheckInDate = dto.CheckInDate;
            reservation.CheckOutDate = dto.CheckOutDate;
            reservation.NightlyRate = dto.NightlyRate;
            reservation.TotalAmount = financials.RoomTotal;
            reservation.ServicesAmount = financials.ServicesTotal;
            reservation.DiscountAmount = dto.DiscountAmount;
            reservation.TaxAmount = financials.TaxAmount;
            reservation.GrandTotal = financials.GrandTotal;
            reservation.RateCode = dto.RateCode;
            reservation.MealPlanId = dto.MealPlanId;
            reservation.BookingSourceId = dto.BookingSourceId;
            reservation.MarketSegmentId = dto.MarketSegmentId;
            reservation.PurposeOfVisit = dto.PurposeOfVisit;
            reservation.Notes = dto.Notes;
            reservation.ExternalReference = dto.ExternalReference;
            reservation.CarPlate = dto.CarPlate;
            reservation.Adults = dto.Adults;
            reservation.Children = dto.Children;
            reservation.CompanyId = dto.CompanyId;
            reservation.IsConfidentialRate = dto.IsConfidentialRate;
            reservation.IsNoExtend = dto.IsNoExtend;

            // Clear old services logic handled by establishing a new list
            // EF Core will handle the diff if we simply replace the collection or clear and add
            if (reservation.Services != null)
            {
                reservation.Services.Clear();
            }
            reservation.Services = financials.ServiceEntities;

            _unitOfWork.Reservations.Update(reservation);
            await _unitOfWork.CompleteAsync();

            return new ResponseObjectDto<ReservationDto>
            {
                IsSuccess = true,
                Message = "Reservation updated successfully",
                StatusCode = 200,
                Data = MapToReservationDto(reservation, reservation.Guest, reservation.RoomType, reservation.Room)
            };
        }
		public async Task<ResponseObjectDto<ReservationStatsDto>> GetReservationStatsAsync()
		{
			var response = new ResponseObjectDto<ReservationStatsDto>();

			var today = DateTime.UtcNow.Date;

			var reservationsQuery = _unitOfWork.Reservations
				.GetQueryable()
				.Where(r => !r.IsDeleted);

			var totalReservations = await reservationsQuery.CountAsync();
			var createdToday = await reservationsQuery.CountAsync(r => r.CreatedAt.Date == today);
			var arrivalsToday = await reservationsQuery.CountAsync(r =>
				r.CheckInDate.Date == today &&
				r.Status == ReservationStatus.Confirmed);
			var departuresToday = await reservationsQuery.CountAsync(r =>
				r.CheckOutDate.Date == today &&
				r.Status == ReservationStatus.CheckIn);
			var activeInHouse = await reservationsQuery.CountAsync(r => r.Status == ReservationStatus.CheckIn);
			var pendingConfirmations = await reservationsQuery.CountAsync(r => r.Status == ReservationStatus.Pending);

			var stats = new ReservationStatsDto
			{
				TotalReservations = totalReservations,
				CreatedToday = createdToday,
				ArrivalsToday = arrivalsToday,
				DeparturesToday = departuresToday,
				ActiveInHouse = activeInHouse,
				PendingConfirmations = pendingConfirmations
			};

			response.IsSuccess = true;
			response.StatusCode = 200;
			response.Message = "Reservation statistics retrieved successfully";
			response.Data = stats;

			return response;
		}

		// ==========================================
		// Private Helper Methods
		// ==========================================

		private async Task<ResponseObjectDto<bool>> ValidateReservationAsync(CreateReservationDto dto)
        {
            if (dto.CheckOutDate <= dto.CheckInDate)
            {
                return new ResponseObjectDto<bool> { IsSuccess = false, Message = "Check-out date must be after check-in date", StatusCode = 400 };
            }

            var guest = await _unitOfWork.Guests.GetByIdAsync(dto.GuestId);
            if (guest == null)
                return new ResponseObjectDto<bool> { IsSuccess = false, Message = "Guest not found", StatusCode = 400 };

            if (dto.IsWalkIn)
            {
                if (string.IsNullOrWhiteSpace(guest.NationalId))
                    return new ResponseObjectDto<bool> { IsSuccess = false, Message = "Walk-in requires Guest National ID/Passport for police reporting", StatusCode = 400 };
                if (!dto.RoomId.HasValue)
                    return new ResponseObjectDto<bool> { IsSuccess = false, Message = "Walk-in requires a room assignment", StatusCode = 400 };
            }

            var roomType = await _unitOfWork.RoomTypes.GetByIdAsync(dto.RoomTypeId);
            if (roomType == null)
                return new ResponseObjectDto<bool> { IsSuccess = false, Message = "Room type not found", StatusCode = 400 };

            var mealPlan = await _unitOfWork.MealPlans.GetByIdAsync(dto.MealPlanId);
            if (mealPlan == null)
                return new ResponseObjectDto<bool> { IsSuccess = false, Message = "Meal plan not found", StatusCode = 400 };

            var bookingSource = await _unitOfWork.BookingSources.GetByIdAsync(dto.BookingSourceId);
            if (bookingSource == null)
                return new ResponseObjectDto<bool> { IsSuccess = false, Message = "Booking source not found", StatusCode = 400 };

            var marketSegment = await _unitOfWork.MarketSegments.GetByIdAsync(dto.MarketSegmentId);
            if (marketSegment == null)
                return new ResponseObjectDto<bool> { IsSuccess = false, Message = "Market segment not found", StatusCode = 400 };

            if (dto.RoomId.HasValue)
            {
                var room = await _unitOfWork.Rooms.GetByIdAsync(dto.RoomId.Value);
                if (room == null)
                    return new ResponseObjectDto<bool> { IsSuccess = false, Message = "Room not found", StatusCode = 400 };

                if (dto.IsWalkIn && room.RoomStatusId != RoomStatusClean)
                    return new ResponseObjectDto<bool> { IsSuccess = false, Message = "Room is not ready for immediate check-in", StatusCode = 400 };

                bool isRoomTaken = await CheckRoomConflictAsync(dto.RoomId.Value, dto.CheckInDate, dto.CheckOutDate);
                if (isRoomTaken)
                {
                    return new ResponseObjectDto<bool> { IsSuccess = false, Message = "Room is already booked for this period", StatusCode = 409 };
                }
            }
            return new ResponseObjectDto<bool> { IsSuccess = true };
        }

        private ResponseObjectDto<bool> ValidateStatusChange(Reservation reservation, ChangeReservationStatusDto dto)
        {
            // Additional business rules on top of the transition matrix
            if (dto.NewStatus == ReservationStatus.CheckIn)
            {
                if (reservation.RoomId == null && dto.RoomId == null)
                {
                    return new ResponseObjectDto<bool>
                    {
                        IsSuccess = false,
                        Message = "Cannot Check-In without a room assignment",
                        StatusCode = 400
                    };
                }
            }

            return new ResponseObjectDto<bool> { IsSuccess = true };
        }

        private ResponseObjectDto<bool> ValidateTransition(ReservationStatus current, ReservationStatus next)
        {
            bool allowed = current switch
            {
                ReservationStatus.Pending => next is ReservationStatus.Confirmed or ReservationStatus.Cancelled,
                ReservationStatus.Confirmed => next is ReservationStatus.CheckIn
                    or ReservationStatus.Cancelled
                    or ReservationStatus.NoShow,
                ReservationStatus.CheckIn => next is ReservationStatus.CheckOut or ReservationStatus.Confirmed,
                ReservationStatus.CheckOut => next is ReservationStatus.CheckIn,
                ReservationStatus.Cancelled => next is ReservationStatus.Confirmed,
                ReservationStatus.NoShow => next is ReservationStatus.Confirmed or ReservationStatus.Cancelled,
                _ => false
            };

            if (!allowed)
            {
                return new ResponseObjectDto<bool>
                {
                    IsSuccess = false,
                    StatusCode = 400,
                    Message = "Invalid status transition"
                };
            }

            return new ResponseObjectDto<bool> { IsSuccess = true, StatusCode = 200 };
        }

        private async Task<bool> CheckRoomConflictAsync(int roomId, DateTime checkIn, DateTime checkOut, int? excludeReservationId = null)
        {
            var query = _unitOfWork.Reservations.GetQueryable()
                .Where(r => r.RoomId == roomId &&
                            !r.IsDeleted &&
                            r.Status != ReservationStatus.Cancelled &&
                            r.CheckInDate < checkOut &&
                            r.CheckOutDate > checkIn);

            if (excludeReservationId.HasValue)
            {
                query = query.Where(r => r.Id != excludeReservationId.Value);
            }

            return await query.AnyAsync();
        }

        private int CalculateNights(DateTime checkIn, DateTime checkOut)
        {
            var nights = (checkOut.Date - checkIn.Date).Days;
            return nights <= 0 ? 1 : nights;
        }

        private (decimal RoomTotal, decimal ServicesTotal, decimal TaxAmount, decimal GrandTotal, List<ReservationService> ServiceEntities)
            CalculateFinancials(decimal nightlyRate, int nights, List<ExtraService> extraServices, List<CreateReservationServiceDto>? serviceDtos, decimal discountAmount)
        {
            decimal roomTotal = nightlyRate * nights;
            decimal servicesTotal = 0;
            var serviceEntities = new List<ReservationService>();

            if (extraServices != null && extraServices.Any() && serviceDtos != null)
            {
                foreach (var entity in extraServices)
                {
                    var dto = serviceDtos.FirstOrDefault(s => s.ExtraServiceId == entity.Id);
                    var quantity = dto?.Quantity ?? 0;
                    if (quantity < 1) continue;

                    var itemTotal = entity.IsPerDay
                        ? (entity.Price * quantity * nights)
                        : (entity.Price * quantity);

                    servicesTotal += itemTotal;

                    serviceEntities.Add(new ReservationService
                    {
                        ServiceName = entity.Name,
                        Price = entity.Price,
                        Quantity = quantity,
                        IsPerDay = entity.IsPerDay,
                        TotalServicePrice = itemTotal
                    });
                }
            }

            var subTotal = (roomTotal + servicesTotal) - discountAmount;
            if (subTotal < 0) subTotal = 0;

            var taxPercentage = _configuration.GetValue<decimal?>(TaxPercentageConfigKey) ?? DefaultTaxPercentage;
            var taxAmount = subTotal * taxPercentage;
            var grandTotal = subTotal + taxAmount;

            return (roomTotal, servicesTotal, taxAmount, grandTotal, serviceEntities);
        }

        private async Task<string> GenerateReservationNumberAsync()
        {
            var today = DateTime.UtcNow.Date;
            var lastReservationNumber = await _unitOfWork.Reservations.GetQueryable()
                .Where(r => r.CreatedAt >= today && r.CreatedAt < today.AddDays(1))
                .OrderByDescending(r => r.CreatedAt)
                .Select(r => r.ReservationNumber)
                .FirstOrDefaultAsync();

            var lastSequence = 0;
            if (!string.IsNullOrWhiteSpace(lastReservationNumber))
            {
                var parts = lastReservationNumber.Split('-');
                if (parts.Length == 3 && int.TryParse(parts[2], out var parsedSeq))
                {
                    lastSequence = parsedSeq;
                }
            }

            return $"BK-{today:yyyyMMdd}-{lastSequence + 1:000}";
        }

        private async Task<Reservation?> GetReservationWithDetailsAsync(int id)
        {
            return await _unitOfWork.Reservations.GetQueryable()
                .Include(r => r.Services)
                .Include(r => r.Guest)
                .Include(r => r.Room)
                .Include(r => r.RoomType)
                .Include(r => r.BookingSource)
                .Include(r => r.MealPlan)
                .Include(r => r.MarketSegment)
                .Include(r => r.Company)
                .FirstOrDefaultAsync(r => r.Id == id);
        }

        private static string GetStatusColor(ReservationStatus status)
        {
            return status switch
            {
                ReservationStatus.Confirmed => StatusColorPalette.Success,
                ReservationStatus.Pending => StatusColorPalette.Warning,
                ReservationStatus.CheckIn => StatusColorPalette.Info,
                ReservationStatus.CheckOut => StatusColorPalette.Secondary,
                ReservationStatus.Cancelled => StatusColorPalette.Danger,
                ReservationStatus.NoShow => StatusColorPalette.Secondary,
                _ => StatusColorPalette.Secondary
            };
        }

        private ResponseObjectDto<T> NotFoundResponse<T>(string message)
        {
            return new ResponseObjectDto<T>
            {
                IsSuccess = false,
                Message = message,
                StatusCode = 404
            };
        }

        private ReservationDto MapToReservationDto(Reservation reservation, Guest? guest, RoomType? roomType, Room? room)
        {
            return new ReservationDto
            {
                Id = reservation.Id,
                ReservationNumber = reservation.ReservationNumber,
                GuestId = reservation.GuestId,
                GuestName = guest?.FullName ?? "Unknown",
                GuestPhone = guest?.PhoneNumber,
                GuestEmail = guest?.Email,
                GuestNationalId = guest?.NationalId,
                RoomTypeId = reservation.RoomTypeId,
                RoomTypeName = roomType?.Name ?? "Unknown",
                RoomId = reservation.RoomId,
                RoomNumber = room?.RoomNumber ?? "Unassigned",
                CompanyId = reservation.CompanyId,
                CompanyName = reservation.Company?.Name,
                CheckInDate = reservation.CheckInDate,
                CheckOutDate = reservation.CheckOutDate,
                Nights = CalculateNights(reservation.CheckInDate, reservation.CheckOutDate),
                NightlyRate = reservation.NightlyRate,
                TotalAmount = reservation.TotalAmount,
                ServicesAmount = reservation.ServicesAmount,
                DiscountAmount = reservation.DiscountAmount,
                TaxAmount = reservation.TaxAmount,
                GrandTotal = reservation.GrandTotal,
                Status = reservation.Status.ToString(),
                RateCode = reservation.RateCode,
                MealPlan = reservation.MealPlan?.Name ?? "Unknown",
                Source = reservation.BookingSource?.Name ?? "Unknown",
                MarketSegment = reservation.MarketSegment?.Name ?? "Unknown",
                PurposeOfVisit = reservation.PurposeOfVisit,
                Notes = reservation.Notes,
                ExternalReference = reservation.ExternalReference,
                CarPlate = reservation.CarPlate,
                Services = reservation.Services?.Select(s => new ReservationServiceDto
                {
                    ServiceName = s.ServiceName,
                    Price = s.Price,
                    Quantity = s.Quantity,
                    IsPerDay = s.IsPerDay,
                    Total = s.TotalServicePrice
                }).ToList() ?? new List<ReservationServiceDto>()
            };
        }

       
    }
}
