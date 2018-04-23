namespace AisParser

open System
open FParsec

type UserState = unit // doesn't have to be unit, of course
type Parser<'t> = Parser<'t, UserState>

module Ais =
    let isSuccess result =
        match result with
        | Success _ -> true
        | _ -> false

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

    let parseRepeat =
        parseBits 2
        |>> (fun x -> Convert.ToInt32(x, 2))
    let parseMmsi =
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

    let parseCommonNavigationBlock: Parser<_> =
        (parseType 1 <|> parseType 2 <|> parseType 3)
        .>>. parseRepeat
        .>>. parseMmsi
        .>>. parseStatus
        .>>. parseRateOfTurn
        .>>. parseSpeedOverGround
        .>>. parsePositionAccuracy
        .>>. parseLongitude
        .>>. parseLatitude
        .>>. parseEpfd

    let parseStaticAndVoyageRelatedData: Parser<_> =
        parseType 5

    let parseFields input : Parser<_> =
        let typeParser = parseCommonNavigationBlock //<|> parseStaticAndVoyageRelatedData
        run typeParser input |> preturn

    // Allowed characters in payload
    let allowedChars = List.map char [48..119]

    let parsePayload : Parser<_> =
        // Parse to string
        comma >>. many1 (anyOf allowedChars)
        // Transform to binary string
        |>> charListToBinaryString
        // Reparse binary string to protocol fields
        >>= parseFields

    let aisParser : Parser<_>=
        parseVdm
        .>>. parseNumber
        .>>. parseCount
        .>>. parseSeq
        .>>. parseChannel
        .>>. parsePayload
        .>>. parsePadBits