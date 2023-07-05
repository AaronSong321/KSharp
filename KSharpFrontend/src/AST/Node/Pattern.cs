namespace KSharp.Compiler.Ast;

public class Pattern {
    private string simpleName = null!;
    public string SimpleName {
        get => simpleName;
        set {
            simpleName = value;
        }
    }

}