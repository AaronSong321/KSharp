using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Threading.Tasks;
using KSharpCompiler;
using Mono.Cecil;


class Program
{
    static async Task<int> Main(string[] args)
    {
        var cm = new RootCommand {
            new Option<string[]>("--src") { AllowMultipleArgumentsPerToken = true },
            new Option<string>("--out"),
            new Option<string>("--target", ()=>"exe") { IsRequired = false },
            new Option<bool>("--bdkslib", ()=>true, "set to false when building KSharp standard library") { IsRequired = false},
        };
        int rt = -1;
        cm.Handler = CommandHandler.Create<string[],string,string,bool>((src, @out, target, bdkslib) => {
            CompilerArguments ar = new CompilerArguments() {
                src = src,
                @out = @out,
                moduleKind = target switch {
                    "library" => ModuleKind.Dll,
                    "exe" => ModuleKind.Console,
                    _ => throw new Exception()
                },
                buildKSharpStdLib = bdkslib
            };
            var f = new Compiler().Compile(ar);
            rt = f.GetAwaiter().GetResult();
            if (f.Exception != null)
                Console.WriteLine(f.Exception);
        });
        await cm.InvokeAsync(args);
        return rt;
    }
}
