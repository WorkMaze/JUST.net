# JUST

JUST Stands for **JSON Under Simple Transformation**

XSLT is a very popular way of transforming XML documents using a simple transformation language. More and more applications are now using JSON as a data format because it is much simpler and less bulkier than XML.
However, there isn't a very easy way to transforming JSON documents.

I have created a library in .NET which enables the transformation of JSON documents using very a simple transformation language. This is an attempt to create an XSLT parallel for JSON.
The library is available as a NUGET package.

This C# project has working examples of the transformations.


# JUST.NET Library

Pull the latest JUST.NET from https://www.nuget.org
``Install-Package JUST``


# Write a simple C# code snippet to transform your JSON

This is demonstrated with various examples in the source code. Once you install the nuget to your project you need to import the following namespace:-

using JUST;

Below is a simple C# code snippet that you can use to transform your JSON:-

``string input = File.ReadAllText("Examples/Input.json"); //read input from JSON file.``

``string transformer = File.ReadAllText("Examples/Transformer.json"); //read the transformer from a JSON file.``

``string transformedString = JsonTransformer.Transform(transformer, input); // do the actual transformation.``


# Using JUST to transform JSON

JUST is a transformation language just like XSLT. It includes functions which are used inside the transformer JSON to transform the input JSON in a desired output JSON. This section describes the various functions present in JUST and how they can be used to transform your JSON.

Every JUST function starts with "#" character.

## valueof

This function is used to extract the value of a given property. The value is extracted using JSON path of the property. For more information on how to use JSON path refer to :- 
http://www.newtonsoft.com/json/help/html/QueryJsonSelectTokenJsonPath.htm

Consider the input:-

``{
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
}``

Transformer:-

``{
  "result": {
    "Open": "#valueof($.menu.popup.menuitem[?(@.value=='Open')].onclick)",
    "Close": "#valueof($.menu.popup.menuitem[?(@.value=='Close')].onclick)"
  }
}``

Output:-

``{
   "result":{"Open":null,"Close":"OpenDoc()"}
}``


## ifcondition

This condition is used to evaluate and if-else condition.

ifcondition(condition expresson, evaluation expression, true result, false result).

All four of the parameters can be a 'valueof' expressions or constants.

Consider the input:-

``{
  "menu": {
    "id" : "github",
    "repository" : "JUST"
  } 
}``

Transformer:-

``{
  "ifconditiontesttrue": "#ifcondition(#valueof($.menu.id),github,#valueof($.menu.repository),fail)",
  "ifconditiontestfalse": "#ifcondition(#valueof($.menu.id),xml,#valueof($.menu.repository),fail)"
}``

Output:-

``{
   "ifconditiontesttrue":"JUST",
   "ifconditiontestfalse":"fail"
}``

## string and math functions

At the moment only the basic and often used string and math functions are provided in the library.

1. lastindexof(input string,search string)
2. firstindexof(input string,search string)
3. substring(input string,start indes,length)
4. concat(string 1,string 2)
5. add(value 1,value 2)
6. subtract(value 1,value 2)
3. multiply(value 1,value 2)
4. divide(value 1,values 2)

Consider the input:-

``{
  "stringref": "thisisandveryunuasualandlongstring",
  "numbers": [ "1", "2", "3", "4", "5" ]
}``

Transformer:-

``{
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
    "divide": "#divide(9,3)"
  }
}``

Output:-

``{"stringresult":
   { 
    "lastindexofand":"21",
    "firstindexofand":"6",
    "substring":"veryunuasua",
    "concat":""
   },
   "mathresult":
   {
    "add":"4",
    "subtract":"4",
    "multiply":"6",
    "divide":"3"
   }
}``


## Aggregate functions

The following aggregate functions are provided for single dimensional arrays:-

1. concatall(array)
2. sum(array)
3. average(array)
4. min(array)
5. max(array)

Consider the input:-
 
``{
  "d": [ "one", "two", "three" ],
  "numbers": [ "1", "2", "3", "4", "5" ]
}``

Transformer:-

``{
  "conacted": "#concatall(#valueof($.d))",
  "sum": "#sum(#valueof($.numbers))",
  "avg": "#average(#valueof($.numbers))",
  "min": "#min(#valueof($.numbers))",
  "max": "#max(#valueof($.numbers))"
}``

Output:-

``{
    "conacted":"onetwothree",
    "sum":"15",
    "avg":"3",
    "min":"1",
    "max":"5"
}``

## Aggregate functions for multidimensional arrays:-

These functions are essentially the same as the above ones, the only difference being that you can also provide a path to point to particluar element inside the array.
1. concatallatpath(array,path)
2. sumatpath(array,path)
3. averageatpath(array,path)
4. minatpath(array,path)
5. maxatpath(array,path)

Consider the input:-

``{
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
}``

Transformer:-

