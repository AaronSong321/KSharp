using KSharpCompiler.Grammar;

namespace KSharpCompiler
{
    public class BaseListener: KSharpParserBaseListener
    {
        protected readonly CompileUnit cu;
        protected readonly Compiler compiler;

        public BaseListener(CompileUnit compileUnit)
        {
            cu = compileUnit;
            compiler = cu.compiler;
        }
    }
}
