using System;

using AisParser;

namespace Kystverket
{
    class Program
    {
        static void Main(string[] args)
        {
            var parser = new Parser();
            AisResult aisResult;
            var result = parser.TryParse("!BSVDM,1,1,,A,13mAwp001m0MMrjSoomG6mWT0<1h,0*16", out aisResult);
            //result.
            Console.WriteLine(aisResult.Channel.ToString());

            result = parser.TryParse(aisResult, out CommonNavigationBlockResult cnbResult);
            Console.WriteLine(cnbResult.ToString());
            Console.ReadLine();
        }
    }
}
