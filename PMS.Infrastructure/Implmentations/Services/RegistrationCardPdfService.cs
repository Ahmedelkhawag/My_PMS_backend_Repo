using System.Globalization;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using PMS.Application.DTOs.Reservations;
using PMS.Application.Interfaces.Services;
using PMS.Application.Interfaces.UOF;
using PMS.Domain.Entities;
using QuestPDF.Drawing;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace PMS.Infrastructure.Implmentations.Services;

public class RegistrationCardPdfService : IRegistrationCardPdfService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IConfiguration _configuration;
    private const string ArabicFontName = "Cairo";
    private static bool _licenseSet;
    private static bool _fontRegistered;

    public RegistrationCardPdfService(IUnitOfWork unitOfWork, IConfiguration configuration)
    {
        _unitOfWork = unitOfWork;
        _configuration = configuration;
        EnsureLicense();
        TryRegisterArabicFont();
    }

    private static void EnsureLicense()
    {
        if (_licenseSet) return;
        QuestPDF.Settings.License = LicenseType.Community;
        _licenseSet = true;
    }

    private static void TryRegisterArabicFont()
    {
        if (_fontRegistered) return;
        try
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = "PMS.Infrastructure.Assets.Fonts.Cairo-Regular.ttf";
            using var stream = assembly.GetManifestResourceStream(resourceName);
            if (stream != null)
            {
                FontManager.RegisterFont(stream);
                _fontRegistered = true;
            }
        }
        catch
        {
            // Font not embedded; PDF will use default (Arabic may not shape correctly)
        }
    }

    public async Task<(byte[] Content, string FileName)?> GenerateRegistrationCardAsync(
        int reservationId,
        string? receptionistName,
        CancellationToken cancellationToken = default)
    {
        var reservation = await _unitOfWork.Reservations.GetQueryable()
            .Include(r => r.Guest)
            .Include(r => r.Room)
            .Include(r => r.RoomType)
            .Include(r => r.GuestFolio)
            .Where(r => r.Id == reservationId && !r.IsDeleted)
            .FirstOrDefaultAsync(cancellationToken);

        if (reservation == null)
            return null;

        var dto = MapToRegistrationCardDataDto(reservation, receptionistName ?? "—");

        var hotelName = _configuration["RegistrationCard:HotelName"] ?? _configuration["App:HotelName"] ?? "Hotel";

        var pdfBytes = BuildDocument(dto, hotelName);

        var fileName = $"RegistrationCard_{dto.ReservationNumber}.pdf";
        return (pdfBytes, fileName);
    }

    private static RegistrationCardDataDto MapToRegistrationCardDataDto(Reservation r, string receptionistName)
    {
        var guest = r.Guest;
        var checkIn = r.CheckInDate.DateTime;
        var checkOut = r.CheckOutDate.DateTime;
        var culture = CultureInfo.InvariantCulture;
        var currency = r.GuestFolio?.Currency ?? "EGP";
        var passportOrId = !string.IsNullOrWhiteSpace(guest.PassportNumber) ? guest.PassportNumber : guest.NationalId;

        return new RegistrationCardDataDto
        {
            ReservationNumber = r.ReservationNumber,
            GuestName = guest.FullName,
            PassportOrIdNumber = passportOrId ?? "—",
            Nationality = guest.Nationality ?? "—",
            Phone = guest.PhoneNumber ?? "—",
            Email = guest.Email,
            RoomNumber = r.Room != null ? r.Room.RoomNumber : "—",
            RoomTypeName = r.RoomType?.Name ?? "—",
            CheckInDateFormatted = checkIn.ToString("dd-MMM-yyyy", culture),
            CheckOutDateFormatted = checkOut.ToString("dd-MMM-yyyy", culture),
            Adults = r.Adults,
            Children = r.Children,
            NightlyRate = r.NightlyRate,
            Currency = currency,
            ReceptionistName = receptionistName,
            TodayFormatted = DateTime.UtcNow.ToString("dd-MMM-yyyy", culture),
            HotelName = null
        };
    }

    private static byte[] BuildDocument(RegistrationCardDataDto d, string hotelName)
    {
        var useArabicFont = _fontRegistered;

        byte[] Generate()
        {
            var doc = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(24);
                    page.PageColor(Colors.White);

                    var titleStyle = useArabicFont
                        ? TextStyle.Default.FontFamily(ArabicFontName).FontSize(14)
                        : TextStyle.Default.FontSize(14);

                    // A. Header
                    page.Header().Column(column =>
                    {
                        column.Item().AlignCenter().Text(hotelName).Style(TextStyle.Default.FontSize(18).Bold());
                        column.Item().PaddingTop(8).AlignCenter()
                            .Text("بطاقة تسجيل النزيل | GUEST REGISTRATION CARD").Style(titleStyle);
                        column.Item().PaddingTop(4).AlignCenter()
                            .Text($"Booking Ref: {d.ReservationNumber}").Style(titleStyle.FontSize(10));
                    });

                    // B. Guest Information
                    page.Content().PaddingTop(16).Column(content =>
                    {
                        content.Spacing(10);

                        content.Item().Text("Guest Information | معلومات النزيل").Style(titleStyle.Bold()).FontSize(12);
                        content.Item().PaddingBottom(8).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);

                        RenderBilingualRow(content, titleStyle, "اسم النزيل | Guest Name:", d.GuestName);
                        RenderBilingualRow(content, titleStyle, "رقم الجواز/الهوية | Passport/ID No:", d.PassportOrIdNumber);
                        RenderBilingualRow(content, titleStyle, "الجنسية | Nationality:", d.Nationality);
                        RenderBilingualRow(content, titleStyle, "رقم الهاتف | Phone Number:", d.Phone);
                        if (!string.IsNullOrEmpty(d.Email))
                            RenderBilingualRow(content, titleStyle, "البريد الإلكتروني | Email:", d.Email);

                        content.Item().PaddingTop(12).Text("Stay Details | تفاصيل الإقامة").Style(titleStyle.Bold()).FontSize(12);
                        content.Item().PaddingBottom(8).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);

                        RenderBilingualRow(content, titleStyle, "رقم الغرفة | Room No:", d.RoomNumber);
                        RenderBilingualRow(content, titleStyle, "نوع الغرفة | Room Type:", d.RoomTypeName);
                        RenderBilingualRow(content, titleStyle, "الوصول | Arrival:", d.CheckInDateFormatted);
                        RenderBilingualRow(content, titleStyle, "المغادرة | Departure:", d.CheckOutDateFormatted);
                        RenderBilingualRow(content, titleStyle, "السعر لليلة | Nightly Rate:", $"{d.NightlyRate:N2} {d.Currency}");
                        RenderBilingualRow(content, titleStyle, "البالغين / الأطفال | Adults / Children:", $"{d.Adults} / {d.Children}");

                        // D. Terms & Conditions (small font)
                        content.Item().PaddingTop(16).Text("Terms & Conditions | الشروط والأحكام").Style(titleStyle.Bold()).FontSize(10);
                        content.Item().PaddingTop(4).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
                        var smallStyle = titleStyle.FontSize(8).FontColor(Colors.Grey.Darken1);
                        content.Item().PaddingTop(4).Column(terms =>
                        {
                            terms.Item().Text("1. Check-out Time: Standard check-out time is 12:00 PM. (وقت تسجيل المغادرة هو الساعة 12:00 ظهراً).").Style(smallStyle);
                            terms.Item().PaddingTop(2).Text("2. Liability: The hotel is not responsible for valuables left in the room. (الفندق غير مسؤول عن الأشياء الثمينة المتروكة بالغرفة).").Style(smallStyle);
                            terms.Item().PaddingTop(2).Text("3. Smoking Policy: Smoking is strictly prohibited in rooms. (يُمنع التدخين تماماً داخل الغرف).").Style(smallStyle);
                        });

                        // E. Signatures
                        content.Item().PaddingTop(20).Row(row =>
                        {
                            row.RelativeItem().Column(c =>
                            {
                                c.Item().Text("Guest Signature | توقيع النزيل:").Style(titleStyle.FontSize(9));
                                c.Item().PaddingTop(4).Text("_________________________").Style(titleStyle.FontSize(9));
                            });
                            row.RelativeItem().Column(c =>
                            {
                                c.Item().Text("Receptionist | الموظف:").Style(titleStyle.FontSize(9));
                                c.Item().PaddingTop(4).Text(d.ReceptionistName ?? "—").Style(titleStyle.FontSize(9));
                            });
                            row.RelativeItem().Column(c =>
                            {
                                c.Item().Text("Date | التاريخ:").Style(titleStyle.FontSize(9));
                                c.Item().PaddingTop(4).Text(d.TodayFormatted).Style(titleStyle.FontSize(9));
                            });
                        });
                    });
                });
            });

            return doc.GeneratePdf();
        }

        return Generate();
    }

    private static void RenderBilingualRow(QuestPDF.Fluent.ColumnDescriptor column, TextStyle titleStyle, string label, string value)
    {
        column.Item().Row(row =>
        {
            row.ConstantItem(180).Text(label).Style(titleStyle.FontSize(9));
            row.RelativeItem().Text(value ?? "—").Style(titleStyle.FontSize(9));
        });
    }
}
