namespace Tests

open System
open NUnit.Framework
open FsUnit

open NAisParser

[<TestClass>]
type TestApi () =

    [<SetUp>]
    member _this.Setup () =
        ()

    [<Test>]
    member _this.``Test API with non-fragmented input returns true`` () =
        // Arrage
        let input = "!BSVDM,1,1,,A,13mAwp001m0MMrjSoomG6mWT0<1h,0*16";
        let mutable result = ref Ais.defaultAisResult
        let parser = Parser()

        // Act
        let result = parser.TryParse(input, result)

        // Assert
        result |> should be True

    [<Test>]
    member _this.TestApiInvalidInputThrowsException () =
        // Arrage
        let input = "BSVDM,1,1,,A,13mAwp001m0MMrjSoomG6mWT0<1h,0*16";
        let mutable result = ref Ais.defaultAisResult
        let parser = Parser()
        let mutable raised = false
        // Act
        try
            parser.TryParse(input, result) |> ignore
        with
            | :? ArgumentException ->
                raised <- true

        // Assert
        raised |> should be True

    [<Test>]
    member _this.TestApiFirstFragmentReturnsFalse () =
        // Arrage
        let input = "!BSVDM,2,1,2,A,53mDDD02>EjthmLJ220HtppE>2222222222222164@G:34rdR?QSkSQDp888,0*15";
        let mutable result = ref Ais.defaultAisResult
        let parser = Parser()

        // Act
        let result = parser.TryParse(input, result)

        // Assert
        result |> should be False


    [<Test>]
    member _this.TestApiLastFragmentReturnsTrue () =
        // Arrage
        let input: string [] = [|
            "!BSVDM,2,1,2,A,53mDDD02>EjthmLJ220HtppE>2222222222222164@G:34rdR?QSkSQDp888,0*15";
            "!BSVDM,2,2,2,A,88888888880,2*3F";
            |]
        let mutable result = ref Ais.defaultAisResult
        let parser = Parser()

        // Act
        let result1 = parser.TryParse(input.[0], result)
        let result2 = parser.TryParse(input.[1], result)

        // Assert
        result1 |> should be False
        result2 |> should be True

    [<Test>]
    member _this.TestApiNonFragmentInputSetsResult () =
        // Arrage
        let input = "!BSVDM,1,1,,A,13mAwp001m0MMrjSoomG6mWT0<1h,0*16";
        let mutable aisResult = ref Ais.defaultAisResult

        let parser = Parser()

        // Act
        let result = parser.TryParse(input, aisResult)

        // Assert
        result |> should be True
        aisResult.Value.Channel |> should equal Channel.A
        aisResult.Value.Vdm |> should equal TalkerId.BS
        aisResult.Value.Type |> should equal 1

    [<Test>]
    member _this.``Test API parse type 1 is success`` () =
        // Arrage
        let input = "!BSVDM,1,1,,A,13mAwp001m0MMrjSoomG6mWT0<1h,0*16";
        let mutable aisResult = ref Ais.defaultAisResult
        let mutable cnbResult = ref Type123.defaultMessageType123
        let parser = Parser()

        // Act
        let result1 = parser.TryParse(input, aisResult)
        let result2 = parser.TryParse(aisResult.Value, cnbResult)

        // Assert
        result2 |> should be True
        cnbResult.Value.Mmsi |> should equal 257196000
        cnbResult.Value.Latitude |> should equal 62.692621666666668
        cnbResult.Value.Longitude |> should equal 6.4372683333333329

    [<Test>]
    member _this.TestApiParseType5IsSuccess () =
        // Arrage
        let input: string [] = [|
            "!BSVDM,2,1,2,A,53mDDD02>EjthmLJ220HtppE>2222222222222164@G:34rdR?QSkSQDp888,0*15";
            "!BSVDM,2,2,2,A,88888888880,2*3F";
            |]
        let mutable aisResult = ref Ais.defaultAisResult
        let mutable savResult = ref Type5.defaultMessageType5
        let parser = Parser()

        // Act
        let result1 = parser.TryParse(input.[0], aisResult)
        let result2 = parser.TryParse(input.[1], aisResult)
        printfn "%A" aisResult
        let result3 = parser.TryParse(aisResult.Value, savResult)

        // Assert
        result1 |> should be False
        result2 |> should be True
        result3 |> should be True
        savResult.Value.Mmsi |> should equal 257234000

    [<Test>]
    member _this.``Test API parse type 1 with short data throws exception`` () =
        // Arrage
        let input = "!BSVDM,1,1,,A,1,0*16";
        let mutable aisResult = ref Ais.defaultAisResult
        let mutable cnbResult = ref Type123.defaultMessageType123
        let mutable raised = false
        let parser = Parser()

        // Act
        let result1 = parser.TryParse(input, aisResult)
        try
            parser.TryParse(aisResult.Value, cnbResult) |> ignore
        with
            | :? ArgumentException ->
                raised <- true

        // Assert
        result1 |> should be True

    [<Test>]
    member _this.TestApiParseType5ShortThrowsException () =
        // Arrage
        let input = "!BSVDM,1,1,,A,5,0*16";
        let mutable aisResult = ref Ais.defaultAisResult
        let mutable cnbResult = ref Type5.defaultMessageType5
        let mutable raised = false
        let parser = Parser()

        // Act
        let result1 = parser.TryParse(input, aisResult)
        try
            parser.TryParse(aisResult.Value, cnbResult) |> ignore
        with
            | :? ArgumentException ->
                raised <- true

        // Assert
        result1 |> should be True

    [<Test>]
    member _this.``Test API parse type 4 is success`` () =
        // Arrage
        let input = "!AIVDM,1,1,,A,400TcdiuiT7VDR>3nIfr6>i00000,0*78";
        let mutable aisResult = ref Ais.defaultAisResult
        let mutable cnbResult = ref Type4.defaultMessageType4
        let parser = Parser()

        // Act
        let result1 = parser.TryParse(input, aisResult)
        let result2 = parser.TryParse(aisResult.Value, cnbResult)

        // Assert
        result2 |> should be True
        cnbResult.Value.Mmsi |> should equal 601011
        cnbResult.Value.Year |> should equal 2012
        cnbResult.Value.Month |> should equal 6
        cnbResult.Value.Day |> should equal 8
        cnbResult.Value.Hour |> should equal 7
        cnbResult.Value.Minute |> should equal 38
        cnbResult.Value.Second |> should equal 20
        cnbResult.Value.FixQuality |> should equal true
        cnbResult.Value.Latitude |> should equal 31.033513333333332
        cnbResult.Value.Longitude |> should equal -29.870835
        cnbResult.Value.Epfd |> should equal EpfdFixType.Gps
        cnbResult.Value.RaimFlag |> should equal false