namespace AisParser

open System
open System.Collections.Generic

open FParsec
open AisParser.Ais


type public Parser() =

    let fragments = new List<ParserResult<AisResult, unit>>()

    member public this.Parse(input: String) =
        let result = run aisParser input

        match result with
        | Success (c, state, pos) ->
            c
        | Failure (a, b, c)  ->
            raise (System.ArgumentException(a))
