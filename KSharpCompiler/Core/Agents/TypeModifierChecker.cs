using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using KSharpCompiler.Grammar;
using Mono.Cecil;


namespace KSharpCompiler
{
    public class TypeModifierChecker: CompileUnitAgent
    {
        public TypeModifierChecker(CompileUnit compiler) : base(compiler) { }

        public TypeAttributes GetAccessibilityAttributes(KSharpParser.AccessibilityModContext? c)
        {
            if (CompileUnit.LocalScopeAgent.types.Traverse().Count() is 1) {
                if (c is null)
                    return TypeAttributes.Public;
                if (c.INTERNAL() != null && c.PROTECTED() != null || c.PRIVATE() != null && c.PROTECTED() != null || c.PROTECTED() != null || c.INTERNAL() != null)
                    Compiler.ErrorCollector.AddCompilerMessage(AccessibilityError.NonNestedType(), c, CompileUnit);
                if (c.PRIVATE() != null)
                    return TypeAttributes.NotPublic;
                if (c.PUBLIC() != null)
                    return TypeAttributes.Public;
            }
            else {
                if (c is null)
                    return TypeAttributes.NestedPublic;
                if (c.INTERNAL() != null && c.PROTECTED() != null)
                    return TypeAttributes.NestedFamORAssem;
                if (c.PRIVATE() != null && c.PROTECTED() != null)
                    return TypeAttributes.NestedFamANDAssem;
                if (c.PROTECTED() != null)
                    return TypeAttributes.NestedFamily;
                if (c.INTERNAL() != null)
                    return TypeAttributes.NestedAssembly;
                if (c.PRIVATE() != null)
                    return TypeAttributes.NestedPrivate;
                if (c.PUBLIC() != null)
                    return TypeAttributes.NestedPublic;
            }
#if DEBUG
            throw new ArgumentOutOfRangeException();
#else
            return 0;
#endif
        }
        
        public static TypeAttributes GetSealedAttributes(KSharpParser.TypeSealedModContext? c)
        {
            if (c is null)
                return TypeAttributes.Sealed;
            if (c.STATIC() != null)
                return TypeAttributes.Sealed | TypeAttributes.Abstract;
            if (c.ABSTRACT() != null)
                return TypeAttributes.Abstract;
            if (c.SEALED() != null)
                return TypeAttributes.Sealed;
            if (c.OPEN() != null)
                return 0;
#if DEBUG
            throw new ArgumentOutOfRangeException();
#else
            return 0;
#endif
        }
    }
}
