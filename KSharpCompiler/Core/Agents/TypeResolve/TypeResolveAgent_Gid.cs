using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using KSharp;
using KSharpCompiler.Grammar;
using Mono.Cecil;


namespace KSharpCompiler
{
    public partial class LocalTypeResolveAgent
    {
        public IdentifierResolveResult ResolveGid(KSharpParser.GidContext c)
        {
            IdentifierResolveResult? ResolveGidInLocals(KSharpParser.GidContext c1)
            {
                var name = c1.GetNonGenericName();
                foreach (var method in CompileUnit.LocalScopeAgent.methods.Traverse()) {
                    var p = method.FindLocalVar(name);
                    if (p != null)
                        return new IdentifierResolveResult(p);
                    var p2 = method.FindParameter(name);
                    if (p2 != null)
                        return new IdentifierResolveResult(p2);
                }
                return null;
            }

            IdentifierResolveResult? ResolveGidInInstanceMember(KSharpParser.GidContext c1)
            {
                string name = c1.GetNonGenericName();
                var type = CompileUnit.LocalScopeAgent.types.Top();
                IdentifierResolveResult.UnionCase? resultCase = null;
                var member2 = new List<MethodDefinition>();
                var member3 = new List<PropertyDefinition>();

                if (type.IsInterface) {
                    foreach (var bt in Compiler.ImportAgent.GetInterfaceFamily(type)){
                        var mem = bt.GetInstanceMembers().Where(t => t.GetReferName() == name);
                        if (c1.HasTypeArguments()) {
                            mem = mem.WithGenericParameters(c1.GetTypeArguments()!.Length);
                        }
                        var members = mem.ToArray();
                        if (members.Length is 0)
                            return null;
                        var firstMember = members[0];
                        bool isNewSlot = false;
                        switch (firstMember) {
                            case MethodDefinition m1:
                                resultCase ??= IdentifierResolveResult.UnionCase.MethodDefinition;
                                var r2 = members.Map(t => (t as MethodDefinition)!).Where(t => !t.IsReuseSlot || t.IsNewSlot);
                                member2.AddRange(r2);
                                if (m1.IsNewSlot) {
                                    isNewSlot = true;
                                }
                                break;
                            case FieldDefinition m1:
                                return new IdentifierResolveResult(m1);
                            case PropertyDefinition m1:
                                resultCase ??= IdentifierResolveResult.UnionCase.PropertyDefinition;
                                member3.AddRange(members.Map(t => (t as PropertyDefinition)!).Where(t => {
                                    var m2 = t.GetAnyAccessor();
                                    return !m2.IsReuseSlot || m2.IsNewSlot;
                                }));
                                if (m1.GetAnyAccessor().IsNewSlot) {
                                    isNewSlot = true;
                                }
                                break;
                            case EventDefinition m1:
                                return new IdentifierResolveResult(m1);
                            default: throw new ArgumentOutOfRangeException();
                        }
                        if (isNewSlot)
                            break;
                    }
                    return resultCase switch {
                        IdentifierResolveResult.UnionCase.Failure => null,
                        IdentifierResolveResult.UnionCase.MethodDefinition => new IdentifierResolveResult(member2),
                        IdentifierResolveResult.UnionCase.PropertyDefinition => new IdentifierResolveResult(member3),
                        _ => throw new ArgumentOutOfRangeException()
                    };
                }
                else {
                    foreach (var bt in Compiler.ImportAgent.GetTypeFamily(type)){
                        var mem = bt.GetInstanceMembers().Where(t => t.GetReferName() == name);
                        if (c1.HasTypeArguments()) {
                            mem = mem.WithGenericParameters(c1.GetTypeArguments()!.Length);
                        }
                        var members = mem.ToArray();
                        if (members.Length is 0)
                            return null;
                        var firstMember = members[0];
                        bool isNewSlot = false;
                        switch (firstMember) {
                            case MethodDefinition m1:
                                resultCase ??= IdentifierResolveResult.UnionCase.MethodDefinition;
                                var r2 = members.Map(t => (t as MethodDefinition)!).Where(t => !t.IsReuseSlot || t.IsNewSlot);
                                member2.AddRange(r2);
                                if (m1.IsNewSlot) {
                                    isNewSlot = true;
                                }
                                break;
                            case PropertyDefinition m1:
                                resultCase ??= IdentifierResolveResult.UnionCase.PropertyDefinition;
                                member3.AddRange(members.Map(t => (t as PropertyDefinition)!).Where(t => {
                                    var m2 = t.GetAnyAccessor();
                                    return !m2.IsReuseSlot || m2.IsNewSlot;
                                }));
                                if (m1.GetAnyAccessor().IsNewSlot) {
                                    isNewSlot = true;
                                }
                                break;
                            case EventDefinition m1:
                                return new IdentifierResolveResult(m1);
                            default: throw new ArgumentOutOfRangeException();
                        }
                        if (isNewSlot)
                            break;
                    }
                    return resultCase switch {
                        IdentifierResolveResult.UnionCase.Failure => null,
                        IdentifierResolveResult.UnionCase.MethodDefinition => new IdentifierResolveResult(member2),
                        IdentifierResolveResult.UnionCase.PropertyDefinition => new IdentifierResolveResult(member3),
                        _ => throw new ArgumentOutOfRangeException()
                    };
                }
            }
            
            IdentifierResolveResult? ResolveGidInStaticMember(KSharpParser.GidContext c1)
            {
                string name = c1.GetNonGenericName();
                var allTypes = CompileUnit.LocalScopeAgent.types.Traverse();
                IdentifierResolveResult.UnionCase? resultCase = null;
                var member1 = new List<TypeDefinition>();
                var member2 = new List<MethodDefinition>();
                var member3 = new List<PropertyDefinition>();

                foreach (var type in allTypes) {
                    if (type.IsInterface) {
                        foreach (var bt in Compiler.ImportAgent.GetInterfaceFamily(type)){
                            var mem = bt.GetStaticMembers().Where(t => t.GetReferName() == name);
                            if (c1.HasTypeArguments()) {
                                mem = mem.WithGenericParameters(c1.GetTypeArguments()!.Length);
                            }
                            var members = mem.ToArray();
                            if (members.Length is 0)
                                return null;
                            var firstMember = members[0];
                            bool isNewSlot = false;
                            switch (firstMember) {
                                case MethodDefinition m1:
                                    resultCase ??= IdentifierResolveResult.UnionCase.MethodDefinition;
                                    var r2 = members.Map(t => (t as MethodDefinition)!).Where(t => !t.IsReuseSlot || t.IsNewSlot);
                                    member2.AddRange(r2);
                                    if (m1.IsNewSlot) {
                                        isNewSlot = true;
                                    }
                                    break;
                                case TypeDefinition m1:
                                    resultCase ??= IdentifierResolveResult.UnionCase.TypeDefinition;
                                    member1.AddRange(members.Map(t => (t as TypeDefinition)!));
                                    break;
                                case FieldDefinition m1:
                                    return new IdentifierResolveResult(m1);
                                case PropertyDefinition m1:
                                    resultCase ??= IdentifierResolveResult.UnionCase.PropertyDefinition;
                                    member3.AddRange(members.Map(t => (t as PropertyDefinition)!).Where(t => {
                                        var m2 = t.GetAnyAccessor();
                                        return !m2.IsReuseSlot || m2.IsNewSlot;
                                    }));
                                    if (m1.GetAnyAccessor().IsNewSlot) {
                                        isNewSlot = true;
                                    }
                                    break;
                                case EventDefinition m1:
                                    return new IdentifierResolveResult(m1);
                                default: throw new ArgumentOutOfRangeException();
                            }
                            if (isNewSlot)
                                break;
                        }
                    }
                    else {
                        foreach (var bt in Compiler.ImportAgent.GetTypeFamily(type)){
                            var mem = bt.GetStaticMembers().Where(t => t.GetReferName() == name);
                            if (c1.HasTypeArguments()) {
                                mem = mem.WithGenericParameters(c1.GetTypeArguments()!.Length);
                            }
                            var members = mem.ToArray();
                            if (members.Length is 0)
                                return null;
                            var firstMember = members[0];
                            bool isNewSlot = false;
                            switch (firstMember) {
                                case MethodDefinition m1:
                                    resultCase ??= IdentifierResolveResult.UnionCase.MethodDefinition;
                                    var r2 = members.Map(t => (t as MethodDefinition)!).Where(t => !t.IsReuseSlot || t.IsNewSlot);
                                    member2.AddRange(r2);
                                    if (m1.IsNewSlot) {
                                        isNewSlot = true;
                                    }
                                    break;
                                case PropertyDefinition m1:
                                    resultCase ??= IdentifierResolveResult.UnionCase.PropertyDefinition;
                                    member3.AddRange(members.Map(t => (t as PropertyDefinition)!).Where(t => {
                                        var m2 = t.GetAnyAccessor();
                                        return !m2.IsReuseSlot || m2.IsNewSlot;
                                    }));
                                    if (m1.GetAnyAccessor().IsNewSlot) {
                                        isNewSlot = true;
                                    }
                                    break;
                                case EventDefinition m1:
                                    return new IdentifierResolveResult(m1);
                                default: throw new ArgumentOutOfRangeException();
                            }
                            if (isNewSlot)
                                break;
                        }
                    }
                    if (resultCase != null) {
                        switch (resultCase) {
                            case IdentifierResolveResult.UnionCase.MethodDefinition:
                                return new IdentifierResolveResult(member2);
                            case IdentifierResolveResult.UnionCase.PropertyDefinition:
                                return new IdentifierResolveResult(member3);
                        }
                    }
                }
                return resultCase switch {
                    IdentifierResolveResult.UnionCase.Failure => null,
                    _ => throw new ArgumentOutOfRangeException()
                };
            }

            return (c.HasTypeArguments() ? null : ResolveGidInLocals(c))??ResolveGidInInstanceMember(c)??ResolveGidInStaticMember(c)??FindGidType(c)??FindGidNs(c)??IdentifierResolveResult.NoMatch;
        }
        
