using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace FirestormSW.SmartGrade.Database.Model
{
    public class Group : IEntryWithKey
    {
        [Key]
        public int Id { get; set; }

        [Searchable]
        public string Name { get; set; }

        public GroupType GroupType { get; set; }
        public GradeLevel GradeLevel { get; set; }
        public ICollection<User> Users { get; set; }
        public ICollection<User> Teachers { get; set; }
        public ICollection<Subject> Subjects { get; set; }
        public User FormMaster { get; set; }
    }
}