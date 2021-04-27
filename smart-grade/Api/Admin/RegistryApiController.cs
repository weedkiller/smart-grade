using System;
using System.Linq;
using FirestormSW.SmartGrade.Database;
using FirestormSW.SmartGrade.Database.Model;
using FirestormSW.SmartGrade.Database.Paging;
using FirestormSW.SmartGrade.Extensions;
using FirestormSW.SmartGrade.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FirestormSW.SmartGrade.Api.Admin
{
    [ApiController]
    [Route("api/admin/registry")]
    [Authorize(Policy = UserClaims.Administrator)]
    public class RegistryApiController : ControllerBase
    {
        private readonly AppDatabase database;

        public RegistryApiController(AppDatabase database)
        {
            this.database = database;
        }

        [HttpGet]
        [Route("list")]
        public ActionResult ListUserMonth()
        {
            if (!int.TryParse(Request.Query["UserId"].FirstOrDefault(), out int userId))
                return BadRequest();
            if (!DateTime.TryParse(Request.Query["MonthDate"].FirstOrDefault(), out var monthDate) && Request.Query["WeekDate"].Any())
                return BadRequest();

            var mondayOfFirstWeek = new DateTime(monthDate.Year, monthDate.Month, 1).GetMonday();
            var sundayOfLastWeek = new DateTime(monthDate.Year, monthDate.Month, DateTime.DaysInMonth(monthDate.Year, monthDate.Month)).GetMonday().AddDays(7);
            var monthWeeks = Enumerable.Range(0, (int) ((sundayOfLastWeek.AddDays(1) - mondayOfFirstWeek).TotalDays / 7))
                .Select(w => mondayOfFirstWeek.AddDays(w * 7));

            var entries = monthWeeks
                .Select(w => new
                {
                    WeekString = $"{w:d} → {w.AddDays(5):d}",
                    WeekDate = $"{w:yyyy-MM-dd}",
                    Count = database.RegistryEntries
                        .Include(e => e.Teacher)
                        .Where(e => e.Teacher.Id == userId)
                        .Count(e => e.Date >= w && e.Date < w.AddDays(7)),
                    Locked = database.RegistryEntries
                        .Include(e => e.Teacher)
                        .Where(e => e.Teacher.Id == userId)
                        .Where(e => e.Date >= w && e.Date < w.AddDays(7))
                        .Any(e => e.IsLocked),
                    UserId = userId
                })
                .ToList();

            return Ok(new PagedObject<object>
            {
                Meta = new PagedObjectMeta
                {
                    Page = 1,
                    Pages = 1,
                    PerPage = 6,
                    Total = entries.Count
                },
                Data = entries.Cast<object>().ToList()
            });
        }

        [HttpGet]
        [Route("list_not_locked")]
        public ActionResult ListUserNotLocked(string range)
        {
            var startDate = DateTime.Parse(range.Split('-')[0].Trim());
            var endDate = DateTime.Parse(range.Split('-')[1].Trim());

            var notLockedUsers = database.Users
                .Include(u => u.Groups)
                .Where(u => u.Groups.Any(g => g.GroupType == GroupType.Teacher))
                .Where(u => !database.RegistryEntries
                    .Include(e => e.Teacher)
                    .Where(e => e.Teacher.Id == u.Id)
                    .Where(e => e.Date >= startDate && e.Date <= endDate)
                    .Any(e => e.IsLocked));

            return Ok(notLockedUsers);
        }
    }
}