using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Mono.Cecil;


namespace KSharpCompiler.Grammar
{
    public partial class KSharpParser
    {
        public interface ITypeExpressionContext
        {
            TypeReference? Type { get; set; }
        }
        
        
        public partial class TypeExpressionContext: ITypeExpressionContext
        {
            public TypeReference? Type { get; set; }
            
        }

        public partial class AtomTypeContext : ITypeExpressionContext
        {
            public TypeReference? Type { get; set; }
            
        }
    }
}
