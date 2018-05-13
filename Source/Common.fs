namespace NAisParser

open System
open FParsec

type NavigationStatus =
    | UnderWayUsingEngine = 0
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

type EpfdFixType =
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

type ManeuverIndicator =
    | NoSpecialManeuver = 0
    | SpecialManeuver = 1

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

type BaseStationReport = {
    Repeat: byte;
    Mmsi: int;
}

type MessageType =
    | Type123 of MessageType123
    | Type4 of MessageType4
    | Type5 of MessageType5

module Common =
    // Allowed characters in payload
    let allowedChars = List.map char [48..119]

    let toPaddedBinary (i: int) =
        // Convert.ToString (i, 2) |> int |> sprintf "%06d"
        let str = Convert.ToString (i, 2)
        let pad = String.replicate (6-str.Length) "0"
        pad + str

    let char2int (chr: char) =
        let value = int chr
        if value > 40 then
            let n = value - 48
            if n > 40 then
                n - 8
            else
                n
        else
            value

    /// Payload handling
    let intListToBinaryString intList =
        let binList = List.map toPaddedBinary intList

        String.concat "" binList

    let parseType (type': string) =
        pstring type'
        |>> fun x -> Convert.ToByte(x, 2)

    let parseType3 (type1: string) (type2: string) (type3: string) =
        pstring type1
        <|> pstring type2
        <|> pstring type3
        |>> fun x -> Convert.ToByte(x, 2)

    let parseRepeat = Core.parseUint2

    let parseMmsi = Core.parseUint30

    let parseLongitude =
        Core.parseBits 28
        |>> fun x -> (String.replicate 5 x.[..0]) + x.[1..]
        |>> fun x -> Convert.ToInt32(x, 2)
        |>> fun x -> float(x) / 600000.0

    let parseLatitude =
        Core.parseBits 27
        |>> fun x -> (String.replicate 6 x.[..0]) + x.[1..]
        |>> fun x -> Convert.ToInt32(x, 2)
        |>> fun x -> float(x) / 600000.0

    let parseEpfd =
        Core.parseBits 4
        |>> (fun x ->
            let value = Convert.ToInt32(x, 2)
            enum<EpfdFixType>(value)
        )

    let parseSpare =
        Core.parseBits 3

    let parseRaimFlag =
        Core.parseBool

    let parseRadioStatus =
        Core.parseBits 19
        |>> fun x -> Convert.ToInt32(x, 2)