namespace Tests

open NUnit.Framework
open FsUnit

open AisParser
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
            |> Seq.map (run Ais.aisParser)
            |> Seq.reduce Ais.defragment

        let result2 =
            match result with
            | Success (ais, _, _) ->
                run Type5.parseMessageType5(Common.intListToBinaryString ais.Payload)
            | Failure (a, b, c) ->
                Failure(a, b, c)

        // Assert
        result |> Ais.isSuccess |> should be True
        result2 |> Ais.isSuccess |> should be True