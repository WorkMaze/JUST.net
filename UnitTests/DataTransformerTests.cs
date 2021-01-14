﻿using NUnit.Framework;

namespace JUST.UnitTests
{
    [TestFixture, Category("DataTransformer")]
    public class DataTransformerTests
    {
        [Test]
        public void XmlTest()
        {
            var input = "<?xml version=\"1.0\" encoding=\"UTF-8\" ?> <root> <root> <ifconditiontesttrue>#ifcondition(#valueof($.menu.id.file),csv,#valueof($.menu.value.Window),fail)</ifconditiontesttrue> <ifconditiontestfalse>#ifcondition(#valueof($.menu.id.file),xml,#valueof($.menu.value.Window),fail)</ifconditiontestfalse> <stringresult> <lastindexofand>#lastindexof(#valueof($.stringref),and)</lastindexofand> <firstindexofand>#firstindexof(#valueof($.stringref),and)</firstindexofand> <subsrting>#substring(#valueof($.stringref),8,10)</subsrting> <concat>#concat(#valueof($.menu.id.file),#valueof($.menu.value.Window))</concat> </stringresult> <mathresult> <add>#add(#valueof($.numbers[0]),3)</add> <subtract>#subtract(#valueof($.numbers[4]),#valueof($.numbers[0]))</subtract> <multiply>#multiply(2,#valueof($.numbers[2]))</multiply> <divide>#divide(9,3)</divide> </mathresult> <conacted>#concatall(#valueof($.d))</conacted> <sum>#sum(#valueof($.numbers))</sum> <avg>#average(#valueof($.numbers))</avg> <min>#min(#valueof($.numbers))</min> <max>#max(#valueof($.numbers))</max> <arrayconacted>#concatallatpath(#valueof($.x),$.v.a)</arrayconacted> <arraysum>#sumatpath(#valueof($.x),$.v.c)</arraysum> <arrayavg>#averageatpath(#valueof($.x),$.v.c)</arrayavg> <arraymin>#minatpath(#valueof($.x),$.v.b)</arraymin> <arraymax>#maxatpath(#valueof($.x),$.v.b)</arraymax> </root> <FullName>#concat(#concat(#concat(#valueof($.Name), ),#concat(#valueof($.MiddleName), )),#valueof($.Surname))</FullName> <Contact_Information> <City>#substring(#valueof($.ContactInformation),#add(#firstindexof(#valueof($.ContactInformation),:),1),#subtract(#subtract(#lastindexof(#valueof($.ContactInformation),:),#firstindexof(#valueof($.ContactInformation),:)),1))</City> <PhoneNumber>#substring(#valueof($.ContactInformation),#add(#lastindexof(#valueof($.ContactInformation),:),1),#subtract(#lastindexof(#valueof($.ContactInformation),),#lastindexof(#valueof($.ContactInformation),:)))</PhoneNumber> <Street_Name>#substring(#valueof($.ContactInformation),0,#firstindexof(#valueof($.ContactInformation),:))</Street_Name> </Contact_Information> <Personal_Information> <Age>#substring(#valueof($.PersonalInformation),0,#firstindexof(#valueof($.PersonalInformation),:))</Age> <Ethnicity>#substring(#valueof($.PersonalInformation),#add(#lastindexof(#valueof($.PersonalInformation),:),1),#subtract(#lastindexof(#valueof($.PersonalInformation),),#lastindexof(#valueof($.PersonalInformation),:)))</Ethnicity> <LogId>#valueof($.LogId)</LogId> <Civil_Status>#substring(#valueof($.PersonalInformation),#add(#firstindexof(#valueof($.PersonalInformation),:),1),#subtract(#subtract(#lastindexof(#valueof($.PersonalInformation),:),#firstindexof(#valueof($.PersonalInformation),:)),1))</Civil_Status> </Personal_Information> <Custom>#customfunction(ExternalMethods,SeasonsHelper.Season.findseason,#valueof($.tree.branch.leaf),#valueof($.tree.branch.flower))</Custom> <iteration>\"#loop($.numbers,<!--Record ends here-->)\": { <Record> <CurrentValue>#currentvalue()</CurrentValue> <CurrentIndex>#currentindex()</CurrentIndex> <IsLast>#ifcondition(#currentindex(),#lastindex(),yes,no)</IsLast> <LastValue>#lastvalue()</LastValue> <SomeValue>#valueof($.LogId)</SomeValue> </Record>} </iteration> <IterateObj> \"#loop($.x,<!-- loop ends -->)\": {<Record> <CurrentValueAtPath>#currentvalueatpath($.v.a)</CurrentValueAtPath> </Record>} </IterateObj> </root>";
            var result = DataTransformer.Transform(input, ExampleInputs.XmlInput);
            Assert.AreEqual("<?xml version=\"1.0\" encoding=\"UTF-8\" ?> <root> <root> <ifconditiontesttrue>popup</ifconditiontesttrue> <ifconditiontestfalse>fail</ifconditiontestfalse> <stringresult> <lastindexofand>21</lastindexofand> <firstindexofand>6</firstindexofand> <subsrting>dveryunuas</subsrting> <concat>csvpopup</concat> </stringresult> <mathresult> <add>4</add> <subtract>4</subtract> <multiply>6</multiply> <divide>3</divide> </mathresult> <conacted>onetwothree</conacted> <sum>15</sum> <avg>3</avg> <min>1</min> <max>5</max> <arrayconacted>a1,a2,a3b1,b2c1,c2,c3</arrayconacted> <arraysum>60</arraysum> <arrayavg>20</arrayavg> <arraymin>1</arraymin> <arraymax>3</arraymax> </root> <FullName>Kari Inger Nordmann</FullName> <Contact_Information> <City>Oslo</City> <PhoneNumber>88880000</PhoneNumber> <Street_Name>Karl johans gate</Street_Name> </Contact_Information> <Personal_Information> <Age>45</Age> <Ethnicity>Norwegian</Ethnicity> <LogId>5000510625</LogId> <Civil_Status>Married</Civil_Status> </Personal_Information> <Custom>summer</Custom> <iteration> <Record> <CurrentValue>1</CurrentValue> <CurrentIndex>0</CurrentIndex> <IsLast>no</IsLast> <LastValue>5</LastValue> <SomeValue>5000510625</SomeValue> </Record><!--Record ends here--> <Record> <CurrentValue>2</CurrentValue> <CurrentIndex>1</CurrentIndex> <IsLast>no</IsLast> <LastValue>5</LastValue> <SomeValue>5000510625</SomeValue> </Record><!--Record ends here--> <Record> <CurrentValue>3</CurrentValue> <CurrentIndex>2</CurrentIndex> <IsLast>no</IsLast> <LastValue>5</LastValue> <SomeValue>5000510625</SomeValue> </Record><!--Record ends here--> <Record> <CurrentValue>4</CurrentValue> <CurrentIndex>3</CurrentIndex> <IsLast>no</IsLast> <LastValue>5</LastValue> <SomeValue>5000510625</SomeValue> </Record><!--Record ends here--> <Record> <CurrentValue>5</CurrentValue> <CurrentIndex>4</CurrentIndex> <IsLast>yes</IsLast> <LastValue>5</LastValue> <SomeValue>5000510625</SomeValue> </Record><!--Record ends here--> </iteration> <IterateObj> <Record> <CurrentValueAtPath>a1,a2,a3</CurrentValueAtPath> </Record><!-- loop ends --><Record> <CurrentValueAtPath>b1,b2</CurrentValueAtPath> </Record><!-- loop ends --><Record> <CurrentValueAtPath>c1,c2,c3</CurrentValueAtPath> </Record><!-- loop ends --> </IterateObj> </root>", result);
        }

        [Test]
        public void CsvTest()
        {
            var input = "\"#loop($.numbers)\": {#currentvalue(),#currentindex(),#ifcondition(#currentindex(),#lastindex(),yes,no),#lastvalue(),#valueof($.LogId)}";
            var result = DataTransformer.Transform(input, ExampleInputs.XmlInput);
            Assert.AreEqual("1,0,no,5,5000510625\r\n2,1,no,5,5000510625\r\n3,2,no,5,5000510625\r\n4,3,no,5,5000510625\r\n5,4,yes,5,5000510625\r\n", result);
        }
    }
}
