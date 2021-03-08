using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Antlr4.Runtime.Tree;
using KSharp;
using KSharpCompiler.Grammar;
using Mono.Cecil;


namespace KSharpCompiler
{
    public abstract class Stage3Listener : BaseListener
    {

        protected Stage3Listener(CompileUnit compileUnit) : base(compileUnit) { }
        protected MethodDescriber MethodTop => cu.LocalScopeAgent.MethodTop;
    }
    public class MethodImplementChecker : Stage3Listener
    {
        public MethodImplementChecker(CompileUnit compileUnit) : base(compileUnit) { }

        IEnumerable<int> F1()
        {
            yield return 1;
            yield return 2;
            // for (int i = 0; i < 10; ++i) {
            //     if (i >= 6) {
            //         yield return i;
            //         ++i;
            //     }
            // }
            int i = 0;
            while (i < 10) {
                if (i >= 6) {
                    yield return i;
                    ++i;
                }
                ++i;
            }
        }

        private class F1Gen : IEnumerable<int>, IEnumerator<int>
        {
            private int state;
            private int current;
            private MethodImplementChecker __this;
            private int __i;
            private int initThreadId;
            
            public F1Gen(int state)
            {
                this.state = state;
            }

            private bool MoveNext()
            {
                int v0 = state;
                switch (v0) {
                    case 0:
                        state = -1;
                        current = 1;
                        state = 1;
                        return true;
                    case 1:
                        state = -1;
                        current = 2;
                        state = 2;
                        return true;
                    case 2: state = -1;
                        __i = 0;
                        goto b8;
                        il68:
                        bool v1 = __i < 6;
                        if (!v1)
                            goto a7;
                        current = __i;
                        state = 3;
                        return true;
                    case 3:
                        state = -1;
                        ++__i;
                        a7:
                        ++__i;
                        b8:
                        if (__i < 10)
                            goto il68;
                        return false;
                    default: return false;
                }
            }
            
            bool IEnumerator.MoveNext()
            {
                return MoveNext();
            }

            IEnumerator<int> IEnumerable<int>.GetEnumerator()
            {
                F1Gen v0;
                if (state != -2)
                    goto il22;
                if (initThreadId != Environment.CurrentManagedThreadId)
                    goto il22;
                state = 0;
                v0 = this;
                goto il35;
                il22:
                v0 = new F1Gen(0);
                v0.__this = __this;
                il35:
                return v0;
            }
            int IEnumerator<int>.Current => current;
            object? IEnumerator.Current => current;
            void IEnumerator.Reset()
            {
                throw new InvalidOperationException();
            }
            void IDisposable.Dispose()
            {
            }
            
            IEnumerator IEnumerable.GetEnumerator()
            {
                return (this as IEnumerable<int>).GetEnumerator();
            }
            
        }

        /// <summary>
        /// check if a method (of return type void) doesn't have a return expression at the end
        /// </summary>
        /// <param name="c"></param>
        private bool CheckImplicitReturn(KSharpParser.CommonFunctionBodyContext c)
        {
            var c1 = c.blockExpression();
            if (c1 != null) {
                var lastExpression = c1.expressions()?.expression();
                if (lastExpression?.SingleTo<KSharpParser.ReturnExpressionContext>() is null) {
                    return true;
                }
            }
            return false;
        }

        private bool VoidReturn(MethodDefinition method)
        {
            return compiler.ImportAgent.IsSameType(method.ReturnType, compiler.TypeResolveAgent.Void);
        }
        public override void EnterMethodDeclare(KSharpParser.MethodDeclareContext context)
        {
            if (VoidReturn(MethodTop.method) && CheckImplicitReturn(context.commonFunctionBody()))
                MethodTop.ImplicitVoidReturn = true;
        }
        
        public override void EnterReturnExpression(KSharpParser.ReturnExpressionContext context)
        {
            cu.LocalScopeAgent.MethodTop.returnExpressions.Add(context);
        }
        public override void EnterYieldExpression(KSharpParser.YieldExpressionContext context)
        {
            cu.LocalScopeAgent.MethodTop.yieldExpressions.Add(context);
        }
        public override void ExitMethodDeclare(KSharpParser.MethodDeclareContext context)
        {
            var m = MethodTop;
            var returnType = m.method.ReturnType;
            var resolve = compiler.ImportAgent.Resolve(returnType);
            if (resolve != null) {
                if (resolve.IsAssignableTo(compiler.TypeResolveAgent.IEnumerableDefinition)) {
                    if (resolve.IsAssignableTo(compiler.TypeResolveAgent.IEnumerable1Definition) || resolve.IsAssignableTo(compiler.TypeResolveAgent.IAsyncEnumerable1Definition)) {
                        
                    }
                }
                else {
                    if (m.YieldExpressionCount > 0) {
                        compiler.ErrorCollector.AddCompilerMessage(YieldError.NonEnumerableMethod());
                    }
                }
            }
            
            if (m.ReturnExpressionsCount > 0 && m.YieldExpressionCount > 0) {
                compiler.ErrorCollector.AddCompilerMessage(YieldError.MixYieldAndReturn());
            }
            if (m.ReturnExpressionsCount > 1) {
                var localRetVar = m.GenerateLocalVar(m.method.ReturnType);
                m.returnExpressions.ForEach(t => t.AuxiliaryLocal = localRetVar);
            }
        }
    }
    
    public class MethodImplementationListener: Stage3Listener
    {
        public MethodImplementationListener(CompileUnit compileUnit) : base(compileUnit) { }
        
        public override void EnterFileScopeNs(KSharpParser.FileScopeNsContext context)
        {
            context.qid().GetIdentifiers().ForEach(t => cu.LocalScopeAgent.ns.MemoryPush());
        }
        public override void EnterFile(KSharpParser.FileContext context)
        {
            cu.LocalScopeAgent.Clear();
        }
        public override void EnterNsDeclare(KSharpParser.NsDeclareContext context)
        {
            cu.LocalScopeAgent.ns.MemoryPush();
        }
        public override void ExitNsDeclare(KSharpParser.NsDeclareContext context)
        {
            cu.LocalScopeAgent.ns.MemoryPop();
        }
        public override void EnterClassDeclare(KSharpParser.ClassDeclareContext context)
        {
            cu.LocalScopeAgent.types.MemoryPush();
        }
        public override void ExitClassDeclare(KSharpParser.ClassDeclareContext context)
        {
            cu.LocalScopeAgent.types.MemoryPop();
        }
        public override void EnterMethodDeclare(KSharpParser.MethodDeclareContext context)
        {
            cu.LocalScopeAgent.methods.MemoryPush();
            var lis = new MethodImplementChecker(cu);
            new ParseTreeWalker().Walk(lis, context);
            
            var body = context.commonFunctionBody().blockExpression().expressions();
            foreach (var expression in body.GetExpressions()) {
                var r1 = cu.ExpressionEmitter.Emit(expression);
                if (r1.ErrorState) {
                    r1.ErrorMessage.ForEach(t => compiler.ErrorCollector.AddCompilerMessage(t));
                }
            }
        }
        public override void ExitMethodDeclare(KSharpParser.MethodDeclareContext context)
        {
            if (MethodTop.ImplicitVoidReturn) {
                MethodTop.method.Body.GetILProcessor().Return();
            }
            cu.LocalScopeAgent.methods.MemoryPop();
        }
    }
}
