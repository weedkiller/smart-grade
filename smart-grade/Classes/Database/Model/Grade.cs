using System.ComponentModel.DataAnnotations;

namespace FirestormSW.SmartGrade.Database.Model
{
    public class Grade : StudentDataEntry, IEntryWithKey
    {
        [Key]
        public int Id { get; set; }
        public int Value { get; set; }
        public bool IsMidterm { get; set; }
    }
}