``{
  "arrayconacted": "#concatallatpath(#valueof($.x),$.v.a)",
  "arraysum": "#sumatpath(#valueof($.x),$.v.c)",
  "arrayavg": "#averageatpath(#valueof($.x),$.v.c)",
  "arraymin": "#minatpath(#valueof($.x),$.v.b)",
  "arraymax": "#maxatpath(#valueof($.x),$.v.b)"
}``

Output:-

``{
    "arrayconacted":"a1,a2,a3b1,b2c1,c2,c3",
    "arraysum":"60",
    "arrayavg":"20",
    "arraymin":"1",
    "arraymax":"3"
}``

## Bulk functions

All the above functions set property values to predefined properties in the output JSON. However, in some cases we don't know what our output will look like as it depends on the input.
Bulk functions are provided for this purpose. They correspond with the template-match functions in XSLT.

Bulk functions by law have to be the first property of the JSON object. All bulk functions are represented as array elements of the property '#'.

These are the bulk functions provided as of now:-

1. copy(path)
2. replace(path)
3. delete(path)

Cosider the input:-

``{
  "tree": {
    "branch": {
      "leaf": "green",
      "flower": "red",
      "bird": "crow",
      "extra": { "twig":"birdnest" }
    },
    "ladder": {"wood": "treehouse" }
  }
}``

Transformer:-

``{
  "#": [ "#copy($)",  "#delete($.tree.branch.bird)", "#replace($.tree.branch.extra,#valueof($.tree.ladder))" ],
  "othervalue" : "othervalue"
}``

Output:-

``{
   "othervalue":"othervalue",
   "tree":{
    "branch":{
     "leaf":"green",
     "flower":"red",
     "extra":{
      "wood":"treehouse"
     }
    },
   "ladder":{
     "wood":"treehouse"
    }
  }
}``

## Array looping

In some cases we don't want to copy the entire array to the destination JSON. We might want to transform the array into a different format, or have some special logic for each element while setting the destination JSON.
For these cases we would use array looping.

These are the functions provided for this pupose:-

1. loop(path) - path is the path of the array to loop
2. currentvalue()
3. currentindex()
4. lastindex()
5. lastvalue()
6. currentvalueatpath(path) - here path denotes the path inside the array
7. lastvalueatpath(path) - here path denotes the path inside the array

Cosider the input:-

``{
  "tree": {
    "branch": {
      "leaf": "green",
      "flower": "red",
      "bird": "crow",
     "extra": { "twig": "birdnest" }
    },
    "ladder": { "wood": "treehouse" }
  },
  "numbers": [ "1", "2", "3", "4" ],
  "arrayobjects": [
    {"country": {"name": "norway","language": "norsk"}},
    {
      "country": {
        "name": "UK",
        "language": "english"
      }
    },
    {
      "country": {
        "name": "Sweden",
        "language": "swedish"
      }
    }]
}``

Transformer:-

``{
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
  "othervalue": "othervalue"
}``

Output:-

``{"iteration":[
   {"CurrentValue":"1","CurrentIndex":"0","IsLast":"no","LastValue":"4"},
   {"CurrentValue":"2","CurrentIndex":"1","IsLast":"no","LastValue":"4"},
   {"CurrentValue":"3","CurrentIndex":"2","IsLast":"no","LastValue":"4"},
   {"CurrentValue":"4","CurrentIndex":"3","IsLast":"yes","LastValue":"4"}
  ],
   "iteration2":[
   {"CurrentValue":"norway","CurrentIndex":"0","IsLast":"no","LastValue":"swedish"},
   {"CurrentValue":"UK","CurrentIndex":"1","IsLast":"no","LastValue":"swedish"},
   {"CurrentValue":"Sweden","CurrentIndex":"2","IsLast":"yes","LastValue":"swedish"}
  ],
"othervalue":"othervalue"
}``

## Calling Custom functions

You can make your own custom functions in C# and call them from your transformer JSON.
A custom function has to reside inside a public class and has to be a public static method.

A custom function is called using the following syntax:-

#customfunction(dll name, FQN for the static function, argument1.......,argumentN)


Consider the following input:-

``{
  "tree": {
    "branch": {
      "leaf": "green",
      "flower": "red",
      "bird": "crow"
    }
  }
}``

Transformer:-

``{
  "Season": "#customfunction(JUST.NET.Test,JUST.NET.Test.Season.findseason,#valueof($.tree.branch.leaf),#valueof($.tree.branch.flower))"
}``

Custom function:-

``using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace JUST.NET.Test
{
    public class Season
    {
        public static string findseason(string leafColour, string flowerColour)
        {
            if (leafColour == "green" && flowerColour == "red")
                return "summer";
            else
                return "winter";
        }

}
}``

Output:-
``{"Season":"summer"}``

## Complex nested functions

You can easily nest functions to do complex transformations. An example of such a transformation would be:-

Consider the following input:-

``{
  "Name": "Kari",
  "Surname": "Nordmann",
  "MiddleName": "Inger",
  "ContactInformation": "Karl johans gate:Oslo:88880000" ,
  "PersonalInformation": "45:Married:Norwegian"
}``

