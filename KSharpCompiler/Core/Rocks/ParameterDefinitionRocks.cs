using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Mono.Cecil;


namespace KSharpCompiler
{
    public static class ParameterDefinitionRocks
    {
        public static IEnumerable<CustomAttribute> GetCustomAttribute(this ParameterDefinition parameter, Type attributeType)
        {
            return parameter.CustomAttributes.Where(t => t.AttributeType.FullName == attributeType.FullName);
        }
        public static bool HasCustomAttribute<T>(this ParameterDefinition parameter)
                where T: Attribute
        {
            return parameter.GetCustomAttribute(typeof(T)).Any();
        }
        
        public static bool IsParams(this ParameterDefinition par)
        {
            return par.HasCustomAttribute<ParamArrayAttribute>();
        }
    }
}
