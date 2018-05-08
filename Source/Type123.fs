namespace NAisParser

open System
open FParsec

open NAisParser.Core


module Type123 =
    let messageType123 repeat mmsi status turn
        speed accuracy lon lat course heading second maneuver: MessageType123=
        {
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
        }

    let defaultMessageType123: MessageType123= {
        Repeat = 0uy;
        Mmsi = 0;
        Status = NavigationStatus.NotDefined;
        RateOfTurn = 128.0;
        SpeedOverGround = 0;
        PositionAccuracy = 0;
        Longitude = 181.0;
        Latitude = 91.0;
        CourseOverGround = 0.0;
        TrueHeading = 511;
        TimeStamp = 0;
        ManeuverIndicator = ManeuverIndicator.NoSpecialManeuver;
    }

    let parseRateOfTurn =
        let inline squareSigned x = x * x * float(Math.Sign(float x))
        Core.parseSByte
        |>> fun x -> squareSigned((float x) / 4.733)

    let parseSpeedOverGround =
        Core.parseBits 10
        |>> fun x -> Convert.ToInt32(x, 2)

    let parsePositionAccuracy =
        Core.parseBits 1
        |>> fun x -> Convert.ToInt32(x, 2)

    let parseLongitude =
        Core.parseBits 28
        |>> fun x -> Convert.ToInt32(x, 2)
        |>> fun x -> float(x) / 600000.0

    let parseLatitude =
        Core.parseBits 27
        |>> fun x -> Convert.ToInt32(x, 2)
        |>> fun x -> float(x) / 600000.0

    let parseCourseOverGround =
        Core.parseBits 12
        |>> fun x -> Convert.ToInt32(x, 2)
        |>> fun x -> float(x) / 10.0

    let parseStatus =
        Core.parseBits 4
        |>> (fun x ->
            let value = Convert.ToInt32(x, 2)
            enum<NavigationStatus>(value)
        )

    let parseTrueHeading =
        Core.parseBits 9
        |>> fun x -> Convert.ToInt32(x, 2)

    let parseTimeStamp =
        Core.parseBits 6
        |>> fun x -> Convert.ToInt32(x, 2)

    let parseManeuverIndicator =
        Core.parseBits 2
        |>> fun x ->
            let value = Convert.ToInt32(x, 2)
            enum<ManeuverIndicator>(value)

    let parseMessageType123: Parser<_> =
        preturn messageType123
        <*> Common.parseRepeat
        <*> Common.parseMmsi
        <*> parseStatus
        <*> parseRateOfTurn
        <*> parseSpeedOverGround
        <*> parsePositionAccuracy
        <*> parseLongitude
        <*> parseLatitude
        <*> parseCourseOverGround
        <*> parseTrueHeading
        <*> parseTimeStamp
        <*> parseManeuverIndicator
        |>> Type123