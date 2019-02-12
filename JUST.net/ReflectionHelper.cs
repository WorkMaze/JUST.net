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

        internal static object caller(Assembly assembly, String myclass, String mymethod, object[] parameters, bool convertParameters = false)
        {
            Type type = assembly?.GetType(myclass) ?? Type.GetType(myclass);
            MethodInfo methodInfo = type.GetTypeInfo().GetMethod(mymethod);
            var instance = !methodInfo.IsStatic ? Activator.CreateInstance(type) : null;

            var parameterInfos = methodInfo.GetParameters();
            if (parameterInfos.Length > parameters.Length)
            {
                parameters = SetOptionalParameters(methodInfo, parameterInfos, parameters);
            }
            else
            {
                var optionParametersNr = parameterInfos.Count(p => p.IsOptional);
                if (optionParametersNr > 0)
                {
                    parameters = InvertParametersOrder(parameters, optionParametersNr);
                    convertParameters = true;
                }
            }

            var typedParameters = new List<object>();
            if (convertParameters)
            {
                for (int i = 0; i < parameterInfos.Length; i++)
                {
                    var pType = parameterInfos[i].ParameterType;
                    typedParameters.Add(GetTypedValue(pType, parameters[i]));
                }
            }
            return methodInfo.Invoke(instance, convertParameters ? typedParameters.ToArray() : parameters);
        }

        private static object[] InvertParametersOrder(object[] parameters, int optionalParametersNr)
        {
            var mandatoryParametersNr = parameters.Length - 1 - optionalParametersNr;
            var result = parameters.Take(mandatoryParametersNr).ToList();
            result.Add(parameters[parameters.Length - 1]);
            result.AddRange(parameters.Skip(mandatoryParametersNr).Take(optionalParametersNr));
            return result.ToArray();
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

        internal static object CallCustomFunction(object[] parameters)
        {
            object[] customParameters = new object[parameters.Length - 3];
            string functionString = string.Empty;
            string dllName = string.Empty;
            int i = 0;
            foreach (object parameter in parameters)
            {
                if (i == 0)
                    dllName = parameter.ToString();
                else if (i == 1)
                    functionString = parameter.ToString();
                else
                if (i != (parameters.Length - 1))
                    customParameters[i - 2] = parameter;

                i++;
            }

            int index = functionString.LastIndexOf(".");

            string className = functionString.Substring(0, index);
            string functionName = functionString.Substring(index + 1, functionString.Length - index - 1);

            className = className + "," + dllName;

            return caller(null, className, functionName, customParameters);
        }

        private static object[] SetOptionalParameters(MethodInfo methodInfo, ParameterInfo[] parameterInfos, object[] parameters)
        {
            var requiredParametersNr = parameterInfos.Count(p => !p.IsOptional);
            var result = new List<object>(parameters.Take(requiredParametersNr));
            for (var i = parameters.Length; i < parameterInfos.Length; i++)
            {
                var parameterInfo = parameterInfos[i];
                if (!parameterInfo.IsOptional || !parameterInfo.HasDefaultValue)
                {
                    throw new Exception($"Trying to invoke {methodInfo.Name} with incorrect number of arguments: actual {parameters.Length - 1}, required {requiredParametersNr}");
                }
                result.Add(parameterInfo.DefaultValue);
            }
            return result.ToArray();
        }

        private static Assembly GetAssembly(bool isAssemblyDefined, string assemblyName, string namespc, string methodName)
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            if (isAssemblyDefined)
            {
                var assemblyFileName = !assemblyName.EndsWith(".dll") ? $"{assemblyName}.dll" : assemblyName;
                var assembly = assemblies.SingleOrDefault(a => a.ManifestModule.Name == assemblyFileName);
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
