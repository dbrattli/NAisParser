namespace NAisParser

open System
open FParsec

open NAisParser.Core

module Type5 =
    let messageType5 repeat mmsi version imo
        callsign shipname shiptype tobow tostern toport tostarboard epfd
        month day hour minute draught destination dte
        : MessageType5 =
        {
            Repeat = repeat;
            Mmsi = mmsi;
            Version = version;
            ImoNumber = imo;
            CallSign = callsign;
            VesselName = shipname;
            ShipType = shiptype;
            ToBow = tobow;
            ToStern = tostern;
            ToPort = toport;
            ToStarBoard = tostarboard;
            Epfd = epfd;
            Month = month;
            Day = day;
            Hour = hour;
            Minute = minute;
            Draught = draught;
            Destination = destination;
            Dte =  dte;
        }
    let defaultMessageType5: MessageType5 = {
        Repeat = 0uy;
        Mmsi = 0;
        Version = 0uy;
        ImoNumber = 0;
        CallSign = "";
        VesselName = "";
        ShipType = ShipType.NotAvailable;
        ToBow = 0;
        ToStern = 0;
        ToPort = 0;
        ToStarBoard = 0;
        Epfd = EpdfFixType.Undefined;
        Month = 0;
        Day = 0;
        Hour = 24;
        Minute = 60;
        Draught = 0.0;
        Destination = "";
        Dte =  false;
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
            enum<ShipType>(value)

    let parseCallSign = Core.parseAscii 42

    let parseVesselName = Core.parseAscii 120

    let parseToBow =
        Core.parseBits 9
        |>> fun x -> Convert.ToInt32(x, 2)

    let parseToStern = parseToBow

    let parseToPort =
        Core.parseBits 6
        |>> fun x -> Convert.ToInt32(x, 2)

    let parseToStarboard = parseToPort

    let parseEpdf =
        Core.parseBits 4
        |>> (fun x ->
            let value = Convert.ToInt32(x, 2)
            enum<EpdfFixType>(value)
        )

    let parseMonth =
        Core.parseBits 4
        |>> fun x -> Convert.ToInt32(x, 2)

    let parseDay =
        Core.parseBits 5
        |>> fun x -> Convert.ToInt32(x, 2)

    let parseHour = parseDay

    let parseMinute =
        Core.parseBits 6
        |>> fun x -> Convert.ToInt32(x, 2)

    let parseDraught =
        Core.parseBits 8
        |>> fun x -> (Convert.ToInt32(x, 2) |> float) / 10.0

    let parseDestination = Core.parseAscii 120

    let parseDte = Core.parseBits 1 |>> fun x -> Convert.ToByte(x, 2) = 1uy

    let parseMessageType5: Parser<MessageType> =
        preturn messageType5
        <*> Common.parseRepeat
        <*> Common.parseMmsi
        <*> parseVersion
        <*> parseImoNumber
        <*> parseCallSign
        <*> parseVesselName
        <*> parseShipType
        <*> parseToBow
        <*> parseToStern
        <*> parseToPort
        <*> parseToStarboard
        <*> parseEpdf
        <*> parseMonth
        <*> parseDay
        <*> parseHour
        <*> parseMinute
        <*> parseDraught
        <*> parseDestination
        <*> parseDte

        |>> Type5
