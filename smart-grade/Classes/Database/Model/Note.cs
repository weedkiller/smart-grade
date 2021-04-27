using System.ComponentModel.DataAnnotations;

namespace FirestormSW.SmartGrade.Database.Model
{
    public class Note : IEntryWithKey
    {
        [Key]
        public int Id { get; set; }

        public string Text { get; set; }
        public User Teacher { get; set; }
        public User Student { get; set; }
        public Subject Subject { get; set; }
    }
}