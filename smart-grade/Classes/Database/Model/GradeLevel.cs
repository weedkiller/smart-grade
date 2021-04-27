using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace FirestormSW.SmartGrade.Database.Model
{
    public class GradeLevel : IEntryWithKey
    {
        [Key]
        public int Id { get; set; }

        [Searchable]
        public string Name { get; set; }

        [JsonIgnore]
        public List<Group> Groups { get; set; }

        public RegistryConfiguration RegistryConfiguration { get; set; }
        public bool EmailOnGradeAdded { get; set; }
        public bool EmailOnGradeDeleted { get; set; }
        public bool EmailOnAbsenceAdded { get; set; }
        public bool EmailOnAbsenceDeleted { get; set; }
        public bool EmailOnDisciplinaryAdded { get; set; }
        public bool EmailOnDisciplinaryDeleted { get; set; }
    }
}