using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using KSharp;
using KSharp.Part1.Core;

namespace KSharpCompiler
{
    public class Compiler
    {
        private readonly List<Exception> fatalErrors = new List<Exception>();
        private readonly List<Exception> errors = new List<Exception>();
        private readonly LockChainManager lockChainManager = new LockChainManager();

        private const string kSharpFileExtension = ".ks";

        private void Init()
        {
            lockChainManager.Add(errors, fatalErrors);
        }

        [ThreadSafe]
        public void AddError(Exception er)
        {
            using (lockChainManager.Lock(errors)) {
                errors.Add(er);
            }
        }
        [ThreadSafe]
        public void AddFatalError(Exception er)
        {
            using (lockChainManager.Lock(fatalErrors)) {
                fatalErrors.Add(er);
            }
        }
        /// <summary>
        /// This function is guaranteed to be executed sync.
        /// </summary>
        /// <returns></returns>
        public bool CheckErrors()
        {
            return errors.Count != 0 || fatalErrors.Count != 0;
        }
        /// <summary>
        /// This function is guaranteed to be executed sync.
        /// </summary>
        /// <returns></returns>
        public void PrintErrors()
        {
            if (CheckErrors())
                Console.WriteLine($"Got {errors.Count + fatalErrors.Count} errors:");
            errors.ForEach(Console.WriteLine);
            fatalErrors.ForEach(Console.WriteLine);
        }
        
        public async ValueTask<int> Compile(string[] paths, string[] parameters)
        {
            List<string> fileToCompile = new List<string>(paths.Length);
            foreach (var path in paths) {
                if (Directory.Exists(path)) {
                    fileToCompile.AddRange(FileExtensions.GetFiles(path, f=>f.EndsWith(kSharpFileExtension)));
                }
                else if (File.Exists(path) && path.EndsWith(kSharpFileExtension)) {
                    fileToCompile.Add(Path.GetFullPath(path));
                }
                else {
                    AddFatalError(new FileNotFoundException("file not found", path));
                }
            }
            if (CheckErrors())
                goto printErrorAndReturn;

            try {
                var cus = fileToCompile.Map(path => new CompileUnit(this, path));
                var taskGroup = cus.Map(cu => cu.Lex());
                await Task.WhenAll(taskGroup);
                if (CheckErrors())
                    goto printErrorAndReturn;

                // taskGroup = cus.Map(cu => cu.CollectTypes());
                // await Task.WhenAll(taskGroup);
            }
            catch (Exception e) {
                Console.WriteLine(e.StackTrace);
                goto printErrorAndReturn;
            }
            Console.WriteLine("Compilation success!");
            return 0;
            
            printErrorAndReturn:
            PrintErrors();
            return 1;
        }
        
        public async Task<int> Compile(CompilerArguments args)
        {
            List<string> fileToCompile = new List<string>(args.src.Length);
            foreach (var path in args.src) {
                if (Directory.Exists(path)) {
                    fileToCompile.AddRange(FileExtensions.GetFiles(path, f=>f.EndsWith(kSharpFileExtension)));
                }
                else if (File.Exists(path) && path.EndsWith(kSharpFileExtension)) {
                    fileToCompile.Add(Path.GetFullPath(path));
                }
                else {
                    AddFatalError(new FileNotFoundException("file or path not found", path));
                }
            }
            if (CheckErrors())
                goto printErrorAndReturn;

            try {
                var cus = fileToCompile.Map(path => new CompileUnit(this, path));
                var taskGroup = cus.Map(cu => cu.Lex());
                await Task.WhenAll(taskGroup);
                if (CheckErrors())
                    goto printErrorAndReturn;

                // taskGroup = cus.Map(cu => cu.CollectTypes());
                // await Task.WhenAll(taskGroup);
            }
            catch (Exception e) {
                Console.WriteLine(e);
                goto printErrorAndReturn;
            }
            Console.WriteLine("Compilation success!");
            return 0;
            
            printErrorAndReturn:
            PrintErrors();
            return 1;
        }
        
    }
}
