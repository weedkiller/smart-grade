using FirestormSW.SmartGrade.Database;
using FirestormSW.SmartGrade.Pages.Shared;
using FirestormSW.SmartGrade.Properties;
using FirestormSW.SmartGrade.Services;
using FirestormSW.SmartGrade.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FirestormSW.SmartGrade.Api.Admin
{
    [ApiController]
    [Route("api/admin/management")]
    [Authorize(Policy = UserClaims.Administrator)]
    public class ManagementApiController : ControllerBase
    {
        private readonly IRazorPartialToStringRenderer partialToStringRenderer;
        private readonly DatabaseProperties properties;
        private readonly EmailService emailService;

        public ManagementApiController(IRazorPartialToStringRenderer partialToStringRenderer, DatabaseProperties properties, EmailService emailService)
        {
            this.partialToStringRenderer = partialToStringRenderer;
            this.properties = properties;
            this.emailService = emailService;
        }

        [HttpPost]
        [Route("general")]
        public void SaveGeneralSettings([FromForm] string defaultLanguage, [FromForm] string registryLanguage, [FromForm] string principalName)
        {
            var settings = properties.GetOrCreateProperty<GeneralSettings>();
            settings.DefaultCulture = defaultLanguage;
            settings.RegistryCulture = registryLanguage;
            settings.PrincipalName = principalName;
            properties.SetProperty(settings);
        }

        [HttpPost]
        [Route("email")]
        public ActionResult SaveEmailSettings([FromForm] string serverHost, [FromForm] int serverPort, [FromForm] string serverUser,
            [FromForm] string serverPass, [FromForm] string senderAddress)
        {
            var settings = properties.GetOrCreateProperty<EmailSettings>();
            settings.SmtpHost = serverHost;
            settings.SmtpPort = serverPort;
            settings.SmtpUser = serverUser;
            settings.SmtpPassword = serverPass;
            settings.SenderAddress = senderAddress;
            properties.SetProperty(settings);
            emailService.TryConnecting();

            return Ok(new {Source = "Email", Status = EmailService.Status});
        }

        [HttpGet]
        [Route("email")]
        public ActionResult DebugEmailTemplate()
            => Content(partialToStringRenderer.RenderPartialToStringAsync("EmailPage", new EmailPage {Text = "qqqqeeee"}).Result, "text/html");
    }
}