Transformer:-

``{
  "FullName": "#concat(#concat(#concat(#valueof($.Name), ),#concat(#valueof($.MiddleName), )),#valueof($.Surname))",
  "Contact Information": {
    "Street Name": "#substring(#valueof($.ContactInformation),0,#firstindexof(#valueof($.ContactInformation),:))",
    "City": "#substring(#valueof($.ContactInformation),#add(#firstindexof(#valueof($.ContactInformation),:),1),#subtract(#subtract(#lastindexof(#valueof($.ContactInformation),:),#firstindexof(#valueof($.ContactInformation),:)),1))",
    "PhoneNumber": "#substring(#valueof($.ContactInformation),#add(#lastindexof(#valueof($.ContactInformation),:),1),#subtract(#lastindexof(#valueof($.ContactInformation),),#lastindexof(#valueof($.ContactInformation),:)))"
  },
  "Personal Information": {
    "Age": "#substring(#valueof($.PersonalInformation),0,#firstindexof(#valueof($.PersonalInformation),:))",
    "Civil Status": "#substring(#valueof($.PersonalInformation),#add(#firstindexof(#valueof($.PersonalInformation),:),1),#subtract(#subtract(#lastindexof(#valueof($.PersonalInformation),:),#firstindexof(#valueof($.PersonalInformation),:)),1))",
    "Ethnicity": "#substring(#valueof($.PersonalInformation),#add(#lastindexof(#valueof($.PersonalInformation),:),1),#subtract(#lastindexof(#valueof($.PersonalInformation),),#lastindexof(#valueof($.PersonalInformation),:)))"
  }``


Output:-
``{
   "FullName":"Kari Inger Nordmann",
   "Contact Information":{
     "Street Name":"Karl johans gate",
     "City":"Oslo",
     "PhoneNumber":"88880000"
    },
   "Personal Information":{
     "Age":"45",
     "Civil Status":"Married",
     "Ethnicity":"Norwegian"
    }
}``

## Multiple argument & constant functions

The transformation in the above scenario looks quite complex. And it could get quite messy when the string becomes longer. Also, since comma(,) is a reserved keyword, it is not possible to concatenate a comma to a string.

Hence, the following 3 functions have been introduced:-

1. xconcat(string1,string2......stringx) - Concatenates multiple strings.
2. xadd(int1,int2......intx) - Adds multiples integers.
3. constant_comma() - Returns comma(,)

Consider the following input:-

``{
  "Name": "Kari",
  "Surname": "Nordmann",
  "MiddleName": "Inger",
  "ContactInformation": "Karl johans gate:Oslo:88880000" ,
  "PersonalInformation": "45:Married:Norwegian"
}``

Transformer:-

``{
  "FullName": "#xconcat(#valueof($.Name),#constant_comma(),#valueof($.MiddleName),#constant_comma(),#valueof($.Surname))",
  "AgeOfParents": "#xadd(#valueof($.AgeOfMother),#valueof($.AgeOfFather))"
}``


Output:-
``{"FullName":"Kari,Inger,Nordmann","AgeOfParents":"67"}``

## Schema Validation against multiple schemas using prefixes

A new feature to validate a JSON against multiple schemas has been introduced in the new Nuget 2.0.0. This is to enable namespace based validation using prefixes like in XSD.

Below is a sample code which you need to write to validate a JSON against 2 schemas using prefixes:-

``string inputJson = File.ReadAllText("Examples/ValidationInput.json");//read input from JSON file.``

``string schemaJsonX = File.ReadAllText("Examples/SchemaX.json");//read first schema from JSON file.``

``string schemaJsonY = File.ReadAllText("Examples/SchemaY.json");//read second input from JSON file.``

``JsonValidator validator = new JsonValidator(inputJson);//create instance of JsonValidator using the input``

``validator.AddSchema("x", schemaJsonX);//Add first schema with prefix 'x'``

``validator.AddSchema("y", schemaJsonY);//Add second schema with prefix 'y'``

``validator.Validate();//Validate``

In the above case if the validation is un-successful an exception will be thrown with the validation errors.

Consider the validation input:-

``{
  "x.tree": { "x.branch": { "x.leaf": "1" } },
  "x.child": 1,
  "y.animal": 1
}``


Schema X JSON:-

``{
  "properties": {
    "tree": {
      "type": "object",
      "properties": {
        "branch": {
          "type": "object",
          "properties": {
            "leaf": { "type": "string" }
          }
        }
      }
    },
    "child": { "type": "string" }
  }
}``

Schema Y JSON:-

``{
  "properties": {
    "animal": { "type": "string" }
  }
}
``

The exception message thrown in the above case would be:-

``Unhandled Exception: System.Exception: Invalid type. Expected String but got Integer. Path '['x.child']', line 3, position 14. AND Invalid type. Expected String but got Integer. Path '['y.animal']', line 4, position 15.``

