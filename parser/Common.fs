namespace AisParser

open System
open FParsec

type CommonNavigationBlockResult = {
    Type: int;
    Repeat: int;
    Mmsi: int;
    Status: string;
    RateOfTurn: float;
    SpeedOverGround: int;
    PositionAccuracy: int;
    Longitude: float;
    Latitude: float;
    Epfd: string;
}

type StaticAndVoyageRelatedData = {
    Type: int;
    Repeat: int;
    Mmsi: int;
    Version: int;
    ImoNumber: int;
    CallSign: string;
    VesselName: string;
    ShipType: string;
    ToBow: int;
    ToPort: int;
    ToStarBoard: int;
    Epfd: string;
    Month: int;
    Day: int;
    Hour: int;
    Minute: int;
    Draught: int;
    Destination: string;
    Dte: bool;
}

type MessageType =
| Type123 of CommonNavigationBlockResult
| Type5 of StaticAndVoyageRelatedData

module Common =
    let parseType (num:int) =
        let bitPattern = Convert.ToString (num, 2) |> int
        sprintf "%06d" bitPattern |> pstring
        |>> (fun x -> Convert.ToInt32(x, 2)) // Map back to int

    let parseEpfd =
        Core.parseBits 4
        |>> (fun x ->
            let value = Convert.ToInt32(x, 2)
            match value with
            | 0 -> "Undefined"
            | 1 -> "GPS"
            | 2 -> "GLONASS"
            | 3 -> "Combined GPS/GLONASS"
            | 4 -> "Loran-C"
            | 5 -> "Chayka"
            | 6 -> "Integrated navigation system"
            | 7 -> "Surveyed"
            | 8 -> "Galileo"
            | _ -> "Unknown"
            )