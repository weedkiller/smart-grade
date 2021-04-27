using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.AspNetCore.Mvc;

namespace FirestormSW.SmartGrade.Export.Renderers
{
    public class CsvRenderer : IDataFileRenderer
    {
        public string GetMimeType() => "text/csv";

        public FileResult Render(IEnumerable<object> data, List<string> headers, string title, string extra)
        {
            var builder = new StringBuilder();
            builder.AppendLine(string.Join(",", headers));

            foreach (var o in data)
                builder.AppendLine(string.Join(",", headers.Select(h => o.GetType().GetProperty(h).GetValue(o)?.ToString())));

            var bytes = Encoding.UTF8.GetBytes(builder.ToString());
            return new FileContentResult(bytes, GetMimeType()){FileDownloadName = $"{title}_{DateTime.Now:s}.csv"};
        }
    }
}