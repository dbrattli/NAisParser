namespace AisParser

open System
open FParsec

type NavigationStatus =
    | UnderWayUsingEngine=0
    | AtAnchor = 1
    | NotUnderCommand = 2
    | RestrictedManoeuverability = 3
    | ConstrainedByHerDraught = 4
    | Moored = 5
    | Aground = 6
    | EngagedInFishing = 7
    | UnderWaySailing = 8
    | AisSartIsActive = 14
    | NotDefined  = 15

type EpdfFixType =
    | Undefined = 0
    | Gps = 1
    | Glonass = 2
    | CombinedGpsGlonass = 3
    | LoranC = 4
    | Chayka = 5
    | IntegratedNavigationSystem = 6
    | Surveyed = 7
    | Galileo = 8

type ShipType =
    | NotAvailable = 0
    | WingInGround = 20
    | WingInGroundHazardousCatagoryA = 20
    | WingInGroundHazardousCatagoryB = 21
    | WingInGroundHazardousCatagoryC = 22
    | WingInGroundHazardousCatagoryD = 23
    | Fishing = 30
    | Towing = 31
    | TowingLarge = 32
    | Dredging = 33
    | DivingOps = 34
    | MilitaryOps = 35
    | Sailing = 36
    | PleasureCraft = 37
    | HighSpeedCraft = 40
    | HighSpeedCraftHazardousCatagoryA = 40
    | HighSpeedCraftHazardousCatagoryB = 41
    | HighSpeedCraftHazardousCatagoryC = 42
    | HighSpeedCraftHazardousCatagoryD = 43
    | PilotVessel = 50
    | SearchAndRescueVessel = 51
    | Tug = 52
    | PortTender = 53
    | MedicalTransport = 58
    | Passenger = 60
    | PassengerHazardousCatagoryA = 61
    | PassengerHazardousCatagoryB = 62
    | PassengerHazardousCatagoryC = 63
    | PassengerHazardousCatagoryD = 64
    | Cargo = 70
    | CargoHazardousCatagoryA = 71
    | CargoHazardousCatagoryB = 72
    | CargoHazardousCatagoryC = 73
    | CargoHazardousCatagoryD = 74
    | Tanker = 80
    | TankerHazardousCatagoryA = 81
    | TankerHazardousCatagoryB = 82
    | TankerHazardousCatagoryC = 83
    | TankerHazardousCatagoryD = 84


type CommonNavigationBlockResult = {
    Type: byte;
    Repeat: byte;
    Mmsi: int;
    Status: NavigationStatus;
    RateOfTurn: float;
    SpeedOverGround: int;
    PositionAccuracy: int;
    Longitude: float;
    Latitude: float;
    CourseOverGround: float;
}

type StaticAndVoyageRelatedData = {
    Type: byte;
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
    Epfd: EpdfFixType;
    Month: int;
    Day: int;
    Hour: int;
    Minute: int;
    Draught: int;
    Destination: string;
    Dte: bool;
}

type BaseStationReport = {
    Type: byte;
    Repeat: byte;
    Mmsi: int;
}

type MessageType =
    | Type123 of CommonNavigationBlockResult
    | Type5 of StaticAndVoyageRelatedData

module Common =
    let parseType (num:int) =
        let bitPattern = Convert.ToString (num, 2) |> int
        sprintf "%06d" bitPattern |> pstring
        |>> fun x -> Convert.ToByte(x, 2) // Map back to byte

    let parseRepeat = Core.parseUint2

    let parseMmsi = Core.parseUint30

    let parseEpfd =
        Core.parseBits 4
        |>> (fun x ->
            let value = Convert.ToInt32(x, 2)
            enum<EpdfFixType>(value)
        )
