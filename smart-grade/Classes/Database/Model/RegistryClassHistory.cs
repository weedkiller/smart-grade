using System.ComponentModel.DataAnnotations;

namespace FirestormSW.SmartGrade.Database.Model
{
    public class RegistryClassHistory : IEntryWithKey
    {
        [Key]
        public int Id { get; set; }
        
        public User Teacher { get; set; }
        public Group Class { get; set; }
    }
}