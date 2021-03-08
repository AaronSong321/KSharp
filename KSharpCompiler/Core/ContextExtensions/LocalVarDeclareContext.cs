using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;



namespace KSharpCompiler.Grammar
{
    public partial class KSharpParser
    {
        public partial class LocalVarDeclareContext
        {
            public string? Name => varTypeDeclare()?.identifier()?.ToString()??varValueDeclare()!.identifier()?.ToString()??varValueDeclare().nameOrDiscard()!.identifier()?.ToString();
            public LocalVariableMutability Mutability {
                get {
                    var mod = localVarModifiers();
                    var t = mod.localVarMutabilityMod();
                    if (t.MUTABLE() != null)
                        return LocalVariableMutability.Mutable;
                    if (t.CONST() != null)
                        return LocalVariableMutability.Const;
                    if (t.CONSTEXPR() != null)
                        return LocalVariableMutability.Constexpr;
                    if (t.CONSTEVAL() != null)
                        return LocalVariableMutability.Consteval;
                    return LocalVariableMutability.Constinit;
                }
            }
            public ExpressionContext? InitValue => varValueDeclare()?.initValue()?.expression();
        }
    }
}