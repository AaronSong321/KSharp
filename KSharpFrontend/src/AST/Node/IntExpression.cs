using System.Numerics;

namespace KSharp.Compiler.Ast;


public sealed class IntExpression : LiteralExpression
{
    public IntExpression(Token token)
    {
        Token = token;
    }
}

public record LiteralParseResult<T>(T? Value, string? Result);

public static partial class LiteralTokenParser
{
    public static LiteralParseResult<BigInteger> ParseDecimalInt(Token intLiteral)
    {
        BigInteger ans = 0;
        int pos = 1;
        for (int i = 0; i < intLiteral.Content!.Length; i++) {
            char k = intLiteral.Content[i];
            if (k == '-')
            {
                pos = -1;
                continue;
            }
            if (k == '_') continue;
            ans = ans * 10 + k - '0';
        }
        ans *= pos;
        if (ans > int.MaxValue) return new(default, $"Decimal integer litearl {intLiteral.Content} (at {intLiteral.BeginPosition}) exceeds int upper bound {int.MaxValue}");
        else if (ans < int.MinValue) return new(default, $"Decimal integer literal {intLiteral.Content} (at {intLiteral.BeginPosition}) exceeds int lower bound {int.MinValue}");
        else return new(ans, null);
    }
}