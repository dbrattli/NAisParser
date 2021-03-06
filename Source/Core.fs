namespace NAisParser

open System
open FParsec

type UserState = unit // doesn't have to be unit, of course
type Parser<'t> = Parser<'t, UserState>

module Core =
    // Allowed characters in payload
    let allowedChars = List.map char [48..119]

    let isBit (c: char) =
        uint32 c - uint32 '0' <= uint32 '1' - uint32 '0'

    let parseBits count : Parser<_> =
        manyMinMaxSatisfy count count isBit

    let parseBool =
        parseBits 1
        |>> fun x -> Convert.ToByte (x, 2) = 1uy

    let parseIntN bits =
        parseBits bits
        |>> fun x -> Convert.ToInt32 (x, 2)

    let parseByteN bits =
        parseBits bits
        |>> fun x -> Convert.ToByte (x, 2)

    let parseSByte =
        parseBits 8
        |>> fun x -> Convert.ToSByte (x, 2)

    let char2int (chr: char) =
        let value = int chr
        if value > 40 then
            let n = value - 48
            if n > 40 then
                n - 8
            else
                n
        else
            value

    let toAscii bits =
        match bits with
        | "000000" -> "@" | "010000" -> "P"  | "100000" -> " "  | "110000" -> "0"
        | "000001" -> "A" | "010001" -> "Q"  | "100001" -> "!"  | "110001" -> "1"
        | "000010" -> "B" | "010010" -> "R"  | "100010" -> "\"" | "110010" -> "2"
        | "000011" -> "C" | "010011" -> "S"  | "100011" -> "#"  | "110011" -> "3"
        | "000100" -> "D" | "010100" -> "T"  | "100100" -> "$"  | "110100" -> "4"
        | "000101" -> "E" | "010101" -> "U"  | "100101" -> "%"  | "110101" -> "5"
        | "000110" -> "F" | "010110" -> "V"  | "100110" -> "&"  | "110110" -> "6"
        | "000111" -> "G" | "010111" -> "W"  | "100111" -> "\\" | "110111" -> "7"
        | "001000" -> "H" | "011000" -> "X"  | "101000" -> "("  | "111000" -> "8"
        | "001001" -> "I" | "011001" -> "Y"  | "101001" -> ")"  | "111001" -> "9"
        | "001010" -> "J" | "011010" -> "Z"  | "101010" -> "*"  | "111010" -> ":"
        | "001011" -> "K" | "011011" -> "]"  | "101011" -> "+"  | "111011" -> ";"
        | "001100" -> "L" | "011100" -> "\\" | "101100" -> ","  | "111100" -> "<"
        | "001101" -> "M" | "011101" -> "["  | "101101" -> "-"  | "111101" -> "="
        | "001110" -> "N" | "011110" -> "^"  | "101110" -> "."  | "111110" -> ">"
        | "001111" -> "O" | "011111" -> "_"  | "101111" -> "/"  | _        -> "?"

    let parseAscii count =
        let chars = count / 6

        let reducer x y =
            x .>>. y
            |>> fun (x, y) -> x + y

        let ps =
            Seq.init chars (fun _ -> parseBits 6 |>> toAscii)
            |> Seq.reduce reducer

        ps |>> fun x -> x.Trim ([| ' '; '@'|])

    let apply pf pa =
        // pf >>= fun f -> pa >>= preturn >> f
        // pf >>= fun f -> pa >>= f -> preturn f a
        pf >>= fun f -> pa |>> f

    let (<*>) f a = apply f a

    let ( *>) pf pa = pa >>. pf

    let (<* ) pf pa = pf .>> pa