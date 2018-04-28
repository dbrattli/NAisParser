namespace AisParser

open System
open FParsec
open AisParser.Core

module Type5 =
    let staticAndVoyageRelatedData type' repeat mmsi version imo callsign
        shipname shiptype
        // tobow tostern toport tostarboard epfd month
        //day hour minute draught destination dte
        : StaticAndVoyageRelatedData =
        {
            Type = type';
            Repeat = repeat;
            Mmsi = mmsi;
            Version = version;
            ImoNumber = imo;
            CallSign = callsign;
            VesselName = shipname;
            ShipType = shiptype;
            (*
            ToBow = tobow;
            ToStern =tostern;
            ToPort = toport;
            ToStarBoard = tostarboard;
            Epfd = epfd;
            Month = month;
            Day = day;
            Hour = hour;
            Minute = minute;
            Draught = draught;
            Destination = destination;
            Dte=  dte;*)
        }
    let defaultStaticAndVoyageRelatedData : StaticAndVoyageRelatedData = {
        Type = 0uy;
        Repeat = 0uy;
        Mmsi = 0;
        Version = 0uy;
        ImoNumber = 0;
        CallSign = "";
        VesselName = "";
        ShipType = ShipType.NotAvailable;
        (*
        ToBow = 0;
        ToStern = 0;
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
        *)
    }

    let parseVersion =
        Core.parseBits 2
        |>> fun x -> Convert.ToByte(x, 2)

    let parseImoNumber =
        Core.parseBits 30
        |>> fun x -> Convert.ToInt32(x, 2)

    let parseShipType =
        Core.parseBits 8
        |>> fun x ->
            let value = Convert.ToInt32(x, 2)
            printfn "Result: %d" value
            enum<ShipType>(value)

    let parseCallSign = Core.parseAscii 42

    let parseVesselName = Core.parseAscii 120

    let parseStaticAndVoyageRelatedData: Parser<MessageType> =
        preturn staticAndVoyageRelatedData
        <*> Common.parseType 5
        <*> Common.parseRepeat
        <*> Common.parseMmsi
        <*> parseVersion
        <*> parseImoNumber
        <*> parseCallSign
        <*> parseVesselName
        <*> parseShipType

        |>> Type5
