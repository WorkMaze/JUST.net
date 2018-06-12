[![Build Status](https://workmaze.visualstudio.com/_apis/public/build/definitions/82d9da6f-b38c-4d61-a668-2087094c2849/1/badge)](https://workmaze.visualstudio.com/JUST.net/_build/index?definitionId=1)

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

A dotnetcore version is also available 
``Install-Package JUST.NETCore``

A .net standard library is now available. This is the version which will be supported from now on.
``Install-Package JUST.NET``

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
   "result":{"Open": "OpenDoc()", "Close": "CloseDoc()"}
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

## Opearators

The following operators have been added to compare strings and numbers :-

1. stringequals(string1, string2)
2. stringcontains(string1, string2)
3. mathequals(decimal1, decimal2)
4. mathgreaterthan(decimal1, decimal2)
5. mathlessthan(decimal1, decimal2)
6. mathgreaterthanorequalto(decimal1, decimal2)
7. mathlessthanorequalto(decimal1, decimal2)

Consider the input:-
 
``{
  "d": [ "one", "two", "three" ],
  "numbers": [ "1", "2", "3", "4", "5" ]
}``

Transformer:-

``{
  "mathresult": {
    "third_element_equals_3": "#ifcondition(#mathequals(#valueof($.numbers[2]),3),true,yes,no)",
    "third_element_greaterthan_2": "#ifcondition(#mathgreaterthan(#valueof($.numbers[2]),2),true,yes,no)",
    "third_element_lessthan_4": "#ifcondition(#mathlessthan(#valueof($.numbers[2]),4),true,yes,no)",
    "third_element_greaterthanorequals_4": "#ifcondition(#mathgreaterthanorequalto(#valueof($.numbers[2]),4),true,yes,no)",
    "third_element_lessthanoreuals_2": "#ifcondition(#mathlessthanorequalto(#valueof($.numbers[2]),2),true,yes,no)",
    "one_stringequals": "#ifcondition(#stringequals(#valueof($.d[0]),one),true,yes,no)",
    "one_stringcontains": "#ifcondition(#stringcontains(#valueof($.d[0]),n),true,yes,no)"
  }
}``

Output:-

``{"mathresult":   {"third_element_equals_3":"yes","third_element_greaterthan_2":"yes","third_element_lessthan_4":"yes","third_element_greaterthanorequals_4":"no","third_element_lessthanoreuals_2":"no","one_stringequals":"yes","one_stringcontains":"yes"}}

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
"othervalue":"othervalue"}``

## Nested array looping (looping within context)
A new function `loopwithincontext` has been introduced to be able to loop withing the context of an outer loop.
Cosider the input:-
``{
  "NestedLoop": {
    "Organization": {
      "Employee": [
        {
          "Name": "E2",
          "Details": [
            {
              "Country": "Iceland",
              "Age": "30",
              "Name": "Sven",
              "Language": "Icelandic"
            }
          ]
        },
        {
          "Name": "E1",
          "Details": [
            {
              "Country": "Denmark",
              "Age": "30",
              "Name": "Svein",
              "Language": "Danish"
            }
          ]
        }
      ]
    }
  }
}``
Transformer:-

``{
  "hello": {
    "#loop($.NestedLoop.Organization.Employee)": {
      "CurrentName": "#currentvalueatpath($.Name)",
      "Details": {
        "#loopwithincontext($.Details)": {
          "CurrentCountry": "#currentvalueatpath($.Country)"
        }
      }
    }
  }
}``

Output:-

``{
  "hello":
    [
      {"CurrentName":"E2","Details":[{"CurrentCountry":"Iceland"}]},
      {"CurrentName":"E1","Details":[{"CurrentCountry":"Denmark"}]}
    ]
}``

## Array grouping

A function similar to SQL GROUP BY clause has been introduced to group an array based on the value of an element.

grouparrayby(path,groupingElementName,groupedElementName)

Input:-

``{
  "Forest": [
    {
      "type": "Mammal",
      "qty": 1,
      "name": "Hippo"
    },
    {
      "type": "Bird",
      "qty": 2,
      "name": "Sparrow"
    },
    {
      "type": "Amphibian",
      "qty": 300,
      "name": "Lizard"
    },
    {
      "type": "Bird",
      "qty": 3,
      "name": "Parrot"
    },
    {
      "type": "Mammal",
      "qty": 1,
      "name": "Elephant"
    },
    {
      "type": "Mammal",
      "qty": 10,
      "name": "Dog"
    }    
  ]
}``

Transformer:-

``{
  "Result": "#grouparrayby($.Forest,type,all)" 
}``

Output:-

``{  
   "Result":[  
      {  
         "type":"Mammal",
         "all":[  
            {  
               "qty":1,
               "name":"Hippo"
            },
            {  
               "qty":1,
               "name":"Elephant"
            },
            {  
               "qty":10,
               "name":"Dog"
            }
         ]
      },
      {  
         "type":"Bird",
         "all":[  
            {  
               "qty":2,
               "name":"Sparrow"
            },
            {  
               "qty":3,
               "name":"Parrot"
            }
         ]
      },
      {  
         "type":"Amphibian",
         "all":[  
            {  
               "qty":300,
               "name":"Lizard"
            }
         ]
      }
   ]
}``

You can group using multiple "grouping elements". They should be seperated by a semicolon (:)

Input:-

``{
  "Vehicle": [
    {
      "type": "air",
      "company": "Boeing",
      "name": "airplane"
    },
    {
      "type": "air",
      "company": "Concorde",
      "name": "airplane"
    },
    {
      "type": "air",
      "company": "Boeing",
      "name": "Chopper"
    },
    {
      "type": "land",
      "company": "GM",
      "name": "car"
    },
    {
      "type": "sea",
      "company": "Viking",
      "name": "ship"
    },
    {
      "type": "land",
      "company": "GM",
      "name": "truck"
    }
  ]  
}``

Transformer:-

``{
  "Result": "#grouparrayby($.Vehicle,type:company,all)"
}``

Output:-

``{  
   "Result":[  
      {  
         "type":"air",
         "company":"Boeing",
         "all":[  
            {  
               "name":"airplane"
            },
            {  
               "name":"Chopper"
            }
         ]
      },
      {  
         "type":"air",
         "company":"Concorde",
         "all":[  
            {  
               "name":"airplane"
            }
         ]
      },
      {  
         "type":"land",
         "company":"GM",
         "all":[  
            {  
               "name":"car"
            },
            {  
               "name":"truck"
            }
         ]
      },
      {  
         "type":"sea",
         "company":"Viking",
         "all":[  
            {  
               "name":"ship"
            }
         ]
      }
   ]
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

`public static string findseason(string leafColour, string flowerColour)
        {
            if (leafColour == "green" && flowerColour == "red")
                return "summer";
            else
                return "winter";
        }`

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
4. constant_hash() - Returns hash(#)

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

## Check for existance 

The following two functions have been added to check for existance:-

1. exists(path)
2. existsandnotempty(path)

Consider the following input:-

``{
   "BuyDate": "2017-04-10T11:36:39+03:00",
   "ExpireDate": ""
}``

Transformer:-

``{
  "BuyDateString": "#ifcondition(#exists($.BuyDate),true,#concat(Buy Date : ,#valueof($.BuyDate)),NotExists)",
  "BuyDateString2": "#ifcondition(#existsandnotempty($.BuyDate),true,#concat(Buy Date : ,#valueof($.BuyDate)),EmptyOrNotExists)",
  "ExpireDateString": "#ifcondition(#exists($.ExpireDate),true,#concat(Expire Date : ,#valueof($.ExpireDate)),NotExists)",
  "ExpireDateString2": "#ifcondition(#existsandnotempty($.ExpireDate),true,#concat(Expire Date : ,#valueof($.ExpireDate)),EmptyOrNotExists)",
  "SellDateString": "#ifcondition(#exists($.SellDate),true,#concat(Sell Date : ,#valueof($.SellDate)),NotExists)",
  "SellDateString2": "#ifcondition(#existsandnotempty($.SellDate),true,#concat(Sell Date : ,#valueof($.SellDate)),EmptyOrNotExists)"
}``

Output:-
``{"BuyDateString":"Buy Date : 2017-04-10T11:36:39+03:00",
   "BuyDateString2":"Buy Date : 2017-04-10T11:36:39+03:00",
   "ExpireDateString":"Expire Date : ",
   "ExpireDateString2":"EmptyOrNotExists",
   "SellDateString":"NotExists",
   "SellDateString2":"EmptyOrNotExists"
}``

## Conditional transformation

Conditional transformation can be achieved using the *ifgroup* function.

The function takes an expression as argument which should evaluate to a boolean value.

Consider the following input:-

``{
  "Tree": {    
    "Branch": "leaf",
    "Flower": "Rose"
  }
}``

Transformer:-

``{
  "Result": {
    "Header": "JsonTransform",
    "#ifgroup(#exists($.Tree.Branch))": {
      "State": {
        "Value1": "#valueof($.Tree.Branch)",
        "Value2": "#valueof($.Tree.Flower)"
      }
    }
 }
}``

Output:-
``{  
   "Result":{  
      "Header":"JsonTransform",
      "State":{  
         "Value1":"leaf",
         "Value2":"Rose"
      }
   }
}``

Now, for the same input if we use the following transformer, we get a diferent output.

Transformer:-

``{
  "Result": {
    "Header": "JsonTransform",
    "#ifgroup(#exists($.Tree.Root))": {
      "State": {
        "Value1": "#valueof($.Tree.Branch)",
        "Value2": "#valueof($.Tree.Flower)"
      }
    }
 }
}``

Output:-
``{  
   "Result":{  
      "Header":"JsonTransform"
   }
}``


## Dynamic Properties

We can now create dynamic properties using the *eval* function. The function takes an expression as an argument.

Consider the following input:-

``{
  "Tree": {    
    "Branch": "leaf",
    "Flower": "Rose"
  }
}``

Transformer:-

``{
  "Result": {
      "#eval(#valueof($.Tree.Flower))": "x"
  }
}``

Output:-
``{  
   "Result":{  
      "Rose":"x"
   }
}``
 


## Schema Validation against multiple schemas using prefixes

A new feature to validate a JSON against multiple schemas has been introduced in the new Nuget 2.0.xxx. This is to enable namespace based validation using prefixes like in XSD.

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

## Splitting JSON into multiple JSON(s) based upon an array token

A JSON file containing an array can now be split into multiple JSON files, each representing a file for every array element.

Two new functions have been added for this purpose:-

`public static IEnumerable<string> SplitJson(string input,string arrayPath)`

`public static IEnumerable<JObject> SplitJson(JObject input, string arrayPath)`

Consider the input:-
``{
  "cars": {
    "Ford": [
      {
        "model": "Taurus",
        "doors": 4
      },
      {
        "model": "Escort",
        "doors": 4
      },
      {
        "model": "Fiesta",
        "doors": 3
      },
      {
        "model": "Bronco",
        "doors": 5
      }
    ],
    "firstName": "John",
    "lastName": "Smith",
  }
}``

Below is a sample code which splits the above input:-

``string input = File.ReadAllText("Input.json");``

``List<string> outputs = JsonTransformer.SplitJson(input, "$.cars.Ford").ToList<string>();``

The output will contain 4 JSON files:-

``{"cars":{"Ford":{"model":"Taurus","doors":4},"firstName":"John","lastName":"Smith"}}``

``{"cars":{"Ford":{"model":"Escort","doors":4},"firstName":"John","lastName":"Smith"}}``

``{"cars":{"Ford":{"model":"Fiesta","doors":3},"firstName":"John","lastName":"Smith"}}``

``{"cars":{"Ford":{"model":"Bronco","doors":5},"firstName":"John","lastName":"Smith"}}``

## Transforming JSON to other data formats

JUST.NET can now transform JSON data into other generic formats too. All functions except the BULK FUNCTIONS are supported in this feature.
The #loop functions excepts an extra argument which defines the seperator between the individual records.

`#loop(path,seaperator)`

If the seperator is not defined, the default seperator used is NEWLINE.

A new class called `DataTransformer` has been introduced for this new feature.

### Example for JSON to XML

Sample code to transform from JSON to XML:-
``string input = File.ReadAllText("Input.json");``
``string transformer = File.ReadAllText("DataTransformer.xml");``
``string transformedString = DataTransformer.Transform(transformer, input);``

Input.json:-
``{
  "menu": {
    "id": {
      "file": "csv"
    },
    "value": {
      "Window": "popup"
    },
    "popup": {
      "menuitem": [
        {
          "value": "New",
          "onclick": {
            "action": "CreateNewDoc()"
          }
        },
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
  },
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
  ],
  "stringref": "thisisandveryunuasualandlongstring",
  "d": [ "one", "two", "three" ],
  "numbers": [ "1", "2", "3", "4", "5" ],
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
}``


DataTransformer.xml:-
``<?xml version="1.0" encoding="UTF-8" ?>
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
</root>``

Output:-
``<?xml version="1.0" encoding="UTF-8" ?>
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
</root>``

### Example for JSON to CSV

Sample code to transform from JSON to CSV:-
``string transformer = File.ReadAllText("Input.json");``
``string transformer = File.ReadAllText("DataTransformer.csv");``
``string transformedString = DataTransformer.Transform(transformer, input);``

The input file is same as the xml example.

DataTransformer.csv:-
``"#loop($.numbers)": {#currentvalue(),#currentindex(),#ifcondition(#currentindex(),#lastindex(),yes,no),#lastvalue(),#valueof($.LogId)}``

Output:-
``1,0,no,5,5000510625
2,1,no,5,5000510625
3,2,no,5,5000510625
4,3,no,5,5000510625
5,4,yes,5,5000510625``
