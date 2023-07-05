using Antlr4.Runtime.Tree;
using System.Diagnostics;
using System.Reflection;

namespace KSharp.Compiler.Ast;

public record Token(string? Content, SourcePos BeginPosition)
{
    public Token(IToken token) : this(token.Text, new SourceFilePos(null, token.Line, token.Column))
    {
    }

    public Token(ITerminalNode node) : this(node.Symbol)
    {
    }

    public Token(ParserRuleContext context): this(null, new SourceFilePos(null, context.Start.Line, context.Start.Column)) {}
}

public abstract partial class Node
{
    public Token? Token { get; init; }
}

// A chunk represents all the code of a source file
public sealed class Chunk: Node
{
    public List<Decl> Decls { get; set; } = new();
}

public abstract class Expression : Node
{
}

public abstract class LiteralExpression : Expression
{
    public string Value
    {
        get
        {
            KDebug.Assert(Token != null);
            return Token!.Content!;
        }
    }
}
