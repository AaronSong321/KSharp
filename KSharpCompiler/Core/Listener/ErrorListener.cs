using System;
using Antlr4.Runtime;
using Antlr4.Runtime.Atn;
using Antlr4.Runtime.Dfa;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Sharpen;

namespace KSharpCompiler
{
    public class ErrorListener : IParserErrorListener
    {
        public CompileUnit CompileUnit { get; }
        public ErrorListener(CompileUnit cu)
        {
            CompileUnit = cu;
        }

        public void ReportAmbiguity([NotNull] Parser recognizer, [NotNull] DFA dfa, int startIndex, int stopIndex, bool exact, [Nullable] BitSet ambigAlts, [NotNull] ATNConfigSet configs)
        {
            Console.WriteLine($"Ambiguity {startIndex}..{stopIndex} {exact}");
            Console.WriteLine($"and {ambigAlts} {configs}");
        }

        public void ReportAttemptingFullContext([NotNull] Parser recognizer, [NotNull] DFA dfa, int startIndex, int stopIndex, [Nullable] BitSet conflictingAlts, [NotNull] SimulatorState conflictState)
        {
            //Console.WriteLine($"Attempting full context dfa={dfa} {startIndex}..{stopIndex}");
            //Console.WriteLine($"and {conflictingAlts} {conflictState}");
        }

        public void ReportContextSensitivity([NotNull] Parser recognizer, [NotNull] DFA dfa, int startIndex, int stopIndex, int prediction, [NotNull] SimulatorState acceptState)
        {
            //Console.WriteLine($"Context sensitivity full context  {startIndex}..{stopIndex}");
            //Console.WriteLine($"and {prediction} {acceptState}");
        }

        public void SyntaxError([NotNull] IRecognizer recognizer, [Nullable] IToken offendingSymbol, int line, int charPositionInLine, [NotNull] string msg, [Nullable] RecognitionException e)
        {
            Console.WriteLine($"Syntax Error: {CompileUnit.Path} at ({line}, {charPositionInLine}) " +
                $"with message: {msg}");
            // This is usually not helpful
            //if (e is not null)
            //    Console.WriteLine($"Inner exception: {e}");
        }
    }
}
