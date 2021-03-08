using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using KSharpCompiler.Grammar;
using Mono.Cecil;


namespace KSharpCompiler
{
    public sealed class LiteralResolveResult: TypeResolveResult
    {
        public enum LiteralType
        {
            Unit,
            Void,
            Bool,
            Int,
            String
        }

        public readonly LiteralType literalType;
        public readonly object? value;

        public LiteralResolveResult(LiteralType literalType, object? value, TypeReference type): base(type)
        {
            this.literalType = literalType;
            this.value = value;
        }
        public LiteralResolveResult(CompilerMessage errorMessage) : base(errorMessage) { }
    }
    
    public partial class TypeResolveAgent
    {
        public TypeReference Int => Compiler.ILModule.TypeSystem.Int32;
        public TypeReference Float => Compiler.ILModule.TypeSystem.Single;
        public TypeReference Double => Compiler.ILModule.TypeSystem.Double;
        public TypeReference Void => Compiler.ILModule.TypeSystem.Void;
        public TypeReference String => Compiler.ILModule.TypeSystem.String;
        public TypeReference Boolean => Compiler.ILModule.TypeSystem.Boolean;
        public TypeReference Object => Compiler.ILModule.TypeSystem.Object;
        
        public TypeDefinition Nullable => FindDefiniteType("System", "Nullable`1");
        public TypeDefinition IEnumerableDefinition => FindDefiniteType("System.Collections", "IEnumerable");
        public TypeDefinition IEnumerable1Definition => FindDefiniteType("System.Collections.Generic", "IEnumerable`1");
        public TypeDefinition IAsyncEnumerable1Definition => FindDefiniteType("System.Collections.Generic", "IAsyncEnumerable`1");
        public TypeReference IEnumerable1 => Compiler.ImportAgent.Import(IEnumerable1Definition);
        
        public TypeDefinition IntDefinition => Compiler.ImportAgent.Resolve(Int)!;
        public TypeDefinition VoidDefinition => Compiler.ImportAgent.Resolve(Void)!;
        public TypeReference Unit => Compiler.ImportAgent.Import(UnitDefinition);
        public TypeDefinition UnitDefinition => FindDefiniteType("KSharp", "Unit");

        private TypeDefinition FindDefiniteType(string ns, string name)
        {
            lock (referencedTypes) {
                return referencedTypes[ns][name];
            }
        }
        
        public LiteralResolveResult ResolveLiteralValue(KSharpParser.LiteralExpressionContext c)
        {
            var literalValue = c.GetText();
            if (c.integerLiteral() != null) {
                return int.TryParse(literalValue, out int a) ? new LiteralResolveResult(LiteralResolveResult.LiteralType.Int, a, Int) : new LiteralResolveResult(LiteralError.ParseError(literalValue, Int));
            }
            if (c.realLiteral() != null) {
                if (NameGenAgent.Float32Suffix.Contains(literalValue[^1])) {
                    return float.TryParse(literalValue, out float a) ? new LiteralResolveResult(LiteralResolveResult.LiteralType.Int, a, Int) : new LiteralResolveResult(LiteralError.ParseError(literalValue, Float));
                }
                else 
                    return float.TryParse(literalValue, out float a) ? new LiteralResolveResult(LiteralResolveResult.LiteralType.Int, a, Int): new LiteralResolveResult(LiteralError.ParseError(literalValue, Double));
            }
            if (c.boolLiteral() != null) {
                return bool.TryParse(literalValue, out bool a) ? new LiteralResolveResult(LiteralResolveResult.LiteralType.Bool, a, Boolean) : new LiteralResolveResult(LiteralError.ParseError(literalValue, Boolean));
            }
            if (c.nullLiteral() != null) {
                return new LiteralResolveResult(LiteralResolveResult.LiteralType.Void, null, Void);
            }
            throw new NotImplementedException();
        }
    }
}
