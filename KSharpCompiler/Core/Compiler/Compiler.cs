using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using IronPython.Modules;
using KSharp;
using KSharp.Part1.Core;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace KSharpCompiler
{
    public class Compiler
    {
        private readonly LockChainManager lockChainManager = new LockChainManager();

        private const string KSharpFileExtension = ".ks";
        
        public AssemblyDefinition ILAssembly { get; private set; } = null!;
        public ModuleDefinition ILModule { get; private set; } = null!;
        public CompilerArguments Arguments { get; private set; } = null!;

        public TypeResolveAgent TypeResolveAgent { get; } = null!;
        public ErrorCollector ErrorCollector { get; } = null!;
        public ImportAgent ImportAgent { get; } = null!;
        public ConversionResolveAgent ConversionResolveAgent { get; } = null!;
        public MethodResolveAgent MethodResolveAgent { get; } = null!;

        public enum AssemblyReferenceType
        {
            SystemCoreLib,
            KSharpCoreLib,
            AspNet,
            CSharpProject,
            KSharpProject,
            UserDefinedDll,
        }

        private void AddAssemblyReference(AssemblyNameDefinition name, AssemblyReferenceType assemblyInfo, string? fullPath = null)
        {
            lock (ILModule) {
                if (ILModule.AssemblyReferences.Contains(name)) 
                    return;
                ILModule.AssemblyReferences.Add(name);
            }
            string? assemblyPath = assemblyInfo switch {
                AssemblyReferenceType.SystemCoreLib => fullPath ?? GetSystemCoreLibPath(name, name.Version.Major),
                AssemblyReferenceType.KSharpCoreLib => fullPath ?? Path.Combine(".", name.Name + ".dll"),
                AssemblyReferenceType.UserDefinedDll => fullPath!,
                _ => throw new NotSupportedException()
            };
            if (assemblyPath is null) {
                ErrorCollector.AddCompilerMessage(MissingSystemLibReference.MissingDllFile(name.Name));
                return;
            }
            TypeResolveAgent.ExtractNamespaceFromAssemblyReference(assemblyPath);
        }

        private static string? GetSystemCoreLibPath(AssemblyNameDefinition ass, int majorVersion)
        {
            var osVer = Environment.OSVersion;
            string basePath = osVer.Platform switch {
                PlatformID.Win32NT => @"C:\Program Files (x86)\dotnet\shared\Microsoft.NETCore.App",
                PlatformID.Unix => "/usr/local/share/dotnet/shared/Microsoft.NETCore.App",
                _ => throw new NotSupportedException("unsupported os")
            };

            const string patternIntegerPart = @"\.(?<minor>\d+)\.(?<build>\d+)";
            string patternString = osVer.Platform switch {
                PlatformID.Win32NT => @$".*App\\{majorVersion}",
                PlatformID.Unix => $".*App/{majorVersion}",
                _ => throw new NotSupportedException("unsupported os")
            } + patternIntegerPart;
            var pattern = new Regex(patternString);
            var ps = Directory.GetDirectories(basePath);
            List<(Version, string)> assemPath = new List<(Version, string)>();
            foreach (var matchedPath in ps) {
                var b = pattern.Matches(matchedPath);
                foreach (Match? c in b) {
                    if (int.TryParse(c!.Groups["minor"].Value, out int minor) && int.TryParse(c.Groups["build"].Value, out int build)) {
                        var t = c.Groups["revision"];
                        int revision = 0;
                        if (int.TryParse(t.Value, out int sss))
                            revision = sss;
                        assemPath.Add((new Version(majorVersion, minor, build, revision), matchedPath));
                        break;
                    }
                }
            }
            assemPath.Sort((pair1, pair2) => pair1.Item1.CompareTo(pair2.Item1));
            if (assemPath.Count == 0) {
                return null;
            }
            string lowestVersionLibraryPath = assemPath[0].Item2;
            var a = Directory.GetFiles(lowestVersionLibraryPath, ass.Name + ".dll");
            if (a.Length == 0)
                return null;
            return a[0];
        }

        private async Task AddSystemAssemblyReferences()
        {
            string[] commonSystemLib = {
                "mscorlib", "System.Private.CoreLib", "System.Linq",
            };
            string[] nonCompulsorySystemLib = {
                "System.Xml",
            };
            // mscorlib contains reference to all these dll
            int major = 5;
            int minor = 0;
            Version commonSystemLibraryVersion = new Version(major, minor);
            var task1 = commonSystemLib.Map(libName => Task.Run(() => {
                AddAssemblyReference(new AssemblyNameDefinition(libName, commonSystemLibraryVersion), AssemblyReferenceType.SystemCoreLib);
            }));

            List<string> kSharpStandardLibrary = new List<string>{
                "KSharp.Part1",
            };
            if (!Arguments.buildKSharpStdLib) {
                kSharpStandardLibrary.Add("KSharp.Part2");
            }
            major = 1;
            minor = 0;
            Version kSharpStandLibVersion = new Version(major, minor);
            var task2 = kSharpStandardLibrary.Map(libName => Task.Run(() => {
                AddAssemblyReference(new AssemblyNameDefinition(libName, kSharpStandLibVersion), AssemblyReferenceType.KSharpCoreLib);
            }));

            await Task.WhenAll(task1);
            await Task.WhenAll(task2);
        }

        private async Task AddUserDefinedAssemblyReferences(IEnumerable<string> fullPath)
        {
            
        }

        private void AddDummyConstructor(TypeDefinition type, MethodReference baseTypeCtor)
        {
            var ct = new MethodDefinition(".ctor", MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName, ILModule.TypeSystem.Void);
            var il = ct.Body.GetILProcessor();
            il.LoadThis();
            il.CallBase(baseTypeCtor);
            il.Return();
            type.Methods.Add(ct);
        }
        private async Task BuildDummyLibrary()
        {
            var lib = AssemblyDefinition.CreateAssembly(new AssemblyNameDefinition(Arguments.@out, new Version(1, 0, 0, 0)), Arguments.@out, new ModuleParameters() { Kind = ModuleKind.Dll, Runtime = TargetRuntime.Net_4_0});
            ILAssembly = lib;
            ILModule = lib.MainModule;
            CompilerAgent.InitAgents(this);
            ErrorCollector.CollectErrorInfo(Array.Empty<string>(), Array.Empty<string>(), Array.Empty<string>());
            var task1 = AddSystemAssemblyReferences();
            var task2 = AddUserDefinedAssemblyReferences(Array.Empty<string>());
            
            var t1 = new TypeDefinition("Jmas", "D3", TypeAttributes.Class | TypeAttributes.Public | TypeAttributes.Sealed, ILModule.TypeSystem.Object);
            ILModule.Types.Add(t1);

            AddDummyConstructor(t1, ILModule.ImportReference(typeof(object).GetConstructor(Type.EmptyTypes)));
            var t1M1 = new MethodDefinition("Roar", MethodAttributes.Public, ILModule.TypeSystem.Void);
            var il = t1M1.Body.GetILProcessor();
            il.LoadConstant("hello world");
            il.CallStatic(ILModule.ImportReference(typeof(Console).GetMethod("WriteLine", new[] { typeof(string) })));
            il.Return();
            t1.Methods.Add(t1M1);

            await task1;
            await task2;
        }

        public TypeDefinition DefineType(string ns, string name, TypeAttributes attributes)
        {
            var def = ImportAgent.DefineType(ns, name, attributes);
            lock (ILModule) {
                ILModule.Types.Add(def);
            }
            TypeResolveAgent.DefineType(def);
            return def;
        }
        
        public async Task<int> Compile(CompilerArguments args)
        {
            Arguments = args;
            var task = BuildDummyLibrary();
            
            List<string> fileToCompile = new List<string>(args.src.Length);
            List<string> a = new List<string>();
            foreach (var path in args.src) {
                if (Directory.Exists(path)) {
                    fileToCompile.AddRange(FileExtensions.GetFiles(path, f=>f.EndsWith(KSharpFileExtension)));
                }
                else if (File.Exists(path) && path.EndsWith(KSharpFileExtension)) {
                    fileToCompile.Add(Path.GetFullPath(path));
                }
                else {
                    ErrorCollector.AddCompilerMessage(new CannotReadFile(path));
                    a.Add(path);
                }
            }
            a.ForEach(t => fileToCompile.Remove(t));
            await task;
            
            if (ErrorCollector.HasError())
                goto printErrorAndReturn;

            try {
                var cus = fileToCompile.Map(path => new CompileUnit(this, path));
                var taskGroup = cus.Map(cu => cu.CollectTypes());
                await Task.WhenAll(taskGroup);
                if (ErrorCollector.HasError())
                    goto printErrorAndReturn;

                taskGroup = cus.Map(cu => cu.DefineMembers());
                await Task.WhenAll(taskGroup);
                if (ErrorCollector.HasError())
                    goto printErrorAndReturn;
                
                taskGroup = cus.Map(cu => cu.ImplementMethods());
                await Task.WhenAll(taskGroup);
                if (ErrorCollector.HasError())
                    goto printErrorAndReturn;
                
                Directory.CreateDirectory("out");
                ILAssembly.Write(Path.Combine("out", $"{Arguments.@out}"));
            }
            catch (Exception e) {
                Console.WriteLine(e);
                goto printErrorAndReturn;
            }
            Console.WriteLine("Compilation success!");
            return 0;
            
            printErrorAndReturn:
            ErrorCollector.PrintCompilerMessages();
            return 1;
        }
        
    }
}
