using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using KSharp;
using Mono.Cecil;


namespace KSharpCompiler
{
    public abstract class CompilerAgent
    {
        public Compiler Compiler { get; }
        
        protected CompilerAgent(Compiler compiler)
        {
            Compiler = compiler;
        }
        public static void InitAgents(object o)
        {
            var agents = o.GetType().GetFields(ReflectionHelper.InstanceMemberFinder).Where(t => t.FieldType.IsAssignableTo(typeof(CompilerAgent)));
            agents.ForEach(p => p.SetValue(o, p.FieldType.GetConstructor(new Type[] { typeof(Compiler) })!.Invoke(new object[] { o })));
        }

        protected TypeDefinition? ResolveTypeDelegate(TypeReference type)
        {
            return Compiler.ImportAgent.Resolve(type);
        }
        protected TypeReference? ImportTypeDelegate(TypeDefinition type)
        {
            return Compiler.ImportAgent.Import(type);
        }
    }
}
