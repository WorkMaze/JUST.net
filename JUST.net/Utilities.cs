using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using JUST.net.Selectables;

namespace JUST
{
    public class Utilities
    {
        public static IDictionary<string,string> FlattenJson(string inputJson)
        {
            JToken parent = JToken.Parse(inputJson);

            Dictionary<string, string> result = null;

            result = PopulateRecursively(parent,result);
            
            return result;
        }
        
        private static Dictionary<string, string> PopulateRecursively(JToken parent, Dictionary<string, string> result)
        {
            if (parent.HasValues)
            {
                foreach (JToken child in parent.Children())
                {
                    if (child is JProperty)
                    {
                        JProperty property = child as JProperty;

                        if (result == null)
                            result = new Dictionary<string, string>();

                        if(property.Value.HasValues)
                        {
                            PopulateRecursively(property.Value, result);
                        }                                              
                        else
                            result.Add(property.Path, property.Value.ToString());
                    }
                   
                }
            }

            return result;
        }

        public static JArray GroupArray<T>(JArray array, string groupingPropertyName, string groupedPropertyName, JUSTContext context) where T: ISelectableToken
        {
            Dictionary<string, JArray> groupedPair = null;

            if (array != null)
            {
                foreach (JObject eachObj in array.Children())
                {
                    var selectable = context.Resolve<T>(eachObj);
                    JToken groupToken = selectable.Select(selectable.RootReference + groupingPropertyName);

                    if (groupedPair == null)
                        groupedPair = new Dictionary<string, JArray>();


                    if (groupToken != null)
                    {
                        object valueOfToken = Transformer.GetValue(groupToken);

                        if (groupedPair.ContainsKey(valueOfToken.ToString()))
                        {
                            JArray oldArr = groupedPair[valueOfToken.ToString()];
                            JObject clonedObj = (JObject)eachObj.DeepClone();
                            clonedObj.Remove(groupingPropertyName);

                            oldArr.Add(clonedObj);

                            groupedPair[valueOfToken.ToString()] = oldArr;
                        }
                        else
                        {
                            JObject clonedObj = (JObject)eachObj.DeepClone();

                            JArray newArr = new JArray();
                            clonedObj.Remove(groupingPropertyName);
                            newArr.Add(clonedObj);

                            groupedPair.Add(valueOfToken.ToString(), newArr);
                        }
                    }
                }
            }

            JArray resultObj = null;
            foreach (KeyValuePair<string, JArray> pair in groupedPair)
            {
                if (resultObj == null)
                    resultObj = new JArray();

                JObject groupObj = new JObject();
                groupObj.Add(groupingPropertyName, pair.Key);
                groupObj.Add(groupedPropertyName, pair.Value);

                resultObj.Add(groupObj);

            }

            return resultObj;
        }

        public static JArray GroupArrayMultipleProperties<T>(JArray array, string[] groupingPropertyNames, string groupedPropertyName, JUSTContext context) where T: ISelectableToken
        {
            Dictionary<string, JArray> groupedPair = null;

            if (array != null)
            {
                foreach (JObject eachObj in array.Children())
                {
                    List<JToken> groupTokens = new List<JToken>();

                    foreach (string groupPropertyName in groupingPropertyNames)
                    {
                        var selectable = context.Resolve<T>(eachObj);
                        groupTokens.Add(selectable.Select(selectable.RootReference + groupPropertyName));
                    }

                    if (groupedPair == null)
                        groupedPair = new Dictionary<string, JArray>();


                    if (groupTokens.Count > 0)
                    {
                        string key = string.Empty;

                        foreach (JToken groupToken in groupTokens)
                        {
                            object valueOfToken = Transformer.GetValue(groupToken);
                            if (key == string.Empty)
                                key += valueOfToken;
                            else
                                key += ":" + valueOfToken;
                        }


                        if (groupedPair.ContainsKey(key))
                        {
                            JArray oldArr = groupedPair[key];
                            JObject clonedObj = (JObject)eachObj.DeepClone();

                            foreach (string groupPropertyName in groupingPropertyNames)
                                clonedObj.Remove(groupPropertyName);

                            oldArr.Add(clonedObj);

                            groupedPair[key] = oldArr;
                        }
                        else
                        {
                            JObject clonedObj = (JObject)eachObj.DeepClone();

                            JArray newArr = new JArray();

                            foreach (string groupPropertyName in groupingPropertyNames)
                                clonedObj.Remove(groupPropertyName);
                            newArr.Add(clonedObj);

                            groupedPair.Add(key, newArr);
                        }
                    }
                }
            }

            JArray resultObj = null;
            foreach (KeyValuePair<string, JArray> pair in groupedPair)
            {
                if (resultObj == null)
                    resultObj = new JArray();

                JObject groupObj = new JObject();

                string[] keys = pair.Key.Split(':');

                int i = 0;
                foreach (string groupPropertyName in groupingPropertyNames)
                {
                    groupObj.Add(groupPropertyName, keys[i]);
                    i++;
                }

                groupObj.Add(groupedPropertyName, pair.Value);

                resultObj.Add(groupObj);

            }

            return resultObj;
        }

        public static JToken GetNestedData(object item)
        {
            var result = new JArray();
            if (item is Array)
            {
                foreach (var innerItem in item as Array)
                {
                    result.Add(GetNestedData(innerItem));
                }
            }
            else
            {
                return JToken.FromObject(item);
            }
            return result;
        }
    }
}
