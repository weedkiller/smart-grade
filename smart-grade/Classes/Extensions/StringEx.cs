namespace FirestormSW.SmartGrade.Extensions
{
    public static class StringEx
    {
        public static string OrNull(this string str) => string.IsNullOrEmpty(str) ? null : str;
    }
}