using System;
using Mono.Cecil;

namespace KSharpCompiler
{
    [Flags]
    public enum ArgumentPassMode
    {
        Value,
        In,
        Out,
        Ref
    }

    public static class ArgumentPassModeExtensions
    {
        private const ParameterAttributes ByPointerBits = ParameterAttributes.In | ParameterAttributes.Out;
        
        private static bool IsByValue(this ArgumentPassMode mode)
        {
            return mode == ArgumentPassMode.Value;
        }
        private static bool IsByPointer(this ArgumentPassMode mode)
        {
            return mode != ArgumentPassMode.Value;
        }
        public static bool IsByPointer(this ParameterAttributes p)
        {
            return (p & ParameterAttributes.In) != 0 || (p & ParameterAttributes.Out) != 0;
        }
        private static bool IsByValue(this ParameterAttributes p)
        {
            return !IsByPointer(p);
        }
        // public static bool IsByPointer(this ParameterDefinition p)
        // {
        //     return IsByPointer(p.Attributes);
        // }
        // public static bool IsByValue(this ParameterDefinition p)
        // {
        //     return IsByValue(p.Attributes);
        // }

        private static ParameterAttributes GetByPointerBits(ParameterAttributes p)
        {
            return p | ByPointerBits;
        }

        public static bool ByPointerModePerfectMatch(ArgumentType argument, in ParameterResolveSignature parameter)
        {
            return !(argument.passMode.IsByPointer() ^ parameter.attributes.IsByPointer());
        }
        
    }
}
