using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Mono.Cecil;


namespace KSharpCompiler
{
    /// <summary>
    /// A diagnostic message doesn't imply an error, it's just a message for more information
    /// </summary>
    public abstract class CompilerDiagnosticMessage: CompilerMessage
    {
        
    }

    /// <summary>
    /// Indicates a method match fail
    /// </summary>
    [ErrorCode(7001, ErrorLevel.Diagnostic)]
    public class MethodMatchFailure : CompilerDiagnosticMessage
    {
        private MethodMatchFailure() { }
        public static MethodMatchFailure NamedArgumentMissingParameter(string argumentName)
        {
            return new MethodMatchFailure { Note = $"Cannot find a corresponding parameter for argument '{argumentName}'" };
        }
        public static MethodMatchFailure NoParameterForArgument(int position)
        {
            return new MethodMatchFailure { Note = $"No place for argument {position}" };
        }
        public static MethodMatchFailure NoArgumentForParameters(IEnumerable<ParameterResolveSignature> missingParameters)
        {
            return new MethodMatchFailure { Note = $"No arguments for parameters {missingParameters}" };
        }
        public static MethodMatchFailure NoConversion(TypeReference argument, TypeReference parameter, string parameterName)
        {
            return new MethodMatchFailure { Note = $"No conversion from type {argument} to type {parameter} at parameter {parameterName}" };
        }
        public static MethodMatchFailure ByPointerParameterIsNotIdentical(TypeReference argument, TypeReference parameter, string parameterName)
        {
            return new MethodMatchFailure { Note = $"Cannot convert from type {argument} to type {parameter} on by pointer parameter {parameterName}" };
        }
    }
}
