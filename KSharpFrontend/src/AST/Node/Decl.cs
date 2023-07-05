namespace KSharp.Compiler.Ast;

public abstract class Decl : Node
{
    public string? Identifier { get; set; }
}

public class VarDecl : Decl
{
    public bool IsVar { get; set; }
    public Expression? InitValue { get; set; }
    public TypeAnno? TypeAnnotation { get; set; }
}
