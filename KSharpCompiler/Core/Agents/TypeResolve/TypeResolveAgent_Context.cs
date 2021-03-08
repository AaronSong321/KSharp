using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using KSharp;
using KSharpCompiler.Grammar;
using Mono.Cecil;


namespace KSharpCompiler
{
    
    public class TypeResolveResult: ResolveResult
    {
        public readonly TypeReference? type;

        public TypeResolveResult(CompilerMessage errorMessage): base(errorMessage)
        {
        }
        public TypeResolveResult(TypeReference type)
        {
            this.type = type;
        }
        public TypeResolveResult(List<CompilerMessage> errorMessage) : base(errorMessage) { }

        public static implicit operator TypeResolveResult(TypeReference type)
        {
            return new TypeResolveResult(type);
        }
        public static implicit operator TypeResolveResult(CompilerMessage message)
        {
            return new TypeResolveResult(message);
        }
    }
    
    public partial class LocalTypeResolveAgent: CompileUnitAgent
    {
        public LocalTypeResolveAgent(CompileUnit compileUnit) : base(compileUnit) { }
        public TypeReference Int => Compiler.TypeResolveAgent.Int;
        public TypeReference Float => Compiler.TypeResolveAgent.Float;
        public TypeReference Double => Compiler.TypeResolveAgent.Double;
        public TypeReference Void => Compiler.TypeResolveAgent.Void;
        public TypeReference String => Compiler.TypeResolveAgent.String;
        public TypeReference Boolean => Compiler.TypeResolveAgent.Boolean;

        public TypeResolveResult ResolveLocalVarType(KSharpParser.LocalVarDeclareContext c)
        {
            var c1 = c.varTypeDeclare();
            if (c1 != null) {
                return ResolveType(c1.localVarTypeAnnotator().type());
            }
            var c2 = c.varValueDeclare()!;
            return c2.localVarTypeAnnotator() != null ? ResolveType(c2.localVarTypeAnnotator().type()) : ResolveExpressionType(c2.initValue().expression());
        }
        public TypeResolveResult ResolveExpressionType(KSharpParser.ExpressionContext c)
        {
            if (c.ExpressionType != null)
                return new TypeResolveResult(c.ExpressionType);
            var c1 = c.lowestPriorityExpression().assignmentExpression();
            var r1 = ResolveExpressionType(c1);
            c.ExpressionType ??= r1.type;
            return r1;
        }

        private TypeResolveResult ResolveExpressionType(KSharpParser.AssignmentExpressionContext c)
        {
            if (c.ExpressionType != null)
                return new TypeResolveResult(c.ExpressionType);
            return ResolveExpressionType(c.infixCallExpression());
        }

        public TypeResolveResult ResolveExpressionType(KSharpParser.InfixCallExpressionContext c)
        {
            if (c.ExpressionType != null)
                return new TypeResolveResult(c.ExpressionType);
            if (c.infixCallExpression() != null)
                throw new NotImplementedException();
            return ResolveExpressionType(c.prefixExpression());
        }

        private TypeResolveResult ResolveExpressionType(KSharpParser.PrefixExpressionContext c)
        {
            if (c.ExpressionType != null)
                return new TypeResolveResult(c.ExpressionType);
            if (c.prefixUnaryOperator() != null)
                throw new NotImplementedException();
            return ResolveExpressionType(c.postfixExpression());
        }

