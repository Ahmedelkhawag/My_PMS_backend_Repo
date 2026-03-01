namespace PMS.Infrastructure.Constants;

public static class PdfConstants
{
    public const string ArabicFontName = "Cairo";
    public const string FontResourcePath = "PMS.Infrastructure.Assets.Fonts.Cairo-Regular.ttf";
    
    public const string ConfigKeyHotelName = "RegistrationCard:HotelName";
    public const string AppConfigKeyHotelName = "App:HotelName";
    public const string DefaultHotelName = "Hotel";
    
    // Bilingual Text Constants
    public const string Title = "بطاقة تسجيل النزيل | GUEST REGISTRATION CARD";
    public const string SectionGuestInfo = "Guest Information | معلومات النزيل";
    public const string SectionStayDetails = "Stay Details | تفاصيل الإقامة";
    public const string SectionTerms = "Terms & Conditions | الشروط والأحكام";
    
    // Labels
    public const string LabelGuestName = "اسم النزيل | Guest Name:";
    public const string LabelPassportId = "رقم الجواز/الهوية | Passport/ID No:";
    public const string LabelNationality = "الجنسية | Nationality:";
    public const string LabelPhone = "رقم الهاتف | Phone Number:";
    public const string LabelEmail = "البريد الإلكتروني | Email:";
    
    public const string LabelRoomNumber = "رقم الغرفة | Room No:";
    public const string LabelRoomType = "نوع الغرفة | Room Type:";
    public const string LabelArrival = "الوصول | Arrival:";
    public const string LabelDeparture = "المغادرة | Departure:";
    public const string LabelNightlyRate = "السعر لليلة | Nightly Rate:";
    public const string LabelAdultsChildren = "البالغين / الأطفال | Adults / Children:";
    
    public const string LabelGuestSignature = "Guest Signature | توقيع النزيل:";
    public const string LabelReceptionist = "Receptionist | الموظف:";
    public const string LabelDate = "Date | التاريخ:";
    
    // Terms
    public const string Term1 = "1. Check-out Time: Standard check-out time is 12:00 PM. (وقت تسجيل المغادرة هو الساعة 12:00 ظهراً).";
    public const string Term2 = "2. Liability: The hotel is not responsible for valuables left in the room. (الفندق غير مسؤول عن الأشياء الثمينة المتروكة بالغرفة).";
    public const string Term3 = "3. Smoking Policy: Smoking is strictly prohibited in rooms. (يُمنع التدخين تماماً داخل الغرف).";
}
