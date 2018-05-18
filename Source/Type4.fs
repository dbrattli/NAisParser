namespace NAisParser

open FParsec

open NAisParser.Core

// Base Station Report
type MessageType4 = {
    Repeat: byte;
    Mmsi: int;
    Year: int;
    Month: int;
    Day: int;
    Hour: int;
    Minute: int;
    Second: int;
    FixQuality: bool;
    Longitude: float;
    Latitude: float;
    Epfd: EpfdFixType;
    RaimFlag: bool;
    RadioStatus: int
}

module Type4 =
    let messageType4 repeat mmsi year month day hour minute second
        accuracy lat lon epfd raim radio
        : MessageType4 =
        {
            Repeat = repeat;
            Mmsi = mmsi;
            Year = year;
            Month = month;
            Day = day;
            Hour = hour;
            Minute = minute;
            Second = second;
            FixQuality = accuracy;
            Longitude = lon;
            Latitude = lat;
            Epfd = epfd
            RaimFlag = raim;
            RadioStatus = radio;
        }
    let defaultMessageType4: MessageType4 = {
        Repeat = 0uy;
        Mmsi = 0;
        Year = 0;
        Month = 0;
        Day = 0;
        Hour = 24;
        Minute = 60;
        Second = 60;
        FixQuality = false;
        Longitude = 181.0;
        Latitude = 91.0;
        Epfd = EpfdFixType.Undefined;
        RaimFlag = false;
        RadioStatus = 0
        }

    let parseYear = Core.parseIntN 14

    let parseMonth = Core.parseIntN 4

    let parseDay = Core.parseIntN 5

    let parseHour = Core.parseIntN 5

    let parseMinute = Core.parseIntN 6

    let parseSecond = Core.parseIntN 6

    let parseFixQuality = Core.parseBool

    let parseType4 = Common.parseType <| Common.toPaddedBinary 4

    let parseMessageType4: Parser<_> =
        preturn messageType4
         *> parseType4
        <*> Common.parseRepeat
        <*> Common.parseMmsi
        <*> parseYear
        <*> parseMonth
        <*> parseDay
        <*> parseHour
        <*> parseMinute
        <*> parseSecond
        <*> parseFixQuality
        <*> Common.parseLongitude
        <*> Common.parseLatitude
        <*> Common.parseEpfd
        <*  Common.parseSpare
        <*> Common.parseRaimFlag
        <*> Common.parseRadioStatus