using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using IronPython.Runtime;
using Mono.Cecil;


namespace KSharpCompiler
{
    [ErrorCode(1006, ErrorLevel.Error)]
    public class MethodResolveFailure: CompilerMessage
    {
        
        public static MethodResolveFailure AmbiguousMatch(IEnumerable<MethodDefinition> methods)
        {
            return new MethodResolveFailure { Note = $"Ambiguous method resolution. Candidates are {methods}" };
        }
        public static MethodResolveFailure NoMatch(IEnumerable<TypeDefinition> argumentTypes)
        {
            return new MethodResolveFailure() { Note = $"Method resolution fails. Argument types are {argumentTypes}" };
        }
    }
}
