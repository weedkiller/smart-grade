using System;
using System.Linq;

namespace FirestormSW.SmartGrade.Utils
{
    public static class TypeUtils
    {
        private static Type[] allTypes;

        public static Type GetType(string name)
        {
            allTypes ??= typeof(Program).Assembly.GetTypes();
            return allTypes.FirstOrDefault(t => t.Name == name || t.FullName == name);
        }
    }
}