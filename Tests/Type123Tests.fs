namespace Tests

open NUnit.Framework
open FsUnit

open AisParser.Ais
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
        let result = run aisParser input

        let result2 =
            match result with
            | Success (ais, state, pos) ->
                run parseFields ais.Payload;
            | Failure (a, b, c) -> Failure(a, b, c)

        // Assert
        isSuccess(result) |> should be True
        isSuccess(result2) |> should be True

