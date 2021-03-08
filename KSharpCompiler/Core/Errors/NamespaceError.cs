using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;


namespace KSharpCompiler
{
    [ErrorCode(1001, ErrorLevel.Error)]
    public sealed class UndefinedIdentifier : CompilerError
    {
        public static UndefinedIdentifier UndefinedNamespace(string ns)
        {
            return new UndefinedIdentifier() { Note = $"namespace '{ns}' not found." };
        }
    }

    [ErrorCode(1002, ErrorLevel.Error)]
    public sealed class AmbiguousIdentifier : CompilerError
    {
        public static AmbiguousIdentifier Ambiguous(string id, IEnumerable<string> candidates)
        {
            return new AmbiguousIdentifier() { Note = $"ambiguous identifier {id} {candidates}" };
        }
    }
}
