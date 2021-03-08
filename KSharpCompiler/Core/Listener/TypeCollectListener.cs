using System;
using System.IO;
using Community.CsharpSqlite;
using KSharp;
using KSharpCompiler;
using KSharpCompiler.Grammar;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace KSharpCompiler
{
    public class TypeCollectListener: BaseListener
    {
        public TypeCollectListener(CompileUnit cu) : base(cu)
        {
            
        }

        public override void EnterFileScopeNs(KSharpParser.FileScopeNsContext context)
        {
            context.qid().GetIdentifiers().Map(t => t.GetText()).ForEach(cu.LocalScopeAgent.DefineNamespace);
        }

        public override void EnterUsingNs(KSharpParser.UsingNsContext context)
        {
            cu.UsingAgent.AddUsingNamespace(context);
        }

        public override void EnterNsDeclare(KSharpParser.NsDeclareContext context)
        {
            context.qid().GetIdentifiers().Map(t => t.GetText()).ForEach(cu.LocalScopeAgent.DefineNamespace);
        }

        public override void ExitNsDeclare(KSharpParser.NsDeclareContext context)
        {
            context.qid().GetIdentifiers().ForEach(_ => cu.LocalScopeAgent.ns.StoragePop());
        }
        
        private void SetMethodAttributes(KSharpParser.ClassModifiersContext c)
        {
            TypeAttributes a = 0;
            a |= cu.TypeModifierChecker.GetAccessibilityAttributes(c.accessibilityMod());
            a |= TypeModifierChecker.GetSealedAttributes(c.typeSealedMod());
            var type = cu.LocalScopeAgent.types.Top();
            lock (type) {
                type.Attributes |= a;
            }
        }

        public override void EnterClassDeclare(KSharpParser.ClassDeclareContext context)
        {
            var attributes = TypeAttributes.Class;
            cu.LocalScopeAgent.DefineType(context.identifier().ToString(), attributes);
            SetMethodAttributes(context.classModifiers());
        }

        public override void ExitClassDeclare(KSharpParser.ClassDeclareContext context)
        {
            cu.LocalScopeAgent.types.StoragePop();
        }

        public override void EnterMethodDeclare(KSharpParser.MethodDeclareContext context)
        {
            
        }
    }
}
