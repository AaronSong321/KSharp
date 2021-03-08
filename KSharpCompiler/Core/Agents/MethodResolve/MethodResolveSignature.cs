using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using KSharp;
using Mono.Cecil;


namespace KSharpCompiler
{
    public sealed class ParameterResolveSignature
    {
        public readonly int index;
        public readonly TypeReference type;
        public readonly ParameterAttributes attributes;
        public readonly bool isParams;
        public readonly string? name;
        
        public ParameterResolveSignature(int index, TypeReference type, ParameterAttributes attributes)
        {
            this.index = index;
            this.type = type;
            this.attributes = attributes;
            isParams = false;
            name = null;
        }

        public ParameterResolveSignature(ParameterDefinition par)
        {
            index = par.Index;
            type = par.ParameterType;
            attributes = par.Attributes;
            isParams = par.IsParams();
            name = par.Name;
        }

        public static implicit operator ParameterResolveSignature(ParameterDefinition par)
        {
            return new ParameterResolveSignature(par);
        }

        public bool IsOptional => (attributes & ParameterAttributes.Optional) != 0;
    }
    
    public class MethodResolveSignature
    {
        public readonly MethodDefinition origin;
        public readonly ParameterResolveSignature[] parameters;

        public MethodResolveSignature(MethodDefinition origin)
        {
            this.origin = origin;
            parameters = origin.Parameters.Select(t => new ParameterResolveSignature(t)).ToArray();
        }
        protected MethodResolveSignature(MethodDefinition origin, ParameterResolveSignature[] parameters)
        {
            this.origin = origin;
            this.parameters = parameters;
        }

        protected virtual bool CanExpand()
        {
            return parameters.Length > 0 && parameters[^1].isParams;
        }
        public MethodResolveSignature? TryExpand(int argumentNumber)
        {
            if (!CanExpand() || argumentNumber < parameters.Length - 1)
                return null;
            var lastParameter = parameters[^1];
            var elementType = lastParameter.type.GetElementType();
            ParameterResolveSignature[] sig = new ParameterResolveSignature[argumentNumber];
            for (int i = 0; i < parameters.Length - 1; ++i)
                sig[i] = parameters[i];
            for (int i = parameters.Length; i < argumentNumber; ++i) {
                sig[i] = new ParameterResolveSignature(i, elementType, lastParameter.attributes);
            }
            return new MethodResolveSignature(origin, sig);
        }

        public MethodResolveSignatureShrink Shrink(IEnumerable<ParameterResolveSignature> optionals)
        {
            var p = parameters.ToList();
            optionals.ForEach(t => p.Remove(t));
            return new MethodResolveSignatureShrink(origin, p.ToArray());
        }
    }

    public class MethodResolveSignatureShrink : MethodResolveSignature
    {
        public MethodResolveSignatureShrink(MethodDefinition origin, ParameterResolveSignature[] parameters) : base(origin, parameters) { }
        
    }

    public sealed class MethodResolveSignatureExpand: MethodResolveSignature
    {
        public readonly MethodResolveSignature unexpandedMethod;

        public MethodResolveSignatureExpand(MethodResolveSignature unexpandedMethod, ParameterResolveSignature[] parameters): base(unexpandedMethod.origin, parameters)
        {
            this.unexpandedMethod = unexpandedMethod;
        }

        protected override bool CanExpand()
        {
            return false;
        }
    }
}
