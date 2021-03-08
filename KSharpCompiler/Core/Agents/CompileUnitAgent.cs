using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using KSharp;


namespace KSharpCompiler
{
    public abstract class CompileUnitAgent
    {
        public CompileUnit CompileUnit { get; }
        public Compiler Compiler { get; }
        private 
        
        protected CompileUnitAgent(CompileUnit compileUnit)
        {
            CompileUnit = compileUnit;
            Compiler = compileUnit.compiler;
        }
        public static void InitAgents(object o)
        {
            var agents = o.GetType().GetFields(ReflectionHelper.InstanceMemberFinder).Where(t => t.FieldType.IsAssignableTo(typeof(CompileUnitAgent)));
            agents.ForEach(p => p.SetValue(o, p.FieldType.GetConstructor(new Type[] { typeof(CompileUnit) })!.Invoke(new object[] { o })));
        }
    }
}
