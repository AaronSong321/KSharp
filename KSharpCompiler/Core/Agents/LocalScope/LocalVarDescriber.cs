using Mono.Cecil.Cil;

namespace KSharpCompiler
{
    public sealed class LocalVarDescriber
    {
        public readonly VariableDefinition variableDefinition;
        public readonly string name;
        public readonly LocalVariableMutability mutability;
        public readonly bool isGenerated;

        public LocalVarDescriber(VariableDefinition variableDefinition, string name, LocalVariableMutability mutability, bool isGenerated)
        {
            this.variableDefinition = variableDefinition;
            this.name = name;
            this.mutability = mutability;
            this.isGenerated = isGenerated;
        }
    }
}
