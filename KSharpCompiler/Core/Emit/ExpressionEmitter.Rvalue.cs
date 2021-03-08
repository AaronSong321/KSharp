

using System;
using System.Collections.Generic;
using System.Linq;
using KSharp;
using KSharpCompiler.Grammar;
using Mono.Cecil;

namespace KSharpCompiler
{
    public partial class ExpressionEmitter
    {
        private ILInstructionGroup EmitRvalue(KSharpParser.InfixCallExpressionContext c)
        {
            return Emit(c);
        }

        private TypeReference ResolveArgument(KSharpParser.ArgumentContext c)
        {
            var r2 = CompileUnit.LocalTypeResolveAgent.ResolveExpressionType(c.expression());
            return r2.ErrorState ? Compiler.TypeResolveAgent.Void : r2.type!;
        }
        
        private (ArgumentType[], NamedArgumentType[], List<CompilerMessage>) ResolveArgumentList(KSharpParser.ArgumentListContext c)
        {
            var positional = c.GetPositionalArguments();
            var e1 = new List<CompilerMessage>(); 
            var r3 = new ArgumentType[positional.Count];
            for (int i = 0; i < positional.Count; ++i) {
                var r4 = CompileUnit.LocalTypeResolveAgent.ResolveExpressionType(positional[i].argument().expression());
                r3[i] = new ArgumentType(r4.type??Compiler.TypeResolveAgent.Void, positional[i].argument().GetPassMode(), i, null, false);
                if (r4.ErrorState)
                    e1.AddRange(r4.ErrorMessage);
            }
            var r2 = new NamedArgumentType[c.GetNamedArguments().Count];
            var named = c.GetNamedArguments();
            for (int i = 0; i < r2.Length; ++i) {
                var c1 = named[i].argument();
                var r4 = CompileUnit.LocalTypeResolveAgent.ResolveExpressionType(c1.expression());
                r2[i] = new NamedArgumentType(r4.type??Compiler.TypeResolveAgent.Void, c1.GetPassMode(), i, named[i].argumentName().GetText(), null, false);
                if (r4.ErrorState)
                    e1.AddRange(r4.ErrorMessage);
            }
            return (r3, r2, e1);
        }

        private ILInstructionGroup EmitInvoke(MethodDefinition method)
        {
            return method.IsStatic ? new ILInstructionGroup(Il.CallStatic(method)) : new ILInstructionGroup(Il.CallInstance(method));
        }

