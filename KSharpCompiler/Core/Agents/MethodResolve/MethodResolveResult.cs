using System.Collections.Generic;
using KSharp;
using Mono.Cecil;

namespace KSharpCompiler
{
    public class MethodResolveResult: ResolveResult
    {
        public readonly List<ArgumentCorrespondGroup> matches;
        public MethodResolveResult(CompilerMessage errorMessage) : base(errorMessage)
        {
            matches = new List<ArgumentCorrespondGroup>();
        }
        public MethodResolveResult(List<ArgumentCorrespondGroup> matches)
        {
            this.matches = matches;
        }
        public MethodResolveResult(ArgumentCorrespondGroup group)
        {
            matches = new List<ArgumentCorrespondGroup> { group };
        }
        public List<MethodDefinition> ToMethods()
        {
            return matches.Map(t => t.signature.origin);
        }

        public bool IsSingleMatch => matches?.Count is 1;
    }
}
