using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;


namespace KSharpCompiler
{
    public partial class LocalTypeResolveAgent
    {
        public TypeResolveResult ResolveParameterType(ParameterCollector parameter)
        {
            var body = parameter.parameterDeclarationBodyContext;
            var typeAnnotator = body.parameterTypeAnnotator();
            if (typeAnnotator is null) {
                var defaultValue = body.defaultArgument()!.expression();
                if (defaultValue.IsLiteralExpression()) {
                    var c1 = defaultValue.GetLiteralExpression()!;
                    var t1 = Compiler.TypeResolveAgent.ResolveLiteralValue(c1);
                    return t1;
                }
                throw new NotImplementedException();
            }
            return ResolveType(typeAnnotator.type());
        }
    }
}
