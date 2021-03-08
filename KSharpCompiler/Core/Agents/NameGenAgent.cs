using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Mono.Cecil;


namespace KSharpCompiler
{
    public class NameGenAgent: CompileUnitAgent
    {

        public NameGenAgent(CompileUnit compileUnit) : base(compileUnit) { }
        public const char NamespaceDelimiter = '.';
        public const char GenericDelimiter = '`';
        private const char MemberAccessDelimiter = '.';
        public const string Float32Suffix = "fF";

        public static string GenericQualify(string rawName, int genericArity)
        {
            return $"{rawName}{GenericDelimiter}{genericArity}";
        }

        public static string NamespaceQualify(string n1, string n2)
        {
            return $"{n1}{NamespaceDelimiter}{n2}";
        }
        
        /// <summary>
        /// Gets the name of a member together with its declaring type's name and namespace, but doesn't include parameter list of a method or type parameter list of a generic member
        /// </summary>
        /// <param name="member"></param>
        /// <returns></returns>
        public static string GetLongName(MemberReference member)
        {
            return member switch {
                PropertyReference p => $"{p.DeclaringType.FullName}{MemberAccessDelimiter}{p.Name}",
                MethodReference m => $"{m.DeclaringType.FullName}{MemberAccessDelimiter}{m.Name}",
                FieldReference f => $"{f.FullName}",
                TypeReference t => $"{t.FullName}",
                _ => throw new Exception()
            };
        }

        public static string GenerateFieldName(string propertyName)
        {
            return $"<{propertyName}>k__backingField";
        }

        public static string GenerateGetterName(string propertyName)
        {
            return $"get_{propertyName}";
        }
        public static string GenerateSetterName(string propertyName)
        {
            return $"set_{propertyName}";
        }

        public static string GenerateDiscardParameterName(int index)
        {
            return $"_<pdis>P_{index}";
        }

        public static string GetNoGenericName(string originalName)
        {
            int index = originalName.IndexOf(GenericDelimiter);
            return index != -1 ? originalName[..index] : originalName;
        }

        public static string GenerateDuplicateLocalName(string localVarName, int index)
        {
            return $"{localVarName}_V__{index}";
        }
        public static string GenerateCaptureLocalName(string localVarName, int index)
        {
            return $"<>{localVarName}__{index}";
        }
        public static string CaptureThisName => "<>__this";
        public static string IEnumerableStateName => "<>__state";
        public static string IEnumerableCurrentName => "<>__current";
        public static string IEnumerableThreadIdName => "<>l__initThreadId";
        public static string GenerateAuxiliaryLocalName(int index)
        {
            return $"V__{index}";
        }
    }
}
