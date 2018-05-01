namespace AisParser

open System
open System.Collections.Generic

open FParsec
open AisParser.Ais
open System.Runtime.InteropServices //for OutAttribute


type public Parser() =
    let fragments = new List<ParserResult<AisResult, unit>>()

    member public this.TryParse(input: String, [<Out>] result : AisResult byref) : bool =
        let res = run aisParser input

        match res with
        | Success (c, _, _) ->
            fragments.Add(res)

            if c.Number < c.Count then
                false
            else
                let fragment = fragments |> Seq.reduce defragment
                match fragment with
                | Success (c, _, _) ->
                    result <- c
                    fragments.Clear()
                    true
                | Failure (error, _, _)  ->
                    raise (System.ArgumentException(error))
        | Failure (error, _, _)  ->
            raise (System.ArgumentException(error))

    member public this.TryParse(input: AisResult, [<Out>] result : CommonNavigationBlockResult byref) : bool =
        let res = run parseFields input.Payload;

        match res with
        | Success (message, _, _) ->
            match message with
            | Type123 cnb ->
                result <- cnb
                true
            | _ ->
                false
        | Failure (error, _, _)  ->
            raise (System.ArgumentException(error))

    member public this.TryParse(input: AisResult, [<Out>] result : StaticAndVoyageRelatedData byref) : bool =
        let res = run parseFields input.Payload;

        match res with
        | Success (message, _, _) ->
            match message with
            | Type5 cnb ->
                result <- cnb
                true
            | _ ->
                false
        | Failure (error, _, _)  ->
            raise (System.ArgumentException(error))