        private TypeResolveResult ResolveExpressionType(KSharpParser.PostfixExpressionContext c)
        {
            if (c.ExpressionType != null)
                return new TypeResolveResult(c.ExpressionType);
            var c1 = c.postfixSuffix();
            if (c1 != null) {
                var r1 = ResolveExpressionType(c.postfixExpression());
                if (r1.ErrorState)
                    return r1;
                var r2 = new IdentifierResolveResult(Compiler.ImportAgent.Resolve(r1.type!)!);
                if (c1.memberAccessSuffix() != null) {
                    var r3 = Compiler.TypeResolveAgent.ResolveNestedGid(c1.memberAccessSuffix().gid(), r2);
                    var r4 = r3.kind switch {
                        IdentifierResolveResult.UnionCase.FieldDefinition => r3.fieldDefinition!.FieldType,
                        IdentifierResolveResult.UnionCase.PropertyDefinition => r3.propertyDefinition!.HasParameters ? (TypeResolveResult)MemberResolveError.WrongAccessType() : r3.propertyDefinition.PropertyType,
                        _ => (TypeResolveResult)MemberResolveError.WrongAccessType()
                    };
                    c.ExpressionType ??= r4.type;
                    return r4;
                }
                if (c1.elementAccessSuffix() != null) {
                    var c2 = c1.elementAccessSuffix().gid();
                    var r3 = Compiler.TypeResolveAgent.ResolveNestedGid(c2, r2);
                    var r4 = r3.kind switch {
                        IdentifierResolveResult.UnionCase.PropertyDefinition => r3.propertyDefinition!.HasParameters ? r3.propertyDefinition.PropertyType : (TypeResolveResult)MemberResolveError.WrongAccessType(),
                        _ => (TypeResolveResult)MemberResolveError.WrongAccessType()
                    };
                    c.ExpressionType ??= r4.type;
                    return r4;
                }
                throw new NotImplementedException();
            }
            else {
                var c2 = c.primaryExpression();
                if (c2.gid() != null) {
                    var r1 = ResolveGid(c2.gid());
                    var r2 = r1.kind switch {
                        IdentifierResolveResult.UnionCase.LocalDefinition => (TypeResolveResult)r1.localDefinition!.variableDefinition.VariableType,
                        IdentifierResolveResult.UnionCase.FieldDefinition => r1.fieldDefinition!.FieldType,
                        IdentifierResolveResult.UnionCase.PropertyDefinition => r1.propertyDefinition!.HasParameters ? (TypeResolveResult)MemberResolveError.WrongAccessType() : r1.propertyDefinition.PropertyType,
                        _ => MemberResolveError.WrongAccessType()
                    };
                    c.ExpressionType ??= r2.type;
                    return r2;
                }
                if (c2.keywordExpression() != null) {
                    var c3 = c2.keywordExpression();
                    if (c3.thisAccess() != null) {
                        c.ExpressionType = CompileUnit.ExpressionEmitter.CurrentMethod.DeclaringType;
                        return c.ExpressionType;
                    }
                    if (c3.baseAccess() != null) {
                        c.ExpressionType = CompileUnit.ExpressionEmitter.CurrentMethod.DeclaringType.BaseType;
                        return c.ExpressionType;
                    }
                    if (c3.valueAccess() != null) {
                        c.ExpressionType = CompileUnit.ExpressionEmitter.CurrentMethod.ReturnType;
                        return c.ExpressionType;
                    }
                    
                }
                if (c2.literalExpression() != null) {
                    var c3 = c2.literalExpression();
                    var r1 = Compiler.TypeResolveAgent.ResolveLiteralValue(c3);
                    c.ExpressionType = r1.type;
                    return r1.ErrorState ? new TypeResolveResult(r1.ErrorMessage) : new TypeResolveResult(r1.type!);
                }
                if (c2.interpolatedString() != null) {
                    c.ExpressionType = String;
                    return ResolveString(c2.interpolatedString());
                }
            }
            throw new NotImplementedException();
        }

        public TypeResolveResult ResolveString(KSharpParser.InterpolatedStringContext c)
        {
            c.ExpressionType = String;
            var parts = c.interpStringPart();
            if (parts.All(part => part.interpValue() is null)) {
                var sb = new StringBuilder();
                foreach (var t in parts) {
                    if (t.IS_EscapeChar() != null) {
                        // TODO: get all
#pragma warning disable 8509
                        sb.Append(t.GetText() switch {
#pragma warning restore 8509
                            "\\\\" => '\\',
                            "\\\n" => '\n',
                            "\\\t"=>'\t',
                            "\\\a"=>'\a',
                            "\\\r"=>'\r'
                        });
                    }
                    else if (t.IS_NormalString() != null)
                        sb.Append(t.GetText());
                    else
                        sb.Append('}');
                }
                var r1 = new LiteralResolveResult(LiteralResolveResult.LiteralType.String, sb.ToString(), String);
                return r1;
            }
            throw new NotImplementedException();
        }
        
        public TypeResolveResult ResolveType(KSharpParser.TypeContext c)
        {
            var f = c.typeExpression().lowestPriorityTypeExpression().prefixType().postfixType();
            var outerPostfix = f.typeExpressionOuterPostfix();
            var atomType = f.innerPostfixType().atomType();
            return ResolveType(atomType);
        }

        private TypeResolveResult ResolveType(KSharpParser.AtomTypeContext c)
        {
            var k = c.keywordType();
            if (k != null) {
                if (k.INT() != null) return Int;
                if (k.DOUBLE() != null) return Double;
                if (k.STRING() != null) return String;
                if (k.UNIT() != null) return Void;
                if (k.BOOL() != null) return Boolean;
            }
            var k2 = c.decltypeExpression();
            if (k2 != null) {
                throw new NotImplementedException();
            }
            
            var k3 = c.gidType().gid();
            var resolveResult = ResolveGid(k3[0]);
            k3.Skip(1).ForEach(g => resolveResult = Compiler.TypeResolveAgent.ResolveNestedGid(g, resolveResult));
            return resolveResult.kind switch {
                IdentifierResolveResult.UnionCase.Failure => new TypeResolveResult(resolveResult.ErrorMessage),
                IdentifierResolveResult.UnionCase.FieldDefinition => TypeResolveError.NonTypeMember(resolveResult.fieldDefinition!),
                IdentifierResolveResult.UnionCase.MethodDefinition => TypeResolveError.NonTypeMember(resolveResult.methodDefinition!.methods[0]),
                IdentifierResolveResult.UnionCase.PropertyDefinition => TypeResolveError.NonTypeMember(resolveResult.propertyDefinition!.properties[0]),
                IdentifierResolveResult.UnionCase.NamespaceDefinition => TypeResolveError.NonTypeMember(resolveResult.namespaceDefinition!),
                IdentifierResolveResult.UnionCase.TypeDefinition => resolveResult.typeDefinition!,
                // IdentifierResolveResult.UnionCase.LocalDefinition => expr,
                _ => throw new ArgumentOutOfRangeException()
            };
        }
        

    }
}
