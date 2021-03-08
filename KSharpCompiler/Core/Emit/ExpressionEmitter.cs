using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using KSharp;
using KSharpCompiler.Grammar;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;


namespace KSharpCompiler
{

    public enum MethodStaticQuality
    {
        Static,
        Instance,
        Both
    }
    
    [NoThreading]
    public partial class ExpressionEmitter: CompileUnitAgent
    {
        public MethodDescriber CurrentMethodDescriber => CompileUnit.LocalScopeAgent.methods.Top(); 
        public MethodDefinition CurrentMethod => CurrentMethodDescriber.method;
        private MethodBody MethodBody => CurrentMethod.Body;
        private ILProcessor Il => MethodBody.GetILProcessor();
        private readonly MethodReference concatMethod;
        private readonly MethodReference toStringMethod;
        private readonly MethodReference formatMethod;

        public ExpressionEmitter(CompileUnit compileUnit) : base(compileUnit)
        {
            var stringType = Compiler.TypeResolveAgent.String;
            var objectType = ImportAgent.Resolve(Compiler.TypeResolveAgent.Object)!;
            var concatMethod = ImportAgent.Resolve(stringType)!.GetMethods().First(t => t.Name == "Concat" && t.Parameters.Count == 1 && ImportAgent.IsSameType(t.Parameters[0].ParameterType, new ArrayType(Compiler.TypeResolveAgent.Object)));
            this.concatMethod = ImportAgent.Import(concatMethod);
            toStringMethod = ImportAgent.Import(objectType.Methods.First(t => t.Name == "ToString" && t.Parameters.Count is 0));
            formatMethod = ImportAgent.Import(ImportAgent.Resolve(stringType)!.GetMethods().First(t => t.Name == "Format" && t.Parameters.Count is 2 && ImportAgent.IsSameType(t.Parameters[0].ParameterType, stringType) && ImportAgent.IsSameType(t.Parameters[1].ParameterType, new ArrayType(objectType))));
        }

        public ILInstructionGroup Emit(KSharpParser.ExpressionContext c)
        {
            return Emit(c.lowestPriorityExpression());
        }

        private ILInstructionGroup Emit(KSharpParser.LowestPriorityExpressionContext c)
        {
            return Emit(c.assignmentExpression());
        }

        private ILInstructionGroup Emit(KSharpParser.AssignmentExpressionContext c)
        {
            if (c.assignmentExpression() != null) {
                var rvalue = c.assignmentExpression().infixCallExpression();
                var lvalue = c.infixCallExpression();
                return EmitStore(lvalue, rvalue);
            }
            else {
                return Emit(c.infixCallExpression());
            }
        }

        private ILInstructionGroup Emit(KSharpParser.InfixCallExpressionContext c)
        {
            if (c.infixCallExpression() != null)
                throw new NotImplementedException();
            return Emit(c.prefixExpression());
        }

        private ILInstructionGroup EmitStore(KSharpParser.InfixCallExpressionContext lvalue, KSharpParser.InfixCallExpressionContext rvalue)
        {
            var r1 = EmitLvalue(lvalue);
            throw new NotImplementedException();
        }
        

        private ILInstructionGroup Emit(KSharpParser.PrefixExpressionContext c)
        {
            if (c.awaitExpression() != null)
                throw new NotImplementedException();
            if (c.prefixUnaryOperator() != null)
                throw new NotImplementedException();
            if (c.postfixExpression() != null)
                return Emit(c.postfixExpression());
            throw new NotImplementedException();
        }

        private ILInstructionGroup Emit(KSharpParser.PostfixExpressionContext c)
        {
            if (c.postfixSuffix() != null)
                throw new NotImplementedException();
            if (c.primaryExpression() != null)
                return Emit(c.primaryExpression());
            throw new NotImplementedException();
        }
        

        private ILInstructionGroup Emit(KSharpParser.PrimaryExpressionContext c)
        {
            var c1 = c.localVarDeclare();
            if (c1 != null) {
                return Emit(c1);
            }
            var c2 = c.parenthesisedExpression();
            if (c2 != null) {
                return Emit(c2.expression());
            }
            var c3 = c.literalExpression();
            if (c3 != null)
                return Emit(c3);
            var c4 = c.interpolatedString();
            if (c4 != null)
                return Emit(c4);
            var c5 = c.gid();
            if (c5 != null)
                return Emit(c5);
            var c6 = c.flowExpression();
            if (c6 != null)
                return Emit(c6);
            throw new NotImplementedException();
        }

        private ILInstructionGroup Emit(KSharpParser.FlowExpressionContext c)
        {
            throw new NotImplementedException();
        }

