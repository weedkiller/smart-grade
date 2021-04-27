using System;
using System.Linq;
using System.Threading;
using FirestormSW.SmartGrade.Database;
using FirestormSW.SmartGrade.Database.Model;
using FirestormSW.SmartGrade.Pages.Shared;
using FirestormSW.SmartGrade.Properties;
using MailKit.Net.Smtp;
using Microsoft.EntityFrameworkCore;
using MimeKit;

namespace FirestormSW.SmartGrade.Services
{
    public class EmailService
    {
        public static ConnectionStatus Status { get; private set; } = ConnectionStatus.NotConfigured;

        private readonly AppDatabase database;
        private readonly DatabaseProperties databaseProperties;
        private readonly IRazorPartialToStringRenderer partialToStringRenderer;

        public EmailService(AppDatabase database, DatabaseProperties databaseProperties, IRazorPartialToStringRenderer partialToStringRenderer)
        {
            this.database = database;
            this.databaseProperties = databaseProperties;
            this.partialToStringRenderer = partialToStringRenderer;
        }

        public void TryConnecting()
        {
            var settings = databaseProperties.GetProperty<EmailSettings>();
            if (string.IsNullOrEmpty(settings?.SmtpHost?.Trim()) || settings.SmtpPort == 0)
            {
                Status = ConnectionStatus.NotConfigured;
                return;
            }

            using var client = CreateClient();
            Status = client == null ? ConnectionStatus.ConnectionError : ConnectionStatus.Connected;
        }

        public async void SendNotification(StudentDataEntry entry, bool created)
        {
            if (string.IsNullOrEmpty(entry.Student.NotificationEmail))
                return;
            var user = database.Users
                .Include(u => u.Groups)
                .ThenInclude(g => g.GradeLevel)
                .Single(u => u.Id == entry.Student.Id);
            if (!CanSendNotification(user.Groups.Single(g => g.GroupType == GroupType.Class).GradeLevel, entry, created))
                return;

            var settings = databaseProperties.GetProperty<EmailSettings>();
            var body = await partialToStringRenderer.RenderPartialToStringAsync("EmailPage", new EmailPage
            {
                Text = $"{entry} was added"
            });
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("SmartGrade", settings.SenderAddress));
            message.To.Add(new MailboxAddress(string.Empty, entry.Student.NotificationEmail));
            message.Subject = "Test Header";
            message.Body = new TextPart("html") {Text = body};

            ThreadPool.QueueUserWorkItem(o => SendEmail(message));
        }

        private void SendEmail(MimeMessage message)
        {
            try
            {
                var client = CreateClient();
                if (client == null)
                {
                    Console.Out.WriteLine("Unable to send email! (couldn't create client)");
                    return;
                }

                client.Send(message);
                client.Disconnect(true);
                client.Dispose();
            }
            catch (Exception ex)
            {
                Console.Out.WriteLine($"Unable to send email! ({ex.Message})");
            }
        }

        private SmtpClient CreateClient()
        {
            try
            {
                var settings = databaseProperties.GetProperty<EmailSettings>();
                var client = new SmtpClient();
                client.Connect(settings.SmtpHost, settings.SmtpPort);
                client.Authenticate(settings.SmtpUser, settings.SmtpPassword);
                if (!client.IsConnected || !client.IsAuthenticated)
                {
                    client.Disconnect(true);
                    client.Dispose();
                    return null;
                }

                return client;
            }
            catch
            {
                return null;
            }
        }

        private static bool CanSendNotification(GradeLevel gradeLevel, StudentDataEntry entry, bool created)
        {
            if (entry is Grade && created && !gradeLevel.EmailOnGradeAdded ||
                entry is Grade && !created && !gradeLevel.EmailOnGradeDeleted ||
                entry is Absence && created && !gradeLevel.EmailOnAbsenceAdded ||
                entry is Absence && !created && !gradeLevel.EmailOnAbsenceDeleted ||
                entry is Disciplinary && created && !gradeLevel.EmailOnDisciplinaryAdded ||
                entry is Disciplinary && !created && !gradeLevel.EmailOnDisciplinaryDeleted)
                return false;
            return true;
        }
    }

    public enum ConnectionStatus
    {
        NotConfigured,
        ConnectionError,
        Connected
    }
}