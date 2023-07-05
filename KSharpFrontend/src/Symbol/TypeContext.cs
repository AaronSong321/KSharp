using System.Collections.Immutable;

namespace KSharp.Compiler.Symbol;

// A TypeContext to perform operations on IKtType, e.g. validity check, or subtyping check
public class TypeContext
{
    public bool IsSubtypeOf(IKtType a, IKtType b)
    {
        throw new NotImplementedException();
    }

    public IKtType GLB(IKtType a, IKtType b)
    {
        throw new NotImplementedException();
    }

    public IKtType LUB(IKtType a, IKtType b) {  throw new NotImplementedException(); }

    public bool IsConcreteType(IKtType type) { throw new NotImplementedException(); }
    public bool IsAbstracType(IKtType type) { return !IsConcreteType(type); }

    public bool IsClassType(IKtType type) { throw new NotImplementedException (); }
    public bool IsInterfaceType(IKtType type) { throw new NotImplementedException (); }

    public bool IsDenotableType(IKtType type) { throw new NotImplementedException (); }

    public SimpleClassifierType Any { get; } = new("kotlin", "Any", false);
    public SimpleClassifierType Nothing { get; } = new("kotlin", "Nothing", true);
    public ParameterizedClassifierType BuiltinFunction { get; } = new("kotlin", "Function", true, Array.Empty<IKtType>(), new TypeParameter("R"));
    public SimpleClassifierType BuiltinInt { get; } = new("kotlin", "Int", true);
    public ArrayType GetArrayType(IKtType type)
    {
        return new(type);
    }

    private static bool HasValidTypeName(KType t)
    {
        return true;
    }

    private bool IsWellFormedBaseType(IKtType type)
    {
        return type is not NullableType && IsConcreteType(type) && IsWellFormedType(type);
    }

    private static ImmutableHashSet<IKtType> GetSupertypeTransitiveClosure(IKtType type)
    {
        if (type is ClassifierType h)
        {
            return h.Supertypes.SelectMany(GetSupertypeTransitiveClosure).ToImmutableHashSet();
        }
        return ImmutableHashSet<IKtType>.Empty.Add(type);
    }

    private static bool IsConsistentSupertypeTransitiveClosure(IImmutableSet<IKtType> supertypes)
    {
        return true;
    }

    public bool IsWellFormedType(IKtType type)
    {
        if (type is SimpleClassifierType h1)
        {
            return HasValidTypeName(h1) && h1.Supertypes.All(IsWellFormedBaseType) && IsConsistentSupertypeTransitiveClosure(GetSupertypeTransitiveClosure(h1));
        } else if (type is ParameterizedClassifierType h2)
        {
            return HasValidTypeName(h2) && h2.Supertypes.All(IsWellFormedBaseType) && h2.Parameters.All(IsWellFormedBaseType);
        } else if (type is ConcreteParameterizedClassifierType h3)
        {
            var constructor = h3.TypeConstructor;
            var arguments = h3.TypeArguments;
            if (IsWellFormedType(constructor) && constructor.ParameterNum == arguments.Length)
            {
                return false;
            }
            if (!arguments.All(IsWellFormedType))
                return false;
            // TODO: capture substitution
            return true;
        }
        throw new NotImplementedException();
    }

    private bool DoesNotContradict(IEnumerable<Variance> definitionSiteVariance, IEnumerable<Variance> callSiteVariance)
    {
        return definitionSiteVariance.Zip(callSiteVariance).All(t => DoesNotContradict(t.First, t.Second));
    }

    private bool DoesNotContradict(Variance definitionSiteVariance, Variance callSiteVariance)
    {
        return (definitionSiteVariance, callSiteVariance) switch
        {
            (Variance.Covariant, Variance.Contravariant) => false,
            (Variance.Contravariant, Variance.Covariant) => false,
            _ => true
        };
    }

    public ConcreteParameterizedClassifierType Instantiate(ParameterizedClassifierType constructor, TypeArgument[] typeArguments)
    {
        return new(constructor, typeArguments);
    }
}
