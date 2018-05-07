namespace Tests

open NUnit.Framework
open FsUnit

open NAisParser.Ais
open FParsec

[<TestClass>]
type TestClass () =

    [<SetUp>]
    member _this.Setup () =
        ()

    [<Test>]
    member _this.``Parse BSVDM data packet header is success`` () =
        let input = "!BSVDM"
        let result = run parseVdm input

        isSuccess(result)|> should be True


    [<Test>]
    member _this.``Parse invalid BSVDM header is failure`` () =
        let input = "BSVDM"
        let result = run parseVdm input

        isSuccess(result)|> should be False

    [<Test>]
    member _this.``Parse unknown BSVDM is failure`` () =
        let input = "XXVDM"
        let result = run parseVdm input

        isSuccess(result)|> should be False

    [<Test>]
    member _this.``Parse AIVDM data packet is success`` () =
        let input = "!AIVDM"
        let result = run parseVdm input

        isSuccess(result) |> should be True

    [<Test>]
    member _this.``Parse ABVDM data packet is success`` () =
        let input = "!ABVDM"
        let result = run parseVdm input

        isSuccess(result)|> should be True

    [<Test>]
    member _this.``Parse BSVDM data packet is success`` () =
        // Arrage
        let input = "!BSVDM,1,1,,A,13mAwp001m0MMrjSoomG6mWT0<1h,0*16";
        let result = input |> run aisParser

        // Assert
        result |> isSuccess |> should be True

    [<Test>]
    member _this.``Parse BSVDM multi fragment data packet is success`` () =
        // Arrage
        let input: string [] = [|
            "!BSVDM,2,1,2,A,53mDDD02>EjthmLJ220HtppE>2222222222222164@G:34rdR?QSkSQDp888,0*15";
            "!BSVDM,2,2,2,A,88888888880,2*3F";
            |]
        let result =
            input
            |> Seq.map (run aisParser)
            |> Seq.reduce defragment

        // Assert
        result |> isSuccess |> should be True