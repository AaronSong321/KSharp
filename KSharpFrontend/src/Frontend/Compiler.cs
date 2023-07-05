
using CommandLine;
using System.IO;

namespace KSharp.Compiler;

public class Compiler
{
    private CompilerArguments Arguments { get; }
    private ExprContext? RootContext { get; set; } = null;

    public Compiler(CompilerArguments arguments)
    {
        Arguments = arguments;
    }

    private void ParseText(string text)
    {
        KSharpLexer lexer = new(new AntlrInputStream(text));
        ParseFromLexer(lexer);
    }

    //private void ParseFile(string filename)
    //{
    //    StreamReader streamReader = new(filename);
    //    KSharpLexer lexer = new(new AntlrInputStream(streamReader));
    //    ParseFromLexer(lexer);
    //}

    private void ParseFromLexer(KSharpLexer lexer)
    {
        KSharpParser parser = new(new CommonTokenStream(lexer));
        RootContext = parser.expr();
    }

    public static void InvokeFromText(string text, string[] args)
    {
        CommandLine.Parser.Default.ParseArguments<CompilerArguments>(args)
            .WithParsed(compilerArguments =>
            {
                var compiler = new Compiler(compilerArguments);
                if (compiler.Arguments.File is not null)
                {
                    throw new("Cannot invoke with any arguments");
                }
                compiler.ParseText(text);
            });

    }

    //public static void InvokeFromCommandLine(string[] args)
    //{
    //    CommandLine.Parser.Default.ParseArguments<CompilerArguments>(args)
    //        .WithParsed(compilerArguments =>
    //        {
    //            new Compiler(compilerArguments).Compile();
    //        })
    //        .WithNotParsed(_ => Console.WriteLine("No arguments given."));
    //}
}