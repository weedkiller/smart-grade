using System.ComponentModel.DataAnnotations;

namespace FirestormSW.SmartGrade.Database.Model
{
    public class DisciplinaryPreset : IEntryWithKey
    {
        [Key]
        public int Id { get; set; }

        public string Text { get; set; }
        public int Value { get; set; }
    }
}