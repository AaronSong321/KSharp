using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using KSharp;
using Mono.Cecil;


namespace KSharpCompiler
{
    public class MethodGroup
    {
        public readonly MethodDefinition[] methods;

        public MethodGroup(IEnumerable<MethodDefinition> methods)
        {
            this.methods = methods.ToArray();
        }

    }
    
    public class PropertyGroup
    {
        public readonly PropertyDefinition[] properties;

        public PropertyGroup(IEnumerable<PropertyDefinition> methods)
        {
            this.properties = methods.ToArray();
        }

        public bool HasParameters {
            get => properties[0].HasParameters;
        }
        public TypeReference PropertyType => properties[0].PropertyType;

        public MethodGroup Getters => new MethodGroup(properties.Map(t => t.GetMethod).NonNull());
        public MethodGroup Setters => new MethodGroup(properties.Map(t => t.SetMethod).NonNull());
    }
}
