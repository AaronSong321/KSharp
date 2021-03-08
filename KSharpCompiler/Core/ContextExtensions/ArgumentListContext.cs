using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using KSharpCompiler.Grammar;


namespace KSharpCompiler.Grammar
{
    public partial class KSharpParser
    {
        public partial class TypeArgumentListContext
        {
            public TypeArgumentContext[] GetTypeArguments()
            {
                return typeArguments().typeArgument();
            } 
        }

        partial class ArgumentListContext
        {
            public List<PositionalArgumentContext> GetPositionalArguments()
            {
                var g = new List<PositionalArgumentContext>();
                var c1 = positionalArguments();
                while (c1 != null) {
                    g.Insert(0, c1.positionalArgument());
                    c1 = c1.positionalArguments();
                }
                return g;
            }

            public List<NamedArgumentContext> GetNamedArguments()
            {
                var g = new List<NamedArgumentContext>();
                var c1 = namedArguments();
                while (c1 != null) {
                    g.Insert(0, c1.namedArgument());
                    c1 = c1.namedArguments();
                }
                return g;
            }
        }

        partial class ArgumentContext
        {
            public ArgumentPassMode GetPassMode()
            {
                var c1 = argumentPassMode();
                if (c1.IN() != null)
                    return ArgumentPassMode.In;
                if (c1.OUT() != null)
                    return ArgumentPassMode.Out;
                var c2 = expression();
                var c3 = c2.SingleTo<RefExpressionContext>();
                if (c3 != null)
                    return ArgumentPassMode.Ref;
                return ArgumentPassMode.Value;
            }
        }
    }
}



namespace KSharpCompiler.Grammar
{
    public partial class KSharpParser
    {
        public partial class TypeArgumentListContext
        {
            
        }
    }
}