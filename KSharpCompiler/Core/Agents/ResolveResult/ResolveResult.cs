using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;


namespace KSharpCompiler
{
    public abstract class ResolveResult
    {
        // public CompilerMessage? ErrorMessage { get; protected set; }
        public List<CompilerMessage> ErrorMessage { get; } = new List<CompilerMessage>();
        public ResolveResult()
        {
        }
        public ResolveResult(IEnumerable<CompilerMessage> errorMessage)
        {
            ErrorMessage.AddRange(errorMessage);
        }
        public ResolveResult(CompilerMessage errorMessage)
        {
            ErrorMessage.Add(errorMessage);
        }

        public bool ErrorState => ErrorMessage.Count != 0;
    }
}
