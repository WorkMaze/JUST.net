using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace JUST
{
    internal class ReflectionHelper
    {
        internal const string EXTERNAL_ASSEMBLY_REGEX = "([\\w.]+)[:]{2}([\\w.]+)[:]{0,2}([\\w.]*)";

        internal static object caller(Assembly assembly, string myclass, string mymethod, object[] parameters, bool convertParameters = false)
        {
            Type type = assembly?.GetType(myclass) ?? Type.GetType(myclass);
            MethodInfo methodInfo = type.GetTypeInfo().GetMethod(mymethod);
            var instance = !methodInfo.IsStatic ? Activator.CreateInstance(type) : null;

            return InvokeCustomMethod(methodInfo, parameters, convertParameters);
        }

        internal static object InvokeCustomMethod(MethodInfo methodInfo, object[] parameters, bool convertParameters = false)
        {
            var instance = !methodInfo.IsStatic ? Activator.CreateInstance(methodInfo.DeclaringType) : null;

            var typedParameters = new List<object>();
            if (convertParameters)
            {
                var parameterInfos = methodInfo.GetParameters();
                for (int i = 0; i < parameterInfos.Length; i++)
                {
                    var pType = parameterInfos[i].ParameterType;
                    typedParameters.Add(GetTypedValue(pType, parameters[i]));
                }
            }
            return methodInfo.Invoke(instance, convertParameters ? typedParameters.ToArray() : parameters);
        }

        internal static object CallExternalAssembly(string functionName, object[] parameters)
        {
            var match = Regex.Match(functionName, EXTERNAL_ASSEMBLY_REGEX);
            var isAssemblyDefined = match.Groups.Count == 4 && match.Groups[3].Value != string.Empty;
            var assemblyName = isAssemblyDefined ? match.Groups[1].Value : null;
            var namespc = match.Groups[isAssemblyDefined ? 2 : 1].Value;
            var methodName = match.Groups[isAssemblyDefined ? 3 : 2].Value;

            var assembly = GetAssembly(isAssemblyDefined, assemblyName, namespc, methodName);
            if (assembly != null)
            {
                return caller(assembly, namespc, methodName, FilterParameters(parameters), true);
            }

            throw new MissingMethodException((assemblyName != null ? $"{assemblyName}." : string.Empty) + $"{namespc}.{methodName}");
        }

        internal static MethodInfo SearchCustomFunction(string assemblyName, string namespc, string methodName)
        {
            var assembly = GetAssembly(assemblyName != null, assemblyName, namespc, methodName);
            Type type = assembly?.GetType(namespc) ?? Type.GetType(namespc);
            return type?.GetTypeInfo().GetMethod(methodName);
        }

        private static Assembly GetAssembly(bool isAssemblyDefined, string assemblyName, string namespc, string methodName)
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            if (isAssemblyDefined)
            {
                var assemblyFileName = !assemblyName.EndsWith(".dll") ? $"{assemblyName}.dll" : assemblyName;

                //SingleOrDefault fails, dll registrated twice????
                //Possible alternative to AppDomain: https://github.com/dotnet/coreclr/issues/14680
                var assembly = assemblies.FirstOrDefault(a => a.ManifestModule.Name == assemblyFileName);
                if (assembly == null)
                {
                    var assemblyLocation = Path.Combine(Directory.GetCurrentDirectory(), assemblyFileName);
                    assembly = Assembly.LoadFile(assemblyLocation);
                    AppDomain.CurrentDomain.Load(assembly.GetName());
                }

                return assembly;
            }
            else
            {
                foreach (var assembly in assemblies.Where(a => !a.FullName.StartsWith("System.")))
                {
                    Type[] types = null;
                    try
                    {
                        types = assembly.GetTypes();
                    }
                    catch (ReflectionTypeLoadException ex)
                    {
                        types = ex.Types;
                    }

                    foreach (var typeInfo in types)
                    {
                        if (string.Compare(typeInfo.FullName, namespc, StringComparison.InvariantCultureIgnoreCase) == 0)
                        {
                            return assembly;
                        }
                    }
                }
            }
            return null;
        }

        private static object[] FilterParameters(object[] parameters)
        {
            if (string.IsNullOrEmpty(parameters[0]?.ToString() ?? string.Empty))
            {
                parameters = parameters.Skip(1).ToArray();
            }
            if (parameters.Length > 0 && parameters.Last().ToString() == "{}")
            {
                parameters = parameters.Take(parameters.Length - 1).ToArray();
            }
            return parameters;
        }

        private static object GetTypedValue(Type pType, object val)
        {
            object typedValue = val;
            var converter = TypeDescriptor.GetConverter(pType);
            if (converter.CanConvertFrom(val.GetType()))
            {
                typedValue = converter.ConvertFrom(val);
            }
            else if (pType.IsPrimitive)
            {
                typedValue = Convert.ChangeType(val, pType);
            }
            else if (!pType.IsAbstract)
            {
                var parse = pType.GetMethod("Parse", BindingFlags.Static | BindingFlags.Public, null, new[] { typeof(string) }, null);
                typedValue = parse?.Invoke(null, new[] { val }) ?? pType.GetConstructor(new[] { typeof(string) })?.Invoke(new[] { val }) ?? val;
            }
            return typedValue;
        }
    }
}
