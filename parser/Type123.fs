namespace AisParser

open System
open FParsec

open AisParser.Core

module Type123 =
    let commonNavigationBlockResult typ repeat mmsi status turn
        speed accuracy lon lat course : CommonNavigationBlockResult =
        {
            Type = typ;
            Repeat = repeat;
            Mmsi = mmsi;
            Status = status;
            RateOfTurn = turn;
            SpeedOverGround = speed;
            PositionAccuracy = accuracy;
            Longitude = lat;
            Latitude = lon;
            CourseOverGround = course;
        }

    let defaultCommonNavigationBlockResult : CommonNavigationBlockResult = {
        Type = 0uy;
        Repeat = 0uy;
        Mmsi = 0;
        Status = "Not defined";
        RateOfTurn = 0.0;
        SpeedOverGround = 0;
        PositionAccuracy = 0;
        Longitude = 181.0;
        Latitude = 91.0;
        CourseOverGround = 0.0;
    }

    let parseRateOfTurn =
        let square x = x * x
        Core.parseInt8 |>> fun (x) -> square((float) x / 4.733)

    let parseSpeedOverGround =
        Core.parseBits 10
        |>> (fun x -> Convert.ToInt32(x, 2))

    let parsePositionAccuracy =
        Core.parseBits 1
        |>> (fun x -> Convert.ToInt32(x, 2))

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
        |>> fun x -> float(x) / 0.1

    let parseStatus =
        Core.parseBits 4
        |>> (fun x ->
            let value = Convert.ToInt32(x, 2)
            match value with
            | 0 -> "Under way using engine"
            | 1 -> "At anchor"
            | 2 -> "Not under command"
            | 3 -> "Restricted manoeuverability"
            | 4 -> "Constrained by her draught"
            | 5 -> "Moored"
            | 6 -> "Aground"
            | 7 -> "Engaged in Fishing"
            | 8 -> "Under way sailing"
            | 14 -> "AIS-SART is active"
            | _ -> "Not defined"
            )
    let parseCommonNavigationBlock: Parser<_> =
        (Common.parseType 1 <|> Common.parseType 2 <|> Common.parseType 3)
        |>> fun x -> { defaultCommonNavigationBlockResult with Type = x }
        .>>. Common.parseRepeat
        |>> fun (x, y) -> { x with Repeat = y }
        .>>. Common.parseMmsi
        |>> fun (x, y) -> { x with Mmsi = y }
        .>>. parseStatus
        |>> fun (x, y) -> { x with Status = y }
        .>>. parseRateOfTurn
        |>> fun (x, y) -> { x with RateOfTurn = y }
        .>>. parseSpeedOverGround
        |>> fun (x, y) -> { x with SpeedOverGround = y }
        .>>. parsePositionAccuracy
        |>> fun (x, y) -> { x with PositionAccuracy = y }
        .>>. parseLongitude
        |>> fun (x, y) -> { x with Longitude = y }
        .>>. parseLatitude
        |>> fun (x, y) -> { x with Latitude = y }
        .>>. parseCourseOverGround
        |>> fun (x, y) -> { x with CourseOverGround = y }

    let parseCommonNavigationBlock'': Parser<_> =
        preturn commonNavigationBlockResult
        <*> (Common.parseType 1 <|> Common.parseType 2 <|> Common.parseType 3)
        <*> Common.parseRepeat
        <*> Common.parseMmsi
        <*> parseStatus
        <*> parseRateOfTurn
        <*> parseSpeedOverGround
        <*> parsePositionAccuracy
        <*> parseLongitude
        <*> parseLatitude
        <*> parseCourseOverGround
        |>> Type123