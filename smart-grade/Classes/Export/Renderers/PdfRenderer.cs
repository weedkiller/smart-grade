using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.AspNetCore.Mvc;
using QuestPDF.Drawing;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;

namespace FirestormSW.SmartGrade.Export.Renderers
{
    public class PdfRenderer : IDataFileRenderer, IDocument
    {
        private readonly IEnumerable<object> data;
        private readonly List<string> headers;
        private readonly string title;

        public PdfRenderer()
        {
        }

        public PdfRenderer(IEnumerable<object> data, List<string> headers, string title)
        {
            this.data = data;
            this.headers = headers;
            this.title = title;
        }

        public string GetMimeType() => "application/pdf";

        public FileResult Render(IEnumerable<object> _data, List<string> _headers, string _title, string extra)
        {
            var document = new PdfRenderer(_data, _headers, _title);
            var stream = new MemoryStream();
            document.GeneratePdf(stream);
            return new FileContentResult(stream.ToArray(), GetMimeType()) {FileDownloadName = $"{_title}_{DateTime.Now:s}.pdf"};
        }

        public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

        public void Compose(IContainer container)
        {
            container
                .PaddingHorizontal(50)
                .PaddingVertical(50)
                .Page(page =>
                {
                    page.Header(ComposeHeader);
                    page.Content(ComposeContent);
                    page.Footer().AlignCenter().PageNumber();
                });
        }

        private void ComposeHeader(IContainer container)
        {
            container.Row(row =>
            {
                row.RelativeColumn().Stack(column =>
                {
                    column.Element().Text(title, TextStyle.Default.Size(18).Bold());
                    column.Element().Text(DateTime.Now);
                });
            });
        }

        private void ComposeContent(IContainer container)
        {
            container.PaddingVertical(30).PageableStack(column =>
            {
                column.Spacing(5);

                column.Element(ComposeTable);
            });
        }

        private void ComposeTable(IContainer container)
        {
            var headerStyle = TextStyle.Default.SemiBold();

            container.Section(section =>
            {
                // Header
                section.Header().BorderBottom(1).Padding(5).Row(row =>
                {
                    foreach (var header in headers)
                        row.RelativeColumn().Text(header, headerStyle);
                });
                
                // Content
                section
                    .Content()
                    .PageableStack(column =>
                    {
                        foreach (var o in data)
                        {
                            column.Element().BorderBottom(1).BorderColor("CCC").Padding(5).Row(row =>
                            {
                                foreach (var header in headers)
                                {
                                    row
                                        .RelativeColumn()
                                        .Text(o.GetType().GetProperty(header).GetValue(o)?.ToString() ?? "");
                                }
                            });
                        }
                    });
            });
        }
    }
}