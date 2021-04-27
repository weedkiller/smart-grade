using System.Collections.Generic;
using FirestormSW.SmartGrade.Database.Paging;
using Microsoft.AspNetCore.Http;

namespace FirestormSW.SmartGrade.Export
{
    public interface IDataProvider
    {
        IEnumerable<object> GetData(IQueryCollection queryCollection);
        IEnumerable<string> GetHeaders();
        string GetTitle();
    }
}