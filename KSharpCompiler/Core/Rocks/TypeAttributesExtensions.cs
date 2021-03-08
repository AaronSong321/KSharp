using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Mono.Cecil;


namespace KSharpCompiler
{
    public static class TypeAttributesExtensions
    {
        public static TypeAttributes Interface(this TypeAttributes a)
        {
            return a | TypeAttributes.Interface;
        }
        public static TypeAttributes Abstract(this TypeAttributes a)
        {
            return a | TypeAttributes.Abstract;
        }
        public static TypeAttributes SpecialName(this TypeAttributes a)
        {
            return a | TypeAttributes.SpecialName | TypeAttributes.RTSpecialName;
        }
        public static TypeAttributes Sealed(this TypeAttributes a)
        {
            return a | TypeAttributes.Sealed;
        }
        public static TypeAttributes Static(this TypeAttributes a)
        {
            return a | TypeAttributes.Sealed | TypeAttributes.Abstract;
        }

        public static FieldAttributes SpecialName(this FieldAttributes a)
        {
            return a | FieldAttributes.SpecialName | FieldAttributes.RTSpecialName;
        }
    }
}
