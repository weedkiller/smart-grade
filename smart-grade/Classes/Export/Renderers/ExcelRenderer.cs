using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.AspNetCore.Mvc;
using NPOI.SS.Util;
using NPOI.XSSF.UserModel;

namespace FirestormSW.SmartGrade.Export.Renderers
{
    public class ExcelRenderer : IDataFileRenderer
    {
        public string GetMimeType() => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

        public FileResult Render(IEnumerable<object> data, List<string> headers, string title, string extra)
        {
            var workbook = new XSSFWorkbook();
            var sheet = workbook.CreateSheet();

            var headerRow = sheet.CreateRow(0);
            int index = 0;
            foreach (var header in headers)
            {
                var cell = headerRow.CreateCell(index++);
                cell.SetCellValue(header);
            }

            int rowIndex = 1;
            foreach (var o in data)
            {
                var row = sheet.CreateRow(rowIndex++);
                index = 0;
                foreach (var header in headers)
                {
                    var cell = row.CreateCell(index++);
                    cell.SetCellValue(o.GetType().GetProperty(header).GetValue(o)?.ToString());
                }
            }

            sheet.SetAutoFilter(new CellRangeAddress(0, rowIndex + 1, 0, headers.Count - 1));
            for(int i = 0;  i < headers.Count; i++)
                sheet.AutoSizeColumn(i);

            var stream = new MemoryStream();
            workbook.Write(stream, true);
            stream.Position = 0;
            return new FileStreamResult(stream, GetMimeType()){FileDownloadName = $"{title}_{DateTime.Now:s}.xlsx"};
        }
    }
}