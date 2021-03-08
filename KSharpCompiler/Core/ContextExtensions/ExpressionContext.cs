using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Antlr4.Runtime;
using Mono.Cecil;


namespace KSharpCompiler.Grammar
{
    public static class IExpressionContextExtensions
    {
        public static T? SingleTo<T>(this ParserRuleContext? c)
            where T : ParserRuleContext
        {
            while (true) {
                if (c is T c1) return c1;
                if (c?.ChildCount is 1) {
                    c = c.children[0] as ParserRuleContext;
                    continue;
                }
                return null;
            }
        }
    }


    public partial class KSharpParser
    {
        public interface IExpressionContext
        {
            TypeReference? ExpressionType { get; set; }
            LocalVarDescriber? AuxiliaryLocal { get; set; }
            bool IsLValue();
            bool IsLiteralExpression();
        }

        partial class ExpressionsContext
        {
            public List<ExpressionContext> GetExpressions()
            {
                var g = new List<ExpressionContext>();
                var a = this;
                while (a != null) {
                    g.Insert(0, a.expression());
                    a = a.expressions();
                }
                return g;
            }
        }
        partial class ExpressionContext : IExpressionContext
        {
            public TypeReference? ExpressionType { get; set; }
            public LocalVarDescriber? AuxiliaryLocal { get; set; }

            public bool IsLValue()
            {
                return lowestPriorityExpression().IsLValue();
            }

            public bool IsLiteralExpression()
            {
                return lowestPriorityExpression().IsLiteralExpression();
            }

            public LiteralExpressionContext? GetLiteralExpression()
            {
                return lowestPriorityExpression()?.assignmentExpression()?.infixCallExpression()?.prefixExpression()?.postfixExpression()?.primaryExpression()?.literalExpression();
            }
        }
        
        public partial class LowestPriorityExpressionContext: IExpressionContext
        {
            public TypeReference? ExpressionType { get; set; }
            public LocalVarDescriber? AuxiliaryLocal { get; set; }
            public bool IsLValue()
            {
                return assignmentExpression().IsLValue();
            }

            public bool IsLiteralExpression()
            {
                return assignmentExpression().IsLiteralExpression();
            }
        }

        partial class AssignmentExpressionContext : IExpressionContext
        {
            public TypeReference? ExpressionType { get; set; }
            public LocalVarDescriber? AuxiliaryLocal { get; set; }
            public bool IsLValue()
            {
                return assignmentExpression() != null || infixCallExpression().IsLValue();
            }

            public bool IsLiteralExpression()
            {
                return assignmentExpression() is null && infixCallExpression().IsLiteralExpression();
            }
        }

        partial class PostfixExpressionContext : IExpressionContext
        {
            public TypeReference? ExpressionType { get; set; }
            public LocalVarDescriber? AuxiliaryLocal { get; set; }
            public bool IsLValue()
            {
                var suffix = postfixSuffix();
                if (suffix != null) {
                    return suffix.delegateinvokeSuffix() is null && suffix.methodInvokeSuffix() is null;
                }
                return primaryExpression().IsLValue();
            }

            public bool IsLiteralExpression()
            {
                return primaryExpression()?.IsLiteralExpression()??false;
            }
        }

        partial class InfixCallExpressionContext : IExpressionContext
        {
            public TypeReference? ExpressionType { get; set; }
            public LocalVarDescriber? AuxiliaryLocal { get; set; }
            public bool IsLValue()
            {
                return infixCallExpression() is null && prefixExpression().IsLValue();
            }

            public bool IsLiteralExpression()
            {
                return infixCallExpression() is null && prefixExpression().IsLiteralExpression();
            }
        }

        partial class PrefixExpressionContext : IExpressionContext
        {
            public TypeReference? ExpressionType { get; set; }
            public LocalVarDescriber? AuxiliaryLocal { get; set; }
            public bool IsLValue()
            {
                return awaitExpression() is null && prefixUnaryOperator() is null && postfixExpression().IsLValue();
            }

            public bool IsLiteralExpression()
            {
                return postfixExpression()?.IsLiteralExpression()??false;
            }
        }

        partial class PrimaryExpressionContext : IExpressionContext
        {
            public TypeReference? ExpressionType { get; set; }
            public LocalVarDescriber? AuxiliaryLocal { get; set; }
            public bool IsLValue()
            {
                if (gid() != null)
                    return gid().IsLValue();
                var ke = keywordExpression();
                if (ke != null) {
                    return ke.backupAccess() != null || ke.valueAccess() != null || ke.AnonymousLambdaParameter() != null;
                }
                return false;
            }

            public bool IsLiteralExpression()
            {
                return literalExpression() != null;
            }
        }

        partial class GidContext : IExpressionContext
        {
            public TypeReference? ExpressionType { get; set; }
            public LocalVarDescriber? AuxiliaryLocal { get; set; }
            private IdentifierResolveResult? resolveResult;
            public IdentifierResolveResult? ResolveResult {
                get => resolveResult;
                set => resolveResult ??= value;
            }
            
            public bool IsLValue()
            {
                if (resolveResult is null)
                    throw new InvalidOperationException();
                var a = ((IdentifierResolveResult)resolveResult).kind;
                var b = a switch {
                    IdentifierResolveResult.UnionCase.Failure => throw new InvalidOperationException(),
                    IdentifierResolveResult.UnionCase.FieldDefinition => true,
                    IdentifierResolveResult.UnionCase.LocalDefinition => true,
                    IdentifierResolveResult.UnionCase.MethodDefinition => false,
                    IdentifierResolveResult.UnionCase.NamespaceDefinition => false,
                    IdentifierResolveResult.UnionCase.TypeDefinition => false,
                    IdentifierResolveResult.UnionCase.PropertyDefinition => true,
                    _ => throw new ArgumentOutOfRangeException()
                };
                return b;
            }

            public bool IsLiteralExpression()
            {
                return false;
            }
        }

        partial class InterpolatedStringContext : IExpressionContext
        {
            private TypeReference? expressionType;
            public TypeReference? ExpressionType {
                get => expressionType;
                set => expressionType ??= value;
            }
            public LocalVarDescriber? AuxiliaryLocal { get; set; }

            public bool IsLValue() => false;
            public bool IsLiteralExpression()
            {
                return interpStringPart().All(t => t.interpValue() == null);
            }
        }

        partial class ReturnExpressionContext : IExpressionContext
        {
            public TypeReference? ExpressionType {
                get => null;
                set => throw new InvalidOperationException();
            }
            public LocalVarDescriber? AuxiliaryLocal { get; set; }
            public bool IsLValue() => false;
            public bool IsLiteralExpression() => false;
        }
    }
}