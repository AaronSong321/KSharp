using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Mono.Cecil;


namespace KSharpCompiler
{
    public class NamespaceDefinition
    {
        public string Name { get; set; }
        public string FullName { get; set; }
        
        public NamespaceDefinition(string name, string fullName)
        {
            Name = name;
            FullName = fullName;
        }
    }


    public sealed class IdentifierResolveResult: ResolveResult
    {
        public enum UnionCase : short
        {
            Failure,
            NamespaceDefinition,
            PropertyDefinition,
            MethodDefinition,
            LocalDefinition,
            TypeDefinition,
            FieldDefinition,
            ParameterDefinition,
            EventDefinition
        }
        
        public readonly UnionCase kind;
        public readonly NamespaceDefinition? namespaceDefinition; 
        public readonly FieldDefinition? fieldDefinition;
        public readonly PropertyGroup? propertyDefinition;
        public readonly MethodGroup? methodDefinition;
        public readonly TypeDefinition? typeDefinition;
        public readonly LocalVarDescriber? localDefinition;
        public readonly ParameterDefinition? parameterDefinition;
        public readonly EventDefinition? eventDefinition;

        private IdentifierResolveResult()
        {
            kind = UnionCase.Failure;
        }
        public IdentifierResolveResult(CompilerMessage errorMessage): base(errorMessage)
        {
            kind = UnionCase.Failure;
        }
        
        public IdentifierResolveResult(NamespaceDefinition namespaceDefinition)
        {
            kind = UnionCase.NamespaceDefinition;
            this.namespaceDefinition = namespaceDefinition;
        }
        public IdentifierResolveResult(IEnumerable<PropertyDefinition> propertyDefinition)
        {
            kind = UnionCase.PropertyDefinition;
            this.propertyDefinition = new PropertyGroup(propertyDefinition);
        }
        public IdentifierResolveResult(IEnumerable<MethodDefinition> methodDefinition)
        {
            kind = UnionCase.MethodDefinition;
            this.methodDefinition = new MethodGroup(methodDefinition);
        }
        public IdentifierResolveResult(TypeDefinition typeDefinition)
        {
            kind = UnionCase.TypeDefinition;
            this.typeDefinition = typeDefinition;
        }
        public IdentifierResolveResult(LocalVarDescriber localDefinition)
        {
            kind = UnionCase.LocalDefinition;
            this.localDefinition = localDefinition!;
        }
        public IdentifierResolveResult(FieldDefinition fieldDefinition)
        {
            kind = UnionCase.LocalDefinition;
            this.fieldDefinition = fieldDefinition;
        }
        public IdentifierResolveResult(ParameterDefinition? parameterDefinition)
        {
            this.parameterDefinition = parameterDefinition;
            kind = UnionCase.ParameterDefinition;
        }
        public IdentifierResolveResult(EventDefinition? eventDefinition)
        {
            kind = UnionCase.EventDefinition;
            this.eventDefinition = eventDefinition;
        }

        public static implicit operator IdentifierResolveResult(TypeDefinition typeInfoHolder)
        {
            return new IdentifierResolveResult(typeInfoHolder);
        }

        public static IdentifierResolveResult NoMatch { get; } = new IdentifierResolveResult();
    }

}
