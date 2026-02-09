using Microsoft.EntityFrameworkCore;
using PMS.Application.DTOs.Common;
using PMS.Application.DTOs.Reservations;
using PMS.Application.Interfaces.Services;
using PMS.Application.Interfaces.UOF;
using PMS.Domain.Entities;
using PMS.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace PMS.Infrastructure.Implmentations.Services
{
    public class ReservationsService : IReservationService
	{
		private readonly IUnitOfWork _unitOfWork;

		public ReservationsService(IUnitOfWork unitOfWork)
		{
			_unitOfWork = unitOfWork;
		}


		public async Task<ResponseObjectDto<ReservationDto>> CreateReservationAsync(CreateReservationDto dto)
		{
			var response = new ResponseObjectDto<ReservationDto>();

			// 1. التحقق من التواريخ (Validation)
			if (dto.CheckInDate.Date < DateTime.UtcNow.Date)
			{
				// ملحوظة: ممكن نسمح بحجز قديم لو الـ Audit Log يسمح، بس الطبيعي لأ
				// response.Message = "تاريخ الدخول لا يمكن أن يكون في الماضي";
			}
			if (dto.CheckOutDate <= dto.CheckInDate)
			{
				return new ResponseObjectDto<ReservationDto>
				{
					IsSuccess = false,
					Message = "تاريخ الخروج يجب أن يكون بعد تاريخ الدخول",
					StatusCode = 400
				};
			}

			// 2. حساب عدد الليالي
			var nights = (dto.CheckOutDate.Date - dto.CheckInDate.Date).Days;
			if (nights == 0) nights = 1; // لو نفس اليوم بنحسبها ليلة (Day Use)

			// 3. حساب تكلفة الغرفة
			var roomTotal = dto.NightlyRate * nights;

			// 4. معالجة وحساب الخدمات الإضافية
			var reservationServices = new List<ReservationService>();
			decimal servicesTotal = 0;

			if (dto.Services != null && dto.Services.Any())
			{
				foreach (var serviceDto in dto.Services)
				{
					// لو الخدمة يومية: السعر * العدد * الليالي
					// لو مرة واحدة: السعر * العدد
					var serviceTotalCost = serviceDto.IsPerDay
						? (serviceDto.Price * serviceDto.Quantity * nights)
						: (serviceDto.Price * serviceDto.Quantity);

					servicesTotal += serviceTotalCost;

					reservationServices.Add(new ReservationService
					{
						ServiceName = serviceDto.ServiceName,
						Price = serviceDto.Price,
						Quantity = serviceDto.Quantity,
						IsPerDay = serviceDto.IsPerDay,
						TotalServicePrice = serviceTotalCost
					});
				}
			}

			// 5. الحسابات النهائية (الضريبة والصافي)
			var amountAfterDiscount = (roomTotal + servicesTotal) - dto.DiscountAmount;
			if (amountAfterDiscount < 0) amountAfterDiscount = 0; // عشان ميبقاش بالسالب

			var taxAmount = amountAfterDiscount * 0.15m; // ضريبة 15% (السعودية)
			var grandTotal = amountAfterDiscount + taxAmount;

			// 6. إنشاء رقم الحجز (Format: BK-yyyyMMdd-XXX)
			// دي طريقة بسيطة، ممكن نطورها لـ Sequence في الداتابيز
			var count = await _unitOfWork.Reservations.CountAsync();
			var reservationNumber = $"BK-{DateTime.Now:yyyyMMdd}-{count + 1:000}";

			// 7. التحويل للـ Entity
			var reservation = new Reservation
			{
				ReservationNumber = reservationNumber,
				GuestId = dto.GuestId,
				RoomTypeId = dto.RoomTypeId,
				RoomId = dto.RoomId, // ممكن يكون Null
				CheckInDate = dto.CheckInDate,
				CheckOutDate = dto.CheckOutDate,

				// الماليات المحسوبة
				NightlyRate = dto.NightlyRate,
				TotalAmount = roomTotal,
				ServicesAmount = servicesTotal,
				DiscountAmount = dto.DiscountAmount,
				TaxAmount = taxAmount,
				GrandTotal = grandTotal,

				// تفاصيل البيزنس
				RateCode = dto.RateCode,
				MealPlan = dto.MealPlan,
				PurposeOfVisit = dto.PurposeOfVisit,
				MarketSegment = dto.MarketSegment,
				IsPostMaster = dto.IsPostMaster,
				IsGuestPay = dto.IsGuestPay,
				IsNoExtend = dto.IsNoExtend,

				Status = ReservationStatus.Pending, // افتراضياً معلق
				Source = (ReservationSource)dto.SourceId,

				Services = reservationServices, // القائمة اللي جهزناها فوق
				Adults = dto.Adults,
				Children = dto.Children,
				Notes = dto.Notes
			};

			if (dto.RoomId.HasValue)
			{
				var isRoomTaken = await _unitOfWork.Reservations.GetQueryable()
					.AnyAsync(r => r.RoomId == dto.RoomId &&
								   !r.IsDeleted &&
								   r.Status != ReservationStatus.Cancelled &&
								   r.CheckInDate < dto.CheckOutDate &&
								   r.CheckOutDate > dto.CheckInDate);

				if (isRoomTaken)
				{
					return new ResponseObjectDto<ReservationDto>
					{
						IsSuccess = false,
						Message = "عفواً، الغرفة محجوزة بالفعل في هذه الفترة!",
						StatusCode = 409
					};
				}
			}

			// 8. الحفظ في قاعدة البيانات
			await _unitOfWork.Reservations.AddAsync(reservation);
			await _unitOfWork.CompleteAsync();

			// 9. تجهيز الرد
			// محتاجين اسم النزيل ونوع الغرفة عشان الرد يكون مقروء
			var guest = await _unitOfWork.Guests.GetByIdAsync(dto.GuestId);
			var roomType = await _unitOfWork.RoomTypes.GetByIdAsync(dto.RoomTypeId);
			var room = dto.RoomId.HasValue ? await _unitOfWork.Rooms.GetByIdAsync(dto.RoomId.Value) : null;

			response.IsSuccess = true;
			response.Message = "تم إنشاء الحجز بنجاح";
			response.StatusCode = 201;
			response.Data = new ReservationDto
			{
				Id = reservation.Id,
				ReservationNumber = reservation.ReservationNumber,
				GuestName = guest?.FullName ?? "Unknown",
				RoomTypeName = roomType?.Name ?? "Unknown",
				RoomNumber = room?.RoomNumber,
				CheckInDate = reservation.CheckInDate,
				CheckOutDate = reservation.CheckOutDate,
				Nights = nights,
				NightlyRate = reservation.NightlyRate,
				TotalAmount = reservation.TotalAmount,
				ServicesAmount = reservation.ServicesAmount,
				DiscountAmount = reservation.DiscountAmount,
				TaxAmount = reservation.TaxAmount,
				GrandTotal = reservation.GrandTotal,
				Status = reservation.Status.ToString(),
				CarPlate = reservation.CarPlate,
				ExternalReference = reservation.ExternalReference,
				Services = reservationServices.Select(s => new ReservationServiceDto
				{
					ServiceName = s.ServiceName,
					Price = s.Price,
					Quantity = s.Quantity,
					IsPerDay = s.IsPerDay,
					Total = s.TotalServicePrice
				}).ToList()
			};

			return response;
		}


		public async Task<ResponseObjectDto<IEnumerable<ReservationListDto>>> GetAllReservationsAsync(string? search, string? status)
		{
			// 1. الكويري (زي ما هو)
			var query = _unitOfWork.Reservations.GetQueryable() // تأكد إنك ضفت GetQueryable في الريبو
				.Include(r => r.Guest)
				.Include(r => r.Room)
				.Include(r => r.RoomType)
				.AsQueryable();

			// 2. البحث (Search)
			if (!string.IsNullOrEmpty(search))
			{
				query = query.Where(r =>
					r.ReservationNumber.Contains(search) ||
					r.Guest.FullName.Contains(search) ||
					r.Guest.PhoneNumber.Contains(search));
			}

			// 3. الفلترة (Filter Status)
			if (!string.IsNullOrEmpty(status) && Enum.TryParse<ReservationStatus>(status, true, out var statusEnum))
			{
				query = query.Where(r => r.Status == statusEnum);
			}

			// 4. الترتيب والتحويل (Projection)
			var data = await query
				.OrderByDescending(r => r.CreatedAt)
				.Select(r => new ReservationListDto
				{
					Id = r.Id,
					ReservationNumber = r.ReservationNumber,
					GuestName = r.Guest.FullName,
					GuestPhone = r.Guest.PhoneNumber,
					RoomNumber = r.Room != null ? r.Room.RoomNumber : "غير مخصص",
					RoomTypeName = r.RoomType.Name,
					CheckInDate = r.CheckInDate.ToString("yyyy-MM-dd"),
					CheckOutDate = r.CheckOutDate.ToString("yyyy-MM-dd"),
					Nights = (r.CheckOutDate - r.CheckInDate).Days,
					GrandTotal = r.GrandTotal,
					Status = r.Status.ToString(),
					StatusColor = r.Status == ReservationStatus.Confirmed ? "green" :
								  r.Status == ReservationStatus.Pending ? "orange" : "red"
				})
				.ToListAsync();

		
			return new ResponseObjectDto<IEnumerable<ReservationListDto>>
			{
				IsSuccess = true,
				Message = "تم استرجاع قائمة الحجوزات بنجاح",
				StatusCode = 200,
				Data = data
			};
		}



		public async Task<ResponseObjectDto<bool>> ChangeStatusAsync(ChangeReservationStatusDto dto)
		{
			var response = new ResponseObjectDto<bool>();

			// 1. نجيب الحجز من الداتابيز
			var reservation = await _unitOfWork.Reservations.GetByIdAsync(dto.ReservationId);
			if (reservation == null)
			{
				response.IsSuccess = false;
				response.Message = "الحجز غير موجود";
				response.StatusCode = 404;
				return response;
			}

			// 2. منطق Check-In (تسجيل الدخول)
			if (dto.NewStatus == ReservationStatus.CheckIn)
			{
				// أ) لازم ميكونش ملغي أو معموله خروج قبل كده
				if (reservation.Status == ReservationStatus.Cancelled || reservation.Status == ReservationStatus.CheckOut)
				{
					response.IsSuccess = false;
					response.Message = "لا يمكن عمل Check-In لحجز ملغي أو منتهي!";
					response.StatusCode = 400;
					return response;
				}

				// ب) التأكد من وجود غرفة
				if (reservation.RoomId == null && dto.RoomId == null)
				{
					response.IsSuccess = false;
					response.Message = "لا يمكن تسجيل الدخول بدون تخصيص غرفة! يرجى اختيار غرفة.";
					response.StatusCode = 400;
					return response;
				}

				// ج) تحديث الغرفة لو اتبعتت جديد
				if (dto.RoomId.HasValue)
				{
					// هنا ممكن نضيف تحقق إن الغرفة فاضية ونضيفة (Logic مؤجل)
					reservation.RoomId = dto.RoomId;
				}

				reservation.CheckInDate = DateTime.UtcNow; // نحدث وقت الدخول الفعلي
			}

			// 3. منطق Cancellation (الإلغاء)
			if (dto.NewStatus == ReservationStatus.Cancelled)
			{
				if (reservation.Status == ReservationStatus.CheckIn || reservation.Status == ReservationStatus.CheckOut)
				{
					response.IsSuccess = false;
					response.Message = "لا يمكن إلغاء حجز قيد التشغيل أو منتهي!";
					response.StatusCode = 400;
					return response;
				}
				// هنا المفروض نرجع الغرفة Available (لو كنا بنغير حالة الغرف)
			}

			// 4. تنفيذ التغيير
			reservation.Status = dto.NewStatus;

			if (!string.IsNullOrEmpty(dto.Note))
			{
				reservation.Notes += $" | Status Update: {dto.Note}";
			}

			_unitOfWork.Reservations.Update(reservation); // لو مش موجودة في الـ Repo مش مشكلة، الـ EF بيتابع التغيير
			await _unitOfWork.CompleteAsync();

			response.IsSuccess = true;
			response.Message = $"تم تغيير حالة الحجز إلى {dto.NewStatus} بنجاح";
			response.StatusCode = 200;
			response.Data = true;

			return response;
		}



		public async Task<ResponseObjectDto<ReservationDto>> GetReservationByIdAsync(int id)
		{
			var response = new ResponseObjectDto<ReservationDto>();

			// 1. استرجاع الحجز بكل علاقاته
			var reservation = await _unitOfWork.Reservations.GetQueryable()
				.Include(r => r.Guest)
				.Include(r => r.Room)
				.Include(r => r.RoomType)
				.Include(r => r.Services) // عشان نجيب قائمة الخدمات
				.FirstOrDefaultAsync(r => r.Id == id);

			if (reservation == null)
			{
				response.IsSuccess = false;
				response.Message = "الحجز غير موجود";
				response.StatusCode = 404;
				return response;
			}

			// 2. التحويل لـ DTO (Mapping)
			var dto = new ReservationDto
			{
				Id = reservation.Id,
				ReservationNumber = reservation.ReservationNumber,

				// Guest Info
				GuestId = reservation.GuestId,
				GuestName = reservation.Guest.FullName,
				GuestPhone = reservation.Guest.PhoneNumber,
				GuestEmail = reservation.Guest.Email,
				GuestNationalId = reservation.Guest.NationalId,

				// Room Info
				RoomTypeId = reservation.RoomTypeId,
				RoomTypeName = reservation.RoomType.Name,
				RoomId = reservation.RoomId,
				RoomNumber = reservation.Room != null ? reservation.Room.RoomNumber : "غير مخصص",

				// Dates
				CheckInDate = reservation.CheckInDate,
				CheckOutDate = reservation.CheckOutDate,
				Nights = (reservation.CheckOutDate - reservation.CheckInDate).Days,

				// Business Details
				RateCode = reservation.RateCode,
				MealPlan = reservation.MealPlan,
				Source = reservation.Source.ToString(),
				Notes = reservation.Notes,

				// Financials
				NightlyRate = reservation.NightlyRate,
				TotalAmount = reservation.TotalAmount,
				ServicesAmount = reservation.ServicesAmount,
				DiscountAmount = reservation.DiscountAmount,
				TaxAmount = reservation.TaxAmount,
				GrandTotal = reservation.GrandTotal,
				Status = reservation.Status.ToString(),
				CarPlate = reservation.CarPlate,
				ExternalReference = reservation.ExternalReference,

				// Services List
				Services = reservation.Services.Select(s => new ReservationServiceDto
				{
					ServiceName = s.ServiceName,
					Price = s.Price,
					Quantity = s.Quantity,
					IsPerDay = s.IsPerDay,
					Total = s.TotalServicePrice
				}).ToList()
			};

			response.IsSuccess = true;
			response.Data = dto;
			response.StatusCode = 200;

			return response;
		}



		public async Task<ResponseObjectDto<bool>> DeleteReservationAsync(int id)
		{
			var response = new ResponseObjectDto<bool>();

			// 1. هات الحجز (ممكن تحتاج Includes لو عايز تتأكد من المدفوعات قدام)
			var reservation = await _unitOfWork.Reservations.GetByIdAsync(id);

			if (reservation == null)
			{
				response.IsSuccess = false;
				response.Message = "الحجز غير موجود";
				response.StatusCode = 404;
				return response;
			}

			// 2. 🛑 حماية البيزنس (Validation)
			// ممنوع حذف حجز دخل الغرفة أو خرج منها، أو حتى اتلغى (عشان التاريخ)
			// الحذف مسموح بس لو "لسه معمول دلوقتي" (Pending)
			if (reservation.Status != ReservationStatus.Pending)
			{
				response.IsSuccess = false;
				response.Message = "لا يمكن حذف هذا الحجز لأنه ليس في حالة الانتظار. يرجى استخدام الإلغاء (Cancel) بدلاً من الحذف.";
				response.StatusCode = 400;
				return response;
			}

			// شرط إضافي مستقبلي:
			// if (reservation.Payments.Any()) return "ممنوع حذف حجز عليه مدفوعات";

			// 3. التنفيذ
			_unitOfWork.Reservations.Delete(reservation);
			await _unitOfWork.CompleteAsync();

			response.IsSuccess = true;
			response.Message = "تم حذف الحجز نهائياً بنجاح";
			response.Data = true;
			response.StatusCode = 200;

			return response;
		}



		public async Task<ResponseObjectDto<bool>> RestoreReservationAsync(int id)
		{
			var response = new ResponseObjectDto<bool>();

			// 1. هات الحجز حتى لو ممسوح (IgnoreQueryFilters)
			// بنعمل Include للغرفة عشان لو طلع فيه تعارض نكتب رقم الغرفة في الرسالة
			var reservation = await _unitOfWork.Reservations.GetQueryable()
				.IgnoreQueryFilters()
				.Include(r => r.Room)
				.FirstOrDefaultAsync(r => r.Id == id);

			// 2. التحقق الأساسي
			if (reservation == null)
			{
				response.IsSuccess = false;
				response.Message = "الحجز غير موجود";
				response.StatusCode = 404;
				return response;
			}

			if (!reservation.IsDeleted)
			{
				response.IsSuccess = false;
				response.Message = "الحجز نشط بالفعل";
				response.StatusCode = 400;
				return response;
			}

			// 3. 🛑 التحقق من تعارض الغرفة (Conflict Check)
			// بننفذ اللوجيك ده بس لو الحجز كان متحددله غرفة (RoomId != null)
			if (reservation.RoomId.HasValue)
			{
				var isRoomTaken = await _unitOfWork.Reservations.GetQueryable()
					.AnyAsync(r =>
						// نفس الغرفة
						r.RoomId == reservation.RoomId &&

						// مش هو هو نفس الحجز (أمان)
						r.Id != reservation.Id &&

						// الحجز التاني مش ممسوح
						!r.IsDeleted &&

						// الحجز التاني مش ملغي (عشان الملغي مش بيشغل حيز زمني)
						r.Status != ReservationStatus.Cancelled &&

						// معادلة التقاطع الزمني (Overlap Logic)
						// (بداية الجديد < نهاية القديم) AND (نهاية الجديد > بداية القديم)
						r.CheckInDate < reservation.CheckOutDate &&
						r.CheckOutDate > reservation.CheckInDate
					);

				if (isRoomTaken)
				{
					response.IsSuccess = false;
					// رسالة ذكية بتقول المشكلة فين بالظبط
					response.Message = $"عفواً، لا يمكن استرجاع الحجز لأن الغرفة {reservation.Room?.RoomNumber} تم حجزها لنزيل آخر في هذه الفترة!";
					response.StatusCode = 409; // Conflict Status Code
					return response;
				}
			}

			// 4. عملية الاسترجاع الآمنة
			reservation.IsDeleted = false;
			reservation.DeletedAt = null;
			reservation.DeletedBy = null;

			// توثيق العملية في الملاحظات (Audit Trail بسيط)
			reservation.Notes = (reservation.Notes ?? "") + $" | تم الاسترجاع في {DateTime.Now:yyyy-MM-dd HH:mm}";

			_unitOfWork.Reservations.Update(reservation);
			await _unitOfWork.CompleteAsync();

			response.IsSuccess = true;
			response.Message = "تم استرجاع الحجز بنجاح";
			response.Data = true;
			response.StatusCode = 200;

			return response;
		}



		public async Task<ResponseObjectDto<ReservationDto>> UpdateReservationAsync(UpdateReservationDto dto)
		{
			var response = new ResponseObjectDto<ReservationDto>();

			// 1. استرجاع الحجز القديم من الداتابيز (شامل الخدمات عشان نعرف نمسحها)
			var reservation = await _unitOfWork.Reservations.GetQueryable()
				.Include(r => r.Services) // ضروري جداً
				.Include(r => r.Guest)    // عشان الرد النهائي
				.Include(r => r.Room)
				.Include(r => r.RoomType)// عشان الرد النهائي
				.FirstOrDefaultAsync(r => r.Id == dto.Id);

			if (reservation == null)
			{
				response.IsSuccess = false;
				response.Message = "الحجز غير موجود";
				response.StatusCode = 404;
				return response;
			}

			// 2. التحقق من التواريخ
			if (dto.CheckOutDate.Date <= dto.CheckInDate.Date)
			{
				response.IsSuccess = false;
				response.Message = "تاريخ المغادرة يجب أن يكون بعد تاريخ الوصول!";
				response.StatusCode = 400;
				return response;
			}

			// 3. 🛑 فحص التوفر (Availability Check) - أهم خطوة!
			// لو تم تغيير الغرفة أو التواريخ، لازم نتأكد إن المكان فاضي
			if (dto.RoomId.HasValue)
			{
				var isRoomTaken = await _unitOfWork.Reservations.GetQueryable()
					.AnyAsync(r =>
						r.RoomId == dto.RoomId &&        // نفس الغرفة
						r.Id != dto.Id &&                // 👈 استثناء الحجز الحالي (عشان ميخبطش في نفسه)
						!r.IsDeleted &&                  // مش ممسوح
						r.Status != ReservationStatus.Cancelled && // مش ملغي
						r.CheckInDate < dto.CheckOutDate && // معادلة التقاطع
						r.CheckOutDate > dto.CheckInDate
					);

				if (isRoomTaken)
				{
					response.IsSuccess = false;
					response.Message = "عفواً، الغرفة المختارة محجوزة بالفعل في التواريخ الجديدة!";
					response.StatusCode = 409;
					return response;
				}
			}

			// 4. إعادة الحسابات المالية (Re-Calculation) 🧮

			// أ) حساب الليالي
			var nights = (dto.CheckOutDate.Date - dto.CheckInDate.Date).Days;
			if (nights < 1) nights = 1;

			// ب) حساب سعر الغرفة
			var roomTotal = dto.NightlyRate * nights;

			// ج) إعادة بناء قائمة الخدمات (بنمسح القديم ونضيف الجديد)
			// الأول: نحذف الخدمات القديمة من الداتابيز
			if (reservation.Services != null && reservation.Services.Any())
			{
				// ملحوظة: الـ EF Core ذكي، لما نغير الـ Collection ونعمل Save هو هيتصرف
				// بس للأمان يفضل نمسحهم لو عندنا Repo للخدمات، أو نعتمد على استبدال القائمة
				reservation.Services.Clear();
			}

			// الثاني: نحسب الجديد
			var newServicesList = new List<ReservationService>();
			decimal servicesTotal = 0;

			if (dto.Services != null && dto.Services.Any())
			{
				foreach (var s in dto.Services)
				{
					var itemTotal = s.IsPerDay
						? (s.Price * s.Quantity * nights)
						: (s.Price * s.Quantity);

					servicesTotal += itemTotal;

					newServicesList.Add(new ReservationService
					{
						ServiceName = s.ServiceName,
						Price = s.Price,
						Quantity = s.Quantity,
						IsPerDay = s.IsPerDay,
						TotalServicePrice = itemTotal
						// ReservationId هيتحط أوتوماتيك لما نضيفهم للـ Parent
					});
				}
			}

			// د) الحساب الختامي
			var subTotal = (roomTotal + servicesTotal) - dto.DiscountAmount;
			if (subTotal < 0) subTotal = 0;

			var taxAmount = subTotal * 0.15m;
			var grandTotal = subTotal + taxAmount;

			// 5. تحديث بيانات الكيان (Mapping) 🔄
			reservation.GuestId = dto.GuestId;
			reservation.RoomTypeId = dto.RoomTypeId;
			reservation.RoomId = dto.RoomId;
			reservation.CheckInDate = dto.CheckInDate;
			reservation.CheckOutDate = dto.CheckOutDate;

			// تحديث الماليات
			reservation.NightlyRate = dto.NightlyRate;
			reservation.TotalAmount = roomTotal;
			reservation.ServicesAmount = servicesTotal;
			reservation.DiscountAmount = dto.DiscountAmount;
			reservation.TaxAmount = taxAmount;
			reservation.GrandTotal = grandTotal;

			// تحديث البيانات الأخرى
			reservation.RateCode = dto.RateCode;
			reservation.MealPlan = dto.MealPlan;
			reservation.Source = (Domain.Enums.ReservationSource)dto.SourceId;
			reservation.PurposeOfVisit = dto.PurposeOfVisit;
			reservation.MarketSegment = dto.MarketSegment;
			reservation.Notes = dto.Notes;
			reservation.ExternalReference = dto.ExternalReference;
			reservation.CarPlate = dto.CarPlate;
			reservation.Adults = dto.Adults;
			reservation.Children = dto.Children;

			// استبدال قائمة الخدمات
			reservation.Services = newServicesList;

			// التوثيق (UpdatedBy هيتملي أوتوماتيك من الـ Interceptor اللي عملناه)

			// 6. الحفظ
			_unitOfWork.Reservations.Update(reservation);
			await _unitOfWork.CompleteAsync();

			// 7. تجهيز الرد
			// (هنا بنعمل نفس كود الـ Create عشان نرجع شكل الحجز الجديد)
			// ... اختصاراً للكود، ممكن تنسخ الـ Map اللي في CreateReservationAsync وتحطه هنا
			// أو تستخدم AutoMapper لو متاح، بس خلينا يدوي للأمان

			// (تجهيز الرد السريع)
			response.IsSuccess = true;
			response.Message = "تم تعديل الحجز وإعادة حساب الفاتورة بنجاح";
			response.StatusCode = 200;

			// هنرجع الحجز بعد التعديل (Mapping بسيط)
			response.Data = new ReservationDto
			{
				Id = reservation.Id,
				ReservationNumber = reservation.ReservationNumber,

				// بيانات النزيل (مهمة عشان الاسم يظهر)
				GuestId = reservation.GuestId,
				// بنعمل Null Check عشان لو النزيل مش محمل
				GuestName = reservation.Guest?.FullName ?? "غير متوفر",
				GuestPhone = reservation.Guest?.PhoneNumber,
				GuestEmail = reservation.Guest?.Email,
				GuestNationalId = reservation.Guest?.NationalId,

				// بيانات الغرفة
				RoomTypeId = reservation.RoomTypeId,
				RoomTypeName = reservation.RoomType?.Name ?? "Unknown",
				RoomId = reservation.RoomId,
				RoomNumber = reservation.Room?.RoomNumber ?? "Non-Room",

				// التواريخ
				CheckInDate = reservation.CheckInDate,
				CheckOutDate = reservation.CheckOutDate,
				// بنحسب الليالي من التواريخ الجديدة
				Nights = (reservation.CheckOutDate.Date - reservation.CheckInDate.Date).Days > 0
				 ? (reservation.CheckOutDate.Date - reservation.CheckInDate.Date).Days
				 : 1,

				// الماليات (عشان الأصفار اللي في الصورة تختفي)
				NightlyRate = reservation.NightlyRate,
				TotalAmount = reservation.TotalAmount,
				ServicesAmount = reservation.ServicesAmount,
				DiscountAmount = reservation.DiscountAmount,
				TaxAmount = reservation.TaxAmount,
				GrandTotal = reservation.GrandTotal, // ده أهم رقم

				// تفاصيل الحالة والبيزنس
				Status = reservation.Status.ToString(),
				RateCode = reservation.RateCode,
				MealPlan = reservation.MealPlan,
				Source = reservation.Source.ToString(),
				PurposeOfVisit = reservation.PurposeOfVisit,
				MarketSegment = reservation.MarketSegment,
				Notes = reservation.Notes,
				ExternalReference = reservation.ExternalReference,
				CarPlate = reservation.CarPlate,

				// قائمة الخدمات الجديدة
				Services = reservation.Services != null
			? reservation.Services.Select(s => new ReservationServiceDto
			{
				ServiceName = s.ServiceName,
				Price = s.Price,
				Quantity = s.Quantity,
				IsPerDay = s.IsPerDay,
				Total = s.TotalServicePrice
			}).ToList()
			: new List<ReservationServiceDto>()

			};

			return response;
		}
	}
}
	