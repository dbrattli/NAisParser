namespace AisParser

open System
open FParsec

type UserState = unit // doesn't have to be unit, of course
type Parser<'t> = Parser<'t, UserState>

module Core =
    let parseBits count : Parser<_> =
        manyMinMaxSatisfy count count isDigit

    let inline isBit (c: char) =
        uint32 c - uint32 '0' <= uint32 '1' - uint32 '0'

    let parseUint2 =
        parseBits 2
        |>> (fun x -> Convert.ToInt32(x, 2))

    let parseUint30 =
        parseBits 30
        |>> (fun x -> Convert.ToInt32(x, 2))

    let parseInt8 =
        parseBits 8
        |>> (fun x -> Convert.ToByte(x, 2))

    let toAscii bits =
        match bits with
        | "000000" -> "@" | "010000" -> "P" | "100000" -> " "
        | "000001" -> "A" | "010001" -> "Q"
        | "000010" -> "B" | "010010" -> "R"
        | "000011" -> "C" | "010011" -> "S"
        | "000100" -> "D" | "010100" -> "T"
        | "000101" -> "E" | "010101" -> "U"
        | "000110" -> "F" | "010110" -> "V"
        | "000111" -> "G" | "010111" -> "W"
        | "001000" -> "H" | "011000" -> "X"
        | "001001" -> "I" | "011001" -> "Y"
        | "001010" -> "J" | "011010" -> "Z"
        | "001011" -> "K" | "011011" -> "]"
        | "001100" -> "L" | "011100" -> "\\"
        | "001101" -> "M" | "011101" -> "["
        | "001110" -> "N" | "011110" -> "^"
        | "001111" -> "O" | "011111" -> "_"

        | _ -> "?"

    let parseAscii count =
        let chars = count / 6

        let reducer x y =
            (x .>>. y)
            |>> (fun (x, y) -> x + y)

        let ps =
            Seq.init chars (fun _ -> parseBits 6 |>> toAscii)
            |> Seq.reduce reducer

        ps |>> (fun (x) -> x.Trim(' '))

