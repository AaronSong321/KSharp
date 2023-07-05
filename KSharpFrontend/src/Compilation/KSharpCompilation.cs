namespace KSharp.Compiler;

using KSharp.Compiler.Ast;
using KSharp.Compiler.Diagnostic;

public class KSharpCompilation {
    private DiagnosticBag dbag;
    private string Text;

    public KSharpCompilation(string text)
    {
        dbag = new();
        Text = text;
    }

    public static int CompileFile(string path)
    {
        KSharpCompilation c = new(File.ReadAllText(path));

        return c.Compile();
    }

    public int Compile()
    {
        return BuildAst();
    }

    public void AddDiagnostic(Diagnostic.Diagnostic diagnostic)
    {
        dbag.Add(diagnostic);
    }

    private int BuildAst()
    {
        AntlrInputStream inputStream = new(Text);
        KSharpParser parser = new(new CommonTokenStream(new KSharpLexer(inputStream)));
        var fileContext = parser.file();
        AstBuilder ast = new();
        var chunk = ast.BuildChunk(fileContext);
        return 0;
    }
}

