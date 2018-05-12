namespace Tests

open NUnit.Framework
open FsUnit
open FParsec

open NAisParser
open NAisParser.Ais

[<TestClass>]
type TestClassType4 () =

    [<SetUp>]
    member _this.Setup () =
        ()

    [<Test>]
    member _this.TestParseType1MessageIsSuccesss () =
        // Arrage
        let input = "!AIVDM,1,1,,A,400TcdiuiT7VDR>3nIfr6>i00000,0*78"

        // Act
        let result = run Ais.aisParser input

        let result2 =
            match result with
            | Success (ais, _, _) ->
                run Type123.parseMessageType123 (Common.intListToBinaryString ais.Payload)
            | Failure (a, b, c) -> Failure(a, b, c)

        // Assert
        isSuccess(result) |> should be True
        isSuccess(result2) |> should be False

