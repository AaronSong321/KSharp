using System.Diagnostics.CodeAnalysis;

namespace KSharp.Compiler.Symbol;

// the abstract base class of all sorts of Kotlin types
public abstract class KType: IKtType
{
    public string Package { get; }
    public string Name { get; }

    public KType(string package, string name)
    {
        Package = package;
        Name = name;
    }
}

public class TypeParameter : KType
{
    public ParameterizedClassifierType DeclarationType { get; internal set; } = null!;
    public Variance DefinitionSiteVariance { get; }
    public TypeParameter(string name, Variance definitionSiteVariance) : base(string.Empty, name)
    {
        DefinitionSiteVariance = definitionSiteVariance;
    }
}

public class TypeArgument : IKtType
{
    public IKtType Type { get; }
    public Variance CallSiteVariance { get; }
    public TypeArgument(IKtType type, Variance callSiteVariance)
    {
        Type = type;
        CallSiteVariance = callSiteVariance;
    }
}

public abstract class ClassifierType : KType
{
    public IKtType[] Supertypes { get; }
    public bool IsClass { get; }
    public ClassifierType(string package, string name, bool isClass, params IKtType[] supertypes): base(package, name)
    {
        IsClass = isClass;
        Supertypes = supertypes;
    }
}

public class SimpleClassifierType : ClassifierType
{
    public SimpleClassifierType(string package, string name, bool isClass, params IKtType[] supertypes)
        : base(package, name, isClass, supertypes)
    {
    }
}

public class ParameterizedClassifierType : ClassifierType
{
    public TypeParameter[] Parameters { get; }
    public int ParameterNum => Parameters.Length;
    public ParameterizedClassifierType(string package, string name, bool isClass, IKtType[] supertypes, params TypeParameter[] parameters)
        : base(package, name, isClass, supertypes)
    {
        foreach (var parameter in parameters)
        {
            parameter.DeclarationType = this;
        }
        Parameters = parameters;
    }
}

public class ConcreteParameterizedClassifierType: IKtType
{
    public TypeArgument[] TypeArguments { get; }
    public ParameterizedClassifierType TypeConstructor { get; }
    public ConcreteParameterizedClassifierType(ParameterizedClassifierType typeConstructor, TypeArgument[] typeArguments)
    {
        TypeArguments = typeArguments;
        TypeConstructor = typeConstructor;
    }
}

//public sealed class FunctionType : KType
//{
//    public required IKtType[] InputTypes { get; init; }
//    public required IKtType? ReceiverType { get; init; }
//    public required IKtType ReturnType { get; init; }
//}

public abstract class ArrayTypeBase: KType
{
    public IKtType ElementType { get; }
    public ArrayTypeBase(IKtType elementType) : base("kotlin", "Array")
    {
        ElementType = elementType;
    }
}

public sealed class ArrayType : ArrayTypeBase
{
    public ArrayType(IKtType elementType) : base(elementType) { }
}

public sealed class SpecializedArrayType: ArrayTypeBase
{
    public SpecializedArrayType(IKtType elementType) : base(elementType) { }
}

public sealed class NullableType : KType
{
    public IKtType Type { get; }
    public NullableType(string package, string name, IKtType type): base(package, name)
    {
        Type = type;
    }
}

public class IntersectionType : KType
{
    public IKtType[] Types { get; }
    public IntersectionType(string package, string name, params IKtType[] types) : base(package, name)
    {
        Types = types;
    }
}

public class UnionType : KType
{
    public IKtType[] Types { get; }
    public UnionType(string package, string name, params IKtType[] types) : base(package, name)
    {
        Types = types;
    }
}
