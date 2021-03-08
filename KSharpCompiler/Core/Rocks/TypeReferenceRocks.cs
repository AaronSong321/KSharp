using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using KSharp;
using KSharpCompiler.Grammar;
using Microsoft.Scripting.Metadata;
using Microsoft.Scripting.Utils;
using Mono.Cecil;


namespace KSharpCompiler
{
    public static class TypeReferenceRocks
    {
        public static IEnumerable<CustomAttribute> GetCustomAttribute(this IMemberDefinition type, Type attributeType)
        {
            return type.CustomAttributes.Where(t => t.AttributeType.FullName == attributeType.FullName);
        }
        public static bool HasCustomAttribute(this IMemberDefinition type, Type attributeType)
        {
            return GetCustomAttribute(type, attributeType).Count() != 0;
        }

        public static string GetTypeReferName(this TypeReference type)
        {
            string name = type.Name;
            int index = name.IndexOf(NameGenAgent.NamespaceDelimiter);
            if (index != -1)
                name = name[0..index];
            return name;
        }

        public static bool IsStatic(FieldDefinition d)
        {
            return d.IsStatic;
        }
        public static bool IsStatic(PropertyDefinition d)
        {
            return d.GetAnyAccessor().IsStatic;
        }
        public static bool IsStatic(MethodDefinition d)
        {
            return d.IsStatic;
        }
        public static bool IsStatic(EventDefinition d)
        {
            return d.GetAnyAccessor().IsStatic;
        }

        [ThreadSafe]
        public static List<IMemberDefinition> GetMembers(this TypeDefinition type)
        {
            lock (type) {
                var g = new List<IMemberDefinition>();
                g.AddRange(type.NestedTypes);
                g.AddRange(type.Fields);
                g.AddRange(type.Methods);
                g.AddRange(type.Properties);
                g.AddRange(type.Events);
                return g;
            }
        }

        public static List<IMemberDefinition> GetInstanceMembers(this TypeDefinition type)
        {
            lock (type) {
                var g = new List<IMemberDefinition>();
                g.AddRange(type.Fields.WhereNot(IsStatic));
                g.AddRange(type.Methods.WhereNot(IsStatic));
                g.AddRange(type.Properties.WhereNot(IsStatic));
                g.AddRange(type.Events.WhereNot(IsStatic));
                return g;
            }
        }
        public static List<IMemberDefinition> GetStaticMembers(this TypeDefinition type)
        {
            lock (type) {
                var g = new List<IMemberDefinition>();
                g.AddRange(type.NestedTypes);
                g.AddRange(type.Fields.Where(IsStatic));
                g.AddRange(type.Methods.Where(IsStatic));
                g.AddRange(type.Properties.Where(IsStatic));
                g.AddRange(type.Events.Where(IsStatic));
                return g;
            }   
        }

        [ThreadSafe]
        public static IEnumerable<IMemberDefinition> FindMember(this TypeDefinition type, string name)
        {
            return type.GetMembers().Where(t => t.Name == name);
        }

        public static IEnumerable<IMemberDefinition> WithGenericParameters(this IEnumerable<IMemberDefinition> members, int argumentCount)
        {
            foreach (var member in members) {
                if (member is TypeDefinition type && type.GenericParameters.Count == argumentCount) {
                    yield return type;
                }
                if (member is MethodDefinition method && method.GenericParameters.Count == argumentCount) {
                    yield return method;
                }
            }
        }
        
    }
}
