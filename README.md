# AIS AIVDM/AIVDO Parser for .NET #

![coverage](https://gitlab.com/dbrattli/AisParser/badges/master/coverage.svg)

An NMEA AIS [AIVDM/AIVDO](http://catb.org/gpsd/AIVDM.html) parser for .NET Core. Written in F# using FParsec. The advantage of using a parser combinator library is that the implementation looks very similar to the specification. Thus the code is clean and  easyer to validate against the specification.

Currently parses:

* Types 1, 2 and 3: Position Report Class A
* Type 4: Base Station Report
* Type 5: Static and Voyage Related Data

Have fun!

## Install ##

## Use API C# ##

```c#
var parser = new Parser();

using (StreamReader reader = new StreamReader(stream))
{
    string line;

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
```

## Use API F# ##