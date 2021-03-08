using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Antlr4.Runtime;


namespace KSharpCompiler
{
    [ErrorCode(100, ErrorLevel.Fatal)]
    public class InnerCompilerError: CompilerFatalError
    {
        public InnerCompilerError()
        {
            Note = "This should never happen";
        }
        public InnerCompilerError(string errorNote)
        {
            Note = errorNote;
        }
    }

    public abstract class NotImplementedFeature: CompilerFatalError
    {
        protected const string FormatNote = "feature '{0}' is not yet implemented.";
        protected NotImplementedFeature()
        {
            var g = GetType().Name;
            List<string> p = new List<string>();
            List<int> cut = new List<int>();
            for (int i = 0; i < g.Length; ++i)
                if (char.IsUpper(g[i]))
                    cut.Add(i);
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < cut.Count; ++i) {
                if (i != cut.Count - 1) {
                    sb.Append(g[cut[i]..cut[i + 1]].ToLower());
                    sb.Append(' ');
                }
                else
                    sb.Append(g.Substring(cut[i]).ToLower());
            }
            Note = string.Format(FormatNote, sb.ToString());
        }
    }

    [ErrorCode(2, ErrorLevel.Fatal)]
    public class Dynamic: NotImplementedFeature
    {
        public Dynamic()
        {
        }
    }

    [ErrorCode(3, ErrorLevel.Fatal)]
    public class FunctionPointer: NotImplementedFeature
    {
        public FunctionPointer()
        {
        }
    }

    [ErrorCode(4, ErrorLevel.Fatal)]
    public class UnsupportedReferenceType: NotImplementedFeature
    {
        public UnsupportedReferenceType()
        {
            Note = "Reference to C#/K# project is not yet supported. Try referencing the dll instead.";
        }
    }

    [ErrorCode(5, ErrorLevel.Fatal)]
    public sealed class CannotReadFile: CompilerFatalError
    {
        public Exception? Cause { get; }

#nullable disable
        private CannotReadFile()
        {
        }
#nullable restore
        public CannotReadFile(string file, Exception innerException) : this()
        {
            Cause = innerException;
            Note = $"Cannot read file '{file}: {innerException}'";
        }
        public CannotReadFile(string file) : this()
        {
            Note = $"Cannot find file '{file}'";
        }
    }

    [ErrorCode(6, ErrorLevel.Fatal)]
    public class ParserError: CompilerFatalError
    {
        protected ParserError()
        {
        }
        public static ParserError SyntaxError(IRecognizer recognizer, IToken offendingSymbol, int line, int charPositionInLine, string msg, RecognitionException? e)
        {
            return new ParserError() {
                Note = $"Syntax Error: symbol {offendingSymbol} at ({line}, {charPositionInLine})\n" +
                $"Message: {msg}\n" +
                (e != null ? $"{e}" : string.Empty)
            };
        }
    }

    [ErrorCode(7, ErrorLevel.Fatal)]
    public class MissingSystemLibReference : CompilerFatalError
    {
        private const string AdditionalHint = "Check on the terminal if .NET (Version >= 5.0) is properly installed.";
        public static MissingSystemLibReference MissingDllFile(string fileName)
        {
            return new MissingSystemLibReference() {Note = $"Cannot find system class {fileName}. {AdditionalHint}"};
        }
        public static MissingSystemLibReference MissingDllFile(string filename, IEnumerable<string> pathTried)
        {
            return new MissingSystemLibReference() { Note = $"Cannot find system library '{filename}' on path:\n{pathTried.Aggregate(new StringBuilder(), (sb, s) => { sb.Append('\t'); sb.Append(s); sb.Append("\n"); return sb; })}" + '\n' + AdditionalHint};
        }
        public static MissingSystemLibReference MissingType(string typeName)
        {
            return new MissingSystemLibReference() {Note = $"Cannot find system class {typeName}. {AdditionalHint}"};
        }
    }

    [ErrorCode(8, ErrorLevel.Fatal)]
    public class CompilerArgumentError : CompilerFatalError
    {
        public static CompilerArgumentError TypeMismatch<TExpect, TGot>(TGot value)
        {
            return new CompilerArgumentError() {Note = $"{typeof(TExpect).Name} expected, {typeof(TGot).Name}) '{value}' got."};
        }
        public static CompilerArgumentError IncorrectElevatedMessage(int number, ErrorLevel originalLevel, ErrorLevel elevatedLevel)
        {
            return new CompilerArgumentError() {Note = $"Compiler Message KS{number} cannot be elevated to {elevatedLevel.ToString()}."};
        }
        public static CompilerArgumentError DuplicateArgument(string arg1, string arg2)
        {
            string f = arg1 == arg2 ? $"'{arg1}'" : "'{arg1}' and '{arg2}'";
            return new CompilerArgumentError() {Note = $"Duplicate argument {f}."};
        }
    }
}
