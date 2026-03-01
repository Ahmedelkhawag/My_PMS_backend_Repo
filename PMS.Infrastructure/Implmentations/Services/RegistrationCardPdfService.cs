using System.Globalization;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PMS.Application.DTOs.Reservations;
using PMS.Application.Interfaces.Services;
using PMS.Application.Interfaces.UOF;
using PMS.Infrastructure.Constants;
using QuestPDF.Drawing;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace PMS.Infrastructure.Implmentations.Services;

public class RegistrationCardPdfService : IRegistrationCardPdfService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IConfiguration _configuration;
    private readonly IMapper _mapper;
    private readonly ILogger<RegistrationCardPdfService> _logger;

    public RegistrationCardPdfService(
        IUnitOfWork unitOfWork, 
        IConfiguration configuration,
        IMapper mapper,
        ILogger<RegistrationCardPdfService> logger)
    {
        _unitOfWork = unitOfWork;
        _configuration = configuration;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<(byte[] Content, string FileName)?> GenerateRegistrationCardAsync(
        int reservationId,
        string? receptionistName,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var reservation = await _unitOfWork.Reservations.GetQueryable()
                .AsNoTracking()
                .Include(r => r.Guest)
                .Include(r => r.Room)
                .Include(r => r.RoomType)
                .Include(r => r.GuestFolio)
                .Where(r => r.Id == reservationId && !r.IsDeleted)
                .FirstOrDefaultAsync(cancellationToken);

            if (reservation == null)
            {
                _logger.LogWarning("GenerateRegistrationCardAsync: Reservation with ID {ReservationId} not found.", reservationId);
                return null;
            }

            var hotelName = _configuration[PdfConstants.ConfigKeyHotelName] 
                            ?? _configuration[PdfConstants.AppConfigKeyHotelName] 
                            ?? PdfConstants.DefaultHotelName;

            var dto = _mapper.Map<RegistrationCardDataDto>(reservation) 
                        with { ReceptionistName = receptionistName ?? "—", HotelName = hotelName };

            var pdfBytes = BuildDocument(dto);

            var fileName = $"RegistrationCard_{dto.ReservationNumber}.pdf";
            return (pdfBytes, fileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while generating the registration card PDF for reservation ID {ReservationId}.", reservationId);
            return null;
        }
    }

    private static byte[] BuildDocument(RegistrationCardDataDto d)
    {
        byte[] Generate()
        {
            var doc = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(24);
                    page.PageColor(Colors.White);

                    var titleStyle = TextStyle.Default.FontFamily(PdfConstants.ArabicFontName).FontSize(14);

                    // A. Header
                    page.Header().Column(column =>
                    {
                        column.Item().AlignCenter().Text(d.HotelName ?? PdfConstants.DefaultHotelName).Style(TextStyle.Default.FontSize(18).Bold());
                        column.Item().PaddingTop(8).AlignCenter()
                            .Text(PdfConstants.Title).Style(titleStyle);
                        column.Item().PaddingTop(4).AlignCenter()
                            .Text($"Booking Ref: {d.ReservationNumber}").Style(titleStyle.FontSize(10));
                    });

                    // B. Guest Information
                    page.Content().PaddingTop(16).Column(content =>
                    {
                        content.Spacing(10);

                        content.Item().Text(PdfConstants.SectionGuestInfo).Style(titleStyle.Bold()).FontSize(12);
                        content.Item().PaddingBottom(8).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);

                        RenderBilingualRow(content, titleStyle, PdfConstants.LabelGuestName, d.GuestName);
                        RenderBilingualRow(content, titleStyle, PdfConstants.LabelPassportId, d.PassportOrIdNumber);
                        RenderBilingualRow(content, titleStyle, PdfConstants.LabelNationality, d.Nationality);
                        RenderBilingualRow(content, titleStyle, PdfConstants.LabelPhone, d.Phone);
                        if (!string.IsNullOrEmpty(d.Email))
                            RenderBilingualRow(content, titleStyle, PdfConstants.LabelEmail, d.Email);

                        content.Item().PaddingTop(12).Text(PdfConstants.SectionStayDetails).Style(titleStyle.Bold()).FontSize(12);
                        content.Item().PaddingBottom(8).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);

                        RenderBilingualRow(content, titleStyle, PdfConstants.LabelRoomNumber, d.RoomNumber);
                        RenderBilingualRow(content, titleStyle, PdfConstants.LabelRoomType, d.RoomTypeName);
                        RenderBilingualRow(content, titleStyle, PdfConstants.LabelArrival, d.CheckInDateFormatted);
                        RenderBilingualRow(content, titleStyle, PdfConstants.LabelDeparture, d.CheckOutDateFormatted);
                        RenderBilingualRow(content, titleStyle, PdfConstants.LabelNightlyRate, $"{d.NightlyRate:N2} {d.Currency}");
                        RenderBilingualRow(content, titleStyle, PdfConstants.LabelAdultsChildren, $"{d.Adults} / {d.Children}");

                        // D. Terms & Conditions (small font)
                        content.Item().PaddingTop(16).Text(PdfConstants.SectionTerms).Style(titleStyle.Bold()).FontSize(10);
                        content.Item().PaddingTop(4).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
                        var smallStyle = titleStyle.FontSize(8).FontColor(Colors.Grey.Darken1);
                        content.Item().PaddingTop(4).Column(terms =>
                        {
                            terms.Item().Text(PdfConstants.Term1).Style(smallStyle);
                            terms.Item().PaddingTop(2).Text(PdfConstants.Term2).Style(smallStyle);
                            terms.Item().PaddingTop(2).Text(PdfConstants.Term3).Style(smallStyle);
                        });

                        // E. Signatures
                        content.Item().PaddingTop(20).Row(row =>
                        {
                            row.RelativeItem().Column(c =>
                            {
                                c.Item().Text(PdfConstants.LabelGuestSignature).Style(titleStyle.FontSize(9));
                                c.Item().PaddingTop(4).Text("_________________________").Style(titleStyle.FontSize(9));
                            });
                            row.RelativeItem().Column(c =>
                            {
                                c.Item().Text(PdfConstants.LabelReceptionist).Style(titleStyle.FontSize(9));
                                c.Item().PaddingTop(4).Text(d.ReceptionistName ?? "—").Style(titleStyle.FontSize(9));
                            });
                            row.RelativeItem().Column(c =>
                            {
                                c.Item().Text(PdfConstants.LabelDate).Style(titleStyle.FontSize(9));
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
