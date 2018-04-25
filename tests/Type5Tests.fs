namespace Tests

open NUnit.Framework
open FsUnit

open AisParser.Ais
open FParsec

[<TestClass>]
type TestClassType5 () =

    [<SetUp>]
    member this.Setup () =
        ()

    [<Test>]
    member this.TestBsvdmMultiFragmentIsSuccess () =
        // Arrage
        let input: string [] = [|
            "!BSVDM,2,1,2,A,53mDDD02>EjthmLJ220HtppE>2222222222222164@G:34rdR?QSkSQDp888,0*15";
            "!BSVDM,2,2,2,A,88888888880,2*3F";
            |]
        let result =
            input
            |> Seq.map (run aisParser)
            |> Seq.reduce defragment

        let result2 =
            match result with
            | Success (ais, state, pos) ->
                run parseFields ais.Payload;
            | Failure (a, b, c) -> Failure(a, b, c)
        printfn "Result: %s" (result2.ToString())

        printfn "Result: %s" (result.ToString())
        // Assert
        result |> isSuccess |> should be True