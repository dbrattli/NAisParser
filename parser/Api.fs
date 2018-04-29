namespace AisParser

open System
open FParsec
open AisParser.Ais


type public Parser() =

    member public this.Parse(input: String) =
        let result = run aisParser input
        match result with 
        | Success (c, state, pos) ->
            c
        | Failure (a, b, c)  ->
            raise (System.ArgumentException(a))
