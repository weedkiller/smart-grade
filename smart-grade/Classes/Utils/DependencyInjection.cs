using System;

namespace FirestormSW.SmartGrade.Utils
{
    public static class DependencyInjection
    {
        public static object Activate(Type type, IServiceProvider serviceProvider)
        {
            var constructors = type.GetConstructors();
            foreach (var constructorInfo in constructors)
            {
                var parameterInfos = constructorInfo.GetParameters();
                var parameters = new object[parameterInfos.Length];
                for (var i = 0; i < parameterInfos.Length; i++)
                    parameters[i] = serviceProvider.GetService(parameterInfos[i].ParameterType);

                return Activator.CreateInstance(type, parameters);
            }

            return null;
        }
    }
}