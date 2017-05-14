# JUST.net
## JUST - JSON Under Simple Transformation (XSLT equivalent for JSON)

XSLT is a very popular and simple way to transform an XML document to another. More and more applications have switches their data formats to JSON as it is more elegant and less bulkier of the two. However, there isn't a simple way to transform a JSON document into another without having to write some code. 

This projects demonstrates the use of **JUST.NET** library which implements **JUST** in .NET.

This library is also written by me can be pulled from the nuget repository and used in your project.

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

