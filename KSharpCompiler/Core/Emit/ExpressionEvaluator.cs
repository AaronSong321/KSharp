using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using KSharpCompiler.Grammar;
using Mono.Cecil;


namespace KSharpCompiler
{
    public partial class ExpressionEmitter
    {
        private ILInstructionGroup EvaluateGet(KSharpParser.PostfixExpressionContext c)
        {
            if (c.postfixSuffix() != null) {
                var c1 = c.postfixSuffix().memberAccessSuffix();
                if (c1 != null) {
                    var r1 = CompileUnit.LocalTypeResolveAgent.ResolveGid(c1.gid());
                    var r2 = r1.kind switch {
                        IdentifierResolveResult.UnionCase.LocalDefinition => Il.LoadLocal(r1.localDefinition!.variableDefinition.Index),
                        _ => throw new Exception()
                    };
                    return new ILInstructionGroup(r2);
                }
                throw new NotImplementedException();
            }
            else {
                return EvaluateGet(c.primaryExpression());
            }
        }

        private ILInstructionGroup EvaluateGet(KSharpParser.PrimaryExpressionContext c)
        {
            if (c.literalExpression() != null)
                return EvaluateGet(c.literalExpression());
            if (c.keywordExpression() != null) {
                var c1 = c.keywordExpression().thisAccess();
                if (c1 != null)
                    return new ILInstructionGroup(Il.LoadThis());
                var c2 = c.keywordExpression().baseAccess();
                if (c2 != null)
                    return new ILInstructionGroup(Il.LoadThis());
            }
            throw new NotImplementedException();
        }

        private ILInstructionGroup EvaluateGet(KSharpParser.LiteralExpressionContext c)
        {
            var literalValue = Compiler.TypeResolveAgent.ResolveLiteralValue(c);
            var r1 = literalValue.literalType switch {
                LiteralResolveResult.LiteralType.Int => Il.LoadConstant((int)literalValue.value!),
                LiteralResolveResult.LiteralType.String => Il.LoadConstant((string)literalValue.value!),
                LiteralResolveResult.LiteralType.Bool => Il.LoadConstant((bool)literalValue.value!),
                // LiteralResolveResult.LiteralType.Unit => expr,
                // LiteralResolveResult.LiteralType.Void => expr,
                _ => throw new ArgumentOutOfRangeException()
            };
            var r2 = new ILInstructionGroup(r1);
            return r2;
        }

        private ILInstructionGroup EvaluateGet(KSharpParser.InfixCallExpressionContext c)
        {
            if (c.infixCallExpression() != null)
                throw new NotImplementedException();
            return EvaluateGet(c.prefixExpression());
        }

        private ILInstructionGroup EvaluateGet(KSharpParser.PrefixExpressionContext c)
        {
            return EvaluateGet(c.postfixExpression());
        }

        /// <summary>
        /// Set the field of value on the stack top
        /// </summary>
        /// <param name="rvalueExpression"></param>
        /// <param name="field"></param>
        /// <returns></returns>
        private ILInstructionGroup EvaluateSetField(KSharpParser.InfixCallExpressionContext rvalueExpression, FieldReference field)
        {
            var r1 = EvaluateGet(rvalueExpression);
            var r2 = new ILInstructionGroup(Il.StoreField(field));
            return new ILInstructionGroup(r1, r2);
        }
    }
}
