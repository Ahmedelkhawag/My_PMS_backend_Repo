using PMS.Application.DTOs.Common;
using PMS.Application.DTOs.Rooms;
using PMS.Application.Interfaces.Services;
using PMS.Application.Interfaces.UOF;
using PMS.Domain.Entities;
using PMS.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace PMS.Infrastructure.Implmentations.Services
{
    public class RoomService : IRoomService
    {
        private readonly IUnitOfWork _unitOfWork;

        public RoomService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<RoomDto>> GetAllRoomsAsync(int? floor, int? roomTypeId, string? status)
        {
            // 1. تجهيز فلتر الحالة (تحويل من string لـ Enum)
            RoomStatus? statusEnum = null;
            if (!string.IsNullOrEmpty(status) && Enum.TryParse<RoomStatus>(status, true, out var parsed))
            {
                statusEnum = parsed;
            }

            // 2. استخدام Repository لجلب البيانات مع الفلترة
            // (الشرط: لو الفلتر null هات كله، لو بقيمة هات اللي بيساويه)
            var rooms = await _unitOfWork.Rooms.FindAllAsync(
                r => (floor == null || r.FloorNumber == floor) &&
                     (roomTypeId == null || r.RoomTypeId == roomTypeId) &&
                     (statusEnum == null || r.Status == statusEnum),
                new[] { "RoomType" } // Include عشان نجيب السعر والاسم
            );

            // 3. التحويل لـ DTO (Manual Mapping)
            return rooms.Select(r => new RoomDto
            {
                Id = r.Id,
                RoomNumber = r.RoomNumber,
                FloorNumber = r.FloorNumber,
                Status = r.Status.ToString(), // بنرجع اسم الحالة (Available, Occupied...)
                RoomType = r.RoomType?.Name ?? "N/A",
                Price = r.RoomType?.BasePrice ?? 0,
                MaxAdults = r.RoomType?.MaxAdults ?? 0
            });
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
                Status = RoomStatus.Available,
                IsActive = true
            };

            await _unitOfWork.Rooms.AddAsync(room);
            await _unitOfWork.CompleteAsync();

            var returnedDto = new RoomDto
            {
                Id = room.Id,
                RoomNumber = room.RoomNumber,
                FloorNumber = room.FloorNumber,
                Status = room.Status.ToString(),
                RoomType = roomType.Name,
                Price = roomType.BasePrice,
                MaxAdults = roomType.MaxAdults
            };

            response.IsSuccess = true;
            response.Message = "تم إضافة الغرفة بنجاح";
            response.Data = returnedDto;
            response.StatusCode = 201;

            return response;
        }


        public async Task<ResponseObjectDto<RoomDto>> UpdateRoomAsync(UpdateRoomDto dto)
        {
            var response = new ResponseObjectDto<RoomDto>();

            var room = await _unitOfWork.Rooms.GetByIdAsync(dto.Id);
            if (room == null)
            {
                response.IsSuccess = false;
                response.Message = "الغرفة غير موجودة!";
                response.StatusCode = 404;
                return response;
            }

            // ب) التأكد إن رقم الغرفة الجديد مش متكرر (مع استثناء الغرفة الحالية)
            var duplicateRoom = await _unitOfWork.Rooms.FindAsync(r => r.RoomNumber == dto.RoomNumber && r.Id != dto.Id);
            if (duplicateRoom != null)
            {
                response.IsSuccess = false;
                response.Message = $"رقم الغرفة {dto.RoomNumber} مستخدم بالفعل لغرفة أخرى!";
                response.StatusCode = 400;
                return response;
            }

            room.RoomNumber = dto.RoomNumber;
            room.FloorNumber = dto.FloorNumber;
            room.RoomTypeId = dto.RoomTypeId;
            room.Notes = dto.Notes;

            if (Enum.TryParse<RoomStatus>(dto.Status, true, out var statusEnum))
            {
                room.Status = statusEnum;
            }

            _unitOfWork.Rooms.Update(room);
            await _unitOfWork.CompleteAsync();


            var roomType = await _unitOfWork.RoomTypes.GetByIdAsync(room.RoomTypeId);

            response.IsSuccess = true;
            response.Message = "تم تحديث بيانات الغرفة بنجاح";
            response.Data = new RoomDto
            {
                Id = room.Id,
                RoomNumber = room.RoomNumber,
                FloorNumber = room.FloorNumber,
                Status = room.Status.ToString(),
                RoomType = roomType?.Name ?? "",
                Price = roomType?.BasePrice ?? 0,
                MaxAdults = roomType?.MaxAdults ?? 0
            };

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

            // هنعمل Soft Delete (إخفاء فقط)
            room.IsActive = false;
            _unitOfWork.Rooms.Update(room);
            await _unitOfWork.CompleteAsync();

            response.IsSuccess = true;
            response.Message = "تم حذف الغرفة (أرشفة) بنجاح";
            response.Data = true;

            return response;
        }
    }
}
