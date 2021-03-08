using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using KSharp;


namespace KSharpCompiler.Grammar
{
    public partial class KSharpParser
    {
        public partial class IdentifierContext
        {
            public override string ToString()
            {
                if (Identifier() != null) {
                    return Identifier().GetText();
                }
                if (Keyword() != null) {
                    return Keyword().GetText();
                }
                return SoftKeyword().GetText();
            }

            public string GetTypeReferName()
            {
                return ToString();
            }
        }
        
        public partial class QidContext
        {
            public override string ToString()
            {
                var a = GetIdentifiers();
                var sb = new StringBuilder(a[0].ToString());
                a.Skip(1).ForEach(t => sb.Append(NameGenAgent.NamespaceDelimiter + t.ToString()));
                return sb.ToString();
            }

            public List<IdentifierContext> GetIdentifiers()
            {
                var qid = this;
                var stack = new List<IdentifierContext>() { qid.identifier() };
                while ((qid = qid.qid()) != null) {
                    stack.Insert(0, qid.identifier());
                }
                return stack;
            }
        }

        public partial class GidContext
        {
            public TypeArgumentContext[]? GetTypeArguments()
            {
                var t = typeArgumentList();
                return t?.typeArguments().typeArgument();
            }
            public bool HasTypeArguments()
            {
                return typeArgumentList() != null;
            }
            
            public string GetTypeReferName()
            {
                string idName = identifier().GetTypeReferName();
                return typeArgumentList() is null ? idName : NameGenAgent.GenericQualify(idName, typeArgumentList().GetTypeArguments().Length);
            }

            public string GetNonGenericName()
            {
                return identifier().ToString();
            }
        }
    }
}
