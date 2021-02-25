using System;
using System.Threading.Tasks;
using KSharpCompiler;
using Mono.Cecil;


// Console.WriteLine("Hello World!");
// Compiler c = new();
// RootCommand rootCmd = new("KSharp Compiler") {
//     TreatUnmatchedTokensAsErrors = true
// };
// rootCmd.Handler = CommandHandler.Create(Compiler.CompileWithArgs)
// string s = ".".GetProjectDir();
// return await c.Compile(new []{Path.Combine(s, "Grammar", "test")}, new string[0]{});

// class Program
// {
//     static async ValueTask<int> Main(string[] src, string dst, string? outputFileName, string target)
//     {
//         CompilerArguments args = new CompilerArguments() {
//             src = src,
//             target = dst,
//             outputFileName = outputFileName,
//             moduleKind = target switch {
//                 "library" => ModuleKind.Dll,
//                 "exe" => ModuleKind.Console,
//                 _ => throw new Exception()
//             }
//         };
//         return await new Compiler().Compile(args);
//     }
//     
// }

class Program2
{
    static void Main(string[] src, string dst, string? outputFileName, string target)
    {
        CompilerArguments args = new CompilerArguments() {
            src = src,
            target = dst,
            outputFileName = outputFileName,
            moduleKind = target switch {
                "library" => ModuleKind.Dll,
                "exe" => ModuleKind.Console,
                _ => throw new Exception()
            }
        };
        var f = new Compiler().Compile(args);
        f.GetAwaiter().GetResult();
    }
}