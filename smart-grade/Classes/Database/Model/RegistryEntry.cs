using System;
using System.ComponentModel.DataAnnotations;

namespace FirestormSW.SmartGrade.Database.Model
{
    public class RegistryEntry : IEntryWithKey
    {
        [Key]
        public int Id { get; set; }
        
        public DateTime Date { get; set; }
        public User Teacher { get; set; }
        public Group Class { get; set; }
        public Subject Subject { get; set; }
        public string Text { get; set; }
        public bool IsPco { get; set; }
        public DateTime EntryDate { get; set; }
        public DateTime ModifyDate { get; set; }
        public bool IsLocked { get; set; }
    }
}