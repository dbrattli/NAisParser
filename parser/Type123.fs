namespace AisParser

open System
open FParsec

module Type123 =
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
        Epfd = "Unknown"
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
        .>>. Core.parseUint2
        |>> fun (x, y) -> { x with Repeat = y }
        .>>. Core.parseUint30
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
        .>>. Common.parseEpfd
        |>> fun (x, y) -> { x with Epfd = y }

        |>> Type123


(*     let parseCommonNavigationBlock': Parser<_> =
        (Common.parseType 1 <|> Common.parseType 2 <|> Common.parseType 3)
        >>= fun _type -> Core.parseUint2
        >>= fun _repeat -> Core.parseUint30
        >>= fun _mssi -> preturn {
            Type = _type
            Repeat = _repeat
            Mssi = _mssi
        }

    parsec {
        let! _type
        let! _repeat =
    } *)

