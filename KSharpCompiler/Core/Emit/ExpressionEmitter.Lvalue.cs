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
        /// <summary>
        /// Emit the expression but not the final step, so it can be stored
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        private ILInstructionGroup EmitLvalue(KSharpParser.InfixCallExpressionContext c)
        {
            if (c.identifier() != null)
                return new ILInstructionGroup(LvalueError.CannotUseAsLvalue<KSharpParser.InfixCallExpressionContext>());
            return EmitLvalue(c.prefixExpression());
        }

        private ILInstructionGroup EmitLvalue(KSharpParser.PrefixExpressionContext c)
        {
            if (c.prefixUnaryOperator() != null)
                throw new NotImplementedException();
            return EmitLvalue(c.postfixExpression());
        }

        private ILInstructionGroup EmitLvalue(KSharpParser.PostfixExpressionContext c)
        {
            if (c.primaryExpression() != null)
                return EmitLvalue(c.primaryExpression());
            return EmitLvalue(c.postfixExpression(), c.postfixSuffix());
        }

        private ILInstructionGroup EmitLvalue(KSharpParser.PrimaryExpressionContext c)
        {
            if (c.gid() != null)
                return EmitLvalue(c.gid());
            if (c.keywordExpression() != null)
                return EmitLvalue(c.keywordExpression());
            return new ILInstructionGroup(LvalueError.CannotUseAsLvalue<KSharpParser.LiteralExpressionContext>());
        }

        private ILInstructionGroup EmitLvalue(KSharpParser.GidContext c)
        {
            var r1 = CompileUnit.LocalTypeResolveAgent.ResolveGid(c);
            var r2 = r1.kind switch {
                IdentifierResolveResult.UnionCase.Failure => new ILInstructionGroup(r1.ErrorMessage),
                IdentifierResolveResult.UnionCase.FieldDefinition => new ILInstructionGroup(Il.LoadThis()),
                IdentifierResolveResult.UnionCase.PropertyDefinition => new ILInstructionGroup(Il.LoadThis()),
                IdentifierResolveResult.UnionCase.EventDefinition => new ILInstructionGroup(Il.LoadThis()),
                _ => new ILInstructionGroup(LvalueError.CannotUseAsLvalue<MethodDefinition>())
            };
            return r2;
        }

        private ILInstructionGroup EmitLvalue(KSharpParser.KeywordExpressionContext c)
        {
            if (c.backupAccess() != null) {
                return new ILInstructionGroup(Il.LoadThis());
            }
            return new ILInstructionGroup(LvalueError.CannotUseAsLvalue<KSharpParser.ThisAccessContext>());
        }

        private ILInstructionGroup EmitLvalue(KSharpParser.PostfixExpressionContext c1, KSharpParser.PostfixSuffixContext c2)
        {
            if (c2.memberAccessSuffix() != null || c2.elementAccessSuffix() != null) {
                var r1 = Emit(c1);
                return r1;
            }
            return new ILInstructionGroup(LvalueError.CannotUseAsLvalue<KSharpParser.PostfixSuffixContext>());
        }
    }
}
