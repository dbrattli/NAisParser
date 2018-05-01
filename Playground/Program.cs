using System;
using System.IO;
using System.Net.Sockets;

using AisParser;

namespace Kystverket
{
    class Program
    {
        static void Main(string[] args)
        {
            var port = 5631;
            var server = "153.44.253.27";
            var client = new TcpClient(server, port);
            var stream = client.GetStream();
            var parser = new Parser();
            AisResult aisResult = null;

            using (StreamReader reader = new StreamReader(stream)) {
                string line;

                while ((line = reader.ReadLine()) != null) {
                    var result = parser.TryParse(line, out aisResult);
                    if (!result) continue;

                    switch (aisResult.Type)
                    {
                        case 1:
                        case 2:
                        case 3:
                            result = parser.TryParse(aisResult, out CommonNavigationBlockResult type123Result);
                            Console.WriteLine(type123Result.ToString());
                            break;
                        case 5:
                            result = parser.TryParse(aisResult, out StaticAndVoyageRelatedData type5Result);
                            Console.WriteLine(type5Result.ToString());
                            break;
                        default:
                            break;
                    }
                }
            }
        }
    }
}
