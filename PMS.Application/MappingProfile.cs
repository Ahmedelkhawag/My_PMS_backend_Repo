using AutoMapper;
using PMS.Application.DTOs.Auth;
using PMS.Application.DTOs.Companies;
using PMS.Application.DTOs.Configuration;
using PMS.Application.DTOs.Folios;
using PMS.Application.DTOs.Guests;
using PMS.Application.DTOs.Reservations;
using PMS.Application.DTOs.Rooms;
using PMS.Application.DTOs.Shifts;
using PMS.Domain.Entities;
using PMS.Domain.Entities.Configuration;
using PMS.Domain.Enums;
using System.Globalization;

namespace PMS.Application
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // ══════════════════════════════════════════════
            // Shift
            // ══════════════════════════════════════════════
            CreateMap<EmployeeShift, ShiftDto>()
                .ForMember(dest => dest.UpdatedBy, opt => opt.MapFrom(src => src.LastModifiedBy))
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => src.LastModifiedAt));

            // ══════════════════════════════════════════════
            // Guest
            // ══════════════════════════════════════════════
            CreateMap<Guest, GuestDto>()
                .ForMember(dest => dest.LoyaltyLevel, opt => opt.MapFrom(src => src.LoyaltyLevel.ToString()))
                .ForMember(dest => dest.UpdatedBy,    opt => opt.MapFrom(src => src.LastModifiedBy))
                .ForMember(dest => dest.UpdatedAt,    opt => opt.MapFrom(src => src.LastModifiedAt));

            // ══════════════════════════════════════════════
            // Room (base fields; occupancy logic handled in RoomService)
            // ══════════════════════════════════════════════
            CreateMap<Room, RoomDto>()
                .ForMember(dest => dest.RoomTypeName, opt => opt.MapFrom(src => src.RoomType != null ? src.RoomType.Name : string.Empty))
                .ForMember(dest => dest.RoomTypeCode, opt => opt.Ignore())
                .ForMember(dest => dest.BedType,      opt => opt.MapFrom(src => src.BedType.ToString()))
                .ForMember(dest => dest.HkStatus,     opt => opt.MapFrom(src => src.HKStatus.ToString()))
                .ForMember(dest => dest.FoStatus,     opt => opt.MapFrom(src => src.FOStatus.ToString()))
                // Occupancy fields — populated by RoomService after mapping
                .ForMember(dest => dest.CurrentReservationId, opt => opt.Ignore())
                .ForMember(dest => dest.GuestName,            opt => opt.Ignore())
                .ForMember(dest => dest.CurrentReservation,   opt => opt.Ignore());

            // ══════════════════════════════════════════════
            // Company
            // ══════════════════════════════════════════════
            CreateMap<CompanyProfile, CompanyProfileDto>()
                .ForMember(dest => dest.UpdatedBy, opt => opt.MapFrom(src => src.LastModifiedBy))
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => src.LastModifiedAt));

            // ══════════════════════════════════════════════
            // Rate Plan
            // ══════════════════════════════════════════════
            CreateMap<RatePlan, RatePlanDto>();

            // ══════════════════════════════════════════════
            // Auth / Users
            // ══════════════════════════════════════════════
            CreateMap<AppUser, UserResponseDto>()
                .ForMember(dest => dest.Username,  opt => opt.MapFrom(src => src.UserName))
                .ForMember(dest => dest.UpdatedBy, opt => opt.MapFrom(src => src.LastModifiedBy))
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => src.LastModifiedAt))
                // Role requires async GetRolesAsync — set post-map in service
                .ForMember(dest => dest.Role, opt => opt.Ignore());

            CreateMap<AppUser, UserDetailDto>()
                .ForMember(dest => dest.Username,      opt => opt.MapFrom(src => src.UserName))
                .ForMember(dest => dest.Gender,        opt => opt.MapFrom(src => src.Gender != null ? src.Gender.ToString() : null))
                .ForMember(dest => dest.UpdatedBy,     opt => opt.MapFrom(src => src.LastModifiedBy))
                .ForMember(dest => dest.UpdatedAt,     opt => opt.MapFrom(src => src.LastModifiedAt))
                // Role and DocumentPaths require extra async calls — set post-map in service
                .ForMember(dest => dest.Role,          opt => opt.Ignore())
                .ForMember(dest => dest.DocumentPaths, opt => opt.Ignore());

            // ══════════════════════════════════════════════
            // Folio
            // ══════════════════════════════════════════════
            CreateMap<GuestFolio, GuestFolioSummaryDto>()
                .ForMember(dest => dest.FolioId, opt => opt.MapFrom(src => src.Id));

            CreateMap<FolioTransaction, FolioTransactionDto>();

            // ══════════════════════════════════════════════
            // Reservation
            // ══════════════════════════════════════════════
            CreateMap<ReservationService, ReservationServiceDto>()
                .ForMember(dest => dest.Total,          opt => opt.MapFrom(src => src.TotalServicePrice))
                .ForMember(dest => dest.ExtraServiceId, opt => opt.Ignore());

            CreateMap<Reservation, ReservationDto>()
                .ForMember(dest => dest.GuestName,       opt => opt.MapFrom(src => src.Guest != null ? src.Guest.FullName : "Unknown"))
                .ForMember(dest => dest.GuestPhone,      opt => opt.MapFrom(src => src.Guest != null ? src.Guest.PhoneNumber : null))
                .ForMember(dest => dest.GuestEmail,      opt => opt.MapFrom(src => src.Guest != null ? src.Guest.Email : null))
                .ForMember(dest => dest.GuestNationalId, opt => opt.MapFrom(src => src.Guest != null ? src.Guest.NationalId : null))
                .ForMember(dest => dest.RoomTypeName,    opt => opt.MapFrom(src => src.RoomType != null ? src.RoomType.Name : "Unknown"))
                .ForMember(dest => dest.RoomNumber,      opt => opt.MapFrom(src => src.Room != null ? src.Room.RoomNumber : "Unassigned"))
                .ForMember(dest => dest.CompanyName,     opt => opt.MapFrom(src => src.Company != null ? src.Company.Name : null))
                .ForMember(dest => dest.RatePlanName,    opt => opt.MapFrom(src => src.RatePlan != null ? src.RatePlan.Name : null))
                .ForMember(dest => dest.MealPlan,        opt => opt.MapFrom(src => src.MealPlan != null ? src.MealPlan.Name : "Unknown"))
                .ForMember(dest => dest.Source,          opt => opt.MapFrom(src => src.BookingSource != null ? src.BookingSource.Name : "Unknown"))
                .ForMember(dest => dest.MarketSegment,   opt => opt.MapFrom(src => src.MarketSegment != null ? src.MarketSegment.Name : "Unknown"))
                .ForMember(dest => dest.Status,          opt => opt.MapFrom(src => src.Status.ToString()))
                .ForMember(dest => dest.Nights,          opt => opt.MapFrom(src => CalculateNights(src.CheckInDate, src.CheckOutDate)))
                .ForMember(dest => dest.Services,        opt => opt.MapFrom(src => src.Services))
                .ForMember(dest => dest.UpdatedBy,       opt => opt.MapFrom(src => src.LastModifiedBy))
                .ForMember(dest => dest.UpdatedAt,       opt => opt.MapFrom(src => src.LastModifiedAt))
                .AfterMap((src, dest) =>
                {
                    if (src.IsConfidentialRate)
                    {
                        dest.NightlyRate     = 0;
                        dest.TotalAmount     = 0;
                        dest.ServicesAmount  = 0;
                        dest.DiscountAmount  = 0;
                        dest.TaxAmount       = 0;
                        dest.GrandTotal      = 0;
                        dest.RateCode        = "CONFIDENTIAL";
                    }
                });

            // ══════════════════════════════════════════════
            // PDF Generation
            // ══════════════════════════════════════════════
            CreateMap<Reservation, RegistrationCardDataDto>()
                .ForMember(dest => dest.GuestName, opt => opt.MapFrom(src => src.Guest != null ? src.Guest.FullName : "—"))
                .ForMember(dest => dest.PassportOrIdNumber, opt => opt.MapFrom(src => 
                    src.Guest != null ? (!string.IsNullOrWhiteSpace(src.Guest.PassportNumber) ? src.Guest.PassportNumber : (!string.IsNullOrWhiteSpace(src.Guest.NationalId) ? src.Guest.NationalId : "—")) : "—"))
                .ForMember(dest => dest.Nationality, opt => opt.MapFrom(src => src.Guest != null && !string.IsNullOrWhiteSpace(src.Guest.Nationality) ? src.Guest.Nationality : "—"))
                .ForMember(dest => dest.Phone, opt => opt.MapFrom(src => src.Guest != null && !string.IsNullOrWhiteSpace(src.Guest.PhoneNumber) ? src.Guest.PhoneNumber : "—"))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Guest != null ? src.Guest.Email : null))
                .ForMember(dest => dest.RoomNumber, opt => opt.MapFrom(src => src.Room != null ? src.Room.RoomNumber : "—"))
                .ForMember(dest => dest.RoomTypeName, opt => opt.MapFrom(src => src.RoomType != null ? src.RoomType.Name : "—"))
                .ForMember(dest => dest.CheckInDateFormatted, opt => opt.MapFrom(src => src.CheckInDate.ToString("dd-MMM-yyyy", CultureInfo.InvariantCulture)))
                .ForMember(dest => dest.CheckOutDateFormatted, opt => opt.MapFrom(src => src.CheckOutDate.ToString("dd-MMM-yyyy", CultureInfo.InvariantCulture)))
                .ForMember(dest => dest.Currency, opt => opt.MapFrom(src => src.GuestFolio != null && !string.IsNullOrWhiteSpace(src.GuestFolio.Currency) ? src.GuestFolio.Currency : "EGP"))
                .ForMember(dest => dest.TodayFormatted, opt => opt.MapFrom(src => DateTime.UtcNow.ToString("dd-MMM-yyyy", CultureInfo.InvariantCulture)))
                .ForMember(dest => dest.HotelName, opt => opt.Ignore())
                .ForMember(dest => dest.ReceptionistName, opt => opt.Ignore());
        }

        private static int CalculateNights(DateTimeOffset checkIn, DateTimeOffset checkOut)
        {
            var days = (checkOut - checkIn).Days;
            return days > 0 ? days : 1;
        }
    }
}
