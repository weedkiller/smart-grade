using System.ComponentModel.DataAnnotations;

namespace FirestormSW.SmartGrade.Database.Model
{
    public class Absence : StudentDataEntry, IEntryWithKey
    {
        [Key]
        public int Id { get; set; }

        public string Comment { get; set; }
        public bool Verified { get; set; }
    }
}