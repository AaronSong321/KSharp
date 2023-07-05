namespace KSharp.Compiler.Symbol;


// anything that may be used as a Kotlin type
public interface IKtType
{

}

public class TypeConstructor: IKtType
{

}

public class ParameterizedType: IKtType
{

}

public class TypeArgument: IKtType
{

}

public class CapturedType: IKtType
{
    public required TypeParameter F { get; init; }
    public required TypeParameter A { get; init; }
}

