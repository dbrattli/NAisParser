namespace AisParser

open System
open FParsec

module Type5 =
    let defaultStaticAndVoyageRelatedData : StaticAndVoyageRelatedData = {
        Type = 0uy;
        Repeat = 0uy;
        Mmsi = 0;
        Version = 0uy;
        ImoNumber = 0;
        CallSign = "";
        VesselName = "";
        ShipType = "";
        ToBow = 0;
        ToPort = 0;
        ToStarBoard = 0;
        Epfd = "";
        Month = 0;
        Day = 0;
        Hour = 0;
        Minute = 0;
        Draught = 0;
        Destination = "";
        Dte=  false;
    }

    let parseStaticAndVoyageRelatedData: Parser<MessageType> =
        Common.parseType 5
        |>> (fun (x) -> { defaultStaticAndVoyageRelatedData with Type = x })
        .>>. Core.parseUint2
        |>> (fun (x, y) -> { x with Repeat = y })
        .>>. Core.parseUint30
        |>> (fun (x, y) -> { x with Mmsi = y })
        .>>. Core.parseUint2
        |>> (fun (x, y) -> { x with Version = y })
        .>>. Core.parseUint30
        |>> (fun (x, y) -> { x with ImoNumber = y })
        .>>. Core.parseAscii 30
        |>> (fun (x, y) -> { x with CallSign = y })
        .>>. Core.parseAscii 120
        |>> (fun (x, y) -> { x with VesselName = y })

        |>> (fun (x) -> (Type5) x)
