using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using KSharp;
using KSharpCompiler.Grammar;
using Mono.Cecil;


namespace KSharpCompiler.SyntaxRepresentations
{
    public abstract class UsingDirective : SyntaxPart
    {
        
    }

    public class UsingNamespace : UsingDirective
    {
        private readonly KSharpParser.QidContext ids;
        
        public UsingNamespace(KSharpParser.QidContext ids)
        {
            this.ids = ids;
        }
        public override string ToString()
        {
            return ids.ToString();
        }
        public override bool Equals(object? obj)
        {
            return obj is UsingNamespace u && ids.ToString() == u.ToString();
        }
        protected bool Equals(UsingNamespace other)
        {
            return ids.Equals(other.ids);
        }
        public override int GetHashCode()
        {
            return ids.GetHashCode();
        }
        public static bool operator==(UsingNamespace a, UsingNamespace b)
        {
            return a.Equals(b);
        }
        public static bool operator!=(UsingNamespace a, UsingNamespace b)
        {
            return !(a == b);
        }

        
    }

    public class UsingStatic : UsingDirective
    {
        
    }
}
