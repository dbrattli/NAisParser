// Learn more about F# at http://fsharp.org

open System.IO
open System.Net.Sockets

open NAisParser

let parser = new Parser()

let rec asyncResponse (reader : StreamReader) =
    async {
        let! line = reader.ReadLineAsync () |> Async.AwaitTask
        let result = parser.Parse line
        match result with
        | Ok aisResult ->
            match aisResult.Type with
            | 1uy | 2uy | 3uy ->
                let res = parser.ParseType123 aisResult
                match res with
                | Result.Ok msg ->
                    printf "%A" (msg.ToString ())
                | Result.Error err -> ()
            | 4uy ->
                let res = parser.ParseType4 aisResult
                match res with
                | Result.Ok msg ->
                    printf "%A" (msg.ToString ())
                | Result.Error err -> ()
            | 5uy ->
                let res = parser.ParseType5 aisResult
                match res with
                | Result.Ok msg ->
                    printf "%A" (msg.ToString ())
                | Result.Error err -> ()
            | _ ->
                ()
        | Error err ->
            printfn "Got exception: %A" (err.ToString ())
        | Continue ->
            ()

        return! asyncResponse reader
    }

[<EntryPoint>]
let main args =
    let port = 5631
    let server = "153.44.253.27"

    let client = new TcpClient ()
    client.Connect (server, port)
    printfn "Connected to %A %A..." server port
    let stream = client.GetStream ()
    use reader = new StreamReader (stream)

    printfn "Got stream, starting asynchronous communication."
    asyncResponse reader |> Async.RunSynchronously
    0