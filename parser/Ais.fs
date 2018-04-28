namespace AisParser

open System
open FParsec
open AisParser.Core

type TalkerId = | AB | AD| AI | AN | AR | AS | AT | AX | BS | SA

type Channel = | A | B

type AisResult = {
    Vdm: TalkerId;
    Number: uint8;
    Count: uint8;
    Seq: uint8 option;
    Channel: Channel;
    Payload: string;
}

module Ais =
    // AisResult constructor to enable currying
    let aisResult vdm number count seq channel payload : AisResult =
        {
            Vdm = vdm;
            Number = number;
            Count = count;
            Seq = seq;
            Channel = channel;
            Payload = payload;
        }

    let defaultAisResult : AisResult = {
        Vdm = TalkerId.AB;
        Number = 0uy;
        Count = 0uy;
        Seq = Some 0uy;
        Channel = Channel.A;
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

    let parseVdm : Parser<_> =
        pchar '!' >>. anyString 2 .>> pstring "VDM"
        |>> fun x ->
            match x with
            | "AB" -> TalkerId.AB | "AD" -> TalkerId.AD
            | "AI" -> TalkerId.AI | "AN" -> TalkerId.AN
            | "AR" -> TalkerId.AR | "AS" -> TalkerId.AS
            | "AT" -> TalkerId.AT | "AX" -> TalkerId.AX
            | "BS" -> TalkerId.BS | "SA" -> TalkerId.SA
            | _ -> TalkerId.AB

    let comma : Parser<_> = pchar ','
    let parseCount : Parser<_> = comma >>. puint8
    let parseNumber : Parser<_> = comma >>. puint8
    let parseSeq : Parser<_> = comma >>. opt puint8
    let parseChannel : Parser<_> =
        comma >>. anyOf ['A';'B';'0';'1']
        |>> fun x ->
            match x with
            | 'B' | '1' -> Channel.B
            | _ -> Channel.A

    let parsePadBits = comma >>. puint8

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

    let aisParser : Parser<_> =
        preturn aisResult
        <*> parseVdm
        <*> parseNumber
        <*> parseCount
        <*> parseSeq
        <*> parseChannel
        <*> parsePayload
        <*  parsePadBits