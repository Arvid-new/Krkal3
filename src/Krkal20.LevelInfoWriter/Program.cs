using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Krkal20.LevelInfoWriter
{
    class Program
    {
        const String levels = @"$KRKAL$\Data\Krkal_4F88_78B7_A01C_48AB";
        const String cfgFile = @"d:\A\A\Levels.cfg";

        static void Main(string[] args)
        {
            var liw = new LevelInfoWriter(levels, cfgFile);
            liw.Import();

            Console.WriteLine("OK");
        }
    }
}
