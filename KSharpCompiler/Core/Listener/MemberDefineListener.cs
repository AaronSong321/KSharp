using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using KSharp;
using KSharpCompiler.Grammar;
using Mono.Cecil;


namespace KSharpCompiler
{
    public class MemberDefineListener: BaseListener
    {

        public MemberDefineListener(CompileUnit compileUnit) : base(compileUnit) { }

        public override void EnterFile(KSharpParser.FileContext context)
        {
            cu.LocalScopeAgent.Clear();
        }

        public override void EnterFileScopeNs(KSharpParser.FileScopeNsContext context)
        {
            context.qid().GetIdentifiers().ForEach(t => cu.LocalScopeAgent.ns.MemoryPush());
        }

        public override void EnterNsDeclare(KSharpParser.NsDeclareContext context)
        {
            context.qid().GetIdentifiers().ForEach(t => cu.LocalScopeAgent.ns.MemoryPush());
        }

        public override void ExitNsDeclare(KSharpParser.NsDeclareContext context)
        {
            context.qid().GetIdentifiers().ForEach(t => cu.LocalScopeAgent.ns.MemoryPop());
        }

        public override void EnterClassDeclare(KSharpParser.ClassDeclareContext context)
        {
            cu.LocalScopeAgent.types.MemoryPush();
        }
        public override void ExitClassDeclare(KSharpParser.ClassDeclareContext context)
        {
            cu.LocalScopeAgent.types.MemoryPop();
        }

        private void SetMethodAttributes(KSharpParser.MethodModifiersContext c)
        {
            MethodAttributes a = 0;
            a |= MethodModifierChecker.GetNewAttribute(c.newMod());
            a |= MethodModifierChecker.GetOverrideAttribute(c.overrideMod());
            a |= MethodModifierChecker.GetMethodAccessibilityAttribute(c.accessibilityMod());
            a |= MethodModifierChecker.GetStaticAttributes(c.staticMod());
            var m = cu.LocalScopeAgent.MethodTop;
            m.method.Attributes = a;
        }

        public override void EnterMethodDeclare(KSharpParser.MethodDeclareContext context)
        {
            var memberName = context.memberName();
            if (memberName.type() != null)
                throw new NotImplementedException();
            var methodName = memberName.identifier().GetText();
            var header = context.commonFunctionHeader();
            var parameters = header.parameterList().GetParameters();
            var paramList = parameters.Map(t => new ParameterDefinition(t.parameterDeclarationBodyContext.parameterLocalName()?.identifier()?.ToString()??NameGenAgent.GenerateDiscardParameterName(t.index), ParameterAttributes.None, cu.LocalTypeResolveAgent.ResolveParameterType(t).type!));
            TypeReference rtType;
            if (header.retTypeAnnotator() != null) {
                var r1 = cu.LocalTypeResolveAgent.ResolveType(header.retTypeAnnotator().type());
                if (r1.ErrorState) {
                    r1.ErrorMessage.ForEach(cu.compiler.ErrorCollector.AddCompilerMessage);
                    rtType = cu.compiler.TypeResolveAgent.Void;
                }
                else {
                    rtType = r1.type!;
                }
            }
            else {
                rtType = cu.compiler.TypeResolveAgent.Void;
            }
            cu.LocalScopeAgent.DefineMethod(methodName, paramList, rtType, MethodModifierChecker.GetMutability(context.methodModifiers().mutableMod()));
            SetMethodAttributes(context.methodModifiers());
        }
        public override void ExitMethodDeclare(KSharpParser.MethodDeclareContext context)
        {
            cu.LocalScopeAgent.methods.StoragePop();
        }
    }
}
