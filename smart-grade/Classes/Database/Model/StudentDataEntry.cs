using System;

namespace FirestormSW.SmartGrade.Database.Model
{
    public abstract class StudentDataEntry
    {
        public int Semester { get; set; }
        public DateTime Date { get; set; }
        public User Teacher { get; set; }
        public User Student { get; set; }
        public Subject Subject { get; set; }
        public bool Seen { get; set; }
    }
}