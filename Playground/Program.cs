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
                    if (result)
                    {
                        result = parser.TryParse(aisResult, out CommonNavigationBlockResult cnbResult);
                        Console.WriteLine(cnbResult.ToString());
                    }
                }
            }
        }
    }
}
