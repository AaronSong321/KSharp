using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;


namespace KSharpCompiler
{
    [ErrorCode(1012, ErrorLevel.Error)]
    public sealed class ReturnError : CompilerError
    {
        public static ReturnError NoReturnAtEnd()
        {
            return new ReturnError() { Note = "Not all paths returns a value." };
        }
        
    }

    [ErrorCode(1013, ErrorLevel.Error)]
    public sealed class YieldError : CompilerError
    {
        public static YieldError MixYieldAndReturn()
        {
            return new YieldError() { Note = "Cannot have both return and yield in a method." };
        }

        public static YieldError NonEnumerableMethod()
        {
            return new YieldError() { Note = $"Cannot use yield expressions in a method where return type is not IEnumerable, IEnumerable<T>, or IAsyncEnumerable<T>" };
        }
    }
}
