using System.ComponentModel.DataAnnotations;

namespace FirestormSW.SmartGrade.Database.Model
{
    public class Disciplinary : StudentDataEntry, IEntryWithKey
    {
        [Key]
        public int Id { get; set; }

        public string Comment { get; set; }
        public int Points { get; set; }
    }
}