using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Mono.Cecil;


namespace KSharpCompiler
{
    [ErrorCode(1005, ErrorLevel.Error)]
    public sealed class LiteralError: CompilerError
    {
        private LiteralError() { }
        public static LiteralError ParseError(string literalValue, TypeReference targetType)
        {
            return new LiteralError() {
                Note = $"Cannot parser literal value '{literalValue}' to type {targetType}"
            };
        }
    }
}
