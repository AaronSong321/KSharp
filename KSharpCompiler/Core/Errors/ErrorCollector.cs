using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Antlr4.Runtime;
using KSharp;


namespace KSharpCompiler
{
    public class ErrorCollector: CompilerAgent
    {
        public ErrorCollector(Compiler compiler) : base(compiler) { }
        
        private Dictionary<int, ErrorLevel> ElevateLevel { get; } = new Dictionary<int, ErrorLevel>();
        private Dictionary<string, (int, ErrorLevel)> ErrorLevelSelect { get; } = new Dictionary<string, (int, ErrorLevel)>();
        
        private List<CompilerMessage> Messages { get; } = new List<CompilerMessage>();
        private List<CompilerMessage> Warnings { get; } = new List<CompilerMessage>();
        private List<CompilerMessage> Errors { get; } = new List<CompilerMessage>();
        private List<CompilerMessage> FatalErrors { get; } = new List<CompilerMessage>();

        [NoThreading]
        public void CollectErrorInfo(IEnumerable<string> warningAsError, IEnumerable<string> messageAsWarning, IEnumerable<string> messageAsError)
        {
            var errorSelect = new Dictionary<int, Type>();
            Assembly.GetAssembly(GetType())!.Modules.SelectMany(m => m.GetTypes()).Where(t => t.IsSubclassOf(typeof(CompilerMessage)) && !t.IsAbstract).ForEach(t => {
                if (t.GetCustomAttribute(typeof(ErrorCodeAttribute)) is ErrorCodeAttribute p) {
                    ErrorLevelSelect[t.FullName!] = (p.ErrorCode, p.Level);
                    if (errorSelect.ContainsKey(p.ErrorCode)) {
                        FatalErrors.Add(new InnerCompilerError($"Duplicate Error Code {p.ErrorCode} on {errorSelect[p.ErrorCode].FullName} and {t.FullName}"));
                    }
                    errorSelect[p.ErrorCode] = t;
                }
                else {
                    FatalErrors.Add(new InnerCompilerError($"Compiler Message {t.FullName} doesn't have a {nameof(ErrorCodeAttribute)} on it."));
                }
            });
            Regex pattern1 = new Regex(@"$(\d{4})^");
            Regex pattern2 = new Regex(@"$(\w\w+)(\d{4})^");

            int Extract(string pa)
            {
                var b = pattern1.Matches(pa);
                if (b.Count != 0) {
                    var match = b[0];
                    if (int.TryParse(match.Groups[0].Value, out int number)) {
                        return number;
                    }
                    goto def;
                }
                b = pattern2.Matches(pa);
                if (b.Count != 0) {
                    var match = b[0];
                    if (int.TryParse(match.Groups[1].Value, out int number)) {
                        return number;
                    }
                }
                def: AddCompilerMessage(CompilerArgumentError.TypeMismatch<int,string>(pa));
                return -1;
            }

            void Process(int errorCode, ErrorLevel level)
            {
                if (errorCode == -1)
                    return;
                var t = errorSelect![errorCode];
                var (f, originalLevel) = ErrorLevelSelect[t.FullName!];
                if ((int)originalLevel <= (int)level)
                    AddCompilerMessage(CompilerArgumentError.IncorrectElevatedMessage(errorCode, originalLevel, level));
                else {
                    if (ElevateLevel.ContainsKey(errorCode))
                        AddCompilerMessage(CompilerArgumentError.DuplicateArgument(errorCode.ToString(), errorCode.ToString()));
                    else {
                        ElevateLevel[errorCode] = level;
                        ErrorLevelSelect[t.FullName!] = (errorCode, level);
                    }
                }
            }
            
            warningAsError.Select(Extract).ForEach(l => Process(l, ErrorLevel.Error));
            messageAsWarning.Select(Extract).ForEach(l => Process(l, ErrorLevel.MessageLevel1));
            messageAsError.Select(Extract).ForEach(l => Process(l, ErrorLevel.Error));
        }
        private void AddMessage(CompilerMessage message)
        {
            lock (Messages) {
                Messages.Add(message);
            }
        }
        private void AddWarning(CompilerMessage warning)
        {
            lock (Warnings) {
                Warnings.Add(warning);
            }
        }
        private void AddError(CompilerMessage error)
        {
            lock (Errors) {
                Errors.Add(error);
            }
        }
        private void AddFatalError(CompilerMessage error)
        {
            lock (FatalErrors) {
                FatalErrors.Add(error);
            }
        }

        [ThreadSafe]
        public void AddCompilerMessage(CompilerMessage message)
        {
            (int, ErrorLevel) fam;
            lock (ErrorLevelSelect) {
                ErrorLevelSelect.TryGetValue(message.GetType().FullName!, out fam);
            }
            var (h, level) = fam;
            message.Code = h;
            message.Level = level;
            if (ErrorLevelUtils.IsFatalError(level))
                AddFatalError(message);
            else if (ErrorLevelUtils.IsError(level))
                AddError(message);
            else if (ErrorLevelUtils.IsWarning(level))
                AddWarning(message);
            else
                AddMessage(message);
        }
        
        [ThreadSafe]
        public void AddCompilerMessage(CompilerMessage message, ParserRuleContext ctx, CompileUnit c)
        {
            message.SourcePath = c.Path;
            message.LineNumber = ctx.Start.Line;
            message.ColNumber = ctx.Start.Column;
            AddCompilerMessage(message);
        }
        
        [NoThreading]
        public void PrintCompilerMessages()
        {
            Messages.ForEach(Console.WriteLine);
            Warnings.ForEach(Console.WriteLine);
            Errors.ForEach(Console.WriteLine);
            FatalErrors.ForEach(Console.WriteLine);
        }
        public bool HasError()
        {
            return Errors.Count != 0 || FatalErrors.Count != 0;
        }
    }
}
