using System;
using System.Linq;
using FirestormSW.SmartGrade.Export;
using FirestormSW.SmartGrade.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json.Linq;

namespace FirestormSW.SmartGrade.Api.Admin
{
    [ApiController]
    [Route("api/admin/export")]
    [Authorize(Policy = UserClaims.Administrator)]
    public class ExportDataApiController : ControllerBase
    {
        protected readonly IServiceProvider serviceProvider;

        public ExportDataApiController(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        [HttpGet]
        public ActionResult ExportData(string dataSourceName, string format, string filters, string returnUrl, string extra)
        {
            var dataSourceType = TypeUtils.GetType(dataSourceName);
            var rendererType = TypeUtils.GetType(format);
            var filterData = JObject.Parse(filters);
            var query = filterData.Values<JProperty>()
                .ToDictionary(k => $"query[{k.Name}]", v => new StringValues(v.Value.Value<string>()));

            //var dataSource = Activator.CreateInstance(dataSourceType, database, loginService) as IDataProvider;
            var dataSource = DependencyInjection.Activate(dataSourceType, serviceProvider) as IDataProvider;
            var data = dataSource.GetData(new QueryCollection(query)).ToList();
            var headers = dataSource.GetHeaders().ToList();
            if (!data.Any())
                return Redirect(returnUrl);

            if (!headers.Any())
                headers = data.First().GetType().GetProperties().Select(p => p.Name).ToList();

            var renderer = Activator.CreateInstance(rendererType) as IDataFileRenderer;

            return renderer.Render(data, headers, dataSource.GetTitle(), extra);
        }
    }
}