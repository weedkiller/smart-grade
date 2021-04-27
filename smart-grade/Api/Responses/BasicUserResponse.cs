using System;
using System.Collections.Generic;
using FirestormSW.SmartGrade.Database.Model;

namespace FirestormSW.SmartGrade.Api.Responses
{
    public class BasicUserResponse
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public string LoginName { get; set; }
        public string NotificationEmail { get; set; }
        public string PlatformID { get; set; }
        public string PreferredLanguage { get; set; }
        public DateTime? LastLogin { get; set; }
        public GradeLevel GradeType { get; set; }
        public bool IsAdmin { get; set; }
        public bool HasPassword { get; set; }
        public int? ClassId { get; set; }
        public string ClassName { get; set; }
        public ICollection<Group> Classes { get; set; }
    }
}