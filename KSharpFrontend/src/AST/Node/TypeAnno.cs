namespace KSharp.Compiler.Ast;


public static partial class Mixin
{

}

public class TypeAnno : Node
{
}

public class SimpleTypeAnno : TypeAnno
{
    public string Identifier { get; }

    public SimpleTypeAnno(string identifier)
    {
        Identifier = identifier;
    }
}