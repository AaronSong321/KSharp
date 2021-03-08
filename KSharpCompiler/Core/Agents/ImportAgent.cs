using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using KSharp;
using Microsoft.Scripting.Metadata;
using Mono.Cecil;


namespace KSharpCompiler
{
    [ThreadSafe]
    public partial class ImportAgent: CompilerAgent
    {
        private readonly Dictionary<IMemberDefinition, MemberReference> imports = new Dictionary<IMemberDefinition, MemberReference>();
        private readonly Dictionary<MemberReference, IMemberDefinition> resolveResult = new Dictionary<MemberReference, IMemberDefinition>();
        
        public ImportAgent(Compiler compiler) : base(compiler) { }

        [ThreadSafe]
        public TypeReference Import(TypeDefinition type)
        {
            lock (imports) {
                return imports.TryGetValue(type, out var b) ? (b as TypeReference)! : ((imports[type] = Compiler.ILModule.ImportReference(type)) as TypeReference)!;
            }
        }
        [ThreadSafe]
        public MethodReference Import(MethodDefinition method)
        {
            lock (imports) {
                return ((imports.TryGetValue(method, out var b) ? b : imports[method] = Compiler.ILModule.ImportReference(method)) as MethodReference)!;
            }
        }
        // public PropertyReference Import(PropertyDefinition property)
        // {
        //     return ((trans.TryGetValue(property, out var b) ? b : trans[property] = Compiler.ILModule.ImportReference(property)) as PropertyReference)!;
        // }

        [ThreadDontCare]
        public bool IsSameType(TypeReference a, TypeReference b)
        {
            return HasSameName(a, b);
        }

        private bool HasSameName(TypeReference a, TypeReference b)
        {
            return a.FullName == b.FullName;
        }

        [ThreadDontCare]
        public bool IsSameType(TypeDefinition? a, TypeDefinition? b)
        {
            return a != null && b != null && a.MetadataToken == b.MetadataToken;
        }

        [ThreadSafe]
        public TypeDefinition? Resolve(TypeReference type)
        {
            lock (resolveResult) {
                if (resolveResult.TryGetValue(type, out var b)) {
                    return (b as TypeDefinition)!;
                }
                var tryResolve = type.Resolve();
                if (tryResolve != null) {
                    resolveResult[type] = tryResolve;
                    return tryResolve;
                }
                var t = Compiler.ILModule.Types.FirstOrDefault(t1 => IsSameType(t1, type));
                if (t != null) {
                    resolveResult[type] = t;
                }
                return t;
            }
        }

        [ThreadSafe]
        public TypeDefinition ResolveTypeBase(TypeDefinition type)
        {
            var r1 = Resolve(type.BaseType);
            if (r1 != null)
                return r1;
            throw new Exception();
        }

        public IEnumerable<TypeDefinition> GetTypeFamily(TypeDefinition type)
        {
            var t = type;
            var obj = Resolve(Compiler.TypeResolveAgent.Object);
            while (!IsSameType(t, obj)) {
                yield return t;
                t = ResolveTypeBase(type);
            }
        }

        public List<TypeDefinition> GetInterfaceFamily(TypeDefinition interfaceType)
        {
            var g = new List<TypeDefinition>();
            var stack = new Stack<TypeDefinition>();
            stack.Push(interfaceType);
            while (stack.Count != 0) {
                var type = stack.Pop();
                if (!g.Any(t => IsSameType(t, type))) {
                    g.Add(type);
                    type.Interfaces.Select(t => Resolve(t.InterfaceType)!).ForEach(stack.Push);
                }
            }
            return g;
        }

        public TypeDefinition DefineType(string ns, string name, TypeAttributes attributes)
        {
            TypeDefinition t = new TypeDefinition(ns, name, attributes);
            lock (imports) {
                imports[t] = t;
            }
            lock (resolveResult) {
                resolveResult[t] = t;
            }
            return t;
        }
    }
}
