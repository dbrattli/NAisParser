namespace AisParser

open System
open FParsec
open AisParser.Ais

[<AutoOpen>]
module Api =

    type public Parser() =

        member this.Parse(input: String) =
            let result = run aisParser input
            result