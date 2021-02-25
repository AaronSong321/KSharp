using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Mono.Cecil;
using Mono.Cecil.Cil;


namespace KSharpCompiler
{
    public static class EmitExtensions
    {
        public static Instruction LoadArgument(this ILProcessor il, uint argumentIndexAboveZero)
        {
            if (argumentIndexAboveZero == 0)
                throw new ArgumentException($"Argument index starts from 1. Use {nameof(LoadThis)} to load this argument.");
            var instruction = argumentIndexAboveZero <= 3 ? il.Create(argumentIndexAboveZero switch {
                1 => OpCodes.Ldarg_1,
                2 => OpCodes.Ldarg_2,
                3 => OpCodes.Ldarg_3
            }) : il.Create(OpCodes.Ldarg_S, argumentIndexAboveZero);
            il.Append(instruction);
            return instruction;
        }
        public static Instruction LoadThis(this ILProcessor il)
        {
            var instruction = il.Create(OpCodes.Ldarg_0);
            il.Append(instruction);
            return instruction;
        }

        public static Instruction CallStatic(this ILProcessor il, MethodReference method)
        {
            var ins = il.Create(OpCodes.Call, method);
            il.Append(ins);
            return ins;
        }
        public static Instruction CallBase(this ILProcessor il, MethodReference method)
        {
            var ins = il.Create(OpCodes.Call, method);
            il.Append(ins);
            return ins;
        }

        public static Instruction CallInstance(this ILProcessor il, MethodReference method)
        {
            var ins = il.Create(OpCodes.Callvirt, method);
            il.Append(ins);
            return ins;
        }

        public static Instruction Return(this ILProcessor il)
        {
            var ins = il.Create(OpCodes.Ret);
            il.Append(ins);
            return ins;
        }

        public static Instruction LoadConstant(this ILProcessor il, string s)
        {
            var ins = il.Create(OpCodes.Ldstr, s);
            il.Append(ins);
            return ins;
        }
        public static Instruction LoadConstant(this ILProcessor il, int number)
        {
            var ins = number <= 8 && number >= -1 ? number switch {
                    -1 => il.Create(OpCodes.Ldc_I4_M1),
                    0 => il.Create(OpCodes.Ldc_I4_0),
                    1 => il.Create(OpCodes.Ldc_I4_1),
                    2 => il.Create(OpCodes.Ldc_I4_2),
                    3 => il.Create(OpCodes.Ldc_I4_3),
                    4 => il.Create(OpCodes.Ldc_I4_4),
                    5 => il.Create(OpCodes.Ldc_I4_5),
                    6 => il.Create(OpCodes.Ldc_I4_6),
                    7 => il.Create(OpCodes.Ldc_I4_7),
                    8 => il.Create(OpCodes.Ldc_I4_8)
                }
                : il.Create(OpCodes.Ldc_I4, number);
            il.Append(ins);
            return ins;
        }
        public static Instruction LoadConstant(this ILProcessor il, long number)
        {
            var ins = il.Create(OpCodes.Ldc_I8, number);
            il.Append(ins);
            return ins;
        }
    }
}
