using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace FirestormSW.SmartGrade.Database.Model
{
    public class RegistryConfiguration : IEntryWithKey
    {
        [Key]
        public int Id { get; set; }

        public TimeSpan StartTime { get; set; }
        public List<RegistryTimeSlot> Slots { get; set; }
        public string GroupName { get; set; }
        public string ActivityName { get; set; }
        public bool EnableGradesPage { get; set; }
        public bool EnableAbsencesPage { get; set; }
        public bool EnableDisciplinaryPage { get; set; }
        public bool EnableNotesPage { get; set; }
        public bool EnableRegistryPage { get; set; }
    }
}