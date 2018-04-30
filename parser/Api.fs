namespace AisParser

open System
open System.Collections.Generic

open FParsec
open AisParser.Ais


type public Parser(cb : Action<ParserResult<AisResult, unit>>) =

    let callback = cb

    let fragments = new List<ParserResult<AisResult, unit>>()

    member public this.Parse(input: String) : unit =
        let result = run aisParser input

        match result with
        | Success (c, state, pos) ->
            if c.Number < c.Count then
                fragments.Add(result)
            else
                callback.Invoke(result)

        | Failure (a, b, c)  ->
            raise (System.ArgumentException(a))
