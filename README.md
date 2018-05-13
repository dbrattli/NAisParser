# NAisParser is an AIVDM/AIVDO Parser for .NET #

![coverage](https://gitlab.com/dbrattli/AisParser/badges/master/coverage.svg)

[NAisParser](https://gitlab.com/dbrattli/NAisParser) is a marine Automatic Identification
System (AIS) [AIVDM/AIVDO](http://catb.org/gpsd/AIVDM.html) NMEA message decoder for .NET Standard.

The library is Written in F# using [FParsec](http://www.quanttec.com/fparsec/). The main advantage of
using a parser combinator library such as FParsec, and using an applicative (functor) style is that
the implementation looks very similar to the specification, thus the code is clean, easy to extend
and validate against the specification.

Currently parses:

* Types 1, 2 and 3: Position Report Class A
* Type 4: Base Station Report
* Type 5: Static and Voyage Related Data

The parser works in two stages. In the first stage you parse the outer layer of the AIS data packet:

```c#
var line = "!BSVDM,1,1,,A,13mAwp001m0MMrjSoomG6mWT0<1h,0*16";
var result = parser.TryParse(line, out AisResult aisResult); // Returns true
```

If result is `true` then you have a valid `AisResult` such as:

```txt
{Vdm = BS;
 Count = 1uy;
 Number = 1uy;
 Seq = None;
 Channel = A;
 Payload =
  [3; 53; 17; 63; 56; 0; 0; 1; 53; 0; 29; 29; 58; 50; 35; 55; 55; 53; 23; 6; 53;
   39; 36; 0; 12; 1; 48];
 Type = 1uy;}
```

If `false` then the packet if fragmented and you will need to supply more data
by calling `TryParse` again with another line of data, e.g.

```c#
var line1 = "!BSVDM,2,1,2,A,53mDDD02>EjthmLJ220HtppE>2222222222222164@G:34rdR?QSkSQDp888,0*15";
var line2 = "!BSVDM,2,2,2,A,88888888880,2*3F";

var result1 = parser.TryParse(line1, out AisResult aisResult); // Returns false
var result2 = parser.TryParse(line2, out AisResult aisResult); // Returns true
```

If an error occurs while parsing `TryParse` will raise an `ArgumentException`.

In the second stage you parse the AIS payload data depending on the type of message. The type of
message is available in the `Type` property of a valid `AisResult` from stage 1. To parse a message
of type 1 you call `TryParse` with an `out` parameter of type `MessageType123`. To parse a message
of type 5 you call `TryParse` with an `out` parameter of type `MessageType5`.

```c#
var result = parser.TryParse(aisResult, out MessageType123 type123Result);
```

If result is `true` then you have a valid `MessageType123` such as:

```txt
{Repeat = 0uy;
 Mmsi = 257196000;
 Status = UnderWayUsingEngine;
 RateOfTurn = 0.0;
 SpeedOverGround = 117;
 PositionAccuracy = 0;
 Longitude = 62.69262167;
 Latitude = 6.437268333;
 CourseOverGround = 181.9;
 TrueHeading = 179;
 TimeStamp = 50;
 ManeuverIndicator = NoSpecialManeuver;}
```

Se below for a full example. Enjoy!

## Dependencies ##

* .NETStandard 1.6
* FParsec (>= 1.0.3)
* FSharp.Core (>= 4.3.4)

## Install ##

NAisParser is available as a [NuGet package](https://www.nuget.org/packages/NAisParser/).

```cli
dotnet add package NAisParser
```

## C# Example ##

```c#
using NAisParser;

...

var parser = new Parser();

using (StreamReader reader = new StreamReader(stream))
{
    string line;
    while ((line = reader.ReadLine()) != null)
    {
        var result = parser.TryParse(line, out AisResult aisResult);
        if (!result) continue;

        switch (aisResult.Type)
        {
            case 1:
            case 2:
            case 3:
                result = parser.TryParse(aisResult, out MessageType123 type123Result);
                Console.WriteLine(type123Result.ToString());
                break;
            case 5:
                result = parser.TryParse(aisResult, out MessageType5 type5Result);
                Console.WriteLine(type5Result.ToString());
                break;
            default:
                break;
        }
    }
}
```