using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Mono.Cecil;
using Mono.Cecil.Cil;


namespace KSharpCompiler
{
    public enum LocalVariableMutability
    {
        Error = 0,
        Mutable,
        Const,
        Constexpr,
        Constinit,
        Consteval
    }

    public static class LocalVariableMutabilityExtensions
    {
        public static bool IsMutable(this LocalVariableMutability mutability)
        {
            return mutability == LocalVariableMutability.Error || mutability == LocalVariableMutability.Mutable;
        }
    }
    
}
