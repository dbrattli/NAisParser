namespace Tests

open NUnit.Framework
open FsUnit
open FParsec

open NAisParser
open NAisParser.Ais

[<TestClass>]
type TestClassType5 () =

    [<SetUp>]
    member _this.Setup () =
        ()

    [<Test>]
    member _this.``Test BSVDM multi fragment is success`` () =
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
            | Success (ais, _, _) ->
                run Type5.parseMessageType5 (Common.intListToBinaryString ais.Payload)
            | Failure (a, b, c) ->
                Failure(a, b, c)

        // Assert
        result |> isSuccess |> should be True
        result2 |> isSuccess |> should be True

    [<Test>]
    member _this.``Test BSVDM multi fragment with invalid packet is failure`` () =
        // Arrage
        let input: string [] = [|
            "!XXXXX,2,1,2,A,53mDDD02>EjthmLJ220HtppE>2222222222222164@G:34rdR?QSkSQDp888,0*15";
            "!BSVDM,2,2,2,A,88888888880,2*3F";
            |]
        let result =
            input
            |> Seq.map (run aisParser)
            |> Seq.reduce defragment

        let result2 =
            match result with
            | Success (ais, _, _) ->
                run Type5.parseMessageType5 (Common.intListToBinaryString ais.Payload)
            | Failure (a, b, c) ->
                Failure(a, b, c)

        // Assert
        result |> isSuccess |> should be False
