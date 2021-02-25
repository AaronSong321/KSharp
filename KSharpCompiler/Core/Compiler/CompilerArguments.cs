using Mono.Cecil;

namespace KSharpCompiler
{
    public class CompilerArguments
    {
        public string[] src;
        public string? target;
        public string? outputFileName;
        public ModuleKind moduleKind;
        
    }
}
