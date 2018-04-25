namespace AisParser

open System
open FParsec

type AisResult = {
    Vdm: string;
    Number: uint8;
    Count: uint8;
    Seq: uint8 option;
    Channel: char;
    Payload: string;
}

module Ais =
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

    let parseFields : Parser<MessageType> =
        let typeParser =
            Type123.parseCommonNavigationBlock <|>
            Type5.parseStaticAndVoyageRelatedData
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
        |>> fun x -> { defaultAisResult with Vdm = x }
        .>>. parseNumber
        |>> fun (x, y) -> { x with Number = y }
        .>>. parseCount
        |>> fun (x, y) -> { x with Count = y }
        .>>. parseSeq
        |>> fun (x, y) -> { x with Seq = y }
        .>>. parseChannel
        |>> fun (x, y) -> { x with Channel = y }
        .>>. parsePayload
        |>> fun (x, y) -> { x with Payload = y }
        .>> parsePadBits