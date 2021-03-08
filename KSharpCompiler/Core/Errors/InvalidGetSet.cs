using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Antlr4.Runtime;
using KSharpCompiler.Grammar;
using Mono.Cecil;


namespace KSharpCompiler
{
    [ErrorCode(1009, ErrorLevel.Error)]
    public sealed class InvalidSet : CompilerError
    {
        private InvalidSet() { }
        public static InvalidSet SetConstField(FieldDefinition d)
        {
            return new InvalidSet() { Note = $"Cannot set field {d.GetReferName()} because it is marked const, constexpr or constinit" };
        }
        public static InvalidSet SetConstProperty(PropertyDefinition d)
        {
            return new InvalidSet() { Note = $"Cannot set property {d.GetReferName()} because it doesn't have a set method." };
        }
        public static InvalidSet SetConstLocal(LocalVarDescriber d)
        {
            return new InvalidSet() { Note = $"Cannot set local variable {d.name} because it is marked {d.mutability}" };
        }
        public static InvalidSet SetParameter(ParameterDefinition d)
        {
            return new InvalidSet() { Note = $"Cannot set parameter {d.Name}" };
        }
    }

    [ErrorCode(1010, ErrorLevel.Error)]
    public sealed class NotLvalue : CompilerError
    {
        private NotLvalue() { }
        public static NotLvalue E(KSharpParser.IExpressionContext c)
        {
            return new NotLvalue() {
                Note = $"{(c as ParserRuleContext)?.GetText()??"expression"} cannot be used as a l-value"
            };
        }
    }
}
