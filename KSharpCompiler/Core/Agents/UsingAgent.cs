using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using KSharp;
using KSharpCompiler.Grammar;
using KSharpCompiler.SyntaxRepresentations;


namespace KSharpCompiler
{
    [NoThreading]
    public class UsingAgent: CompileUnitAgent
    {
        public readonly List<UsingNamespace> usingNsBlocks = new List<UsingNamespace>(8);
        
        public UsingAgent(CompileUnit compileUnit) : base(compileUnit) { }

        [ThreadReadOnly]
        public void AddUsingNamespace(KSharpParser.UsingNsContext ctx)
        {
            var qid = ctx.qid();
            var m = Compiler.TypeResolveAgent.ResolveNamespace(qid);
            if (m is null) {
                Compiler.ErrorCollector.AddCompilerMessage(UndefinedIdentifier.UndefinedNamespace(qid.ToString()));
                return;
            }
            var a = new UsingNamespace(qid);
            if (usingNsBlocks.Any(t => t == a)) {
                return;
            }
            usingNsBlocks.Add(a);
        }
    }
}
