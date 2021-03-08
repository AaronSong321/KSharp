using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Mono.Cecil;


namespace KSharpCompiler
{
    public enum ConversionType: ushort
    {
        Identity = 0,
        NumericConversion,
        EnumerationConversion,
        ReferenceConversion,
        Boxing,
        DynamicConversion,
        TypeParameter,
        ConstantExpression,
        UserDefinedImplicitConversion,
        AnonymousFunction,
        MethodGroup,
        NullLiteral,
        Nullable,
        LiftedUserDefinedImplicitConversion,
        None = ushort.MaxValue,
    }

    public static class ConversionTypeExtensions
    {
        public static bool IsBetterThan(this ConversionType a, ConversionType b)
        {
            return (ushort)a < (ushort)b;
        }
        public static bool IsNoWorseThan(this ConversionType a, ConversionType b)
        {
            return (ushort)a <= (ushort)b;
        }
    }
    
    public class ConversionResolveAgent: CompilerAgent
    {

        public ConversionResolveAgent(Compiler compiler) : base(compiler) { }

        public ConversionType GetConversionType(ArgumentType argument, TypeReference parameter)
        {
            var argType = argument.type;
            if (Compiler.ImportAgent.IsSameType(argType, parameter))
                return ConversionType.Identity;
            if (Compiler.TypeResolveAgent.ExistImplicitNumericConversion(argType, parameter))
                return ConversionType.NumericConversion;
            if (Compiler.ImportAgent.IsSameType(argType, Compiler.TypeResolveAgent.Int) && argument.hasConstantValue && argument.constantValue is int k && k is 0)
                return ConversionType.EnumerationConversion;
            var argResolve = Compiler.ImportAgent.Resolve(argType);
            var parResolve = Compiler.ImportAgent.Resolve(parameter);
            if (argResolve is null || parResolve is null)
                return ConversionType.None;
            if (TypeDefinitionExtensions.IsAssignableTo(argResolve, parResolve))
                return ConversionType.ReferenceConversion;
            if (argResolve.IsValueType && Compiler.ImportAgent.IsSameType(argResolve, parResolve))
                return ConversionType.Boxing;
            // TODO: dynamic ...
            if (argument.hasConstantValue && argument.constantValue == null)
                return ConversionType.NullLiteral;
            if (!parResolve.IsValueType || Compiler.ImportAgent.IsSameType(parResolve, Compiler.TypeResolveAgent.Nullable))
                return ConversionType.Nullable;
            return ConversionType.None;
        }
        public bool ExistImplicitConversion(ArgumentType argument, TypeReference parameter)
        {
            return GetConversionType(argument, parameter).IsBetterThan(ConversionType.None);
        }
    }
}
