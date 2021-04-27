using FirestormSW.SmartGrade.Database.Model;

namespace FirestormSW.SmartGrade.Api.Teacher
{
    public interface ISummaryProvider
    {
        string GetSummary(User user, Subject subject);
    }
}