using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Mono.Cecil;


namespace KSharpCompiler
{
    public enum NumericType
    {
        None,
        SByte,
        Byte,
        Int16,
        UInt16,
        Int32,
        UInt32,
        Int64,
        UInt64,
        Char,
        Float,
        Double,
        Decimal
    }

    public partial class TypeResolveAgent
    {
        private static readonly int[] ImplicitNumericConversionTable = new int[]
        {
            0,
            0b1110010101000,
            0b1110111111000,
            0b1110010100000,
            0b1110010000000,
            0b1110110000000,
            0b1110000000000,
            0b1110000000000,
            0b1110111110000,
            0b0100000000000,
        };

        public NumericType Convert(TypeReference type)
        {
            if (Compiler.ImportAgent.IsSameType(type, Int))
                return NumericType.Int32;
            if (Compiler.ImportAgent.IsSameType(type, Double))
                return NumericType.Double;
            return NumericType.None;
        }

        public static bool ExistImplicitNumericConversion(NumericType argument, NumericType target)
        {
            if ((int)argument >= ImplicitNumericConversionTable.Length)
                return false;
            return (ImplicitNumericConversionTable[(int)argument] & (1 << (int)target)) != 0;
        }

        public bool ExistImplicitNumericConversion(TypeReference argument, TypeReference target)
        {
            return ExistImplicitNumericConversion(Convert(argument), Convert(target));
        }
    }
}
