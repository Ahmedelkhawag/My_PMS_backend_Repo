using ClosedXML.Excel;
using Microsoft.EntityFrameworkCore;
using PMS.Application.DTOs.Reports;
using PMS.Application.Interfaces.Services;
using PMS.Application.Interfaces.UOF;
using PMS.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace PMS.Infrastructure.Implmentations.Services
{
    public class ReportService : IReportService
    {
        private readonly IUnitOfWork _unitOfWork;

        public ReportService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public async Task<byte[]> GeneratePoliceReportAsync(DateTime? businessDate)
        {
            DateTime reportDate;
            if (businessDate.HasValue)
            {
                reportDate = businessDate.Value.Date;
            }
            else
            {
                var currentBusinessDay = await _unitOfWork.BusinessDays
                    .GetQueryable()
                    .AsNoTracking()
                    .FirstOrDefaultAsync(b => b.Status == BusinessDayStatus.Open);

                reportDate = currentBusinessDay?.Date ?? DateTime.UtcNow.Date;
            }

            // 2. سحب البيانات (The Data Fetching Logic)
            // الشرط: الحجوزات اللي حالتها CheckedIn (مقيمين حالياً)
            var activeReservations = await _unitOfWork.Reservations
                .GetQueryable()
                .Include(r => r.Guest)
                    .ThenInclude(g => g.Nationality) // Join عشان نجيب اسم الدولة
                .Include(r => r.Room)                // Join عشان نجيب رقم الغرفة
                .Where(r => r.Status == ReservationStatus.CheckIn && !r.IsDeleted)
                .AsNoTracking() // Performance Boost للتقارير
                .ToListAsync();

            // 3. تحويل البيانات لـ Flat DTO (Mapping)
            var reportData = new List<PoliceReportDto>();

            foreach (var res in activeReservations)
            {
                // Logic ذكي لتحديد نوع الوثيقة (مصري = بطاقة، أجنبي = باسبور)
                // بنعتمد على وجود الرقم القومي، لو موجود يبقى مصري/بطاقة، غير كدة باسبور
                bool hasNationalId = !string.IsNullOrWhiteSpace(res.Guest.NationalId);

                var item = new PoliceReportDto
                {
                    GuestName = res.Guest.FullName,
                    Nationality = res.Guest.Nationality?? "Unknown",

                    DocumentType = hasNationalId ? "National ID" : "Passport",
                    DocumentNumber = hasNationalId ? res.Guest.NationalId : (res.Guest.PassportNumber ?? "N/A"),

                    RoomNumber = res.Room != null ? res.Room.RoomNumber : "Unassigned",

                    ArrivalDate = res.CheckInDate,
                    DepartureDate = res.CheckOutDate,

                    // لو مفيش وظيفة مسجلة نكتب N/A
                    Profession = !string.IsNullOrWhiteSpace(res.Guest.PhoneNumber) ? "Guest" : "N/A"
                    // ملحوظة: لو عندك حقل JobTitle في الـ Guest استخدمه هنا، حالياً حطيت Placeholder
                };

                reportData.Add(item);
            }

            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Daily Police Report");

                // A. إعداد الـ Headers
                string[] headers = { "Guest Name", "Nationality", "Document Type", "Document Number", "Room", "Arrival", "Departure", "Profession" };

                for (int i = 0; i < headers.Length; i++)
                {
                    var cell = worksheet.Cell(1, i + 1);
                    cell.Value = headers[i];
                    cell.Style.Font.Bold = true;
                    cell.Style.Fill.BackgroundColor = XLColor.LightGray; // لون خلفية خفيف للتمييز
                    cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                }

                // B. تعبئة البيانات (Data Filling)
                int row = 2;
                foreach (var item in reportData)
                {
                    worksheet.Cell(row, 1).Value = item.GuestName;
                    worksheet.Cell(row, 2).Value = item.Nationality;
                    worksheet.Cell(row, 3).Value = item.DocumentType;
                    worksheet.Cell(row, 4).Value = item.DocumentNumber;
                    worksheet.Cell(row, 5).Value = item.RoomNumber;

                    // تنسيق التواريخ
                    worksheet.Cell(row, 6).Value = item.ArrivalDate;
                    worksheet.Cell(row, 6).Style.DateFormat.Format = "dd/MM/yyyy";

                    worksheet.Cell(row, 7).Value = item.DepartureDate;
                    worksheet.Cell(row, 7).Style.DateFormat.Format = "dd/MM/yyyy";

                    worksheet.Cell(row, 8).Value = item.Profession;

                    row++;
                }

                // C. تنسيق نهائي (Final Styling)
                worksheet.Columns().AdjustToContents(); // توسيع الأعمدة حسب المحتوى

                // D. الحفظ في الذاكرة (Save to Memory)
                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    return stream.ToArray();
                }
            }
        }
    }
    }

