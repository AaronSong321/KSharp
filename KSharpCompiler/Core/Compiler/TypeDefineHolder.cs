using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using IronPython.Modules;
using KSharp;
using Microsoft.Scripting.Metadata;
using Mono.Cecil;


namespace KSharpCompiler
{
    // public abstract class TypeInfoHolder
    // {
    //     public int ReferencedTimes { get; protected set; }
    //     public abstract TypeDefinition TypeDefinition { get; }
    //     public abstract TypeReference Refer(ModuleDefinition module);
    // }
    //
    // public class TypeDefineHolder: TypeInfoHolder
    // {
    //     
    //     public override TypeDefinition TypeDefinition { get; }
    //     public TypeDefineHolder(TypeDefinition typeDefinition)
    //     {
    //         TypeDefinition = typeDefinition;
    //     }
    //     
    //     public override TypeReference Refer(ModuleDefinition _)
    //     {
    //         ++ReferencedTimes;
    //         return TypeDefinition;
    //     }
    // }
}
