using CommandLine;

namespace KSharp.Compiler;

#pragma warning disable 8618

public class CompilerArguments
{
    [Value(0, Default = null)]
    //public IEnumerable<string> File { get; set; }
    public string? File { get; set; }
}
