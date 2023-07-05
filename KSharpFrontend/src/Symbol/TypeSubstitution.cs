namespace KSharp.Compiler.Symbol;

public class TypeSubstitution
{
    public required IKtType Constructor { get; init; }
    public required IKtType[] TypeArguments { get; init; }
}