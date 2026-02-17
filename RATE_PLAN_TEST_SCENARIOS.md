# Rate Plan Testing Scenarios - Swagger API Tests

## 📋 جدول المحتويات
1. [المرحلة 1: إنشاء Rate Plans مختلفة](#المرحلة-1-إنشاء-rate-plans-مختلفة)
2. [المرحلة 2: عرض وفحص Rate Plans](#المرحلة-2-عرض-وفحص-rate-plans)
3. [المرحلة 3: تحديث Rate Plans](#المرحلة-3-تحديث-rate-plans)
4. [المرحلة 4: ربط Rate Plan بشركة](#المرحلة-4-ربط-rate-plan-بشركة)
5. [المرحلة 5: إنشاء حجز مع Rate Plan](#المرحلة-5-إنشاء-حجز-مع-rate-plan)
6. [المرحلة 6: تحديث حجز مع Rate Plan](#المرحلة-6-تحديث-حجز-مع-rate-plan)
7. [المرحلة 7: حذف Rate Plan](#المرحلة-7-حذف-rate-plan-soft-delete)
8. [المرحلة 8: Restore Rate Plan](#المرحلة-8-restore-rate-plan)
9. [المرحلة 9: اختبارات Pricing Engine](#المرحلة-9-اختبارات-pricing-engine)
10. [المرحلة 10: اختبارات Lookups Integration](#المرحلة-10-اختبارات-lookups-integration)

---

## 📝 ملاحظات مهمة قبل البدء

### RateType Values:
- `1` = **FixedAmount** (سعر ثابت لليلة)
- `2` = **PercentageDiscount** (خصم نسبة مئوية من السعر الأساسي)
- `3` = **FlatDiscount** (خصم مبلغ ثابت من السعر الأساسي)

### أولوية Rate Plan في الحجز:
1. **Company Rate Plan** (الأولوية الأولى)
2. **User-provided RatePlanId**
3. **Default STANDARD** (Id = 1)

### Authorization:
⚠️ **مهم:** معظم الـ endpoints تحتاج `[Authorize]` - تأكد من تسجيل الدخول في Swagger أولاً!

---

## المرحلة 1: إنشاء Rate Plans مختلفة

### 1.1 إنشاء Rate Plan - Fixed Amount (سعر ثابت)
**Endpoint:** `POST /api/rate-plans`

**Request Body:**
```json
{
  "code": "CORP_FIXED_1000",
  "name": "Corporate Fixed 1000 EGP",
  "description": "Fixed rate for corporate clients - 1000 EGP per night",
  "rateType": 1,
  "rateValue": 1000,
  "isPublic": false,
  "isActive": true
}
```

**Expected Response:** Status `201 Created`
```json
{
  "isSuccess": true,
  "message": "Rate plan created successfully",
  "statusCode": 201,
  "data": {
    "id": 5,
    "code": "CORP_FIXED_1000",
    "name": "Corporate Fixed 1000 EGP",
    "description": "Fixed rate for corporate clients - 1000 EGP per night",
    "rateType": 1,
    "rateValue": 1000,
    "isPublic": false,
    "isActive": true
  }
}
```

**ملاحظات:**
- `rateType: 1` = FixedAmount
- `isPublic: false` = B2B only (غير متاح للعملاء الأفراد)
- احفظ الـ `id` للاستخدام في الاختبارات التالية

---

### 1.2 إنشاء Rate Plan - Percentage Discount (خصم نسبة مئوية)
**Endpoint:** `POST /api/rate-plans`

**Request Body:**
```json
{
  "code": "EARLY_BIRD",
  "name": "Early Bird Discount",
  "description": "15% discount for early bookings",
  "rateType": 2,
  "rateValue": 15,
  "isPublic": true,
  "isActive": true
}
```

**Expected Response:** Status `201 Created`

**ملاحظات:**
- `rateType: 2` = PercentageDiscount
- `rateValue: 15` = 15% discount
- `isPublic: true` = متاح للعملاء الأفراد (B2C)

---

### 1.3 إنشاء Rate Plan - Flat Discount (خصم مبلغ ثابت)
**Endpoint:** `POST /api/rate-plans`

**Request Body:**
```json
{
  "code": "WEEKEND_SPECIAL",
  "name": "Weekend Special",
  "description": "Flat 200 EGP discount on weekends",
  "rateType": 3,
  "rateValue": 200,
  "isPublic": true,
  "isActive": true
}
```

**Expected Response:** Status `201 Created`

**ملاحظات:**
- `rateType: 3` = FlatDiscount
- `rateValue: 200` = 200 EGP discount

---

### 1.4 محاولة إنشاء Rate Plan بنفس Code (يجب أن يفشل)
**Endpoint:** `POST /api/rate-plans`

**Request Body:**
```json
{
  "code": "CORP_FIXED_1000",
  "name": "Duplicate Code Test",
  "rateType": 1,
  "rateValue": 500
}
```

**Expected Response:** Status `400 Bad Request`
```json
{
  "isSuccess": false,
  "message": "Rate plan code must be unique",
  "statusCode": 400
}
```

**ملاحظات:**
- يجب أن يفشل لأن `CORP_FIXED_1000` موجود بالفعل

---

## المرحلة 2: عرض وفحص Rate Plans

### 2.1 عرض كل Rate Plans
**Endpoint:** `GET /api/rate-plans`

**Expected Response:** Status `200 OK`
```json
{
  "isSuccess": true,
  "message": "Rate plans retrieved successfully",
  "statusCode": 200,
  "data": [
    {
      "id": 1,
      "code": "STANDARD",
      "name": "Standard Rate",
      ...
    },
    ...
  ]
}
```

**ملاحظات:**
- يعرض كل Rate Plans (حتى الممسوحة)

---

### 2.2 عرض Public Rate Plans فقط
**Endpoint:** `GET /api/rate-plans?isPublicOnly=true`

**Expected Response:** Status `200 OK`
- قائمة بـ Rate Plans اللي `isPublic = true` فقط

**ملاحظات:**
- مفيد للـ B2C bookings
- الـ Rate Plans اللي `isPublic = false` مش هتظهر

---

### 2.3 عرض Rate Plan محدد
**Endpoint:** `GET /api/rate-plans/{id}`

**مثال:** `GET /api/rate-plans/5`

**Expected Response:** Status `200 OK`
```json
{
  "isSuccess": true,
  "message": "Rate plan retrieved successfully",
  "statusCode": 200,
  "data": {
    "id": 5,
    "code": "CORP_FIXED_1000",
    "name": "Corporate Fixed 1000 EGP",
    ...
  }
}
```

---

### 2.4 عرض Rate Plans من Lookups (للاستخدام في Dropdowns)
**Endpoint:** `GET /api/lookups/rate-plans`

**Expected Response:** Status `200 OK`
```json
[
  {
    "id": 1,
    "code": "STANDARD",
    "name": "Standard Rate"
  },
  {
    "id": 2,
    "code": "NONREF",
    "name": "Non-Refundable"
  },
  ...
]
```

**ملاحظات:**
- قائمة مختصرة (id, code, name فقط)
- مفيد للـ dropdowns في الـ Frontend

---

### 2.5 عرض Rate Plans من Lookups - Public فقط
**Endpoint:** `GET /api/lookups/rate-plans?isPublicOnly=true`

**Expected Response:** Status `200 OK`
- Public Rate Plans فقط (`isPublic = true`)

---

## المرحلة 3: تحديث Rate Plans

### 3.1 تحديث Code (الاختبار الجديد)
**Endpoint:** `PUT /api/rate-plans/{id}`

**مثال:** `PUT /api/rate-plans/5`

**Request Body:**
```json
{
  "code": "CORP_FIXED_1200"
}
```

**Expected Response:** Status `200 OK`
```json
{
  "isSuccess": true,
  "message": "Rate plan updated successfully",
  "statusCode": 200,
  "data": {
    "id": 5,
    "code": "CORP_FIXED_1200",
    ...
  }
}
```

**ملاحظات:**
- Code يتغير من `CORP_FIXED_1000` إلى `CORP_FIXED_1200`

---

### 3.2 تحديث جزئي (Name و Description فقط)
**Endpoint:** `PUT /api/rate-plans/{id}`

**Request Body:**
```json
{
  "name": "Updated Corporate Rate",
  "description": "Updated description"
}
```

**Expected Response:** Status `200 OK`
- Name و Description يتغيروا
- باقي الحقول تفضل كما هي (RateType, RateValue, etc.)

---

### 3.3 تحديث RateValue (تغيير قيمة السعر)
**Endpoint:** `PUT /api/rate-plans/{id}`

**Request Body:**
```json
{
  "rateValue": 1200
}
```

**Expected Response:** Status `200 OK`
- RateValue يتغير من 1000 إلى 1200

---

### 3.4 تحديث RateType و RateValue معاً
**Endpoint:** `PUT /api/rate-plans/{id}`

**Request Body:**
```json
{
  "rateType": 2,
  "rateValue": 20
}
```

**Expected Response:** Status `200 OK`
- RateType يتغير من FixedAmount إلى PercentageDiscount
- RateValue يتغير إلى 20%

---

### 3.5 محاولة تحديث Code لـ Code موجود (يجب أن يفشل)
**Endpoint:** `PUT /api/rate-plans/{id}`

**Request Body:**
```json
{
  "code": "STANDARD"
}
```

**Expected Response:** Status `400 Bad Request`
```json
{
  "isSuccess": false,
  "message": "Rate plan code must be unique",
  "statusCode": 400
}
```

**ملاحظات:**
- يفشل لو `STANDARD` موجود في rate plan تاني

---

### 3.6 تحديث IsActive (تعطيل Rate Plan)
**Endpoint:** `PUT /api/rate-plans/{id}`

**Request Body:**
```json
{
  "isActive": false
}
```

**Expected Response:** Status `200 OK`
- Rate Plan يتعطل (`isActive = false`)
- مش هيظهر في lookups بعد كده

---

## المرحلة 4: ربط Rate Plan بشركة

⚠️ **ملاحظة مهمة:** `UpdateCompanyProfileDto` لا يحتوي على `RatePlanId` حالياً. للاختبار الكامل، يجب إضافة `RatePlanId` في DTO و Service.

### 4.1 إنشاء شركة مع Rate Plan
**Endpoint:** `POST /api/companies`

**Request Body:**
```json
{
  "name": "ABC Corporation",
  "contactPerson": "John Doe",
  "phoneNumber": "+201234567890",
  "email": "contact@abc.com",
  "ratePlanId": 5
}
```

**Expected Response:** Status `201 Created`
- الشركة تُنشأ مع Rate Plan مربوط

---

### 4.2 عرض شركة مع Rate Plan
**Endpoint:** `GET /api/companies/{id}`

**Expected Response:** Status `200 OK`
- تفاصيل الشركة (RatePlanId موجود في Response)

---

## المرحلة 5: إنشاء حجز (Reservation) مع Rate Plan

### 5.1 إنشاء حجز بدون Company (يستخدم Rate Plan الافتراضي STANDARD)
**Endpoint:** `POST /api/reservations`

**Request Body:**
```json
{
  "guestId": 1,
  "roomTypeId": 1,
  "checkInDate": "2026-03-01T14:00:00",
  "checkOutDate": "2026-03-05T14:00:00",
  "nightlyRate": 0,
  "rateCode": "STANDARD",
  "isRateOverridden": false,
  "mealPlanId": 1,
  "bookingSourceId": 1,
  "marketSegmentId": 1,
  "adults": 2,
  "children": 0
}
```

**Expected Response:** Status `201 Created`
- الحجز يُنشأ مع `ratePlanId = 1` (STANDARD)
- السعر يُحسب تلقائياً بناءً على RoomType.BasePrice و Rate Plan

---

### 5.2 إنشاء حجز مع Company (يستخدم Rate Plan الخاص بالشركة)
**Endpoint:** `POST /api/reservations`

**Request Body:**
```json
{
  "guestId": 1,
  "roomTypeId": 1,
  "companyId": 1,
  "checkInDate": "2026-03-01T14:00:00",
  "checkOutDate": "2026-03-05T14:00:00",
  "nightlyRate": 0,
  "isRateOverridden": false,
  "mealPlanId": 1,
  "bookingSourceId": 1,
  "marketSegmentId": 1,
  "adults": 2
}
```

**Expected Response:** Status `201 Created`
- الحجز يُنشأ مع RatePlanId الخاص بالشركة
- السعر يُحسب بناءً على Rate Plan الخاص بالشركة (أولوية أعلى)

---

### 5.3 إنشاء حجز مع Rate Plan محدد يدوياً
**Endpoint:** `POST /api/reservations`

**Request Body:**
```json
{
  "guestId": 1,
  "roomTypeId": 1,
  "ratePlanId": 3,
  "checkInDate": "2026-03-01T14:00:00",
  "checkOutDate": "2026-03-05T14:00:00",
  "nightlyRate": 0,
  "isRateOverridden": false,
  "mealPlanId": 1,
  "bookingSourceId": 1,
  "marketSegmentId": 1,
  "adults": 2
}
```

**Expected Response:** Status `201 Created`
- الحجز يُنشأ مع `ratePlanId = 3` (Early Bird Discount)
- السعر يُحسب بناءً على Early Bird Discount

---

### 5.4 إنشاء حجز مع Override للسعر (تجاهل الحساب التلقائي)
**Endpoint:** `POST /api/reservations`

**Request Body:**
```json
{
  "guestId": 1,
  "roomTypeId": 1,
  "ratePlanId": 3,
  "checkInDate": "2026-03-01T14:00:00",
  "checkOutDate": "2026-03-05T14:00:00",
  "nightlyRate": 1500,
  "isRateOverridden": true,
  "mealPlanId": 1,
  "bookingSourceId": 1,
  "marketSegmentId": 1,
  "adults": 2
}
```

**Expected Response:** Status `201 Created`
- الحجز يُنشأ مع `nightlyRate = 1500` (مش محسوب تلقائياً)
- `isRateOverridden = true` يعني تجاهل الحساب التلقائي

---

### 5.5 عرض حجز مع تفاصيل Rate Plan
**Endpoint:** `GET /api/reservations/{id}`

**Expected Response:** Status `200 OK`
```json
{
  "isSuccess": true,
  "data": {
    "id": 1,
    "ratePlanId": 3,
    "ratePlanName": "Early Bird Discount",
    "nightlyRate": 1700,
    ...
  }
}
```

**ملاحظات:**
- تفاصيل الحجز تتضمن `ratePlanId` و `ratePlanName`

---

## المرحلة 6: تحديث حجز (Reservation) مع Rate Plan

### 6.1 تغيير Rate Plan في حجز موجود
**Endpoint:** `PUT /api/reservations/{id}`

**Request Body:**
```json
{
  "ratePlanId": 4
}
```

**Expected Response:** Status `200 OK`
- RatePlanId يتغير
- السعر يُحسب تلقائياً بناءً على Rate Plan الجديد

---

### 6.2 Override السعر في حجز موجود
**Endpoint:** `PUT /api/reservations/{id}`

**Request Body:**
```json
{
  "nightlyRate": 1800,
  "isRateOverridden": true
}
```

**Expected Response:** Status `200 OK`
- NightlyRate يتغير لـ 1800
- `isRateOverridden = true` يعني تجاهل الحساب التلقائي

---

## المرحلة 7: حذف Rate Plan (Soft Delete)

### 7.1 حذف Rate Plan غير مستخدم
**Endpoint:** `DELETE /api/rate-plans/{id}`

**Expected Response:** Status `200 OK`
```json
{
  "isSuccess": true,
  "message": "Rate plan deleted successfully",
  "statusCode": 200,
  "data": true
}
```

**ملاحظات:**
- Rate Plan يُحذف (soft delete)
- `isDeleted = true`
- مش هيظهر في lookups بعد كده

---

### 7.2 محاولة حذف Rate Plan مستخدم في حجز (يجب أن يفشل)
**Endpoint:** `DELETE /api/rate-plans/{id}`

**Expected Response:** Status `400 Bad Request`
```json
{
  "isSuccess": false,
  "message": "Cannot delete rate plan: it is linked to active reservations or companies",
  "statusCode": 400
}
```

**ملاحظات:**
- يفشل لو Rate Plan مربوط بحجز أو شركة

---

### 7.3 محاولة حذف Rate Plan مستخدم في شركة (يجب أن يفشل)
**Endpoint:** `DELETE /api/rate-plans/{id}`

**Expected Response:** Status `400 Bad Request`
- نفس الرسالة السابقة

---

## المرحلة 8: Restore Rate Plan

### 8.1 استعادة Rate Plan محذوف
**Endpoint:** `POST /api/rate-plans/{id}/restore`

**Expected Response:** Status `200 OK`
```json
{
  "isSuccess": true,
  "message": "Rate plan restored successfully",
  "statusCode": 200,
  "data": true
}
```

**ملاحظات:**
- Rate Plan يُستعاد
- `isDeleted = false`
- هيظهر في lookups تاني

---

## المرحلة 9: اختبارات Pricing Engine (حساب السعر)

### 9.1 اختبار Fixed Amount
**السيناريو:**
- RoomType.BasePrice = **2000 EGP**
- RatePlan: FixedAmount = **1000 EGP**

**Expected NightlyRate:** `1000 EGP`

**ملاحظات:**
- السعر الثابت يتجاهل السعر الأساسي

---

### 9.2 اختبار Percentage Discount
**السيناريو:**
- RoomType.BasePrice = **2000 EGP**
- RatePlan: PercentageDiscount = **15%**

**Expected NightlyRate:** `1700 EGP` (2000 - 15% = 2000 - 300)

**الحساب:**
```
BasePrice = 2000
Discount = 2000 × 15% = 300
NightlyRate = 2000 - 300 = 1700
```

---

### 9.3 اختبار Flat Discount
**السيناريو:**
- RoomType.BasePrice = **2000 EGP**
- RatePlan: FlatDiscount = **200 EGP**

**Expected NightlyRate:** `1800 EGP` (2000 - 200)

**الحساب:**
```
BasePrice = 2000
Discount = 200
NightlyRate = 2000 - 200 = 1800
```

---

### 9.4 اختبار Percentage Discount > 100% (يجب أن يفشل)
**Endpoint:** `PUT /api/rate-plans/{id}`

**Request Body:**
```json
{
  "rateType": 2,
  "rateValue": 150
}
```

**Expected Response:** Status `400 Bad Request`
```json
{
  "isSuccess": false,
  "message": "Percentage discount cannot exceed 100%",
  "statusCode": 400
}
```

**ملاحظات:**
- 150% discount غير منطقي
- يجب أن يفشل validation

---

### 9.5 اختبار Negative Rate Value (يجب أن يفشل)
**Endpoint:** `PUT /api/rate-plans/{id}`

**Request Body:**
```json
{
  "rateValue": -50
}
```

**Expected Response:** Status `400 Bad Request`
- RateValue لا يمكن أن يكون سالب

---

## المرحلة 10: اختبارات Lookups Integration

### 10.1 عرض كل Lookups (بما فيها Rate Plans)
**Endpoint:** `GET /api/lookups/all`

**Expected Response:** Status `200 OK`
```json
{
  "isSuccess": true,
  "data": {
    "ratePlans": [
      {
        "id": 1,
        "code": "STANDARD",
        "name": "Standard Rate"
      },
      ...
    ],
    "bookingSources": [...],
    "marketSegments": [...],
    ...
  }
}
```

**ملاحظات:**
- Response يحتوي على `ratePlans` array
- مفيد للـ Frontend dropdowns

---

## 📊 Checklist للاختبار الكامل

### ✅ Rate Plans CRUD
- [ ] إنشاء Rate Plan (Fixed Amount)
- [ ] إنشاء Rate Plan (Percentage Discount)
- [ ] إنشاء Rate Plan (Flat Discount)
- [ ] عرض كل Rate Plans
- [ ] عرض Public Rate Plans فقط
- [ ] عرض Rate Plan محدد
- [ ] تحديث Code
- [ ] تحديث جزئي
- [ ] تحديث RateValue
- [ ] تحديث RateType و RateValue
- [ ] محاولة تحديث Code مكرر (يجب أن يفشل)
- [ ] تعطيل Rate Plan
- [ ] حذف Rate Plan غير مستخدم
- [ ] محاولة حذف Rate Plan مستخدم (يجب أن يفشل)
- [ ] Restore Rate Plan

### ✅ Reservations Integration
- [ ] إنشاء حجز بدون Company (يستخدم STANDARD)
- [ ] إنشاء حجز مع Company (يستخدم Company Rate Plan)
- [ ] إنشاء حجز مع Rate Plan محدد
- [ ] إنشاء حجز مع Override للسعر
- [ ] عرض حجز مع تفاصيل Rate Plan
- [ ] تحديث Rate Plan في حجز
- [ ] Override السعر في حجز

### ✅ Pricing Engine
- [ ] Fixed Amount calculation
- [ ] Percentage Discount calculation
- [ ] Flat Discount calculation
- [ ] Percentage > 100% validation (يجب أن يفشل)
- [ ] Negative RateValue validation (يجب أن يفشل)

### ✅ Lookups
- [ ] عرض Rate Plans من lookups
- [ ] عرض Public Rate Plans فقط من lookups
- [ ] عرض كل Lookups (بما فيها Rate Plans)

---

## 🔍 Troubleshooting

### المشكلة: Rate Plan مش بيظهر في Lookups
**الحل:**
- تأكد إن `IsActive = true`
- تأكد إن `IsDeleted = false`

### المشكلة: السعر مش بيتحسب صح
**الحل:**
- تأكد إن RoomType.BasePrice موجود
- تأكد إن Rate Plan موجود و active
- تأكد إن `IsRateOverridden = false`

### المشكلة: مش قادر أحذف Rate Plan
**الحل:**
- تأكد إن Rate Plan مش مربوط بحجز أو شركة
- لو مربوط، لازم تحذف الحجوزات/الشركات الأول

---

## 📝 ملاحظات نهائية

1. **Authorization:** تأكد من تسجيل الدخول في Swagger قبل الاختبار
2. **IDs:** احفظ الـ IDs اللي بتستخدمها في الاختبارات (Rate Plans, Companies, Reservations)
3. **Data Cleanup:** بعد الاختبار، ممكن تحذف البيانات اللي أنشأتها للاختبار
4. **Company Update:** حالياً `UpdateCompanyProfileDto` لا يحتوي على `RatePlanId` - يجب إضافته للاختبار الكامل

---

**تاريخ الإنشاء:** 2026-02-17  
**آخر تحديث:** 2026-02-17
