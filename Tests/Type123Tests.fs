namespace Tests

open NUnit.Framework
open FsUnit
open FParsec

open NAisParser

[<TestClass>]
type TestClassType123 () =

    [<SetUp>]
    member _this.Setup () =
        ()

    [<Test>]
    member _this.TestParseType1MessageIsSuccesss () =
        // Arrage
        let input = "!BSVDM,1,1,,A,13mAwp001m0MMrjSoomG6mWT0<1h,0*16"

        // Act
        let result = run Ais.aisParser input

        let result2 =
            match result with
            | Success (ais, _, _) ->
                run Type123.parseMessageType123 (Common.intListToBinaryString ais.Payload)
            | Failure (a, b, c) -> Failure(a, b, c)

        // Assert
        Ais.isSuccess(result) |> should be True
        Ais.isSuccess(result2) |> should be True

