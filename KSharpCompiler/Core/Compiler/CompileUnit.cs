using System.IO;
using System.Threading.Tasks;
using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using KSharpCompiler.Grammar;

namespace KSharpCompiler
{
    public class CompileUnit
    {
        public readonly Compiler compiler;
        public string Path { get; }
        private ParserRuleContext rootCtx;
        
        public CompileUnit(Compiler compiler, string path)
        {
            this.compiler = compiler;
            Path = path;
            rootCtx = null!;
        }

        public async Task Lex()
        {
            KSharpLexer lex = new KSharpLexer(new AntlrInputStream(await File.ReadAllTextAsync(Path)));
            KSharpParser parser = new KSharpParser(new CommonTokenStream(lex));
            parser.AddErrorListener(new ErrorListener(this));
            ParseTreeWalker walker = new ParseTreeWalker();
            var lis = new TypeCollectListener(this);
            await Task.Run(() => walker.Walk(lis, rootCtx = parser.file()));
            lis.BuildDummyExecutable();
            lis.BuildDummyLibrary();
        }
        
        public async Task CollectTypes()
        {
            BaseListener l = new BaseListener(this);
            await Task.Run(() => new ParseTreeWalker().Walk(l, rootCtx));
        }
    }
}
