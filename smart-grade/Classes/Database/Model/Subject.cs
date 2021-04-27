using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace FirestormSW.SmartGrade.Database.Model
{
    public class Subject : IEntryWithKey
    {
        [Key]
        public int Id { get; set; }

        [Searchable]
        public string Name { get; set; }

        [Searchable]
        public string RegistryName { get; set; }

        public bool HasMidterm { get; set; }
        
        public ICollection<Group> Classes { get; set; }
        public ICollection<User> Teachers { get; set; }
    }
}