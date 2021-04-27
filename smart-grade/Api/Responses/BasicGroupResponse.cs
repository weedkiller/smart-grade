namespace FirestormSW.SmartGrade.Api.Responses
{
    public class BasicGroupResponse
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int? GradeLevelId { get; set; }
        public string GradeLevelName { get; set; }
        public int? UserCount { get; set; }
        public int? FormMasterId { get; set; }
        public string FormMasterName { get; set; }
    }
}