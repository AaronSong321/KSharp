using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using KSharp;
using KSharpCompiler.Grammar;
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
#pragma warning disable 8509
            var instruction = argumentIndexAboveZero <= 3 ? il.Create(argumentIndexAboveZero switch {
#pragma warning restore 8509
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
#pragma warning disable 8509
            var ins = number <= 8 && number >= -1 ? number switch {
#pragma warning restore 8509
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
        public static Instruction LoadConstant(this ILProcessor il, bool number)
        {
            var ins = il.Create(number? OpCodes.Ldc_I4_1: OpCodes.Ldc_I4_0);
            il.Append(ins);
            return ins;
        }

        public static Instruction LoadConstant(this ILProcessor il, char value)
        {
            return LoadConstant(il, (int)value);
        }
        
        public static Instruction LoadNull(this ILProcessor il)
        {
            var ins = il.Create(OpCodes.Ldnull);
            il.Append(ins);
            return ins;
        }

        public static Instruction LoadLocal(this ILProcessor il, int index)
        {
            var ins = index switch {
                0 => il.Create(OpCodes.Ldloc_0),
                1 => il.Create(OpCodes.Ldloc_1),
                2 => il.Create(OpCodes.Ldloc_1),
                3 => il.Create(OpCodes.Ldloc_1),
                _ => il.Create(OpCodes.Ldloc_S, (ushort)index)
            };
            il.Append(ins);
            return ins;
        }
        public static Instruction LoadField(this ILProcessor il, FieldReference r)
        {
            var ins = il.Create(OpCodes.Ldfld, r);
            il.Append(ins);
            return ins;
        }
        public static VariableDefinition AddVariable(this MethodBody m, TypeReference type, string name)
        {
            var v = new VariableDefinition(type);
            m.Variables.Add(v);
            return v;
        }

        public static Instruction DuplicateStackTop(this ILProcessor il)
        {
            var ins = il.Create(OpCodes.Dup);
            il.Append(ins);
            return ins;
        }

        public static Instruction StoreField(this ILProcessor il, FieldReference r)
        {
            var ins = il.Create(OpCodes.Stfld, r);
            il.Append(ins);
            return ins;
        }

        public static Instruction StoreLocal(this ILProcessor il, int index)
        {
            checked {
                var ins = index switch {
                    0 => il.Create(OpCodes.Stloc_0),
                    1 => il.Create(OpCodes.Stloc_1),
                    2 => il.Create(OpCodes.Stloc_2),
                    3 => il.Create(OpCodes.Stloc_3),
                    _ => il.Create(OpCodes.Stloc_S, (ushort)index)
                };
                il.Append(ins);
                return ins;
            }
        }

        public static ILInstructionGroup NewArray(this ILProcessor il, TypeReference type, int length)
        {
            var ins1 = il.LoadConstant(length);
            var ins2 = il.Create(OpCodes.Newarr, type);
            il.Append(ins1);
            il.Append(ins2);
            return new ILInstructionGroup(ins1, ins2);
        }

        public static Instruction NewObject(this ILProcessor il, MethodReference constructor)
        {
            var ins = il.Create(OpCodes.Newobj, constructor);
            il.Append(ins);
            return ins;
        }
        public static Instruction Box(this ILProcessor il)
        {
            var ins = il.Create(OpCodes.Box);
            il.Append(ins);
            return ins;
        }
        public static Instruction Unbox(this ILProcessor il)
        {
            var ins = il.Create(OpCodes.Unbox);
            il.Append(ins);
            return ins;
        }
    }

    public partial class ExpressionEmitter : CompileUnitAgent
    {
        private ImportAgent ImportAgent => Compiler.ImportAgent;
        public Instruction IlStoreArrayElement(TypeReference type)
        {
            if (ImportAgent.IsSameType(Compiler.TypeResolveAgent.Int, type)) {
                var ins = Il.Create(OpCodes.Stelem_I4);
                Il.Append(ins);
                return ins;
            }
            else if (ImportAgent.IsSameType(Compiler.TypeResolveAgent.Double, type)) {
                var ins = Il.Create(OpCodes.Stelem_R8);
                Il.Append(ins);
                return ins;
            }
            else {
                var ins = Il.Create(OpCodes.Stelem_Any);
                Il.Append(ins);
                return ins;
            }
        }
        
        public ILInstructionGroup EmitLiteralArray(TypeReference type, IList<object> items)
        {
            var g = new List<ILInstructionGroup> { Il.NewArray(type, items.Count) };
            for (int index = 0; index < items.Count; index++) {
                var item = items[index];
                Il.DuplicateStackTop();
                Il.LoadConstant(index);
                switch (item) {
                    case KSharpParser.ExpressionContext c:
                        Emit(c);
                        break;
                    case int c:
                        Il.LoadConstant(c);
                        break;
                    case bool c:
                        Il.LoadConstant(c);
                        break;
                    case string c:
                        Il.LoadConstant(c);
                        break;
                    default:
                        throw new NotImplementedException();
                }
                g.Add(new ILInstructionGroup(IlStoreArrayElement(type)));
            }
            return new ILInstructionGroup(g);
        }

        public ILInstructionGroup EmitInterpStringArray(IList<object> items)
        {
            var type = Compiler.TypeResolveAgent.String;
            var g = new List<ILInstructionGroup> { Il.NewArray(type, items.Count) };
            for (int index = 0; index < items.Count; index++) {
                var item = items[index];
                Il.DuplicateStackTop();
                Il.LoadConstant(index);
                switch (item) {
                    case KSharpParser.ExpressionContext c:
                        Emit(c);
                        var t = CompileUnit.LocalTypeResolveAgent.ResolveExpressionType(c);
                        if (!t.ErrorState && t.type!.IsValueType)
                            Il.Box();
                        break;
                    case (KSharpParser.ExpressionContext c, string f) :
                        Emit(c);
                        Il.LoadConstant(f);
                        Il.CallInstance(toStringMethod);
                        break;
                    case string c:
                        Il.LoadConstant(c);
                        break;
                    default:
                        throw new NotImplementedException();
                }
                g.Add(new ILInstructionGroup(IlStoreArrayElement(type)));
            }
            return new ILInstructionGroup(g);
        }
    }
}
