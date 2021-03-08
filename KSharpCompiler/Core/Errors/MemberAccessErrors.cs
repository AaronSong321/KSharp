

using Mono.Cecil;

namespace KSharpCompiler
{
    [ErrorCode(1003, ErrorLevel.Error)]
    public sealed class MemberAccessError : CompilerError
    {
        public static MemberAccessError AccessIndexerMember(PropertyGroup p)
        {
            return new MemberAccessError() {
                Note = $"Cannot access member of an indexer group {NameGenAgent.GetLongName(p.properties[0])}"
            };
        }
        public static MemberAccessError AccessMethodMember(MethodGroup m)
        {
            return new MemberAccessError() {
                Note = $"Cannot access member of a method group {NameGenAgent.GetLongName((m.methods[0] as MethodReference)!)}"
            };
        }

        public static MemberAccessError CannotCallMember(IMemberDefinition d)
        {
            return new MemberAccessError() {
                Note = $"Cannot call field, property, event, or nested type member {d.GetReferName()}"
            };
        }
        public static MemberAccessError CannotSetMember(IMemberDefinition d)
        {
            return new MemberAccessError() {
                Note = $"Cannot set event, method, or nested type member {d.GetReferName()}"
            };
        }
        public static MemberAccessError CannotSetNamespace(NamespaceDefinition d)
        {
            return new MemberAccessError() {
                Note = "Cannot set a namespace"
            };
        }
        public static MemberAccessError IndexerNoParameter(PropertyDefinition d)
        {
            return new MemberAccessError() {
                Note = $"Cannot access indexer {d.GetReferName()} without parameters."
            };
        }
    }
    
    
}