using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using FirestormSW.SmartGrade.Database;
using FirestormSW.SmartGrade.Database.Model;
using FirestormSW.SmartGrade.Extensions;
using FirestormSW.SmartGrade.Services;
using FirestormSW.SmartGrade.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FirestormSW.SmartGrade.Api.Teacher
{
    [ApiController]
    [Route("api/teacher/registry")]
    [Authorize(Policy = UserClaims.Teacher)]
    public class RegistryApiController : ControllerBase
    {
        private readonly AppDatabase database;
        private readonly LoginService loginService;

        public RegistryApiController(AppDatabase database, LoginService loginService)
        {
            this.database = database;
            this.loginService = loginService;
        }

        [HttpGet]
        public ActionResult GetRegistryEntries(int year, int month, int weekFirstDay)
            => Ok(GetEntriesForWeek(year, month, weekFirstDay));

        [HttpGet]
        [Route("fullcalendar")]
        public ActionResult GetRegistryEntriesCalendar(DateTime start, DateTime end)
        {
            var entries = GetEntriesForWeek(start, end)
                .Select(e => new
                {
                    Title = e.Text ?? string.Empty,
                    Start = e.Date,
                    End = e.Date.AddHours(1), // TODO custom lengths?
                    Display = "auto",
                    BackgroundColor = e.IsLocked ? "#ffaeae" : "",
                    Extra = new
                    {
                        Id = e.Id,
                        ClassId = e.Class == null ? -1 : e.Class.Id,
                        ClassName = e.Class == null ? null : e.Class.Name,
                        SubjectId = e.Subject == null ? -1 : e.Subject.Id,
                        SubjectName = e.Subject == null ? null : e.Subject.Name,
                        SubjectRegName = e.Subject == null ? null : e.Subject.RegistryName,
                        IsPco = e.IsPco,
                        IsLocked = e.IsLocked
                    }
                })
                .Union(GetEntriesForWeek(start.AddDays(-7), end.AddDays(-7)).Select(e => new
                {
                    Title = e.Text ?? string.Empty,
                    Start = e.Date.AddDays(7),
                    End = e.Date.AddHours(1).AddDays(7), // TODO custom lengths?
                    Display = "background",
                    BackgroundColor = "",
                    Extra = new
                    {
                        Id = e.Id,
                        ClassId = e.Class == null ? -1 : e.Class.Id,
                        ClassName = e.Class == null ? null : e.Class.Name,
                        SubjectId = e.Subject == null ? -1 : e.Subject.Id,
                        SubjectName = e.Subject == null ? null : e.Subject.Name,
                        SubjectRegName = e.Subject == null ? null : e.Subject.RegistryName,
                        IsPco = e.IsPco,
                        IsLocked = e.IsLocked
                    }
                }));
            return Ok(JsonSerializer.Serialize(entries, new JsonSerializerOptions {PropertyNamingPolicy = JsonNamingPolicy.CamelCase}));
        }

        [HttpPost]
        [Route("lock")]
        public ActionResult LockWeek([FromForm] int year, [FromForm] int month, [FromForm] int weekFirstDay)
        {
            var entries = GetEntriesForWeek(year, month, weekFirstDay);
            foreach (var entry in entries)
                entry.IsLocked = true;
            database.SaveChanges();
            return Ok();
        }

        [HttpPost]
        [Route("unlock")]
        public ActionResult UnlockWeek([FromForm] int year, [FromForm] int month, [FromForm] int weekFirstDay)
        {
            var entries = GetEntriesForWeek(year, month, weekFirstDay);
            foreach (var entry in entries)
                entry.IsLocked = false;
            database.SaveChanges();
            return Ok();
        }

        private IEnumerable<RegistryEntry> GetEntriesForWeek(int year, int month, int weekFirstDay)
        {
            var startDate = new DateTime(year, month, weekFirstDay);
            var endDate = startDate + TimeSpan.FromDays(7);
            return GetEntriesForWeek(startDate, endDate);
        }

        private IQueryable<RegistryEntry> GetEntriesForWeek(DateTime startDate, DateTime endDate)
        {
            var teacher = loginService.GetCurrentLoggedInUser(HttpContext);
            return database.RegistryEntries
                .Include(e => e.Class)
                .Include(e => e.Subject)
                .Include(e => e.Teacher)
                .Where(e => e.Teacher == teacher && e.Date >= startDate && e.Date <= endDate);
        }

        [HttpPost]
        public ActionResult CreateOrUpdateRegistryEntry([FromForm] int year, [FromForm] int month, [FromForm] int day, [FromForm] int hour,
            [FromForm] int? classId, [FromForm] int? subjectId, [FromForm] string text, [FromForm] string isPco)
        {
            var teacher = loginService.GetCurrentLoggedInUser(HttpContext);
            var entry = database.RegistryEntries
                            .Include(e => e.Class)
                            .Include(e => e.Teacher)
                            .Include(e => e.Subject)
                            .Where(e => e.Date.Year == year && e.Date.Month == month && e.Date.Day == day && e.Date.Hour == hour)
                            .SingleOrDefault(e => e.Teacher.Id == teacher.Id)
                        ?? new RegistryEntry
                        {
                            Teacher = teacher,
                            Class = database.Groups.GetById(classId),
                            Subject = database.Subjects.GetById(subjectId),
                            Date = new DateTime(year, month, day, hour, 0, 0),
                            EntryDate = DateTime.Now,
                            ModifyDate = DateTime.Now,
                            IsLocked = false,
                            IsPco = isPco == "on",
                            Text = text
                        };
            entry.Class = database.Groups.GetById(classId);
            entry.Subject = database.Subjects.GetById(subjectId);
            entry.ModifyDate = DateTime.Now;
            entry.Text = text;
            entry.IsPco = isPco == "on";
            if (!database.RegistryEntries.Contains(entry))
                database.RegistryEntries.Add(entry);

            // Update priority
            var historyEntries = database.RegistryClassHistory
                .Include(h => h.Teacher)
                .Include(h => h.Class)
                .Where(h => h.Class.Id == classId && h.Teacher.Id == teacher.Id)
                .ToList();
            database.RegistryClassHistory.RemoveRange(historyEntries);
            database.RegistryClassHistory.Add(new RegistryClassHistory
            {
                Class = database.Groups.GetById(classId),
                Teacher = teacher
            });
            database.SaveChanges();

            return Ok();
        }

        [HttpPost]
        [Route("delete")]
        public ActionResult DeleteEntry([FromForm] int entryId)
        {
            var entry = database.RegistryEntries.GetById(entryId);
            if (entry != null)
                database.RegistryEntries.Remove(entry);
            database.SaveChanges();

            return Ok();
        }

        [HttpGet]
        [Route("config")]
        public ActionResult GetConfigurations()
        {
            return Ok(database.GradeLevels
                .Include(g => g.RegistryConfiguration)
                .ThenInclude(g => g.Slots)
                .Select(g => new
                {
                    GradeLevelId = g.Id,
                    Configuration = g.RegistryConfiguration
                }));
        }

        [HttpPost]
        [Route("config/setData")]
        public ActionResult SetConfigurationData([FromForm] int gradeLevelId, [FromForm] string startTime,
            [FromForm] string groupName, [FromForm] string activityName, [FromForm] bool pageGrades, [FromForm] bool pageAbsences,
            [FromForm] bool pageDisciplinary, [FromForm] bool pageNotes, [FromForm] bool pageRegistry)
        {
            var gradeLevel = database.GradeLevels.Include(g => g.RegistryConfiguration).GetById(gradeLevelId);
            if (gradeLevel == null)
                return NotFound();

            gradeLevel.RegistryConfiguration ??= new RegistryConfiguration();
            gradeLevel.RegistryConfiguration.StartTime = TimeSpan.Parse(startTime);
            gradeLevel.RegistryConfiguration.GroupName = groupName;
            gradeLevel.RegistryConfiguration.ActivityName = activityName;
            gradeLevel.RegistryConfiguration.EnableGradesPage = pageGrades;
            gradeLevel.RegistryConfiguration.EnableAbsencesPage = pageAbsences;
            gradeLevel.RegistryConfiguration.EnableDisciplinaryPage = pageDisciplinary;
            gradeLevel.RegistryConfiguration.EnableNotesPage = pageNotes;
            gradeLevel.RegistryConfiguration.EnableRegistryPage = pageRegistry;
            database.SaveChanges();

            return Ok();
        }

        [HttpPost]
        [Route("config/addRow")]
        public ActionResult AddConfigurationRow([FromForm] int existingId, [FromForm] int gradeLevelId, [FromForm] int duration,
            [FromForm] string hasClass, [FromForm] string hasSubject, [FromForm] string hasPco, [FromForm] string hasText,
            [FromForm] string presets, [FromForm] string customLabel)
        {
            var gradeLevel = database.GradeLevels
                .Include(g => g.RegistryConfiguration)
                .ThenInclude(g => g.Slots)
                .GetById(gradeLevelId);
            if (gradeLevel == null)
                return NotFound();

            gradeLevel.RegistryConfiguration ??= new RegistryConfiguration
            {
                StartTime = TimeSpan.FromHours(8)
            };
            gradeLevel.RegistryConfiguration.Slots ??= new List<RegistryTimeSlot>();
            var slot = gradeLevel.RegistryConfiguration.Slots.SingleOrDefault(s => s.Id == existingId) ?? new RegistryTimeSlot();
            slot.Duration = duration;
            slot.HasClass = hasClass == "on";
            slot.HasSubject = hasSubject == "on";
            slot.HasPCO = hasPco == "on";
            slot.HasText = hasText == "on";
            slot.CustomLabel = customLabel;
            slot.Presets = presets.Split('\n').ToList();
            if (!gradeLevel.RegistryConfiguration.Slots.Contains(slot))
                gradeLevel.RegistryConfiguration.Slots.Add(slot);
            database.SaveChanges();

            return Ok();
        }

        [HttpDelete]
        [Route("config/deleteRow")]
        public ActionResult DeleteConfigurationRow([FromForm] int rowId)
        {
            var gradeLevel = database.GradeLevels
                .Include(e => e.RegistryConfiguration)
                .ThenInclude(e => e.Slots)
                .SingleOrDefault(l => l.RegistryConfiguration.Slots.Any(s => s.Id == rowId));
            if (gradeLevel == null)
                return NotFound();

            gradeLevel.RegistryConfiguration.Slots.Remove(gradeLevel.RegistryConfiguration.Slots.First(s => s.Id == rowId));
            database.SaveChanges();

            return Ok();
        }
    }
}