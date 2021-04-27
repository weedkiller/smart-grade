using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using NPOI.SS.Util;
using NPOI.XSSF.UserModel;

namespace FirestormSW.SmartGrade
{
    public class Program
    {
        public static void Main(string[] args)
        {
            /*var workbook = new XSSFWorkbook();
            var sheet = workbook.CreateSheet();
            sheet.AddMergedRegion(new CellRangeAddress(0, 0, 0, 8));
            sheet.AddMergedRegion(new CellRangeAddress(1, 1, 0, 32));
            sheet.AddMergedRegion(new CellRangeAddress(2, 2, 0, 32));
            sheet.SetColumnWidth(0, 2213);
            sheet.SetColumnWidth(1, 1288);
            for (int i = 2; i <= 32; i++)
                sheet.SetColumnWidth(i, 924);
            sheet.CreateRow(0).CreateCell(0).SetCellValue("Title");
            sheet.CreateRow(1).CreateCell(0).SetCellValue("Title1");
            sheet.CreateRow(2).CreateCell(0).SetCellValue("Title2");

            workbook.Write(File.Create("test.xlsx"));*/

            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder => { webBuilder.UseStartup<Startup>(); });
    }
}