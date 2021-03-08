using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;


namespace KSharpCompiler
{   
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public sealed class ErrorCodeAttribute: Attribute
    {
        /// <summary>
        /// ErrorCode of the Error. This is guaranteed to be unique.
        /// </summary>
        public int ErrorCode { get; }
        /// <summary>
        /// Level 0 indicates <see cref="CompilerFatalError"/>
        /// Level 1 indicates <see cref="CompilerError"/>
        /// Level 2-4 indicates <see cref="CompilerWarning"/>
        /// Level 5-6 indicates <see cref="CompilerMessage"/>
        /// </summary>
        public ErrorLevel Level { get; }
        public ErrorCodeAttribute(int errorCode, ErrorLevel level)
        {
            ErrorCode = errorCode;
            Level = level;
        }
    }

    public enum ErrorLevel
    {
        Fatal=0,
        Error=1,
        WarningLevel1=2,
        WarningLevel2=3,
        WarningLevel3=4,
        MessageLevel1=5,
        MessageLevel2=6,
        Diagnostic = 7
    }

    public static class ErrorLevelUtils
    {
        public static bool IsFatalError(ErrorLevel level)
        {
            return (int)level <= Convert.ToInt32(ErrorLevel.Fatal);
        }
        public static bool IsError(ErrorLevel level)
        {
            return (int)level <= Convert.ToInt32(ErrorLevel.Error);
        }
        public static bool IsWarning(ErrorLevel level)
        {
            return (int)level <= Convert.ToInt32(ErrorLevel.WarningLevel1);
        }
    }
    
    public abstract class CompilerMessage
    {
#pragma warning disable 8618
        // public ErrorLevel Level { get => (GetType().GetCustomAttribute(typeof(ErrorCodeAttribute)) as ErrorCodeAttribute)!.Level; }
        // public int Code { get => (GetType().GetCustomAttribute(typeof(ErrorCodeAttribute)) as ErrorCodeAttribute)!.ErrorCode; }
        public ErrorLevel Level { get; set; }
        public int Code { get; set; }
        public string Note { get; set; }
        public string SourcePath { get; set; }
        public int LineNumber { get; set; }
        public int ColNumber { get; set; }
#pragma warning restore 8618

        public override string ToString()
        {
            return $"{SourcePath} ({LineNumber}, {ColNumber}: Compiler Message KS{Code:d4}: {Note}";
        }
        public void SetInformation(CompileUnit cu, Antlr4.Runtime.ParserRuleContext context)
        {
            SourcePath = cu.Path;
            LineNumber = context.Start.Line;
            ColNumber = context.Start.Column;
        }
    }

    public abstract class CompilerWarning: CompilerMessage
    {
        public override string ToString()
        {
            return $"{SourcePath} ({LineNumber}, {ColNumber}: Compiler Warning Level {Level} KS{Code:d4}: {Note}";
        }
    }

    public abstract class CompilerError : CompilerMessage
    {
        public override string ToString()
        {
            return $"{SourcePath}({LineNumber}, {ColNumber}): Compiler Error KS{Code:d4}: {Note}";
        }
    }

    public abstract class CompilerFatalError : CompilerMessage
    {
        public override string ToString()
        {
            return $"{SourcePath} ({LineNumber}, {ColNumber}: Compiler Fatal Error KS{Code:d4}: {Note}";
        }
    }
}
