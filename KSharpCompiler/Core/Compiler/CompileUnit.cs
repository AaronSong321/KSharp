using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using KSharp;
using KSharpCompiler.Grammar;

namespace KSharpCompiler
{
    public class CompileUnit
    {
        public readonly Compiler compiler;
        public string Path { get; }
        private ParserRuleContext rootCtx;

#nullable disable
        public UsingAgent UsingAgent { get; }
        public LocalTypeResolveAgent LocalTypeResolveAgent { get; }
        public ExpressionEmitter ExpressionEmitter { get; }
        public LocalScopeAgent LocalScopeAgent { get; }
        public TypeModifierChecker TypeModifierChecker { get; }
#nullable restore

        public CompileUnit(Compiler compiler, string path)
        {
            this.compiler = compiler;
            Path = path;
            rootCtx = null!;
        }
        
        public async Task CollectTypes()
        {
            CompileUnitAgent.InitAgents(this);
            KSharpLexer lex = new KSharpLexer(new AntlrInputStream(await File.ReadAllTextAsync(Path)));
            KSharpParser parser = new KSharpParser(new CommonTokenStream(lex));
            parser.AddErrorListener(new ErrorListener(this));
            ParseTreeWalker walker = new ParseTreeWalker();
            var lis = new TypeCollectListener(this);
            var task = Task.Run(() => walker.Walk(lis, rootCtx = parser.file()));
            await task;
        }
        
        public async Task DefineMembers()
        {
            ParseTreeWalker walker = new ParseTreeWalker();
            var lis = new MemberDefineListener(this);
            var task = Task.Run(() => walker.Walk(lis, rootCtx));
            await task;
        }

        public async Task ImplementMethods()
        {
            ParseTreeWalker walker = new ParseTreeWalker();
            var lis = new MethodImplementationListener(this);
            var task = Task.Run(() => walker.Walk(lis, rootCtx));
            await task;
        }
    }
}
