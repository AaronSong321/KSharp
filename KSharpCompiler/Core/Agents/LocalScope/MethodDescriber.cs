using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using KSharpCompiler.Grammar;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Collections.Generic;


namespace KSharpCompiler
{
    public abstract class MemberDescriber
    {
        
    }

    public enum MethodMutability
    {
        Mutable,
        Immutable,
        Pure,
        Constexpr,
        Consteval
    }
    
    public class MethodDescriber: MemberDescriber, IMemberDefinition
    {
        public readonly MethodDefinition method;
        public Collection<ParameterDefinition> Parameters => method.Parameters;
        public readonly List<LocalVarDescriber> locals = new List<LocalVarDescriber>();
        public readonly MethodMutability mutability;
        private LocalVarDescriber? retValueVar;

        public readonly List<KSharpParser.ReturnExpressionContext> returnExpressions = new List<KSharpParser.ReturnExpressionContext>();
        public bool ImplicitVoidReturn { get; set; }

        public int ReturnExpressionsCount => returnExpressions.Count + (ImplicitVoidReturn ? 1 : 0);
        public readonly List<KSharpParser.YieldExpressionContext> yieldExpressions = new List<KSharpParser.YieldExpressionContext>();
        public int YieldExpressionCount => yieldExpressions.Count;

        public MethodDescriber(MethodDefinition method, MethodMutability mutability)
        {
            this.method = method;
            this.mutability = mutability;
        }

        public bool IsMutable => mutability == MethodMutability.Mutable;

        public MetadataToken MetadataToken {
            get => method.MetadataToken;
            set => method.MetadataToken = value;
        }
        public Collection<CustomAttribute> CustomAttributes {
            get => method.CustomAttributes;
        }
        public bool HasCustomAttributes {
            get => method.HasCustomAttributes;
        }
        public string Name {
            get => method.Name;
            set => method.Name = value;
        }
        public string FullName {
            get => ((IMemberDefinition)method).FullName;
        }
        public bool IsSpecialName {
            get => method.IsSpecialName;
            set => method.IsSpecialName = value;
        }
        public bool IsRuntimeSpecialName {
            get => method.IsRuntimeSpecialName;
            set => method.IsRuntimeSpecialName = value;
        }
        public TypeDefinition DeclaringType {
            get => method.DeclaringType;
            set => method.DeclaringType = value;
        }

        public LocalVarDescriber DefineLocalVar(string name, TypeReference type, LocalVariableMutability mutability, bool isGenerated)
        {
            var local = new VariableDefinition(type);
            method.Body.Variables.Add(local);
            var localVar = new LocalVarDescriber(local, name, mutability, isGenerated);
            locals.Add(localVar);
            return localVar;
        }

        public LocalVarDescriber GenerateLocalVar(TypeReference type)
        {
            var local = new VariableDefinition(type);
            method.Body.Variables.Add(local);
            var localVar = new LocalVarDescriber(local, NameGenAgent.GenerateAuxiliaryLocalName(local.Index), LocalVariableMutability.Mutable, true);
            locals.Add(localVar);
            return localVar;
        }

        public LocalVarDescriber? FindLocalVar(string name)
        {
            return locals.FirstOrDefault(t => t.name == name);
        }
        public ParameterDefinition? FindParameter(string name)
        {
            return Parameters.FirstOrDefault(t => t.Name == name);
        }
        public LocalVarDescriber AddRetValueVar()
        {
            var t = new VariableDefinition(method.ReturnType);
            method.Body.Variables.Add(t);
            var localVar = new LocalVarDescriber(t, "__ret", LocalVariableMutability.Mutable, true);
            locals.Add(localVar);
            retValueVar = localVar;
            return localVar;
        }
        public LocalVarDescriber RetValueVar {
            get {
                return retValueVar??throw new InvalidOperationException();
            }
        }
    }
}
