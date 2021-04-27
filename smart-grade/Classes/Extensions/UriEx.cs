using System;
using System.Web;

namespace FirestormSW.SmartGrade.Extensions
{
    public static class UriEx
    {
        public static string RemoveQueryStringByKey(this Uri uri, string key)
        {
            var newQueryString = HttpUtility.ParseQueryString(uri.Query);
            newQueryString.Remove(key);
            string pagePathWithoutQueryString = uri.GetLeftPart(UriPartial.Path);

            return newQueryString.Count > 0
                ? $"{pagePathWithoutQueryString}?{newQueryString}"
                : pagePathWithoutQueryString;
        }
    }
}