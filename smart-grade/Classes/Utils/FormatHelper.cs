using System;

namespace FirestormSW.SmartGrade.Utils
{
    public static class FormatHelper
    {
        private static readonly string[] Suffixes = { "byte", "KB", "MB", "GB", "TB", "PB" };
        
        public static string FormatSize(long size, int decimalPlaces = 0)
        {
            int mag = (int) Math.Log(size, 1024);
            double val = Math.Round(size / Math.Pow(1024, mag), decimalPlaces);
            return $"{val} {Suffixes[mag]}";
        }
    }
}