using System.Collections.Generic;
using KSharp;
using KSharpCompiler.Grammar;

namespace KSharpCompiler
{
    public sealed class ParameterCollector
    {
        public readonly bool isFixed;
        public LiteralResolveResult? LiteralValue { get; set; }
        public readonly KSharpParser.ParameterDeclarationBodyContext parameterDeclarationBodyContext;
        public readonly int index;
        public bool HasDefaultValue {
            get => parameterDeclarationBodyContext.HasDefaultValue;
        }

        public ParameterCollector(bool isFixed, KSharpParser.ParameterDeclarationBodyContext parameterDeclarationBodyContext, int index)
        {
            this.isFixed = isFixed;
            this.parameterDeclarationBodyContext = parameterDeclarationBodyContext;
            this.index = index;
        }
    }
}


namespace KSharpCompiler.Grammar
{
    public partial class KSharpParser
    {
        public partial class ParameterListContext
        {
            public List<ParameterCollector> GetParameters()
            {
                var f = parameters().fixedParameters()?.fixedParameter();
                var g = new List<ParameterCollector>();
                int index = -1;
                if (f != null)
                    g.AddRange(f.Map(t => new ParameterCollector(true, t.parameterDeclarationBody(), ++index)));
                var c2 = parameters().parameterArray();
                if (c2 != null)
                    g.Add(new ParameterCollector(false, c2.parameterDeclarationBody(), ++index));
                return g;
            }
        }

        partial class ParameterDeclarationBodyContext
        {
            public bool HasDefaultValue => defaultArgument() != null;
            
        }
    }
}
