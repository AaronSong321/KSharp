using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;


namespace KSharpCompiler
{
    public static class DebugExtensions
    {
        public static void Print(this string s)
        {
            Console.WriteLine(s);
        }
    }
}
