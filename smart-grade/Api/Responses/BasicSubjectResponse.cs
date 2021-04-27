namespace FirestormSW.SmartGrade.Api.Responses
{
    public class BasicSubjectResponse
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string RegistryName { get; set; }
        public int ClassCount { get; set; }
        public int TeacherCount { get; set; }
        public bool HasMidterm { get; set; }
        public object Teachers { get; set; }
        public object Classes { get; set; }
    }
}