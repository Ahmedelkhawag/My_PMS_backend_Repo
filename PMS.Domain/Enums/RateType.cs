using System;
using System.Collections.Generic;
using System.Text;

namespace PMS.Domain.Enums
{
    /// <summary>
    /// نوع خطة السعر:
    /// 1 = FixedAmount        (سعر ثابت لليلة)
    /// 2 = PercentageDiscount (خصم نسبة مئوية من السعر الأساسي)
    /// 3 = FlatDiscount       (خصم مبلغ ثابت من السعر الأساسي)
    /// </summary>
    public enum RateType
    {
        FixedAmount = 1,
        PercentageDiscount = 2,
        FlatDiscount = 3
    }
}