        private ILInstructionGroup Emit(KSharpParser.GidContext c)
        {
            var r1 = CompileUnit.LocalTypeResolveAgent.ResolveGid(c);
            return r1.kind switch {
                IdentifierResolveResult.UnionCase.Failure => new ILInstructionGroup(r1.ErrorMessage),
                IdentifierResolveResult.UnionCase.ParameterDefinition => new ILInstructionGroup(Il.LoadArgument((uint)r1.parameterDefinition!.Index)),
                IdentifierResolveResult.UnionCase.LocalDefinition => new ILInstructionGroup(Il.LoadLocal(r1.localDefinition!.variableDefinition.Index)),
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        private void AddNormalString(KSharpParser.InterpStringPartContext t, StringBuilder sb)
        {
            if (t.IS_EscapeChar() != null) {
#pragma warning disable 8509
                sb.Append(t.GetText() switch {
#pragma warning restore 8509
                    "\\\\" => '\\',
                    "\\\n" => '\n',
                    "\\\t" => '\t',
                    "\\\a" => '\a',
                    "\\\r" => '\r',
                    "\\\f" => '\f',
                    "\\\'" => '\\',
                    "\\\"" => '\"',
                    "\\\0" => '\0',
                    "\\\b" => '\b',
                    "\\\v" => '\v',
                });
            }
            else if (t.IS_NormalString() != null)
                sb.Append(t.GetText());
            else if (t.IS_DOUBLELCURL() != null)
                sb.Append('{');
        }

        private ILInstructionGroup CreateConcatString(KSharpParser.InterpolatedStringContext c)
        {
            var list = new List<object>();
            var sb = new StringBuilder();
            foreach (var t in c.interpStringPart()) {
                if (t.interpValue() != null) {
                    if (sb.Length != 0)
                        list.Add(sb.ToString());
                    var ex1 = t.interpValue().expression();
                    list.Add(ex1);
                    sb.Clear();
                }
                else {
                    AddNormalString(t, sb);
                }
            }
            if (sb.Length != 0) {
                list.Add(sb.ToString());
            }
            if (list.Count is 1) {
                var r1 = Il.LoadConstant(list[0].ToString()!);
                return new ILInstructionGroup(r1);
            }
            else {
                var r1 = EmitInterpStringArray(list);
                var r2 = Il.CallStatic(concatMethod);
                return new ILInstructionGroup(r1, r2);
            }
        }

        private ILInstructionGroup CreateFormatString(KSharpParser.InterpolatedStringContext c)
        {
            var sb = new StringBuilder();
            var list = new List<object>();
            int interpValueIndex = -1;
            foreach (var t in c.interpStringPart()) {
                if (t.interpValue() != null) {
                    var ex1 = t.interpValue().expression();
                    list.Add(ex1);
                    var format = t.interpValue().interpFormat();
                    sb.Append(format is null ? $"{{{++interpValueIndex}}}" : $"{{{++interpValueIndex}:{format.GetText()}}}");
                    sb.Clear();
                }
                else {
                    AddNormalString(t, sb);
                }
            }
            var r1 = Il.LoadConstant(sb.ToString());
            EmitInterpStringArray(list);
            var r3 = Il.CallStatic(formatMethod);
            return new ILInstructionGroup(r1, r3);
        }

        private ILInstructionGroup Emit(KSharpParser.InterpolatedStringContext c)
        {
            var parts = c.interpStringPart();
            bool useFormat = parts.Any(t => t.interpValue()?.interpFormat() != null);
            return useFormat ? CreateFormatString(c) : CreateConcatString(c);
        }

        private ILInstructionGroup Emit(KSharpParser.LiteralExpressionContext c)
        {
            var r = Compiler.TypeResolveAgent.ResolveLiteralValue(c);
            if (r.ErrorState)
                return new ILInstructionGroup(r.ErrorMessage);
            var literalValue = r.value!;
            var r2 = r.literalType switch {
                LiteralResolveResult.LiteralType.Bool => Il.LoadConstant((bool)literalValue),
                LiteralResolveResult.LiteralType.Int => Il.LoadConstant((int)literalValue),
                LiteralResolveResult.LiteralType.String => Il.LoadConstant((string)literalValue),
                LiteralResolveResult.LiteralType.Void => Il.LoadNull(),
                _ => throw new NotImplementedException()
            };
            return new ILInstructionGroup(r2);
        }

        private ILInstructionGroup Emit(KSharpParser.LocalVarDeclareContext c)
        {
            var locType = CompileUnit.LocalTypeResolveAgent.ResolveLocalVarType(c);
            if (locType.ErrorState) {
                locType.ErrorMessage!.ForEach(Compiler.ErrorCollector.AddCompilerMessage);
                return ILInstructionGroup.Empty;
            }
            string? name = c.Name;
            if (name is null) {
                throw new NotImplementedException();
            }
            var loc = CurrentMethodDescriber.DefineLocalVar(name, locType.type!, c.Mutability, false);
            var index = loc.variableDefinition.Index;
            var initValue = c.InitValue;
            if (initValue != null) {
                var p = Emit(initValue);
                var ins2 = Il.StoreLocal(index);
                return new ILInstructionGroup(p, ins2);
            }
            return ILInstructionGroup.Empty;
        }


        // /// <summary>
        // /// Push a null value as this argument onto the stack, push all normal arguments onto the stack, then call the given method
        // /// </summary>
        // /// <returns></returns>
        // private ILInstructionGroup EmitGlobalInvoke(MethodReference method, params KSharpParser.IExpressionContext[] parameters)
        // {
        //     var pushNull = Il.LoadNull();
        //     var pa = parameters.Map(Emit);
        //     var call = Il.CallStatic(method);
        //     var g = new List<ILInstructionGroup> { new ILInstructionGroup(pushNull) };
        //     g.AddRange(pa);
        //     g.Add(new ILInstructionGroup(call));
        //     return new ILInstructionGroup(g);
        // }
    }
}
