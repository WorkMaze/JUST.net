# JUST.NET Library

Pull the latest JUST.NET from https://www.nuget.org
Install-Package JUST 


# Write a simple C# code snippet to transform your JSON

This is demonstrated with various examples in the source code. Once you install the nuget to your project you need to import the following namespace:-

using JUST;

Below is a simple C# code snippet that you can use to transform your JSON:-

string input = File.ReadAllText("Examples/Input.json"); //read input from JSON file.

string transformer = File.ReadAllText("Examples/Transformer.json"); //read the transformer from a JSON file.

string transformedString = JsonTransformer.Transform(transformer, input); // do the actual transformation.


# Using JUST to transform JSON

JUST is a transformation language just like XSLT. It includes functions which are used inside the transformer JSON to transform the input JSON in a desired output JSON. This section describes the various functions present in JUST and how they can be used to transform your JSON.

Every JUST function starts with "#" character.

## valueof

This function is used to extract the value of a given property. The value is extracted using JSON path of the property. For more information on how to use JSON path refer to :- 
http://www.newtonsoft.com/json/help/html/QueryJsonSelectTokenJsonPath.htm

Consider the input:-

{
  "menu": {   
    "popup": {
      "menuitem": [
       {
          "value": "Open",
          "onclick": "OpenDoc()"
        },
        {
          "value": "Close",
          "onclick": "CloseDoc()"
        }
      ]
    }
  } 
}

Transformer:-

{
  "result": {
    "Open": "#valueof($.menu.popup.menuitem[?(@.value=='Open')].onclick)",
    "Close": "#valueof($.menu.popup.menuitem[?(@.value=='Close')].onclick)"
  }
}

Output:-

{
   "result":{"Open":null,"Close":"OpenDoc()"}
}


## ifcondition

This condition is used to evaluate and if-else condition.

ifcondition(condition expresson, evaluation expression, true result, false result).

All four of the parameters can be a 'valueof' expressions or constants.

Consider the input:-

{
  "menu": {
    "id" : "github",
    "repository" : "JUST"
  } 
}

Transformer:-

{
  "ifconditiontesttrue": "#ifcondition(#valueof($.menu.id),github,#valueof($.menu.repository),fail)",
  "ifconditiontestfalse": "#ifcondition(#valueof($.menu.id),xml,#valueof($.menu.repository),fail)"
}

Output:-

{"ifconditiontesttrue":"JUST","ifconditiontestfalse":"fail"}

## string and math functions

At the moment only the basic and often used string and math functions are provided in the library.

1. lastindexof(input string,search string)
2. firstindexof(input string,search string)
3. substring(input string,start indes,length)
4. concat(string 1,string 2)
5. add(value 1,value 2)
6. subtract(value 1,value 2)
3. multiply(value 1,value 2)
4. devide(value 1,values 2)

Consider the input:-

{
  "stringref": "thisisandveryunuasualandlongstring",
  "numbers": [ "1", "2", "3", "4", "5" ]
}

Transformer:-

{
  "stringresult": {
    "lastindexofand": "#lastindexof(#valueof($.stringref),and)",
    "firstindexofand": "#firstindexof(#valueof($.stringref),and)",
    "substring": "#substring(#valueof($.stringref),9,11)",
    "concat": "#concat(#valueof($.menu.id.file),#valueof($.menu.value.Window))"
  },
  "mathresult": {
    "add": "#add(#valueof($.numbers[0]),3)",
    "subtract": "#subtract(#valueof($.numbers[4]),#valueof($.numbers[0]))",
    "multiply": "#multiply(2,#valueof($.numbers[2]))",
    "devide": "#devide(9,3)"
  }
}

Output:-

{"stringresult":{"lastindexofand":"21","firstindexofand":"6","substring":"veryunuasua","concat":""},"mathresult":{"add":"4","subtract":"4","multiply":"6","devide":"3"}}


## Aggregate functions

The following aggregate functions are provided for single dimensional arrays:-

1. concatall(array)
2. sum(array)
3. average(array)
4. min(array)
5. max(array)

Consider the input:-
 
{
  "d": [ "one", "two", "three" ],
  "numbers": [ "1", "2", "3", "4", "5" ]
}

Transformer:-

{
  "conacted": "#concatall(#valueof($.d))",
  "sum": "#sum(#valueof($.numbers))",
  "avg": "#average(#valueof($.numbers))",
  "min": "#min(#valueof($.numbers))",
  "max": "#max(#valueof($.numbers))"
}

Output:-

{"conacted":"onetwothree","sum":"15","avg":"3","min":"1","max":"5"}

## Aggregate functions for multidimensional arrays:-

These functions are essentially the same as the above ones, the only difference being that you can also provide a path to point to particluar element inside the array.
1. concatallatpath(array,path)
2. sumatpath(array,path)
3. averageatpath(array,path)
4. minatpath(array,path)
5. maxatpath(array,path)

Consider the input:-

{
   "x": [
    {
      "v": {
        "a": "a1,a2,a3",
        "b": "1",
        "c": "10"
      }
    },
    {
      "v": {
        "a": "b1,b2",
        "b": "2",
        "c": "20"
      }

    },
    {
      "v": {
        "a": "c1,c2,c3",
        "b": "3",
        "c": "30"
      }
    }
  ]
}

Transformer:-

{
  "arrayconacted": "#concatallatpath(#valueof($.x),$.v.a)",
  "arraysum": "#sumatpath(#valueof($.x),$.v.c)",
  "arrayavg": "#averageatpath(#valueof($.x),$.v.c)",
  "arraymin": "#minatpath(#valueof($.x),$.v.b)",
  "arraymax": "#maxatpath(#valueof($.x),$.v.b)"
}

Output:-

{"arrayconacted":"a1,a2,a3b1,b2c1,c2,c3","arraysum":"60","arrayavg":"20","arraymin":"1","arraymax":"3"}
