using Mono.Cecil;

namespace KSharpCompiler
{
    public class CompilerArguments
    {
#nullable disable
        public string[] src;
        public string @out;
        public ModuleKind moduleKind;
        /// <summary>
        /// Set to false when building KSharp standard library
        /// </summary>
        public bool buildKSharpStdLib;
#nullable restore
    }
}
