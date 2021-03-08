using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using KSharp;
using KSharpCompiler.Grammar;
using KSharpCompiler.SyntaxRepresentations;
using Mono.Cecil;


namespace KSharpCompiler
{
    public enum IdUse
    {
        Get,
        Set,
        Invoke
    }
    
    public partial class TypeResolveAgent: CompilerAgent
    {
        private readonly List<AssemblyNameDefinition> referenceAssemblies = new List<AssemblyNameDefinition>();
        private readonly Dictionary<string, Dictionary<string, TypeDefinition>> referencedTypes = new Dictionary<string, Dictionary<string, TypeDefinition>>();
        private readonly Dictionary<string, Dictionary<string, TypeDefinition>> definedTypes = new Dictionary<string, Dictionary<string, TypeDefinition>>();
        private readonly List<IMemberDefinition> globalMethods = new List<IMemberDefinition>();

        public TypeResolveAgent(Compiler compiler) : base(compiler) { }
        
        [ThreadSafe]
        public Dictionary<string, TypeDefinition>? ResolveNamespace(KSharpParser.QidContext id)
        {
            lock (referencedTypes) {
                return referencedTypes.TryGetValue(id.ToString(), out var m) ? m : null;
            }
        }

        [ThreadSafe]
        public void DefineType(TypeDefinition type)
        {
            lock (referencedTypes) {
                var m = referencedTypes.FindOrAddNew(type.Namespace);
                m[type.Name] = type;
            }
        }
        
        [ThreadSafe]
        public void ExtractNamespaceFromAssemblyReference(string assemblyPath)
        {
            var ns = new Dictionary<string, List<TypeDefinition>>();
            var tar = AssemblyDefinition.ReadAssembly(assemblyPath);
            lock (referenceAssemblies) {
                if (referenceAssemblies.Any(name => name.Name == tar.Name.Name && name.Version >= tar.Name.Version)) {
                    return;
                }
                else {
                    referenceAssemblies.Add(tar.Name);
                }
            }
            var accessibleTypes = tar.MainModule.Types.Where(GlobalTypeResolveAgent.NeedToImport);
            accessibleTypes.ForEach(t => {
                ns.FindOrAddNew(t.Namespace).Add(t);
            });
            lock (referencedTypes) {
                ns.ForEach(kvp => {
                    var (key, value) = kvp;
                    var m = referencedTypes.FindOrAddNew(key);
                    value.Where(p => !m.ContainsKey(p.Name)).ForEach(p => m[p.Name] = p);
                });
            }
        }
        

        public IdentifierResolveResult ResolveNestedGid(KSharpParser.GidContext gid, IdentifierResolveResult lastResult)
        {
            var t = lastResult.kind switch {
                IdentifierResolveResult.UnionCase.Failure => lastResult,
                IdentifierResolveResult.UnionCase.NamespaceDefinition => TryFindType(lastResult.namespaceDefinition!.FullName, gid.GetTypeReferName())??FindNestedGidNs(gid, lastResult.namespaceDefinition),
                IdentifierResolveResult.UnionCase.TypeDefinition => FindGidMember(gid, lastResult.typeDefinition!),
                IdentifierResolveResult.UnionCase.PropertyDefinition => FindGidMember(gid, lastResult.propertyDefinition!),
                _ => throw new Exception()
            };
            return t??IdentifierResolveResult.NoMatch;
        }

        public TypeDefinition? TryFindType(string ns, string referName)
        {
            lock (referencedTypes) {
                if (referencedTypes.TryGetValue(ns.ToString(), out var b)) {
                    if (b.TryGetValue(referName, out var c)) {
                        return c;
                    }
                }
                return null;
            }
        }
        
        private IdentifierResolveResult? FindNestedGidNs(KSharpParser.GidContext gid, NamespaceDefinition ns)
        {
            return gid.HasTypeArguments() ? null : FindGidNs(NameGenAgent.NamespaceQualify(ns.FullName, gid.GetTypeReferName()));
        }

        
        [ThreadSafe]
        public IdentifierResolveResult? FindGidNs(string n)
        {
            lock (referencedTypes) {
                return referencedTypes.ContainsKey(n) ? new IdentifierResolveResult(new NamespaceDefinition(n, n)) : null;
            }
        }

        private IdentifierResolveResult? FindGidMember(KSharpParser.GidContext gid, TypeDefinition type)
        {
            var find = type.FindMember(gid.GetNonGenericName()).ToArray();
            if (find.Length is 0)
                return null;
            if (find[0] is PropertyDefinition) {
                var t = new List<PropertyDefinition>();
                find.ForEach(e => t.Add(e as PropertyDefinition??throw new InvalidOperationException()));
                return new IdentifierResolveResult(t);
            }
            if (find[0] is MethodDefinition) {
                var t = new List<MethodDefinition>();
                find.ForEach(e => t.Add(e as MethodDefinition??throw new InvalidOperationException()));
                return new IdentifierResolveResult(t);
            }
            if (find[0] is FieldDefinition m) {
                return new IdentifierResolveResult(m);
            }
            return null;
        }

        private IdentifierResolveResult? FindGidMember(KSharpParser.GidContext gid, PropertyGroup p)
        {
            if (p.HasParameters) {
                return new IdentifierResolveResult(MemberAccessError.AccessIndexerMember(p));
            }
            var propertyType = p.properties[0].PropertyType;
            var r = Compiler.ImportAgent.Resolve(propertyType);
            if (r is null) {
                return new IdentifierResolveResult(TypeResolveError.Error(propertyType));
            }
            return FindGidMember(gid, r);
        }

        private IdentifierResolveResult? FindGidMember(KSharpParser.GidContext gid, MethodGroup m)
        {
            return new IdentifierResolveResult(MemberAccessError.AccessMethodMember(m));
        }

        // public TypeDefinition? ResolveNestType(TypeDefinition? type, KSharpParser.IdentifierContext id)
        // {
        //     return type?.NestedTypes.FirstOrDefault(t => t.GetTypeReferName() == id.GetTypeReferName());
        // }
        // public TypeDefinition? ResolveNestType(TypeDefinition? type, KSharpParser.GidContext id)
        // {
        //     return type?.NestedTypes.FirstOrDefault(t => t.GetTypeReferName() == id.GetTypeReferName());
        // }

        // public TypeDefinition? ResolveType(KSharpParser.GidContext id, UsingAgent a)
        // {
        //     var find = FindType(id.GetTypeReferName(), a);
        //     return GetSingleHolder(id.GetTypeReferName(), find);
        // }
        

    }
}
