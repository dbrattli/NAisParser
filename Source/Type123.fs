namespace NAisParser

open System
open FParsec

open NAisParser.Core

// Common Navigation Block
type MessageType123 = {
    Type: byte;
    Repeat: byte;
    Mmsi: int;
    Status: NavigationStatus;
    RateOfTurn: float;
    SpeedOverGround: int;
    PositionAccuracy: bool;
    Longitude: float;
    Latitude: float;
    CourseOverGround: float;
    TrueHeading: int;
    TimeStamp: int;
    ManeuverIndicator: ManeuverIndicator;
    RaimFlag: bool;
    RadioStatus: int
}

module Type123 =

    let messageType123 type' repeat mmsi status turn speed accuracy lon
        lat course heading second maneuver raim radio: MessageType123=
        {
            Type = type'
            Repeat = repeat;
            Mmsi = mmsi;
            Status = status;
            RateOfTurn = turn;
            SpeedOverGround = speed;
            PositionAccuracy = accuracy;
            Longitude = lon;
            Latitude = lat;
            CourseOverGround = course;
            TrueHeading = heading;
            TimeStamp = second;
            ManeuverIndicator = maneuver;
            RaimFlag = raim;
            RadioStatus = radio;
        }

    let defaultMessageType123: MessageType123= {
        Type = 0uy;
        Repeat = 0uy;
        Mmsi = 0;
        Status = NavigationStatus.NotDefined;
        RateOfTurn = 128.0;
        SpeedOverGround = 0;
        PositionAccuracy = false;
        Longitude = 181.0;
        Latitude = 91.0;
        CourseOverGround = 0.0;
        TrueHeading = 511;
        TimeStamp = 0;
        ManeuverIndicator = ManeuverIndicator.NoSpecialManeuver;
        RaimFlag = false;
        RadioStatus = 0;
    }

    let parseRateOfTurn =
        let squareSigned x = x * x * float(Math.Sign(float x))
        Core.parseSByte
        |>> fun x -> squareSigned((float x) / 4.733)

    let parseSpeedOverGround = Core.parseIntN 10

    let parsePositionAccuracy = Core.parseBool

    let parseCourseOverGround =
        Core.parseIntN 12
        |>> fun x -> float(x) / 10.0

    let parseStatus =
        Core.parseIntN 4
        |>> fun x -> enum<NavigationStatus>(x)

    let parseTrueHeading = Core.parseIntN 9

    let parseTimeStamp = Core.parseIntN 6

    let parseManeuverIndicator =
        Core.parseIntN 2
        |>> fun x -> enum<ManeuverIndicator>(x)

    let parseType123  : Parser<_> =
        // A little repetitive, but better to do it here at declaration time
        Common.parseType3 (Common.toPaddedBinary 1) (Common.toPaddedBinary 2) (Common.toPaddedBinary  3)

    let parseMessageType123: Parser<_> =
        preturn messageType123
        <*> parseType123
        <*> Common.parseRepeat
        <*> Common.parseMmsi
        <*> parseStatus
        <*> parseRateOfTurn
        <*> parseSpeedOverGround
        <*> parsePositionAccuracy
        <*> Common.parseLongitude
        <*> Common.parseLatitude
        <*> parseCourseOverGround
        <*> parseTrueHeading
        <*> parseTimeStamp
        <*> parseManeuverIndicator
        <*  Common.parseSpare
        <*> Common.parseRaimFlag
        <*> Common.parseRadioStatus