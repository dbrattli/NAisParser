namespace AisParser

open System
open FParsec

type UserState = unit // doesn't have to be unit, of course
type Parser<'t> = Parser<'t, UserState>

type AisResult = {
    Vdm: string;
    Number: uint8;
    Count: uint8;
    Seq: uint8 option;
    Channel: char;
    Payload: string;
}

type CommonNavigationBlockResult = {
    Type: int;
    Repeat: int;
    Mmsi: int;
    Status: string;
    RateOfTurn: int;
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

module Ais =
    let defaultCommonNavigationBlockResult : CommonNavigationBlockResult = {
        Type = 0;
        Repeat = 0;
        Mmsi = 0;
        Status = "Not defined";
        RateOfTurn = 0;
        SpeedOverGround = 0;
        PositionAccuracy = 0;
        Longitude = 181.0;
        Latitude = 91.0;
        Epfd = "Unknown"
    }

    let defaultStaticAndVoyageRelatedData : StaticAndVoyageRelatedData = {
        Type = 0;
        Repeat = 0;
        Mmsi = 0;
        Version = 0;
        ImoNumber = 0;
        CallSign = "";
        VesselName = "";
        ShipType = "";
        ToBow = 0;
        ToPort = 0;
        ToStarBoard = 0;
        Epfd = "";
        Month = 0;
        Day = 0;
        Hour = 0;
        Minute = 0;
        Draught = 0;
        Destination = "";
        Dte=  false;
    }

    let defaultAisResult : AisResult = {
        Vdm = "";
        Number = 0uy;
        Count = 0uy;
        Seq = Some 0uy;
        Channel = '0';
        Payload = "";
    }

    let isSuccess result =
        match result with
        | Success _ -> true
        | _ -> false

    let defragment (prev: ParserResult<AisResult, unit>) (curr : ParserResult<AisResult, unit>) =
        let result =
            match curr with
            | Success (c, state, pos) ->
                match prev with
                | Success (p, _, _) ->
                    if (c.Count > 1uy) && (c.Number > 1uy) then
                        let payload = p.Payload + c.Payload
                        Success ({ c with Payload = payload }, state, pos)
                    else
                        curr
                | Failure (a, b, c) -> Failure(a, b, c)
            | Failure (a, b, c) -> Failure(a, b, c)
        result

    let vdms =
            pstring "!BS" // Base AIS station
        <|> pstring "!AI" // Mobile AIS station

    let parseVdm : Parser<_> =
        vdms .>>. pstring "VDM"
        |>> (fun (x, y) -> x + y) // Concat e.g "!AI" with "VDM"
    let comma : Parser<_> = pchar ','
    let parseCount : Parser<_> = comma >>. puint8
    let parseNumber : Parser<_> = comma >>. puint8
    let parseSeq : Parser<_> = comma >>. opt puint8
    let parseChannel : Parser<_> = comma >>. anyOf ['A';'B';'0';'1']

    let parsePadBits = comma >>. puint8
    // define the function
    let toPaddedBinary (i: int) =
        Convert.ToString (i, 2) |> int |> sprintf "%06d"

    /// Payload handling
    let charListToBinaryString charList =
        let convert chr =
            let value = int chr
            if value > 40 then
                let n = value - 48
                if n > 40 then
                    n - 8
                else
                    n
            else
                value

        let binList =
            List.map (convert >> toPaddedBinary) charList

        String.concat "" binList

    let parseBits count : Parser<_> =
        manyMinMaxSatisfy count count isDigit

    let inline isBit (c: char) =
        uint32 c - uint32 '0' <= uint32 '1' - uint32 '0'

    let parseTypeNumber num: Parser<_> =
        pstring (sprintf "%02d" num)

    let parseType (num:int) =
        let bitPattern = Convert.ToString (num, 2) |> int
        sprintf "%06d" bitPattern |> pstring
        |>> (fun x -> Convert.ToInt32(x, 2)) // Map back to int

    let parseUint2 =
        parseBits 2
        |>> (fun x -> Convert.ToInt32(x, 2))

    let parseUint30 =
        parseBits 30
        |>> (fun x -> Convert.ToInt32(x, 2))

    let parseStatus =
        parseBits 4
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
    let parseRateOfTurn =
        parseBits 8
        |>> (fun x -> Convert.ToInt32(x, 2))
    let parseSpeedOverGround =
        parseBits 10
        |>> (fun x -> Convert.ToInt32(x, 2))

    let parsePositionAccuracy =
        parseBits 1
        |>> (fun x -> Convert.ToInt32(x, 2))
    let parseLongitude =
        parseBits 28
        |>> (fun x -> Convert.ToInt32(x, 2))
        |>> (fun x -> float(x) / 600000.0 )
    let parseLatitude =
        parseBits 27
        |>> (fun x -> Convert.ToInt32(x, 2))
        |>> (fun x -> float(x) / 600000.0 )

    let parseEpfd =
        parseBits 4
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

    let toAscii bits =
        printfn "bits: %s" bits
        match bits with
        | "000000" -> "@"
        | "000001" -> "A"
        | "000010" -> "B"
        | "000011" -> "C"
        | "000100" -> "D"
        | "000101" -> "E"
        | "000110" -> "F"
        | "000111" -> "G"
        | "001000" -> "H"
        | "001001" -> "I"
        | "001010" -> "J"
        | "001011" -> "K"
        | "001100" -> "L"
        | "001101" -> "M"
        | "001110" -> "N"
        | "001111" -> "O"
        | "010000" -> "P"
        | "010001" -> "Q"
        | "010010" -> "R"
        | "010011" -> "S"
        | "010100" -> "T"
        | "010101" -> "U"
        | "010110" -> "V"
        | "010111" -> "W"
        | "100000" -> "P"
        | _ -> "?"

    let parseAscii count =
        let chars = count / 6
        let reducer x y =
            (x .>>. y)
            |>> (fun (x, y) -> x + y)

        let ps =
            Seq.init chars (fun _ -> parseBits 6 |>> toAscii)
            |> Seq.reduce reducer
        ps

    let parseCommonNavigationBlock: Parser<_> =
        (parseType 1 <|> parseType 2 <|> parseType 3)
        |>> (fun (x) -> { defaultCommonNavigationBlockResult with Type = x })
        .>>. parseUint2
        |>> (fun (x, y) -> { x with Repeat = y })
        .>>. parseUint30
        |>> (fun (x, y) -> { x with Mmsi = y })
        .>>. parseStatus
        |>> (fun (x, y) -> { x with Status = y })
        .>>. parseRateOfTurn
        |>> (fun (x, y) -> { x with RateOfTurn = y })
        .>>. parseSpeedOverGround
        |>> (fun (x, y) -> { x with SpeedOverGround = y })
        .>>. parsePositionAccuracy
        |>> (fun (x, y) -> { x with PositionAccuracy = y })
        .>>. parseLongitude
        |>> (fun (x, y) -> { x with Longitude = y })
        .>>. parseLatitude
        |>> (fun (x, y) -> { x with Latitude = y })
        .>>. parseEpfd
        |>> (fun (x, y) -> { x with Epfd = y })
        |>> (fun (x) -> (Type123) x)

    let parseStaticAndVoyageRelatedData: Parser<MessageType> =
        parseType 5
        |>> (fun (x) -> { defaultStaticAndVoyageRelatedData with Type = x })
        .>>. parseUint2
        |>> (fun (x, y) -> { x with Repeat = y })
        .>>. parseUint30
        |>> (fun (x, y) -> { x with Mmsi = y })
        .>>. parseUint2
        |>> (fun (x, y) -> { x with Version = y })
        .>>. parseUint30
        |>> (fun (x, y) -> { x with ImoNumber = y })
        .>>. parseAscii 30
        |>> (fun (x, y) -> { x with CallSign = y })
        //.>>. parseAscii 120
        //|>> (fun (x, y) -> { x with VesselName = y })

        |>> (fun (x) -> (Type5) x)

    let parseFields : Parser<MessageType> =
        let typeParser = parseCommonNavigationBlock <|> parseStaticAndVoyageRelatedData
        typeParser

    // Allowed characters in payload
    let allowedChars = List.map char [48..119]

    let parsePayload : Parser<_> =
        // Parse to string
        comma >>. many1 (anyOf allowedChars)
        // Transform to binary string
        |>> charListToBinaryString

    let aisParser : Parser<_>=
        parseVdm
        |>> (fun (x) -> { defaultAisResult with Vdm = x })
        .>>. parseNumber
        |>> (fun (x, y) -> { x with Number = y })
        .>>. parseCount
        |>> (fun (x, y) -> { x with Count = y })
        .>>. parseSeq
        |>> (fun (x, y) -> { x with Seq = y })
        .>>. parseChannel
        |>> (fun (x, y) -> { x with Channel = y })
        .>>. parsePayload
        |>> (fun (x, y) -> { x with Payload = y })
        .>> parsePadBits