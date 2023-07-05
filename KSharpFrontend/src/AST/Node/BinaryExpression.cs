namespace KSharp.Compiler.Ast;

public class BinaryExpression : Expression
{
    public Expression Left { get; set; } = null!;
    public NumOp Op { get; set; } = null!;
    public Expression Right { get; set; } = null!;
}

public abstract class Operator : Node
{
}

public enum NumericOperatorKind
{
    Invalid,
    Add,
    Subtract,
    Multiply
}

public class NumOp : Operator
{
    public NumericOperatorKind OpertorKind { get; set; }
}

public static partial class LiteralTokenParser
{
    public static LiteralParseResult<NumericOperatorKind> ParseNumericOperator(Token content)
    {
        return content.Content switch
        {
            "+" => new(NumericOperatorKind.Add, null),
            "-" => new(NumericOperatorKind.Subtract, null),
            "*" => new(NumericOperatorKind.Multiply, null),
            _ => new(default, $"bad operator {content.Content}")
        };
    }
}