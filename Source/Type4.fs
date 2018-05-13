namespace NAisParser

open System
open FParsec

open NAisParser.Core

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

    let parseYear =
        Core.parseBits 14
        |>> fun x -> Convert.ToInt32(x, 2)

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

    let parseSecond = parseMinute

    let parseFixQuality = Core.parseBool

    let parseType4 = Common.parseType <| Common.toPaddedBinary 4

    let parseMessageType4: Parser<MessageType> =
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

        |>> Type4
