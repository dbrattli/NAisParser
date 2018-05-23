namespace NAisParser

open FParsec

open NAisParser.Core

// Static And Voyage Related Data
type MessageType5 = {
    Repeat: byte;
    Mmsi: int;
    Version: byte;
    ImoNumber: int;
    CallSign: string;
    VesselName: string;
    ShipType: ShipType;
    ToBow: int;
    ToStern: int;
    ToPort: int;
    ToStarBoard: int;
    Epfd: EpfdFixType;
    Month: int;
    Day: int;
    Hour: int;
    Minute: int;
    Draught: float;
    Destination: string;
    Dte: bool;
}

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
        Epfd = EpfdFixType.Undefined;
        Month = 0;
        Day = 0;
        Hour = 24;
        Minute = 60;
        Draught = 0.0;
        Destination = "";
        Dte =  false;
    }

    let parseVersion = Core.parseByteN 2

    let parseImoNumber = Core.parseIntN 30

    let parseShipType =
        Core.parseIntN 8
        |>> fun x -> enum<ShipType>(x)

    let parseCallSign = Core.parseAscii 42

    let parseVesselName = Core.parseAscii 120

    let parseToBow = Core.parseIntN 9

    let parseToStern = parseToBow

    let parseToPort = Core.parseIntN 6

    let parseToStarboard = parseToPort

    let parseMonth = Core.parseIntN 4

    let parseDay = Core.parseIntN 5

    let parseHour = Core.parseIntN 5

    let parseMinute = Core.parseIntN 6

    let parseDraught =
        Core.parseIntN 8
        |>> fun x -> float x / 10.0

    let parseDestination = Core.parseAscii 120

    let parseDte = Core.parseBool

    let parseType5 = Common.parseType <| Common.toPaddedBinary 5

    let parseMessageType5: Parser<_> =
        preturn messageType5
         *> parseType5
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
        <*> Common.parseEpfd
        <*> parseMonth
        <*> parseDay
        <*> parseHour
        <*> parseMinute
        <*> parseDraught
        <*> parseDestination
        <*> parseDte
