using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json.Linq;

namespace FirestormSW.SmartGrade.Database.Model
{
    public class RegistryTimeSlot : IEntryWithKey
    {
        [Key]
        public int Id { get; set; }

        public int Duration { get; set; }
        public bool HasClass { get; set; }
        public bool HasSubject { get; set; }
        public bool HasPCO { get; set; }
        public bool HasText { get; set; }
        public string CustomLabel { get; set; }
        public List<string> Presets { get; set; }
    }
}