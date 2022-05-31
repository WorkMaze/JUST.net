[![Build Status](https://courela.visualstudio.com/JUST.net/_apis/build/status/Truphone.JUST.net?branchName=develop)](https://courela.visualstudio.com/JUST.net/_build/latest?definitionId=1&branchName=develop)
[![Downloads](https://img.shields.io/nuget/dt/JUST.net.svg)](https://img.shields.io/nuget/dt/JUST.net.svg)
# JUST

JUST Stands for **JSON Under Simple Transformation**

XSLT is a very popular way of transforming XML documents using a simple transformation language. More and more applications are now using JSON as a data format because it is much simpler and less bulkier than XML.
However, there isn't a very easy way to transforming JSON documents.

I have created a library in .NET which enables the transformation of JSON documents using very a simple transformation language. This is an attempt to create an XSLT parallel for JSON.
The library is available as a NUGET package.

This C# project has working examples of the transformations.

**Types are now supported**. [New functions](#typeconversions) were added to provide type conversions.
Also a new enum field called `EvaluationMode` was added to `JUSTContext`, which lets you select how type mismatches are handled:
- option `Strict` mode will throw an exception on error;
- option `FallbackToDefault` will return the default value for the return type of the function/expression being evaluated

There's also an option to tell how #copy will behave:
- option `AddOrReplaceProperties` will add or replace any property that may be present both in #copy and transformer.

**New query languages accepted** besides [JsonPath](https://goessner.net/articles/JsonPath/). All you have to do is create a class that implements `ISelectableToken` and call generic `Transform` method with your type.
[JmesPath](http://jmespath.org/) is included as an alternative ([example here](#jmesexample)).

```C#
string transformedString = new JsonTransformer<JmesPathSelectable>.Transform(transformer, input);
```


# <a name="install"></a> JUST.NET Library

Pull the latest JUST.NET from https://www.nuget.org
``Install-Package JUST.NET``

It's a .Net Standard library, so it can be used with .Net Framework and .Net Core.

Older versions not updated anymore:
- .Net Core version ``Install-Package JUST.NETCore``
- .Net Framework version ``Install-Package JUST``


# <a name="example"></a> Write a simple C# code snippet to transform your JSON

This is demonstrated with various examples in the source code. Once you install the nuget to your project you need to import the following namespace:

```C#
using JUST;
```

Below is a simple C# code snippet that you can use to transform your JSON:

```C#
//read input from JSON file
string input = File.ReadAllText("Examples/Input.json"); 

//read the transformer from a JSON file
string transformer = File.ReadAllText("Examples/Transformer.json");

// do the actual transformation [equal to new JsonTransformer<JsonPathSelectable>(...) for backward compatibility]
string transformedString = new JsonTransformer().Transform(transformer, input);

// with context
JUSTContext context = new JUSTContext 
{ 
  EvaluationMode = EvaluationMode.Strict,
  DefaultDecimalPlaces = 4
};
string transformedString = new JsonTransformer(context).Transform(transformer, input);

// with generic method
string transformedString = new JsonTransformer<JmesPathSelectable>.Transform(transformer, input);
```

# <a name="using"></a> Using JUST to transform JSON

JUST is a transformation language just like XSLT. It includes functions which are used inside the transformer JSON to transform the input JSON in a desired output JSON. This section describes the various functions present in JUST and how they can be used to transform your JSON.

Every JUST function starts with "#" character and must start at the begininng of the expression or at the beginning of an argument.

Example:

```JSON
{
  "valid_function": "#valueof(#xconcat($.prop[,other_prop_or_filter,]))",
  "not_a_function": "some_sentence_at_the_beginning_invalidates_evaluation_#xconcat(sencence,_argument_not_evaluated_#valueof($.prop)_does_not_start_with_#)"
}
```

## <a name="valueof"></a> valueof

This function is used to extract the value of a given property. The value is extracted using JSON path of the property. For more information on how to use JSON path refer to : 
http://www.newtonsoft.com/json/help/html/QueryJsonSelectTokenJsonPath.htm

Consider the input:

```JSON
{
  "menu": {
    "popup": {
      "menuitem": [{
          "value": "Open",
          "onclick": "OpenDoc()"
        }, {
          "value": "Close",
          "onclick": "CloseDoc()"
        }
      ],
	  "submenuitem": "CloseSession()"
    }
  }
}
```

Transformer:

```JSON
{
  "result": {
    "Open": "#valueof($.menu.popup.menuitem[?(@.value=='Open')].onclick)",
    "Close": "#valueof($.menu.popup.menuitem[?(@.value=='Close')].onclick)"
  }
}
```

Output:

```JSON
{
  "result": {
    "Open": "OpenDoc()",
	"Close": "CloseDoc()"
  }
}
```

#### <a name="jmesexample"></a> ...with JmesPath 
Note: default is JsonPath

Input:

```JSON
{
  "locations": [
    { "name": "Seattle", "state": "WA" },
    { "name": "New York", "state": "NY" },
    { "name": "Bellevue", "state": "WA" },
    { "name": "Olympia", "state": "WA" }
  ]
}
```

Transformer:

```JSON
{
  "result": "#valueof(locations[?state == 'WA'].name | sort(@) | {WashingtonCities: join(', ', @)})" 
}
```

Output:

```JSON
{
  "result": {
    "WashingtonCities": "Bellevue, Olympia, Seattle"
  }
}
```

## <a name="ifcondition"></a> ifcondition

This condition is used to evaluate and if-else condition.

ifcondition(condition expression, evaluation expression, true result, false result).

All four of the parameters can be JUST functions or constants. It will perform lazy evaluation, which means
that only the corresponding true or false result is evaluated according with condition/evaluation result 
(as programming languages do). 

Consider the input:

```JSON
{
  "menu": {
    "id" : "github",
    "repository" : "JUST"
  } 
}
```

Transformer:

```JSON
{
  "ifconditiontesttrue": "#ifcondition(#valueof($.menu.id),github,#valueof($.menu.repository),fail)",
  "ifconditiontestfalse": "#ifcondition(#valueof($.menu.id),xml,#valueof($.menu.repository),fail)"
}
```

Output:

```JSON
{
  "ifconditiontesttrue": "JUST",
  "ifconditiontestfalse": "fail"
}
```

## <a name="stringmath"></a> string and math functions

At the moment only the basic and often used string and math functions are provided in the library.

1. lastindexof(input string,search string)
2. firstindexof(input string,search string)
3. substring(input string,start indes,length)
4. concat(string 1,string 2)
5. length(string or array)
6. add(value 1,value 2)
7. subtract(value 1,value 2)
8. multiply(value 1,value 2)
9. divide(value 1,values 2)
10. round(value, decimal places)

Consider the input:

```JSON
{
  "stringref": "thisisandveryunuasualandlongstring",
  "numbers": [ 1, 2, 3, 4, 5 ]
}
```

Transformer:

```JSON
{
  "stringresult": {
    "lastindexofand": "#lastindexof(#valueof($.stringref),and)",
    "firstindexofand": "#firstindexof(#valueof($.stringref),and)",
    "substring": "#substring(#valueof($.stringref),9,11)",
    "concat": "#concat(#valueof($.menu.id.file),#valueof($.menu.value.Window))",
	"length_string": "#length(#valueof($.stringref))",
	"length_array": "#length(#valueof($.numbers))",
	"length_path": "#length($.numbers)"
  },
  "mathresult": {
    "add": "#add(#valueof($.numbers[0]),3)",
    "subtract": "#subtract(#valueof($.numbers[4]),#valueof($.numbers[0]))",
    "multiply": "#multiply(2,#valueof($.numbers[2]))",
    "divide": "#divide(9,3)",
	"round": "#round(10.005,2)"
  }
}
```

Output:

```JSON
{
  "stringresult": { 
    "lastindexofand": 21,
    "firstindexofand": 6,
    "substring": "veryunuasua",
    "concat":"",
	"length_string": 34,
	"length_array": 5,
	"length_path": 5
  },
  "mathresult": {
    "add": 4,
    "subtract": 4,
    "multiply": 6,
    "divide": 3,
	"round": 10.01
  }
}
```

## <a name="operators"></a> Operators

The following operators have been added to compare strings and numbers :

1. stringequals(string1, string2)
2. stringcontains(string1, string2)
3. mathequals(decimal1, decimal2)
4. mathgreaterthan(decimal1, decimal2)
5. mathlessthan(decimal1, decimal2)
6. mathgreaterthanorequalto(decimal1, decimal2)
7. mathlessthanorequalto(decimal1, decimal2)

Consider the input:
 
```JSON
{
  "d": [ "one", "two", "three" ],
  "numbers": [ 1, 2, 3, 4, 5 ]
}
```

Transformer:

```JSON
{
  "mathresult": {
    "third_element_equals_3": "#ifcondition(#mathequals(#valueof($.numbers[2]),3),True,yes,no)",
    "third_element_greaterthan_2": "#ifcondition(#mathgreaterthan(#valueof($.numbers[2]),2),True,yes,no)",
    "third_element_lessthan_4": "#ifcondition(#mathlessthan(#valueof($.numbers[2]),4),True,yes,no)",
    "third_element_greaterthanorequals_4": "#ifcondition(#mathgreaterthanorequalto(#valueof($.numbers[2]),4),True,yes,no)",
    "third_element_lessthanoreuals_2": "#ifcondition(#mathlessthanorequalto(#valueof($.numbers[2]),2),True,yes,no)",
    "one_stringequals": "#ifcondition(#stringequals(#valueof($.d[0]),one),True,yes,no)",
    "one_stringcontains": "#ifcondition(#stringcontains(#valueof($.d[0]),n),True,yes,no)"
  }
}
```

Output:

```JSON
{
  "mathresult": {
    "third_element_equals_3": "yes",
    "third_element_greaterthan_2": "yes",
    "third_element_lessthan_4": "yes",
    "third_element_greaterthanorequals_4": "no",
    "third_element_lessthanoreuals_2": "no",
    "one_stringequals": "yes",
    "one_stringcontains": "yes"
  }
}
```

## <a name="aggregate"></a> Aggregate functions

The following aggregate functions are provided for single dimensional arrays:

1. concatall(path or array)
2. sum(path or array)
3. average(path or array)
4. min(path or array)
5. max(path or array)

Consider the input:
 
```JSON
{
  "d": [ "one", "two", "three" ],
  "numbers": [ 1, 2, 3, 4, 5 ]
}
```

Transformer:

```JSON
{
  "conacted": "#concatall(#valueof($.d))",
  "sum": "#sum($.numbers)",
  "avg": "#average(#valueof($.numbers))",
  "min": "#min($.numbers)",
  "max": "#max(#valueof($.numbers))"
}
```

Output:

```JSON
{
  "conacted": "onetwothree",
  "sum": 15,
  "avg": 3,
  "min": 1,
  "max": 5
}
```

## <a name="multiarrays"></a> Aggregate functions for multidimensional arrays:

These functions are essentially the same as the above ones, the only difference being that you can also provide a path to point to particular element inside the array.
1. concatallatpath(array,path)
2. sumatpath(array,path)
3. averageatpath(array,path)
4. minatpath(array,path)
5. maxatpath(array,path)

Consider the input:

```JSON
{
  "x": [{
      "v": {
        "a": "a1,a2,a3",
        "b": 1,
        "c": 10
      }
    }, {
      "v": {
        "a": "b1,b2",
        "b": 2,
        "c": 20
      }
    }, {
      "v": {
        "a": "c1,c2,c3",
        "b": 3,
        "c": 30
      }
    }
  ]
}
```

Transformer:

```JSON
{
  "arrayconacted": "#concatallatpath(#valueof($.x),$.v.a)",
  "arraysum": "#sumatpath(#valueof($.x),$.v.c)",
  "arrayavg": "#averageatpath(#valueof($.x),$.v.c)",
  "arraymin": "#minatpath(#valueof($.x),$.v.b)",
  "arraymax": "#maxatpath(#valueof($.x),$.v.b)"
}
```

Output:

```JSON
{
  "arrayconacted": "a1,a2,a3b1,b2c1,c2,c3",
  "arraysum": 60,
  "arrayavg": 20,
  "arraymin": 1,
  "arraymax": 3
}
```


## <a name="typeconversions"></a> Type conversions

As type handling was introduced, functions to make type convertions are handy. 
The following functions are available:

1. tointeger
2. tostring
3. toboolean
4. todecimal

*Note*: some convertions will make use of application's CultureInfo to define output formats 
(ex: comma or dot for decimal separator when converting to string)!

Consider the input:

```JSON
{
  "booleans": {
    "affirmative_string": "true",
    "negative_string": "false",
    "affirmative_int": 123,
    "negative_int": 0,
  },
  "strings": {
    "integer": 123,
    "decimal": 12.34,
    "affirmative_boolean": true,
    "negative_boolean": false
  },
  "integers": {
    "string": "123",
    "decimal": 1.23,
    "affirmative_boolean": true,
    "negative_boolean": false
  },
  "decimals": {
    "integer": 123,
    "string": "1.23"
  }
}
```

Transformer:
```JSON
{
  "booleans": {
    "affirmative_string": "#toboolean(#valueof($.booleans.affirmative_string))",
    "negative_string": "#toboolean(#valueof($.booleans.negative_string))",
    "affirmative_int": "#toboolean(#valueof($.booleans.affirmative_int))",
    "negative_int": "#toboolean(#valueof($.booleans.negative_int))",
  },
  "strings": {
    "integer": "#tostring(#valueof($.strings.integer))",
    "decimal": "#tostring(#valueof($.strings.decimal))",
    "affirmative_boolean": "#tostring(#valueof($.strings.affirmative_boolean))",
    "negative_boolean": "#tostring(#valueof($.strings.negative_boolean))"
  },
  "integers": {
    "string": "#tointeger(#valueof($.integers.string))",
    "decimal": "#tointeger(#valueof($.integers.decimal))",
    "affirmative_boolean": "#tointeger(#valueof($.integers.affirmative_boolean))",
    "negative_boolean": "#tointeger(#valueof($.integers.negative_boolean))"
  },
  "decimals": {
    "integer": "#todecimal(#valueof($.decimals.integer))",
    "string": "#todecimal(#valueof($.decimals.string))"
  }
}
```

Output:
```JSON
{
  "booleans": {
    "affirmative_string": true,
    "negative_string": false,
    "affirmative_int": true,
    "negative_int": false
  },
  "strings": {
    "integer": "123",
    "decimal": "12,34",
    "affirmative_boolean": "True",
    "negative_boolean": "False"
  },
  "integers": {
    "string": 123,
    "decimal": 1,
    "affirmative_boolean": 1,
    "negative_boolean": 0
  },
  "decimals": {
    "integer": 123.0,
    "string": 1.23
  }
}
```

## <a name="typecheck"></a> Type check

Functions to check the type of a value:

1. isnumber
2. isboolean
3. isstring
4. isarray

Consider the input:

```JSON
{
  "integer": 0,
  "decimal": 1.23,
  "boolean": true,
  "string": "abc",
  "array": [ "abc", "xyz" ]
}
```

Transformer:
```JSON
{
  "isNumberTrue1": "#isnumber(#valueof($.integer))",
  "isNumberTrue2": "#isnumber(#valueof($.decimal))",
  "isNumberFalse": "#isnumber(#valueof($.boolean))",
  "isBooleanTrue": "#isboolean(#valueof($.boolean))",
  "isBooleanFalse": "#isboolean(#valueof($.integer))",
  "isStringTrue": "#isstring(#valueof($.string))",
  "isStringFalse": "#isstring(#valueof($.array))",
  "isArrayTrue": "#isarray(#valueof($.array))",
  "isArrayFalse": "#isarray(#valueof($.decimal))"
}
```

Output:
```JSON
{
  "isNumberTrue1": true,
  "isNumberTrue2": true,
  "isNumberFalse": false,
  "isBooleanTrue": true,
  "isBooleanFalse": false,
  "isStringTrue": true,
  "isStringFalse": false,
  "isArrayTrue": true,
  "isArrayFalse": false
}
```


## <a name="bulk"></a> Bulk functions

All the above functions set property values to predefined properties in the output JSON. However, in some cases we don't know what our output will look like as it depends on the input.
Bulk functions are provided for this purpose. They correspond with the template-match functions in XSLT.

Bulk functions by law have to be the first property of the JSON object. All bulk functions are represented as array elements of the property '#'.

These are the bulk functions provided as of now:

1. copy(path)
2. replace(path)
3. delete(path)

Cosider the input:

```JSON
{
  "tree": {
    "branch": {
      "leaf": "green",
      "flower": "red",
      "bird": "crow",
      "extra": { "twig":"birdnest" }
    },
    "ladder": {"wood": "treehouse" }
  }
}
```

Transformer:

```JSON
{
  "#": ["#copy($)", "#delete($.tree.branch.bird)", "#replace($.tree.branch.extra,#valueof($.tree.ladder))"],
  "othervalue": "othervalue"
}
```

Output:

```JSON
{
  "othervalue": "othervalue",
  "tree": {
    "branch": {
      "leaf": "green",
      "flower": "red",
      "extra": {
        "wood": "treehouse"
      }
    },
    "ladder": {
      "wood": "treehouse"
    }
  }
}
```

## <a name="arrays"></a> Array looping

In some cases we don't want to copy the entire array to the destination JSON. We might want to transform the array into a different format, or have some special logic for each element while setting the destination JSON. 
Also we might want to traverse all properties of an object, just like in JavaScript, and perform some tranformation over values. When applying JsonPath over looped properties beware that each property/value will be considered an object (note the two dots in the example below \[$..sounds\]).
For these cases we would use array looping.

These are the functions provided for this pupose:

1. loop(path) - path is the path of the array to loop
2. currentvalue()
3. currentindex()
4. currentproperty() - when looping over properties of an object
5. lastindex()
6. lastvalue()
7. currentvalueatpath(path) - here path denotes the path inside the array
8. lastvalueatpath(path) - here path denotes the path inside the array

Cosider the input:

```JSON
{
  "tree": {
    "branch": {
      "leaf": "green",
      "flower": "red",
      "bird": "crow",
      "extra": {
        "twig": "birdnest"
      }
    },
    "ladder": {
      "wood": "treehouse"
    }
  },
  "numbers": [1, 2, 3, 4],
  "arrayobjects": [{
      "country": {
        "name": "norway",
        "language": "norsk"
      }
    }, {
      "country": {
        "name": "UK",
        "language": "english"
      }
    }, {
      "country": {
        "name": "Sweden",
        "language": "swedish"
      }
    }
  ],
  "animals": {
    "cat": {
      "number_of_legs": 4,
      "sound": "meow"
    },
    "dog": {
      "number_of_legs": 4,
      "sound": "woof"
    },
    "human": {
      "number_of_legs": 2,
      "sound": "@!#$?"
    }
  },
  "spell_numbers": {
    "3": "three",
    "2": "two",
    "1": "one"
  }
}
```

Transformer:

```JSON
{
  "iteration": {
    "#loop($.numbers)": {
      "CurrentValue": "#currentvalue()",
      "CurrentIndex": "#currentindex()",
      "IsLast": "#ifcondition(#currentindex(),#lastindex(),yes,no)",
      "LastValue": "#lastvalue()"
    }
  },
  "iteration2": {
    "#loop($.arrayobjects)": {
      "CurrentValue": "#currentvalueatpath($.country.name)",
      "CurrentIndex": "#currentindex()",
      "IsLast": "#ifcondition(#currentindex(),#lastindex(),yes,no)",
      "LastValue": "#lastvalueatpath($.country.language)"
    }
  },
  "sounds": { 
	"#loop($.animals)": { 
		"#eval(#currentproperty())": "#currentvalueatpath($..sound)" 
	} 
  },
  "number_index": { 
    "#loop($.spell_numbers)": { 
	  "#eval(#currentindex())": "#currentvalueatpath(#concat($.,#currentproperty()))" 
	} 
  },
  "othervalue": "othervalue"
}
```

Output:

```JSON
{
  "iteration": [{
      "CurrentValue": 1,
      "CurrentIndex": 0,
      "IsLast": "no",
      "LastValue": 4
    }, {
      "CurrentValue": 2,
      "CurrentIndex": 1,
      "IsLast": "no",
      "LastValue": 4
    }, {
      "CurrentValue": 3,
      "CurrentIndex": 2,
      "IsLast": "no",
      "LastValue": 4
    }, {
      "CurrentValue": 4,
      "CurrentIndex": 3,
      "IsLast": "yes",
      "LastValue": 4
    }
  ],
  "iteration2": [{
      "CurrentValue": "norway",
      "CurrentIndex": 0,
      "IsLast": "no",
      "LastValue": "swedish"
    }, {
      "CurrentValue": "UK",
      "CurrentIndex": 1,
      "IsLast": "no",
      "LastValue": "swedish"
    }, {
      "CurrentValue": "Sweden",
      "CurrentIndex": 2,
      "IsLast": "yes",
      "LastValue": "swedish"
    }
  ],
  "sounds": {
    "cat": "meow",
    "dog": "woof",
    "human": "@!#$?"
  },
  "number_index": {
    "0": "three",
    "1": "two",
    "2": "one"
  },
  "othervalue": "othervalue"
}
```

## <a name="arraysconcat"></a> Array concatenation
When a concatenation is needed, one can use #concat or #xconcat to join two arrays
1. concat(object1, object2)
2. xconcat(object1,object2......objectx)

Input:

```JSON
{
  "drugs": [{ 
      "code": "001", "display": "Drug1" 
    },{
      "code": "002", "display": "Drug2" 
  }],
  "pa": [{ 
      "code": "pa1", "display": "PA1" 
	},{
      "code": "pa2", "display": "PA2" 
  }],
  "sa": [{ 
      "code": "sa1", "display": "SA1" 
	},{
      "code": "sa2", "display": "SA2" 
  }]
}
```

Transformer:

```JSON
{
  "concat": "#concat(#valueof($.drugs), #valueof($.pa))", 
  "multipleConcat": "#concat(#concat(#valueof($.drugs), #valueof($.pa)), #valueof($.sa))\",
  "xconcat": "#xconcat(#valueof($.drugs), #valueof($.pa), #valueof($.sa))" 
}
```

Output:

```JSON
{
  "concat": [{
      "code": "001", "display": "Drug1" 
	},{
      "code": "002", "display": "Drug2" 
    },{
      "code": "pa1", "display": "PA1" 
	},{ 
      "code": "pa2", "display": "PA2" 
  }],
  "multipleConcat": [{ 
      "code": "001", "display": "Drug1" 
    },{ 
      "code": "002", "display": "Drug2"
    },{ 
      "code": "pa1", "display": "PA1" 
    },{ 
      "code": "pa2", "display": "PA2" 
    },{ 
      "code": "sa1", "display": "SA1"
    },{ 
	  "code": "sa2", "display": "SA2"
	}],
  "xconcat": [{ 
      "code": "001", "display": "Drug1" 
    },{
      "code": "002", "display": "Drug2"
    },{ 
      "code": "pa1", "display": "PA1"
    },{ 
      "code": "pa2", "display": "PA2" 
    },{ 
      "code": "sa1", "display": "SA1"
    },{
      "code": "sa2", "display":"SA2" 
    }]
}
```

## <a name="nestedarrays"></a> Nested array looping
It is possible to loop over more than one array at once. By default, the last array is used, but one can use properties from other arrays by using alias for array looping.
There's a special alias 'root', that refers to the whole input.
One side note: loops must be last property, any properties after that will be ignored.

Cosider the input:
```JSON
{
  "NestedLoop": {
    "Organization": {
      "Employee": [{
          "Name": "E2",
		  "Surname": "S2",
          "Details": [{
              "Countries": [{
			      "Name": "Iceland",
                  "Language": "Icelandic"
                }
			  ],
              "Age": "30"
            }
          ]
        }, {
          "Name": "E1",
		  "Surname": "S1",
          "Details": [{
              "Countries": [{
                  "Name": "Denmark",
                  "Language": "Danish"
                },{
                  "Name": "Greenland",
                  "Language": "Danish"
                }
              ],
              "Age": "31"
            }
          ]
        }
      ]
    }
  }
}
```

Transformer:

```JSON
{
  "hello": {
    "#loop($.NestedLoop.Organization.Employee, employees)": {
      "CurrentName": "#currentvalueatpath($.Name, employees)",
      "Details": {
        "#loop($.Details)": {
          "Surname": "#currentvalueatpath($.Surname, employees)",
		  "Age": "#currentvalueatpath($.Age)",
          "Country": {
            "#loop($.Countries[0], countries)": "#currentvalueatpath($.Name, countries)"
          }
        }
      }
    }
  }
}
```

Output:

```JSON
{
  "hello":
  [{
      "CurrentName": "E2",
      "Details": [{
          "Surname": "S2",
		  "Age": 30,
		  "Country": [ "Iceland" ]
        }
      ]
    }, {
      "CurrentName": "E1",
      "Details": [{
          "Surname": "S1",
          "Age": 31,
          "Country": [ "Denmark" ]
        }
      ]
    }
  ]
}
```

## <a name="arraysgrouping"></a> Array grouping

A function similar to SQL GROUP BY clause has been introduced to group an array based on the value of an element.

grouparrayby(path,groupingElementName,groupedElementName)

Input:

```JSON
{
  "Forest": [{
      "type": "Mammal",
      "qty": 1,
      "name": "Hippo"
    }, {
      "type": "Bird",
      "qty": 2,
      "name": "Sparrow"
    }, {
      "type": "Amphibian",
      "qty": 300,
      "name": "Lizard"
    }, {
      "type": "Bird",
      "qty": 3,
      "name": "Parrot"
    }, {
      "type": "Mammal",
      "qty": 1,
      "name": "Elephant"
    }, {
      "type": "Mammal",
      "qty": 10,
      "name": "Dog"
    }
  ]
}
```

Transformer:

```JSON
{
  "Result": "#grouparrayby($.Forest,type,all)" 
}
```

Output:

```JSON
{
  "Result": [{
      "type": "Mammal",
      "all": [{
          "qty": 1,
          "name": "Hippo"
        }, {
          "qty": 1,
          "name": "Elephant"
        }, {
          "qty": 10,
          "name": "Dog"
        }
      ]
    }, {
      "type": "Bird",
      "all": [{
          "qty": 2,
          "name": "Sparrow"
        }, {
          "qty": 3,
          "name": "Parrot"
        }
      ]
    }, {
      "type": "Amphibian",
      "all": [{
          "qty": 300,
          "name": "Lizard"
        }
      ]
    }
  ]
}
```

You can group using multiple "grouping elements". They should be seperated by a semicolon (:)

Input:

```JSON
{
  "Vehicle": [{
      "type": "air",
      "company": "Boeing",
      "name": "airplane"
    }, {
      "type": "air",
      "company": "Concorde",
      "name": "airplane"
    }, {
      "type": "air",
      "company": "Boeing",
      "name": "Chopper"
    }, {
      "type": "land",
      "company": "GM",
      "name": "car"
    }, {
      "type": "sea",
      "company": "Viking",
      "name": "ship"
    }, {
      "type": "land",
      "company": "GM",
      "name": "truck"
    }
  ]
}
```

Transformer:

```JSON
{
  "Result": "#grouparrayby($.Vehicle,type:company,all)"
}
```

Output:

```JSON
{
  "Result": [{
      "type": "air",
      "company": "Boeing",
      "all": [{
          "name": "airplane"
        }, {
          "name": "Chopper"
        }
      ]
    }, {
      "type": "air",
      "company": "Concorde",
      "all": [{
          "name": "airplane"
        }
      ]
    }, {
      "type": "land",
      "company": "GM",
      "all": [{
          "name": "car"
        }, {
          "name": "truck"
        }
      ]
    }, {
      "type": "sea",
      "company": "Viking",
      "all": [{
          "name": "ship"
        }
      ]
    }
  ]
}
```

## <a name="customfunc"></a> Calling Custom functions

You can make your own custom functions in C# and call them from your transformer JSON.
A custom function has to reside inside a public class and has to be a public static method.

A custom function is called using the following syntax:

#customfunction(dll name, FQN for the static function, argument1.......,argumentN)


Consider the following input:

```JSON
{
  "tree": {
    "branch": {
      "leaf": "green",
      "flower": "red",
      "bird": "crow"
    }
  }
}
```

Transformer:

```JSON
{
  "Season": "#customfunction(JUST.NET.Test,JUST.NET.Test.Season.findseason,#valueof($.tree.branch.leaf),#valueof($.tree.branch.flower))"
}
```

Custom function:

```C#
public static string findseason(string leafColour, string flowerColour)
{
  if (leafColour == "green" && flowerColour == "red")
    return "summer";
  else
    return "winter";
}
```

Output:
```JSON
{
  "Season": "summer"
}
```

## <a name="userdefined"></a> Calling User Defined methods by name

You can also call your methods by name, by providing the full path (namespace including class name, method name, assembly if necessary).
The assembly file doesn't need to be referenced in your program, it only has to exist in the same directory. It will load the assembly into
the application's default domain, if not loaded yet.

Here's the syntax:
`#NameSpace.Plus.ClassName::Your_Method_Name(argument1.......,argumentN)`
`#AssemblyName::NameSpace.Plus.ClassName::Your_Method_Name(argument1.......,argumentN)`

Consider the following input:
```JSON
{
  "season": {
    "characteristics": {
      "hot": true,
      "averageDaysOfRain": 10,
      "randomDay": "2018-08-01T00:00:00.000Z"
    }
  }
}
```

Transformer:

```JSON
{
  "summer": "#ExternalMethods::SeasonsHelper.Season::IsSummer(#valueof($.season.characteristics.hot),#valueof($.season.characteristics.averageDaysOfRain),#valueof($.season.characteristics.randomDay))"
}
```

Output:
```JSON
{
  "summer": true
}
```

## <a name="registercustom"></a> Register User Defined methods for seamless use

To reduce the fuzz of calling custom methods, there's this class `JUSTContext`, where you can register your custom functions, and then pass it as a parameter to `JsonTransformer` constructor. All calls of `Transform` methods will use the supplied `JUSTContext`.
There's also this class `CustomFunction` that as all the necessary properties to register a custom function. One can create a list of there classes and pass it to `JUSTContext` constructor.

Examples:
```C#
new JUSTContext().RegisterCustomFunction(assemblyName, namespace, methodName, methodAlias);
new JUSTContext().RegisterCustomFunction(new CustomFunction{ AssemblyName = "someAssembly", Namespace = "someNamespace", MethodName = "someMethod", MethodAlias = "someAlias" });
new JUSTContext(new[] { new CustomFunction{ AssemblyName = "someAssembly", Namespace = "someNamespace", MethodName = "someMethod", MethodAlias = "someAlias" } });
```

Parameter 'namespace' must include the class name as well, 'assemblyName' is optional, so as 'methodAlias', which can be 
used to register methods with the same name under diferent namespaces.
After registration you can call it like any other built-in function.

You have the possibility to unregister a custom function or remove all registrations with the following methods:
`new JUSTContext().UnregisterCustomFunction(aliasOrName)`
`new JUSTContext().ClearCustomFunctionRegistrations()`

Consider the following input:

```JSON
{
  "season": {
    "characteristics": {
      "hot": true,
      "averageDaysOfRain": 10,
      "randomDay": "2018-08-01T00:00:00.000Z"
    }
  }
}
```

Registration:
```C#
var context = new JUSTContext();
context.RegisterCustomFunction("SomeAssemblyName", "NameSpace.Plus.ClassName", "IsSummer");
new JsonTransformer(context).Transform(<transformer>, <input>);
```


Transformer:
```JSON
{
  "summer": "#IsSummer(#valueof($.season.characteristics.hot),#valueof($.season.characteristics.averageDaysOfRain),#valueof($.season.characteristics.randomDay))"
}
```

Output:
```JSON
{
  "summer": true
}
```


## <a name="complexnestedfunc"></a> Complex nested functions

You can easily nest functions to do complex transformations. An example of such a transformation would be:

Consider the following input:

```JSON
{
  "Name": "Kari",
  "Surname": "Nordmann",
  "MiddleName": "Inger",
  "ContactInformation": "Karl johans gate:Oslo:88880000",
  "PersonalInformation": "45:Married:Norwegian"
}
```

Transformer:

```JSON
{
  "FullName": "#concat(#concat(#concat(#valueof($.Name), ),#concat(#valueof($.MiddleName), )),#valueof($.Surname))",
  "Contact Information": {
    "Street Name": "#substring(#valueof($.ContactInformation),0,#firstindexof(#valueof($.ContactInformation),:))",
    "City": "#substring(#valueof($.ContactInformation),#add(#firstindexof(#valueof($.ContactInformation),:),1),#subtract(#subtract(#lastindexof(#valueof($.ContactInformation),:),#firstindexof(#valueof($.ContactInformation),:)),1))",
	"PhoneNumber": "#substring(#valueof($.ContactInformation),#add(#lastindexof(#valueof($.ContactInformation),:),1),#subtract(#subtract(#length(#valueof($.ContactInformation)),1),#lastindexof(#valueof($.ContactInformation),:)))"
  },
  "Personal Information": {
    "Age": "#substring(#valueof($.PersonalInformation),0,#firstindexof(#valueof($.PersonalInformation),:))",
    "Civil Status": "#substring(#valueof($.PersonalInformation),#add(#firstindexof(#valueof($.PersonalInformation),:),1),#subtract(#subtract(#lastindexof(#valueof($.PersonalInformation),:),#firstindexof(#valueof($.PersonalInformation),:)),1))",
    "Ethnicity": "#substring(#valueof($.PersonalInformation),#add(#lastindexof(#valueof($.PersonalInformation),:),1),#subtract(#subtract(#length(#valueof($.PersonalInformation)),1),#lastindexof(#valueof($.PersonalInformation),:)))"
  }
}
```


Output:
```JSON
{
  "FullName": "Kari Inger Nordmann",
  "Contact Information": {
    "Street Name": "Karl johans gate",
    "City": "Oslo",
    "PhoneNumber": "88880000"
  },
  "Personal Information": {
    "Age": "45",
    "Civil Status": "Married",
    "Ethnicity": "Norwegian"
  }
}
```

## <a name="multiargsconstants"></a> Multiple argument & constant functions

The transformation in the above scenario looks quite complex. And it could get quite messy when the string becomes longer.
Some reserved keywords, like comma (,) and hash (#), have a proper function. Also empty string has a proper function so it can be represented.

Hence, the following 5 functions have been introduced:

1. xconcat(string1,string2......stringx) - Concatenates multiple strings.
2. xadd(int1,int2......intx) - Adds multiples integers.
3. constant_comma() - Returns comma(,)
4. constant_hash() - Returns hash(#)
5. stringempty() - Returns ""

Consider the following input:

```JSON
{
  "Name": "Kari",
  "Surname": "Nordmann",
  "MiddleName": "Inger",
  "ContactInformation": "Karl johans gate:Oslo:88880000",
  "PersonalInformation": "45:Married:Norwegian",
  "AgeOfMother": 67,
  "AgeOfFather": 70,
  "Empty": ""
}
```

Transformer:

```JSON
{
  "FullName": "#xconcat(#valueof($.Name),#constant_comma(),#valueof($.MiddleName),#constant_comma(),#valueof($.Surname))",
  "AgeOfParents": "#xadd(#valueof($.AgeOfMother),#valueof($.AgeOfFather))",
  "TestSomeEmptyString": "#ifcondition(#valueof($.Empty),#stringempty(),String is empty,String is not empty)",
  "TestSomeOtherString": "#ifcondition(#valueof($.Name),#stringempty(),String is empty,String is not empty)"
}
```


Output:
```JSON
{
  "FullName":"Kari,Inger,Nordmann",
  "AgeOfParents": 137,
  "TestSomeEmptyString": "String is empty",
  "TestSomeOtherString": "String is not empty"
}
```


## <a name="escaping"></a> Escaping reserved characters

Characters like '(' and ')' (round brackets), and ',' (comma) are considered reserved characters when used within a function (not on a regular string). Also '#' (sharp) is a reserved character when used at the start of a statement (value of property or argument of a function).
To avoid parsing these characters as reserved characters you may "escape" them, adding a character before the reserved character. By default the escape character is a '/' (slash), but one can change it in `JUSTContext` class, property `EscapeChar`. For '#', it is only necessary to escape when it occurs at the start of a statement. Don't forget to escape the 'EscapeChar' also.
One note, it is not advisable to choose '\' (backslash) as an escape char, as it is already the escape char for JSON, it can make expressions cumbersome!
This is especially useful when creating dynamic expressions.

Consider the following input:

```JSON
{
  "arg": 1,
  "arr": [{
	"id": 1,
	"val": 100
  },{
	"id": 2,
	"val": 200
  }]
}
```

Transformer:

```JSON
{
  "sharp": "/#not_a_function",
  "sharp_arg": "#xconcat(/#not,_a_function_arg)",
  "parentheses": "#xconcat(func/(',#valueof($.arg),'/))", 
  "comma": "#xconcat(func/(',#valueof($.arg),'/,'other_value'/))",
  "dynamic_expr": "#valueof(#xconcat($.arr[?/(@.id==,#valueof($.arg),/)].val))"
}
```


Output:
```JSON
{
  "sharp": "#not_a_function",
  "sharp_arg": "#not_a_function_arg",
  "parentheses": "func('1')", 
  "comma": "func('1','other_value')",
  "dynamic_expr": 100
}
```



## <a name="existance"></a> Check for existance 

The following two functions have been added to check for existance:

1. exists(path)
2. existsandnotempty(path)

Consider the following input:

```JSON
{
  "BuyDate": "2017-04-10T11:36:39+03:00",
  "ExpireDate": ""
}
```

Transformer:

```JSON
{
  "BuyDateString": "#ifcondition(#exists($.BuyDate),True,#concat(Buy Date : ,#valueof($.BuyDate)),NotExists)",
  "BuyDateString2": "#ifcondition(#existsandnotempty($.BuyDate),True,#concat(Buy Date : ,#valueof($.BuyDate)),EmptyOrNotExists)",
  "ExpireDateString": "#ifcondition(#exists($.ExpireDate),True,#concat(Expire Date : ,#valueof($.ExpireDate)),NotExists)",
  "ExpireDateString2": "#ifcondition(#existsandnotempty($.ExpireDate),True,#concat(Expire Date : ,#valueof($.ExpireDate)),EmptyOrNotExists)",
  "SellDateString": "#ifcondition(#exists($.SellDate),True,#concat(Sell Date : ,#valueof($.SellDate)),NotExists)",
  "SellDateString2": "#ifcondition(#existsandnotempty($.SellDate),True,#concat(Sell Date : ,#valueof($.SellDate)),EmptyOrNotExists)"
}
```

Output:
```JSON
{
  "BuyDateString": "Buy Date : 2017-04-10T11:36:39+03:00",
  "BuyDateString2": "Buy Date : 2017-04-10T11:36:39+03:00",
  "ExpireDateString": "Expire Date : ",
  "ExpireDateString2": "EmptyOrNotExists",
  "SellDateString": "NotExists",
  "SellDateString2": "EmptyOrNotExists"
}

```

## <a name="condtrans"></a> Conditional transformation

Conditional transformation can be achieved using the *ifgroup* function.

The function can be used to include (or not) an object or a value. If it is used as property key, it takes a single argument and it will include everything inside the following scope. If it used as property value, it will include the value passed as second argument. First argument should always evaluate to a boolean value.

Consider the following input:

```JSON
{
  "Tree": {    
    "Branch": "leaf",
    "Flower": "Rose"
  }
}
```

Transformer:

```JSON
{
  "Result": {
    "Header": "JsonTransform",
    "#ifgroup(#exists($.Tree.Branch))": {
      "State": {
        "Value1": "#valueof($.Tree.Branch)",
        "Value2": "#valueof($.Tree.Flower)"
      }
    },
	"Shrubs": [ "#ifgroup(#ifcondition(#valueof($.Tree.Flower),Rose,True,False),#valueof($.Tree.Flower))" ]
  }
}
```

Output:
```JSON
{
  "Result": {
    "Header": "JsonTransform",
    "State": {
      "Value1": "leaf",
      "Value2": "Rose"
    },
	"Shrubs": [ "Rose" ]
  }
}
```

Now, for the same input if we use the following transformer, we get a diferent output.

Transformer:

```JSON
{
  "Result": {
    "Header": "JsonTransform",
    "#ifgroup(#exists($.Tree.Root))": {
      "State": {
        "Value1": "#valueof($.Tree.Branch)",
        "Value2": "#valueof($.Tree.Flower)"
      }
    },
	"Shrubs": [ "#ifgroup(#ifcondition(#valueof($.Tree.Flower),Olive,True,False),#valueof($.Tree.Flower))" ]
  }
}
```

Output:
```JSON
{  
  "Result":{  
    "Header": "JsonTransform",
	"Shrubs": [ ]
  }
}
```


## <a name="dynamicprop"></a> Dynamic Properties

We can now create dynamic properties using the *eval* function. The function takes an expression as an argument.

Consider the following input:

```JSON
{
  "Tree": {    
    "Branch": "leaf",
    "Flower": "Rose"
  }
}
```

Transformer:

```JSON
{
  "Result": {
    "#eval(#valueof($.Tree.Flower))": "x"
  }
}
```

Output:
```JSON
{  
  "Result":{  
    "Rose":"x"
  }
}
```


## <a name="applyover"></a> Apply function over transformation

Sometimes you cannnot achieve what you want directly from a single function (or composition). To overcome this you may want to apply a function over a previous transformation. That's what #applyover does.

Consider the following input:

```JSON
{
  "d": [ "one", "two", "three" ], 
  "values": [ "z", "c", "n" ]
}
```

Transformer:

```JSON
{
  "result": "#applyover({ 'condition': { '#loop($.values)': { 'test': '#ifcondition(#stringcontains(#valueof($.d[0]),#currentvalue()),True,yes,no)' } } }, '#exists($.condition[?(@.test=='yes')])')" 
}
```

Output:
```JSON
{
  "result": true
}
```


## <a name="schemavalidation"></a> Schema Validation against multiple schemas using prefixes

A new feature to validate a JSON against multiple schemas has been introduced in the new Nuget 2.0.xxx. This is to enable namespace based validation using prefixes like in XSD.

Below is a sample code which you need to write to validate a JSON against 2 schemas using prefixes:

```C#
//read input from JSON file
string inputJson = File.ReadAllText("Examples/ValidationInput.json");

//read first schema from JSON file
string schemaJsonX = File.ReadAllText("Examples/SchemaX.json");

//read second input from JSON file
string schemaJsonY = File.ReadAllText("Examples/SchemaY.json");

//create instance of JsonValidator using the input
JsonValidator validator = new JsonValidator(inputJson);

//Add first schema with prefix 'x'
validator.AddSchema("x", schemaJsonX);

//Add second schema with prefix 'y'
validator.AddSchema("y", schemaJsonY);

//Validate
validator.Validate();
```

In the above case if the validation is un-successful an exception will be thrown with the validation errors.

Consider the validation input:

```JSON
{
  "x.tree": { "x.branch": { "x.leaf": "1" } },
  "x.child": 1,
  "y.animal": 1
}
```

Schema X JSON:

```JSON
{
  "properties": {
    "tree": {
      "type": "object",
      "properties": {
        "branch": {
          "type": "object",
          "properties": {
            "leaf": {
              "type": "string"
            }
          }
        }
      }
    },
    "child": {
      "type": "string"
    }
  }
}
```

Schema Y JSON:

```JSON
{
  "properties": {
    "animal": { 
      "type": "string"    
    }
  }
}
```

The exception message thrown in the above case would be:

``Unhandled Exception: System.Exception: Invalid type. Expected String but got Integer. Path '['x.child']', line 3, position 14. AND Invalid type. Expected String but got Integer. Path '['y.animal']', line 4, position 15.``

## <a name="splitjson"></a> Splitting JSON into multiple JSON(s) based upon an array token

A JSON file containing an array can now be split into multiple JSON files, each representing a file for every array element.

Two new functions have been added for this purpose:

```C#
public static IEnumerable<string> SplitJson(string input,string arrayPath)
```

```C#
public static IEnumerable<JObject> SplitJson(JObject input, string arrayPath)
```

Consider the input:
```JSON
{
  "cars": {
    "Ford": [{
        "model": "Taurus",
        "doors": 4
      }, {
        "model": "Escort",
        "doors": 4
      }, {
        "model": "Fiesta",
        "doors": 3
      }, {
        "model": "Bronco",
        "doors": 5
      }
    ],
    "firstName": "John",
    "lastName": "Smith",
  }
}
```

Below is a sample code which splits the above input:

```C#
string input = File.ReadAllText("Input.json");
List<string> outputs = JsonTransformer.SplitJson(input, "$.cars.Ford").ToList<string>();
```

The output will contain 4 JSON files:

```JSON
{"cars":{"Ford":{"model":"Taurus","doors":4},"firstName":"John","lastName":"Smith"}}
```

```JSON
{"cars":{"Ford":{"model":"Escort","doors":4},"firstName":"John","lastName":"Smith"}}
```

```JSON
{"cars":{"Ford":{"model":"Fiesta","doors":3},"firstName":"John","lastName":"Smith"}}
```

```JSON
{"cars":{"Ford":{"model":"Bronco","doors":5},"firstName":"John","lastName":"Smith"}}
```

## <a name="otherformats"></a> Transforming JSON to other data formats

JUST.NET can now transform JSON data into other generic formats too. All functions except the BULK FUNCTIONS are supported in this feature.
The #loop functions excepts an extra argument which defines the seperator between the individual records.

```JSON
#loop(path,seaperator)
```

If the seperator is not defined, the default seperator used is NEWLINE.

A new class called `DataTransformer` has been introduced for this new feature.

### Example for JSON to XML

Sample code to transform from JSON to XML:
```C#
string input = File.ReadAllText("Input.json");``
string transformer = File.ReadAllText("DataTransformer.xml");
string transformedString = DataTransformer.Transform(transformer, input);
```

Input.json:
```JSON
{
  "menu": {
    "id": {
      "file": "csv"
    },
    "value": {
      "Window": "popup"
    },
    "popup": {
      "menuitem": [{
          "value": "New",
          "onclick": {
            "action": "CreateNewDoc()"
          }
        }, {
          "value": "Open",
          "onclick": "OpenDoc()"
        }, {
          "value": "Close",
          "onclick": "CloseDoc()"
        }
      ]
    }
  },
  "x": [{
      "v": {
        "a": "a1,a2,a3",
        "b": "1",
        "c": "10"
      }
    }, {
      "v": {
        "a": "b1,b2",
        "b": "2",
        "c": "20"
      }
    }, {
      "v": {
        "a": "c1,c2,c3",
        "b": "3",
        "c": "30"
      }
    }
  ],
  "stringref": "thisisandveryunuasualandlongstring",
  "d": ["one", "two", "three"],
  "numbers": ["1", "2", "3", "4", "5"],
  "tree": {
    "branch": {
      "leaf": "green",
      "flower": "red",
      "bird": "crow"
    }
  },
  "Name": "Kari",
  "Surname": "Nordmann",
  "MiddleName": "Inger",
  "ContactInformation": "Karl johans gate:Oslo:88880000",
  "PersonalInformation": "45:Married:Norwegian",
  "AgeOfMother": "67",
  "AgeOfFather": "70",
  "BuyDate": "2017-04-10T11:36:39+03:00",
  "ExpireDate": "",
  "LogId": 5000510625
}
```


DataTransformer.xml:
```XML
<?xml version="1.0" encoding="UTF-8" ?>
<root>
  <root>
    <ifconditiontesttrue>#ifcondition(#valueof($.menu.id.file),csv,#valueof($.menu.value.Window),fail)</ifconditiontesttrue>
    <ifconditiontestfalse>#ifcondition(#valueof($.menu.id.file),xml,#valueof($.menu.value.Window),fail)</ifconditiontestfalse>
    <stringresult>
      <lastindexofand>#lastindexof(#valueof($.stringref),and)</lastindexofand>
      <firstindexofand>#firstindexof(#valueof($.stringref),and)</firstindexofand>
      <subsrting>#substring(#valueof($.stringref),8,10)</subsrting>
      <concat>#concat(#valueof($.menu.id.file),#valueof($.menu.value.Window))</concat>
    </stringresult>
    <mathresult>
      <add>#add(#valueof($.numbers[0]),3)</add>
      <subtract>#subtract(#valueof($.numbers[4]),#valueof($.numbers[0]))</subtract>
      <multiply>#multiply(2,#valueof($.numbers[2]))</multiply>
      <divide>#divide(9,3)</divide>
    </mathresult>
    <conacted>#concatall(#valueof($.d))</conacted>
    <sum>#sum(#valueof($.numbers))</sum>
    <avg>#average(#valueof($.numbers))</avg>
    <min>#min(#valueof($.numbers))</min>
    <max>#max(#valueof($.numbers))</max>
    <arrayconacted>#concatallatpath(#valueof($.x),$.v.a)</arrayconacted>
    <arraysum>#sumatpath(#valueof($.x),$.v.c)</arraysum>
    <arrayavg>#averageatpath(#valueof($.x),$.v.c)</arrayavg>
    <arraymin>#minatpath(#valueof($.x),$.v.b)</arraymin>
    <arraymax>#maxatpath(#valueof($.x),$.v.b)</arraymax>
  </root>
  <FullName>#concat(#concat(#concat(#valueof($.Name), ),#concat(#valueof($.MiddleName), )),#valueof($.Surname))</FullName>
  <Contact_Information>
    <City>#substring(#valueof($.ContactInformation),#add(#firstindexof(#valueof($.ContactInformation),:),1),#subtract(#subtract(#lastindexof(#valueof($.ContactInformation),:),#firstindexof(#valueof($.ContactInformation),:)),1))</City>
    <PhoneNumber>#substring(#valueof($.ContactInformation),#add(#lastindexof(#valueof($.ContactInformation),:),1),#subtract(#lastindexof(#valueof($.ContactInformation),),#lastindexof(#valueof($.ContactInformation),:)))</PhoneNumber>
    <Street_Name>#substring(#valueof($.ContactInformation),0,#firstindexof(#valueof($.ContactInformation),:))</Street_Name>
  </Contact_Information>
  <Personal_Information>
    <Age>#substring(#valueof($.PersonalInformation),0,#firstindexof(#valueof($.PersonalInformation),:))</Age>
    <Ethnicity>#substring(#valueof($.PersonalInformation),#add(#lastindexof(#valueof($.PersonalInformation),:),1),#subtract(#lastindexof(#valueof($.PersonalInformation),),#lastindexof(#valueof($.PersonalInformation),:)))</Ethnicity>
    <LogId>#valueof($.LogId)</LogId>
    <Civil_Status>#substring(#valueof($.PersonalInformation),#add(#firstindexof(#valueof($.PersonalInformation),:),1),#subtract(#subtract(#lastindexof(#valueof($.PersonalInformation),:),#firstindexof(#valueof($.PersonalInformation),:)),1))</Civil_Status>
  </Personal_Information>
  <Custom>#customfunction(JUST.NET.Test,JUST.NET.Test.Season.findseason,#valueof($.tree.branch.leaf),#valueof($.tree.branch.flower))</Custom>
  <iteration>
    "#loop($.numbers,<!--Record ends here-->)": {
    <Record>
      <CurrentValue>#currentvalue()</CurrentValue>
      <CurrentIndex>#currentindex()</CurrentIndex>
      <IsLast>#ifcondition(#currentindex(),#lastindex(),yes,no)</IsLast>
      <LastValue>#lastvalue()</LastValue> 
      <SomeValue>#valueof($.LogId)</SomeValue>
    </Record>}
  </iteration>  
</root>
```

Output:
```XML
<?xml version="1.0" encoding="UTF-8" ?>
<root>
  <root>
    <ifconditiontesttrue>popup</ifconditiontesttrue>
    <ifconditiontestfalse>fail</ifconditiontestfalse>
    <stringresult>
      <lastindexofand>21</lastindexofand>
      <firstindexofand>6</firstindexofand>
      <subsrting>dveryunuas</subsrting>
      <concat>csvpopup</concat>
    </stringresult>
    <mathresult>
      <add>4</add>
      <subtract>4</subtract>
      <multiply>6</multiply>
      <divide>3</divide>
    </mathresult>
    <conacted>onetwothree</conacted>
    <sum>15</sum>
    <avg>3</avg>
    <min>1</min>
    <max>5</max>
    <arrayconacted>a1,a2,a3b1,b2c1,c2,c3</arrayconacted>
    <arraysum>60</arraysum>
    <arrayavg>20</arrayavg>
    <arraymin>1</arraymin>
    <arraymax>3</arraymax>
  </root>
  <FullName>Kari Inger Nordmann</FullName>
  <Contact_Information>
    <City>Oslo</City>
    <PhoneNumber>88880000</PhoneNumber>
    <Street_Name>Karl johans gate</Street_Name>
  </Contact_Information>
  <Personal_Information>
    <Age>45</Age>
    <Ethnicity>Norwegian</Ethnicity>
    <LogId>5000510625</LogId>
    <Civil_Status>Married</Civil_Status>
  </Personal_Information>
  <Custom>summer</Custom>
  <iteration>
    <Record>
      <CurrentValue>1</CurrentValue>
      <CurrentIndex>0</CurrentIndex>
      <IsLast>no</IsLast>
      <LastValue>5</LastValue>
      <SomeValue>5000510625</SomeValue>
    </Record><!--Record ends here-->
    <Record>
      <CurrentValue>2</CurrentValue>
      <CurrentIndex>1</CurrentIndex>
      <IsLast>no</IsLast>
      <LastValue>5</LastValue>
      <SomeValue>5000510625</SomeValue>
    </Record><!--Record ends here-->
    <Record>
      <CurrentValue>3</CurrentValue>
      <CurrentIndex>2</CurrentIndex>
      <IsLast>no</IsLast>
      <LastValue>5</LastValue>
      <SomeValue>5000510625</SomeValue>
    </Record><!--Record ends here-->
    <Record>
      <CurrentValue>4</CurrentValue>
      <CurrentIndex>3</CurrentIndex>
      <IsLast>no</IsLast>
      <LastValue>5</LastValue>
      <SomeValue>5000510625</SomeValue>
    </Record><!--Record ends here-->
    <Record>
      <CurrentValue>5</CurrentValue>
      <CurrentIndex>4</CurrentIndex>
      <IsLast>yes</IsLast>
      <LastValue>5</LastValue>
      <SomeValue>5000510625</SomeValue>
    </Record><!--Record ends here-->
  </iteration>
</root>
```

### Example for JSON to CSV

Sample code to transform from JSON to CSV:
```C#
string transformer = File.ReadAllText("Input.json");
string transformer = File.ReadAllText("DataTransformer.csv");
string transformedString = DataTransformer.Transform(transformer, input);
```

The input file is same as the xml example.

DataTransformer.csv:
```
"#loop($.numbers)": {#currentvalue(),#currentindex(),#ifcondition(#currentindex(),#lastindex(),yes,no),#lastvalue(),#valueof($.LogId)}
```

Output:
```CSV
1,0,no,5,5000510625
2,1,no,5,5000510625
3,2,no,5,5000510625
4,3,no,5,5000510625
5,4,yes,5,5000510625
```
