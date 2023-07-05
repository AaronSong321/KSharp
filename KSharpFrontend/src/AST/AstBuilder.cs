namespace KSharp.Compiler.Ast;

public partial class AstBuilder
{
    public partial Chunk BuildChunk(FileContext context)
    {
        return FileListener.Visit(context);
    }
}