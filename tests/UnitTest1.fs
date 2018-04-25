namespace Tests

open NUnit.Framework
open FsUnit

open AisParser.Ais
open FParsec

[<TestClass>]
type TestClass () =

    [<SetUp>]
    member this.Setup () =
        ()

    [<Test>]
    member this.ParseBsvdmIsSuccess () =
        let input = "!BSVDM"
        let result = run parseVdm input

        isSuccess(result)|> should be True

    [<Test>]
    member this.ParseAivdmIsSuccess () =
        let input = "!AIVDM"
        let result = run parseVdm input

        isSuccess(result) |> should be True

    [<Test>]
    member this.ParseAbvdmIsSuccess () =
        let input = "!ABVDM"
        let result = run parseVdm input

        isSuccess(result)|> should be False

    [<Test>]
    member this.Bsvdm () =
        // Arrage
        let input = "!BSVDM,1,1,,A,13mAwp001m0MMrjSoomG6mWT0<1h,0*16"

        // Act
        let result = run aisParser input
        printfn "Result: %s" (result.ToString())

        let result2 =
            match result with
            | Success (ais, state, pos) ->
                run parseFields ais.Payload;
            | Failure (a, b, c) -> Failure(a, b, c)
        printfn "Result: %s" (result2.ToString())

        // Assert
        isSuccess(result) |> should be True

    //[<Test>]
    member this.BsvdmSequence1 () =
        // Arrage
        let input: string [] = [|
            "!BSVDM,1,1,,A,13mAwp001m0MMrjSoomG6mWT0<1h,0*16";
            |]
        let result = input |> Seq.map (run aisParser)

        // Assert
        result |> Seq.map isSuccess |> should not' (contain false)

    [<Test>]
    member this.BsvdmSequence2 () =
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

    //[<Test>]
    member this.BsvdmMultiFragment () =
        // Arrage
        let input = "!BSVDM,2,1,8,A,53os<002?U8dh5Mb221HTdTpN0h4AV222222221J:P8>75580A2lRDm2@CTm,0*48"

        // Act
        let result = run aisParser input
        printfn "Result: %s" (result.ToString())

        // Assert
        isSuccess(result) |> should be True
