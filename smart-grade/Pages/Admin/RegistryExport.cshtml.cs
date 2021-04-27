using System;
using System.Collections.Generic;
using System.Linq;
using FirestormSW.SmartGrade.Database;
using FirestormSW.SmartGrade.Database.Model;
using FirestormSW.SmartGrade.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace FirestormSW.SmartGrade.Pages.Admin
{
    [AllowAnonymous]
    public class RegistryExport : PageModel
    {
        public User Teacher { get; private set; }
        public DateTime WeekStart { get; private set; }
        public IEnumerable<RegistryEntry> RegEntries { get; private set; }
        public Dictionary<RegistryTimeSlot, int> StartTimes { get; } = new();
        public RegistryConfiguration RegConfig { get; set; }

        private readonly AppDatabase database;

        public RegistryExport(AppDatabase database)
        {
            this.database = database;
        }

        public void OnGet(int userId, DateTime weekDate)
        {
            WeekStart = weekDate.Date;

            Teacher = database.Users
                .Include(u => u.TeacherGradeLevel)
                .ThenInclude(l => l.RegistryConfiguration)
                .ThenInclude(c => c.Slots)
                .GetById(userId);
            if (Teacher == null)
                return;

            RegConfig = Teacher.TeacherGradeLevel.RegistryConfiguration;
            var endTime = RegConfig.StartTime.Hours;
            foreach (var slot in RegConfig.Slots)
            {
                StartTimes[slot] = endTime;
                endTime += slot.Duration;
            }

            RegEntries = database.RegistryEntries
                .Include(e => e.Teacher)
                .Include(e => e.Class)
                .Include(e => e.Subject)
                .Where(e => e.Teacher == Teacher)
                .Where(e => e.Date >= WeekStart && e.Date <= WeekStart.AddDays(5));
        }
    }
}