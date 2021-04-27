using System.ComponentModel.DataAnnotations;

namespace FirestormSW.SmartGrade.Database.Model
{
    public class Property : IEntryWithKey
    {
        [Key]
        public int Id { get; set; }
        public string Key { get; set; }
        public string Value { get; set; }
    }
}