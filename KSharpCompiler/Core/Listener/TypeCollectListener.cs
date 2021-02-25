using System;
using System.IO;
using Community.CsharpSqlite;
using KSharpCompiler;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace KSharpCompiler
{
    public class TypeCollectListener: BaseListener
    {
        public TypeCollectListener(CompileUnit cu) : base(cu)
        {
            
        }

        private void AddDummyConstructor(TypeDefinition type, MethodReference baseTypeCtor, ModuleDefinition module)
        {
            var ct = new MethodDefinition(".ctor", MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName, module.TypeSystem.Void);
            var il = ct.Body.GetILProcessor();
            il.LoadThis();
            il.CallBase(baseTypeCtor);
            il.Return();
            type.Methods.Add(ct);
        }
        
        public void BuildDummyExecutable()
        {
            var myHelloWorldApp = AssemblyDefinition.CreateAssembly(new AssemblyNameDefinition("HelloWorld", new Version(1, 0, 0, 0)), "HelloWorld", new ModuleParameters());

            var module = myHelloWorldApp.MainModule;
            var programType = new TypeDefinition("HelloWorld", "Program", TypeAttributes.Class | TypeAttributes.Public, module.TypeSystem.Object);
            module.Types.Add(programType);
            var ctor = new MethodDefinition(".ctor", MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName, module.TypeSystem.Void);
            
            var il = ctor.Body.GetILProcessor();
            il.Append(il.Create(OpCodes.Ldarg_0));
            il.Append(il.Create(OpCodes.Call, module.ImportReference(typeof(object).GetConstructor(Array.Empty<Type>()))));
            il.Append(il.Create(OpCodes.Nop));
            il.Append(il.Create(OpCodes.Ret));
            programType.Methods.Add(ctor);


            var mainMethod = new MethodDefinition("Main", MethodAttributes.Public | MethodAttributes.Static, module.TypeSystem.Void);
            programType.Methods.Add(mainMethod);
            var argsParameter = new ParameterDefinition("args", ParameterAttributes.None, module.ImportReference(typeof(string[])));
            mainMethod.Parameters.Add(argsParameter);
            
            il = mainMethod.Body.GetILProcessor();
            il.Append(il.Create(OpCodes.Nop));
            il.Append(il.Create(OpCodes.Ldstr, "Hello World"));
            var writeLineMethod = il.Create(OpCodes.Call, module.ImportReference(typeof(Console).GetMethod("WriteLine", new[] { typeof(string) })));
            il.Append(writeLineMethod);
            il.Append(il.Create(OpCodes.Nop));
            il.Append(il.Create(OpCodes.Ret));


            myHelloWorldApp.EntryPoint = mainMethod;
            myHelloWorldApp.Write("HelloWorld.dll");
        }

        public void BuildDummyLibrary()
        {
            var lib = AssemblyDefinition.CreateAssembly(new AssemblyNameDefinition("KSharpLibrary1", new Version(1, 0, 0, 0)), "KSharpLibrary1", new ModuleParameters() { Kind = ModuleKind.Dll, Runtime = TargetRuntime.Net_4_0});
            var module = lib.MainModule;
            
            module.Name.Print();
            module.FileName.Print();
            Console.WriteLine("here");
            var t1 = new TypeDefinition("Jmas", "D3", TypeAttributes.Class | TypeAttributes.Public | TypeAttributes.Sealed, module.TypeSystem.Object);
            module.Types.Add(t1);

            AddDummyConstructor(t1, module.ImportReference(typeof(object).GetConstructor(Type.EmptyTypes)), module);
            var t1M1 = new MethodDefinition("Roar", MethodAttributes.Public, module.TypeSystem.Void);
            var il = t1M1.Body.GetILProcessor();
            il.LoadConstant("hello world");
            il.CallStatic(module.ImportReference(typeof(Console).GetMethod("WriteLine", new[] { typeof(string) })));
            il.Return();
            t1.Methods.Add(t1M1);

            Directory.CreateDirectory("out");
            lib.Write("out/KSharpLibrary1.dll");
        }
    }
}
