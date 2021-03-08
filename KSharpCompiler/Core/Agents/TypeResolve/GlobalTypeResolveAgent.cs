using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Scripting.Metadata;
using Mono.Cecil;


namespace KSharpCompiler
{
    
    public static class GlobalTypeResolveAgent
    {
        private static bool PossiblyAccessibleHere(TypeDefinition type)
        {
            if (type.IsNested) {
                if (!PossiblyAccessibleHere(type.DeclaringType))
                    return false;
                var k = type.Attributes;
                return (k & TypeAttributes.NestedPublic) != 0 || (k & TypeAttributes.NestedFamORAssem) != 0 || (k & TypeAttributes.NestedFamily) != 0 && (k & TypeAttributes.NestedFamANDAssem) == TypeAttributes.NestedFamANDAssem;
            }
            return (type.Attributes & TypeAttributes.Public) != 0;
        }

        private static bool ContainsCompilerGeneratedCharacter(TypeDefinition type)
        {
            return ContainsCompilerGeneratedCharacter(type.FullName);
        }

        private static bool ContainsCompilerGeneratedCharacter(string name)
        {
            return name.All(t => char.IsLetterOrDigit(t) || t == '_');
        }

        static bool IsCompilerGenerated(TypeDefinition type)
        {
            return type.HasCustomAttribute(typeof(CompilerGeneratedAttribute));
        }

        public static bool NeedToImport(this TypeDefinition type)
        {
            return PossiblyAccessibleHere(type) && !IsCompilerGenerated(type);
        }
    }
}
