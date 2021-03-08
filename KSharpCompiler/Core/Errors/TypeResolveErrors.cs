using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Mono.Cecil;


namespace KSharpCompiler
{
    [ErrorCode(1004, ErrorLevel.Error)]
    public sealed class TypeResolveError: CompilerMessage
    {
        private TypeResolveError() { }
        public static TypeResolveError Error(TypeReference type)
        {
            return new TypeResolveError() {
                Note = $"Cannot resolve type {type}"
            };
        }

        public static TypeResolveError NonTypeMember(MemberReference reference)
        {
            return new TypeResolveError() {
                Note = $"Cannot use member {NameGenAgent.GetLongName(reference)} as a type"
            };
        }
        public static TypeResolveError NonTypeMember(NamespaceDefinition ns)
        {
            return new TypeResolveError() {
                Note = $"Cannot use namespace {ns.FullName} as a type"
            };
        }
    }
}
