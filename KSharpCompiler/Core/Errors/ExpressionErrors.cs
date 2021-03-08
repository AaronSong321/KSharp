using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Antlr4.Runtime;


namespace KSharpCompiler
{
    [ErrorCode(1008, ErrorLevel.Error)]
    public sealed class LvalueError: CompilerError
    {
        private LvalueError() { }
        public static LvalueError CannotUseAsLvalue<T>()
        {
            return new LvalueError() { Note = $"Cannot use {nameof(T)} as a l-value expression" };
        }
    }
}
