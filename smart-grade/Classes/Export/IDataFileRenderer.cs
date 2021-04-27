using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;

namespace FirestormSW.SmartGrade.Export
{
    public interface IDataFileRenderer
    {
        string GetMimeType();
        FileResult Render(IEnumerable<object> data, List<string> headers, string title, string extra);
    }
}