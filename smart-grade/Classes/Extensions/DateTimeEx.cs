using System;

namespace FirestormSW.SmartGrade.Extensions
{
    public static class DateTimeEx
    {
        public static DateTime EndOfMonth(this DateTime dateTime)
        {
            var daysInMonth = DateTime.DaysInMonth(dateTime.Year, dateTime.Month);
            return new DateTime(dateTime.Year, dateTime.Month, daysInMonth, 23, 59, 59);
        }

        public static DateTime GetMonday(this DateTime dateTime)
        {
            int diff = (7 + (dateTime.DayOfWeek - DayOfWeek.Monday)) % 7;
            return dateTime.AddDays(-1 * diff).Date;
        }
    }
}