using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace FirestormSW.SmartGrade.Database.Model
{
    public class User : IEntryWithKey
    {
        [Key]
        public int Id { get; set; }

        [Searchable]
        public string FullName { get; set; }

        [Searchable]
        public string LoginName { get; set; }

        [JsonIgnore]
        public string PasswordHash { get; set; }

        [Searchable]
        public string PlatformId { get; set; }
        public string NotificationEmail { get; set; }

        public DateTime? LastLogin { get; set; }

        [JsonIgnore]
        public ICollection<Group> Groups { get; set; }

        [JsonIgnore]
        public ICollection<Subject> TaughtSubjects { get; set; }
        
        [JsonIgnore]
        public ICollection<Group> TaughtClasses { get; set; }

        public GradeLevel TeacherGradeLevel { get; set; }

        public string PreferredLanguage { get; set; }

        [JsonIgnore]
        public Group CurrentRole { get; set; }

        [NotMapped]
        public bool HasPassword => !string.IsNullOrEmpty(PasswordHash);
    }
}