using System;
using System.Collections.Concurrent;
using System.Reflection;

namespace JUST
{
    public static class JUSTContext
    {
        private static ConcurrentDictionary<string, MethodInfo> _customFunctions = new ConcurrentDictionary<string, MethodInfo>();

        public static void RegisterCustomFunction(string assemblyName, string namespc, string methodName, string methodAlias = null)
        {
            var methodInfo = ReflectionHelper.SearchCustomFunction(assemblyName, namespc, methodName);
            if (methodInfo == null)
            {
                throw new Exception("Unable to find specified method!");
            }

            if (!_customFunctions.TryAdd(methodAlias ?? methodName, methodInfo))
            {
                throw new Exception("Error while registering custom method!");
            }
        }

        internal static MethodInfo GetCustomMethod(string key)
        {
            if (!_customFunctions.TryGetValue(key, out var result))
            {
                throw new Exception($"Custom function {key} is not registered!");
            }
            return result;
        }

        public static void ClearCustomFunctionRegistrations()
        {
            _customFunctions.Clear();
        }

        internal static bool IsRegisteredCustomFunction(string name)
        {
            return _customFunctions.ContainsKey(name);
        }
    }
}
