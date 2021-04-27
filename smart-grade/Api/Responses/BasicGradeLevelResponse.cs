namespace FirestormSW.SmartGrade.Api.Responses
{
    public class BasicGradeLevelResponse
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int? GroupCount { get; set; }
        public bool GradeAdded { get; set; }
        public bool GradeDeleted { get; set; }
        public bool AbsenceAdded { get; set; }
        public bool AbsenceDeleted { get; set; }
        public bool DisciplinaryAdded { get; set; }
        public bool DisciplinaryDeleted { get; set; }
    }
}