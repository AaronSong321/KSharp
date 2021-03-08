using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Mono.Cecil;


namespace KSharpCompiler
{
    // public sealed class TypeReferenceHolder: TypeInfoHolder
    // {
    //     public override TypeDefinition TypeDefinition { get; }
    //     private TypeReference typeReference = null!;
    //
    //     public TypeReferenceHolder(TypeDefinition typeDefinition)
    //     {
    //         TypeDefinition = typeDefinition;
    //     }
    //
    //     public override TypeReference Refer(ModuleDefinition module)
    //     {
    //         if (ReferencedTimes++ is 0) {
    //             typeReference = module.ImportReference(TypeDefinition);
    //         }
    //         return typeReference;
    //     }
    // }
}
