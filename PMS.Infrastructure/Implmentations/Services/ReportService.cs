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
            // 1. تحديد تاريخ التقرير (تاريخ البيزنس المفتوح)
            DateTime reportDate;
            if (businessDate.HasValue)
            {
                reportDate = businessDate.Value.Date;
            }
            else
            {
                // بنجيب اليوم اللي حالته Open من السيستم
                var currentBusinessDay = await _unitOfWork.BusinessDays
                    .GetQueryable()
                    .AsNoTracking()
                    .FirstOrDefaultAsync(b => b.Status == BusinessDayStatus.Open);

                reportDate = currentBusinessDay?.Date ?? DateTime.UtcNow.Date;
            }

            // 2. سحب البيانات (تم تصحيح الـ Query هنا) 👇
            var activeReservations = await _unitOfWork.Reservations
                .GetQueryable()
                .Include(r => r.Guest) // هنجيب النزيل بس
                .Include(r => r.Room)  // هنجيب الغرفة
                .Where(r => r.Status == ReservationStatus.CheckIn && !r.IsDeleted)
                .AsNoTracking()
                .ToListAsync();

            // 3. تحويل البيانات لـ DTO
            var reportData = new List<PoliceReportDto>();

            foreach (var res in activeReservations)
            {
                // تحديد نوع الوثيقة بناءً على وجود الرقم القومي
                bool hasNationalId = !string.IsNullOrWhiteSpace(res.Guest.NationalId);

                var item = new PoliceReportDto
                {
                    GuestName = res.Guest.FullName,

                    // هنا بنسحب الجنسية كـ string عادي من غير Include 👇
                    Nationality = !string.IsNullOrEmpty(res.Guest.Nationality) ? res.Guest.Nationality : "Unknown",

                    DocumentType = hasNationalId ? "National ID" : "Passport",

                    // Fallback لو مفيش باسبور مسجل في الـ Entity نستخدم الـ NationalId
                    DocumentNumber = hasNationalId ? res.Guest.NationalId : (res.Guest.NationalId ?? "N/A"),

                    RoomNumber = res.Room != null ? res.Room.RoomNumber : "N/A",
                    ArrivalDate = res.CheckInDate.DateTime,
                    DepartureDate = res.CheckOutDate.DateTime,
                    Profession = "Guest"
                };

                reportData.Add(item);
            }

            // 4. توليد ملف الإكسيل (نفس كود ClosedXML اللي عملناه في Task 3)
            using (var workbook = new ClosedXML.Excel.XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Daily Police Report");

                // Headers
                string[] headers = { "Guest Name", "Nationality", "Document Type", "Document Number", "Room", "Arrival", "Departure", "Profession" };
                for (int i = 0; i < headers.Length; i++)
                {
                    var cell = worksheet.Cell(1, i + 1);
                    cell.Value = headers[i];
                    cell.Style.Font.Bold = true;
                    cell.Style.Fill.BackgroundColor = ClosedXML.Excel.XLColor.LightGray;
                }

                // Data Rows
                int row = 2;
                foreach (var item in reportData)
                {
                    worksheet.Cell(row, 1).Value = item.GuestName;
                    worksheet.Cell(row, 2).Value = item.Nationality;
                    worksheet.Cell(row, 3).Value = item.DocumentType;
                    worksheet.Cell(row, 4).Value = item.DocumentNumber;
                    worksheet.Cell(row, 5).Value = item.RoomNumber;

                    worksheet.Cell(row, 6).Value = item.ArrivalDate;
                    worksheet.Cell(row, 6).Style.DateFormat.Format = "dd/MM/yyyy";

                    worksheet.Cell(row, 7).Value = item.DepartureDate;
                    worksheet.Cell(row, 7).Style.DateFormat.Format = "dd/MM/yyyy";

                    worksheet.Cell(row, 8).Value = item.Profession;
                    row++;
                }

                worksheet.Columns().AdjustToContents();

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    return stream.ToArray();
                }
            }
        }
    }
    }

