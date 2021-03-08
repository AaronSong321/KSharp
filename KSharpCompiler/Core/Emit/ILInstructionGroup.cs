using System.Collections.Generic;
using System.Linq;
using KSharp;
using Mono.Cecil.Cil;

namespace KSharpCompiler
{
    public sealed class ILInstructionGroup: ResolveResult
    {
        public readonly Instruction? first;
        public readonly Instruction? last;

        private ILInstructionGroup()
        {
        }
        public ILInstructionGroup(Instruction first, Instruction last)
        {
            this.first = first;
            this.last = last;
        }
        public ILInstructionGroup(params ILInstructionGroup[] p)
        {
            p.ForEach(t => ErrorMessage.AddRange(t.ErrorMessage));
            first = p.Aggregate(null as Instruction, (instruction, group) => instruction??group.first);
            last = p.Reverse().Aggregate(null as Instruction, (instruction, group) => instruction??group.last);
        }
        public ILInstructionGroup(ILInstructionGroup first, Instruction last)
        {
            this.first = first.first??last;
            this.last = last;
            ErrorMessage.AddRange(first.ErrorMessage);
        }
        public ILInstructionGroup(IEnumerable<ILInstructionGroup> p): this(p.ToArray())
        {
        }
        public ILInstructionGroup(Instruction first)
        {
            this.first = first;
            last = first;
        }
        public ILInstructionGroup(CompilerMessage errorMessage): base(errorMessage)
        {
        }
        public ILInstructionGroup(IEnumerable<CompilerMessage> errorMessage) : base(errorMessage) { }

        public static ILInstructionGroup Empty { get; } = new ILInstructionGroup();
    }
}
