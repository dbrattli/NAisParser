namespace AisParser

open System
open System

type Result<'a> =
    | Success of 'a
    | Failure of string

type Parser<'T> = Parser of (string -> Result<'T * string>)

module Parser =
    /// Match an input token if the predicate is satisfied
    let satisfy predicate =
        let innerFn input =
            if String.IsNullOrEmpty(input) then
                Failure "No more input"
            else
                let first = input.[0]
                if predicate first then
                    let remainingInput = input.[1..]
                    Success (first, remainingInput)
                else
                    let err = sprintf "Unexpected '%c'" first
                    Failure err
        // return the parser
        Parser innerFn

    let pchar charToMatch =
        let predicate ch = (ch = charToMatch)
        satisfy predicate

    let parseA = pchar 'A'

    let run parser input =
        let (Parser innerFn) = parser
        innerFn input

    let isSuccess result =
        match result with
        | Success _ -> true
        | _ -> false

    let andThen parser1 parser2 =
        let innerFn input =
            let result1 = run parser1 input

            match result1 with
            | Failure err ->
                Failure err

            | Success (value1, remaining1) ->
                let result2 = run parser2 remaining1

                match result2 with
                | Failure err ->
                    Failure err

                | Success (value2, remaining2) ->
                    let newValue = (value1, value2)
                    Success (newValue, remaining2)
        Parser innerFn

    /// Infix version of andThen
    let ( .>>. ) = andThen

    /// Combine two parsers as "A orElse B"
    let orElse parser1 parser2 =
        let innerFn input =
            // run parser1 with the input
            let result1 = run parser1 input

            // test the result for Failure/Success
            match result1 with
            | Success result ->
                // if success, return the original result
                result1

            | Failure err ->
                // if failed, run parser2 with the input
                let result2 = run parser2 input

                // return parser2's result
                result2

        // return the inner function
        Parser innerFn

    /// Infix version of orElse
    let ( <|> ) = orElse

    let Map parser mapper =
        let innerFn input =
            let result = run parser input
            match result with
            | Success (value, remaining) ->
                let newValue = mapper value
                Success (newValue, remaining)
            | Failure msg ->
                Failure msg
        Parser innerFn

    let ( |>> ) = Map

    /// Choose any of a list of parsers
    let choice listOfParsers =
        List.reduce ( <|> ) listOfParsers

    /// Choose any of a list of characters
    let anyOf listOfChars =
        listOfChars
        |> List.map pchar // convert into parsers
        |> choice

    let mapP f parser =
        let innerFn input =
            // run parser with the input
            let result = run parser input

            // test the result for Failure/Success
            match result with
            | Success (value,remaining) ->
                // if success, return the value transformed by f
                let newValue = f value
                Success (newValue, remaining)

            | Failure err ->
                // if failed, return the error
                Failure err
        // return the inner function
        Parser innerFn
    let ( <!> ) = mapP

    let parseDigit = anyOf ['0'..'9']

    let returnP x =
        let innerFn input =
            // ignore the input and return x
            Success (x, input )
        // return the inner function
        Parser innerFn

    let fromResult result =
        let innerFn input =
            result
        Parser innerFn

    let applyP fP xP =
        // create a Parser containing a pair (f,x)
        (fP .>>. xP)
        // map the pair by applying f to x
        |> mapP (fun (f,x) -> f x)

    let ( <*> ) = applyP

    // lift a two parameter function to Parser World
    let lift2 f xP yP =
        returnP f <*> xP <*> yP

    let startsWith (str:string) (prefix:string) =
        str.StartsWith(prefix)

    let startsWithP =
        lift2 startsWith

    let rec sequence parserList =
        // define the "cons" function, which is a two parameter function
        let cons head tail = head::tail

        // lift it to Parser World
        let consP = lift2 cons

        // process the list of parsers recursively
        match parserList with
        | [] ->
            returnP []
        | head::tail ->
            consP head (sequence tail)

    /// Helper to create a string from a list of chars
    let charListToStr charList =
         String(List.toArray charList)

    // match a specific string
    let pstring str =
        str
        // convert to list of char
        |> List.ofSeq
        // map each char to a pchar
        |> List.map pchar
        // convert to Parser<char list>
        |> sequence
        // convert Parser<char list> to Parser<string>
        |> mapP charListToStr

    let rec parseZeroOrMore parser input =
        // run parser with the input
        let firstResult = run parser input
        // test the result for Failure/Success
        match firstResult with
        | Failure err ->
            // if parse fails, return empty list
            ([],input)
        | Success (firstValue,inputAfterFirstParse) ->
            // if parse succeeds, call recursively
            // to get the subsequent values
            let (subsequentValues,remainingInput) =
                parseZeroOrMore parser inputAfterFirstParse
            let values = firstValue::subsequentValues
            (values,remainingInput)

    /// match zero or more occurences of the specified parser
    let many parser =

        let rec innerFn input =
            // parse the input -- wrap in Success as it always succeeds
            Success (parseZeroOrMore parser input)

        Parser innerFn

    let whitespaceChar = anyOf [' '; '\t'; '\n']
    let whitespace = many whitespaceChar

    /// match one or more occurences of the specified parser
    let many1 parser =
        let rec innerFn input =
            // run parser with the input
            let firstResult = run parser input
            // test the result for Failure/Success
            match firstResult with
            | Failure err ->
                Failure err // failed
            | Success (firstValue,inputAfterFirstParse) ->
                // if first found, look for zeroOrMore now
                let (subsequentValues,remainingInput) =
                    parseZeroOrMore parser inputAfterFirstParse
                let values = firstValue::subsequentValues
                Success (values,remainingInput)
        Parser innerFn

    let opt p =
        let some = p |>> Some
        let none = returnP None
        some <|> none

    let pint =
        // helper
        let resultToInt (sign,charList) =
            let i = String(List.toArray charList) |> int
            match sign with
            | Some _ -> -i  // negate the int
            | None -> i

        // define parser for one digit
        let digit = anyOf ['0'..'9']

        // define parser for one or more digits
        let digits = many1 digit

        // parse and convert
        opt (pchar '-') .>>. digits
        |>> resultToInt

    /// Keep only the result of the left side parser
    let (.>>) p1 p2 =
        // create a pair
        p1 .>>. p2
        // then only keep the first value
        |> mapP (fun (a,_) -> a)

    /// Keep only the result of the right side parser
    let (>>.) p1 p2 =
        // create a pair
        p1 .>>. p2
        // then only keep the second value
        |> mapP (fun (_, b) -> b)

    /// Keep only the result of the middle parser
    let between p1 p2 p3 =
        p1 >>. p2 .>> p3

    /// Parses one or more occurrences of p separated by sep
    let sepBy1 p sep =
        let sepThenP = sep >>. p
        p .>>. many sepThenP
        |>> fun (p,pList) -> p::pList

    /// Parses zero or more occurrences of p separated by sep
    let sepBy p sep =
        sepBy1 p sep <|> returnP []

    /// "bindP" takes a parser-producing function f, and a parser p
    /// and passes the output of p into f, to create a new parser
    let bindP fn p =
        let innerFn input =
            let result1 = run p input
            match result1 with
            | Failure err ->
                // return error from parser1
                Failure err
            | Success (value1, remainingInput) ->
                // apply f to get a new parser
                let p2 = fn value1
                // run parser with remaining input
                run p2 remainingInput
        Parser innerFn

    let ( >>= ) p f = bindP f p


