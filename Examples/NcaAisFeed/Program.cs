using System;
using System.IO;
using System.Net.Sockets;
using System.Diagnostics;

using NAisParser;

namespace NcaAisFeed
{
    class Program
    {
        static void Main(string[] args)
        {
            var port = 5631;
            var server = "153.44.253.27";
            var client = new TcpClient(server, port);
            var parser = new Parser();

            Stopwatch stopWatch = new Stopwatch();

            var stream = client.GetStream();
            using (StreamReader reader = new StreamReader(stream)) {
                string line;

                while ((line = reader.ReadLine()) != null) {
                    stopWatch.Reset();
                    stopWatch.Start();
                    var result = parser.TryParse(line, out AisResult aisResult);
                    if (!result) continue;
                    switch (aisResult.Type)
                    {
                        case 1:
                        case 2:
                        case 3:
                            result = parser.TryParse(aisResult, out MessageType123 type123Result);
                            stopWatch.Stop();
                            Console.WriteLine(type123Result.ToString());
                            break;
                        case 5:
                            result = parser.TryParse(aisResult, out MessageType5 type5Result);
                            stopWatch.Stop();
                            Console.WriteLine(type5Result.ToString());
                            break;
                        default:
                            throw new NotImplementedException(String.Format("Type: {0}", aisResult.Type));
                    }

                    long ts = stopWatch.ElapsedTicks;

                    // Format and display the TimeSpan value.
                    var tickTime = 1.0 / Stopwatch.Frequency;
                    string elapsedTime = String.Format("Milliseconds {0}", ts*tickTime*1000);
                    Console.WriteLine("RunTime " + elapsedTime);
                }
            }
        }
    }
}
