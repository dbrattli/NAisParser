namespace NAisParser

open FParsec
open NAisParser.Core

type TalkerId = | AB = 0 | AD = 1 | AI = 2 | AN = 3 | AR = 4 | AS = 5 | AT = 6 | AX = 7 | BS = 8| SA =9

type Channel = | A = 0 | B = 1

type AisResult = {
    Vdm: TalkerId
    Count: uint8
    Number: uint8
    Seq: uint8 option
    Channel: Channel
    Payload: list<int>
    Type: byte
}

module Ais =
    // AisResult constructor to enable currying
    let aisResult vdm count number seq channel payload : AisResult =
        {
            Vdm = vdm
            Count = count
            Number = number
            Seq = seq
            Channel = channel
            Payload = payload
            Type = 0uy
        }

    let defaultAisResult : AisResult = {
        Vdm = TalkerId.AB
        Count = 0uy
        Number = 0uy
        Seq = Some 0uy
        Channel = Channel.A
        Payload = []
        Type = 0uy
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
                        let payload = List.append p.Payload c.Payload
                        Success ({ p with Payload = payload }, state, pos)
                    else
                        curr
                | Failure (a, b, c) -> Failure (a, b, c)
            | Failure (a, b, c) -> Failure (a, b, c)
        result

    let parseVdm : Parser<_> =
        pchar '!' >>. anyString 2 .>> (pstring "VDM" <|> pstring "VDO")
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

    let parsePayload : Parser<_> =
        // Parse to string
        comma >>. many1 (anyOf Core.allowedChars)
        |>> List.map Core.char2int

    let aisParser : Parser<_> =
        preturn aisResult
        <*> parseVdm
        <*> parseCount
        <*> parseNumber
        <*> parseSeq
        <*> parseChannel
        <*> parsePayload
        <*  parsePadBits