namespace FirestormSW.SmartGrade.Api.Responses
{
    public class ResponseWithClass<T>
    {
        public int Id { get; set; }
        public T Value { get; set; }
        public int? ClassId { get; set; }
        public string ClassName { get; set; }
    }
}