        private IdentifierResolveResult? FindGidNs(KSharpParser.GidContext gid)
        {
            return gid.HasTypeArguments() ? null : Compiler.TypeResolveAgent.FindGidNs(gid.GetNonGenericName());
        }

        private IdentifierResolveResult? FindGidType(KSharpParser.GidContext gid)
        {
            var find = FindType(gid.GetTypeReferName());
            return GetSingleHolder(gid.GetTypeReferName(), find);
        }
        
        private IdentifierResolveResult GetSingleHolder(string referName, IReadOnlyList<TypeDefinition>? h)
        {
            if (h is null || h.Count == 0) {
                return new IdentifierResolveResult(UndefinedIdentifier.UndefinedNamespace(referName));
            }
            if (h.Count > 1) {
                return new IdentifierResolveResult(AmbiguousIdentifier.Ambiguous(referName, h.Select(t => t.FullName)));
            }
            return new IdentifierResolveResult(h[0]);
        }

        [ThreadSafe]
        private List<TypeDefinition>? FindType(string referName)
        {
            var res1 = FindTypeInUsingNamespace(referName);
            if (res1.Count >= 1) {
                return res1;
            }
            var res2 = FindTypeInGlobalNamespace(referName);
            if (res2 != null)
                return new List<TypeDefinition>() { res2 };
            
            return null;
        }
        [ThreadSafe]
        private TypeDefinition? FindTypeInGlobalNamespace(string referName)
        {
            return Compiler.TypeResolveAgent.TryFindType("", referName);
        }

        [ThreadSafe]
        private List<TypeDefinition> FindTypeInUsingNamespace(string referName)
        {
            var findTypes = new List<TypeDefinition>();
            CompileUnit.UsingAgent.usingNsBlocks.ForEach(ns => {
                var r1 = Compiler.TypeResolveAgent.TryFindType(ns.ToString(), referName);
                if (r1 != null)
                    findTypes.Add(r1);
            });
            return findTypes;
        }

    }
}