        private ILInstructionGroup EmitMove(KSharpParser.InfixCallExpressionContext lvalue, KSharpParser.InfixCallExpressionContext rvalue)
        {
            var lvalueLastExpression = lvalue.prefixExpression().postfixExpression();
            while (lvalueLastExpression.postfixExpression()?.postfixSuffix() != null) {
                lvalueLastExpression = lvalueLastExpression.postfixExpression();
            }
            if (lvalueLastExpression.postfixExpression() is null) {
                var c1 = lvalueLastExpression.primaryExpression();
                var c2 = c1.gid();
                if (c2 != null) {
                    var r1 = CompileUnit.LocalTypeResolveAgent.ResolveGid(c2);
                    if (r1.ErrorState) {
                        return new ILInstructionGroup(r1.ErrorMessage);
                    }
                    ILInstructionGroup? r2;
                    switch (r1.kind) {
                        case IdentifierResolveResult.UnionCase.LocalDefinition:
                            r2 = new ILInstructionGroup(Il.StoreLocal(r1.localDefinition!.variableDefinition.Index));
                            break;
                        case IdentifierResolveResult.UnionCase.FieldDefinition:
                            r2 = new ILInstructionGroup(Il.StoreField(r1.fieldDefinition!));
                            break;
                        case IdentifierResolveResult.UnionCase.PropertyDefinition:
                            var m1 = r1.propertyDefinition!;
                            if (m1.HasParameters)
                                r2 = new ILInstructionGroup(MemberAccessError.IndexerNoParameter(m1.properties[0]));
                            else {
                                var t1 = CompileUnit.LocalTypeResolveAgent.ResolveExpressionType(rvalue);
                                var m2 = Compiler.MethodResolveAgent.ResolveMethod(m1.Setters, MethodInvokeKind.PropertySet, Array.Empty<ArgumentType>(), Array.Empty<NamedArgumentType>(), new ArgumentType(t1.type!, ArgumentPassMode.Value, 0, null, false));
                                if (m2.IsSingleMatch) {
                                    var m3 = m2.matches![0].signature.origin;
                                    var m4 = ImportAgent.Import(m3);
                                    r2 = new ILInstructionGroup(m3.IsStatic ? Il.CallStatic(m4) : Il.CallInstance(m4));
                                }
                                else if (m2.ErrorState) {
                                    r2 = new ILInstructionGroup(m2.ErrorMessage);
                                }
                                else {
                                    r2 = new ILInstructionGroup(MethodResolveFailure.AmbiguousMatch(m2.matches!.Map(t => t.signature.origin)));
                                }
                            }
                            break;
                        case IdentifierResolveResult.UnionCase.ParameterDefinition:
                            r2 = new ILInstructionGroup(InvalidSet.SetParameter(r1.parameterDefinition!));
                            break;
                        case IdentifierResolveResult.UnionCase.TypeDefinition:
                        case IdentifierResolveResult.UnionCase.EventDefinition:
                            r2 = new ILInstructionGroup(MemberAccessError.CannotSetMember(((r1.typeDefinition as IMemberDefinition)??r1.eventDefinition)!));
                            break;
                        case IdentifierResolveResult.UnionCase.NamespaceDefinition:
                            r2 = new ILInstructionGroup(MemberAccessError.CannotSetNamespace(r1.namespaceDefinition!));
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                    return r2;
                }
                var c3 = c1.keywordExpression().backupAccess();
                if (c3 != null) {
                    
                }
                {
                    var e1 = NotLvalue.E(c1);
                    return new ILInstructionGroup(e1);
                }
            }
            {
                var c1 = lvalueLastExpression.postfixSuffix();
                var c2 = c1.elementAccessSuffix();
                var r1 = CompileUnit.LocalTypeResolveAgent.ResolveGid(c2.gid());
                if (r1.ErrorState) {
                    return new ILInstructionGroup(r1.ErrorMessage);
                }
                if (r1.kind == IdentifierResolveResult.UnionCase.PropertyDefinition) {
                    var m1 = r1.propertyDefinition!;
                    if (!m1.HasParameters) {
                        return new ILInstructionGroup(MemberAccessError.AccessIndexerMember(m1));
                    }
                    var (r2, r3, e1) = ResolveArgumentList(c2.argumentList());
                    var r4 = CompileUnit.LocalTypeResolveAgent.ResolveExpressionType(rvalue);
                    var r5 = Compiler.MethodResolveAgent.ResolveMethod(m1.Setters, MethodInvokeKind.IndexerSet, r2, r3,
                        new ArgumentType(r4.type??Compiler.TypeResolveAgent.Void, rvalue.SingleTo<KSharpParser.RefExpressionContext>() != null ? ArgumentPassMode.Ref : ArgumentPassMode.Value, r2.Length + r3.Length, null, false));
                    e1.AddRange(r4.ErrorMessage);
                    e1.AddRange(r5.ErrorMessage);
                    if (!r5.IsSingleMatch) {
                        e1.Add(MethodResolveFailure.AmbiguousMatch(r5.ToMethods()));
                    }
                    return e1.Count is 0 ? EmitInvoke(r5.ToMethods()[0]) : new ILInstructionGroup(e1);
                }
                if (r1.kind == IdentifierResolveResult.UnionCase.NamespaceDefinition)
                    return new ILInstructionGroup(MemberAccessError.CannotSetNamespace(r1.namespaceDefinition!));
                return new ILInstructionGroup(MemberAccessError.CannotSetMember(r1.eventDefinition??(IMemberDefinition)r1.fieldDefinition??r1.typeDefinition!));
            }
        }
    }
}