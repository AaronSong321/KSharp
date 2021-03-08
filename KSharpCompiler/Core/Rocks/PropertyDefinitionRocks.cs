using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Mono.Cecil;


namespace KSharpCompiler
{
    public static class PropertyDefinitionRocks
    {
        public static MethodDefinition GetAnyAccessor(this PropertyDefinition property)
        {
            return (property.GetMethod??property.SetMethod)!;
        }
        public static MethodDefinition GetAnyAccessor(this EventDefinition @event)
        {
            return (@event.AddMethod??@event.RemoveMethod)!;
        }
    }
}
