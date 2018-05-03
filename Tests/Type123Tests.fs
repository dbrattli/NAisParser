namespace Tests

open NUnit.Framework
open FsUnit

open AisParser
open FParsec

[<TestClass>]
type TestClassTest123 () =

    [<SetUp>]
    member this.Setup () =
        ()

    [<Test>]
    member this.TestParseType1MessageIsSuccesss () =
        // Arrage
        let input = "!BSVDM,1,1,,A,13mAwp001m0MMrjSoomG6mWT0<1h,0*16"

        // Act
        let result = run Ais.aisParser input

        let result2 =
            match result with
            | Success (ais, state, pos) ->
                run Type123.parseCommonNavigationBlock (Common.intListToBinaryString ais.Payload)
            | Failure (a, b, c) -> Failure(a, b, c)

        // Assert
        Ais.isSuccess(result) |> should be True
        Ais.isSuccess(result2) |> should be True

