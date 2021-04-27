using System.Collections.Generic;
using System.Linq;
using FirestormSW.SmartGrade.Database.Model;

namespace FirestormSW.SmartGrade.Utils
{
    public static class AverageCalculator
    {
        public static float GetAverageForSubject(Subject subject, int semester, IEnumerable<Grade> grades)
        {
            var _grades = grades.Where(g => g.Subject.Id == subject.Id && g.Semester == semester).ToArray();
            var normalGrades = _grades.Where(g => !g.IsMidterm).Select(g => g.Value).ToArray();
            var midtermGrade = _grades.FirstOrDefault(g => g.IsMidterm)?.Value;

            if (normalGrades.Length == 0)
                return midtermGrade ?? 0;

            var average = normalGrades.Length > 0 ? normalGrades.Sum() / (float) normalGrades.Length : 0;
            if (midtermGrade != null)
                return (average * 3 + midtermGrade.Value) / 4f;
            return average;
        }

        public static float GetAverageForAllSubjects(IEnumerable<Grade> grades)
        {
            var averagesPerSubject = grades
                .GroupBy(g => new {Subject = g.Subject, Semester = g.Semester})
                .Select(g => GetAverageForSubject(g.Key.Subject, g.Key.Semester, g))
                .ToArray();
            if (!averagesPerSubject.Any())
                return 0;
            return averagesPerSubject.Average();
        }
    }
}