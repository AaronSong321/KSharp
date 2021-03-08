using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using KSharpCompiler.Grammar;
using Mono.Cecil;


namespace KSharpCompiler
{
    public class MethodModifierChecker: CompilerAgent
    {

        public MethodModifierChecker(Compiler compiler) : base(compiler) { }

        public static bool IsStatic(KSharpParser.StaticModContext? c)
        {
            return c != null;
        }
        public static MethodMutability GetMutability(KSharpParser.MutableModContext? c)
        {
            if (c is null)
                return MethodMutability.Immutable;
            if (c.MUTABLE() != null)
                return MethodMutability.Mutable;
            if (c.IMMUTABLE() != null)
                return MethodMutability.Immutable;
            if (c.PURE() != null)
                return MethodMutability.Pure;
            if (c.CONSTEXPR() != null)
                return MethodMutability.Constexpr;
            if (c.CONSTEVAL() != null)
                return MethodMutability.Consteval;
#if DEBUG
            throw new ArgumentOutOfRangeException();
#else
            return 0;
#endif
        }

        public static MethodAttributes GetStaticAttributes(KSharpParser.StaticModContext? c)
        {
            if (c != null)
                return MethodAttributes.Static;
            return 0;
        }
        
        public static MethodAttributes GetOverrideAttribute(KSharpParser.OverrideModContext? c)
        {
            if (c is null) {
                return 0;
            }
            if (c.OVERRIDE() != null) {
                return MethodAttributes.Virtual | MethodAttributes.ReuseSlot;
            }
            if (c.SEALED() != null)
                return MethodAttributes.Virtual | MethodAttributes.ReuseSlot | MethodAttributes.Final;
            if (c.OPEN() != null)
                return MethodAttributes.Virtual;
#if DEBUG
            throw new ArgumentOutOfRangeException();
#else
            return 0;
#endif
        }

        public static MethodAttributes GetNewAttribute(KSharpParser.NewModContext? c)
        {
            return c != null ? MethodAttributes.NewSlot : 0;
        }

        public static MethodAttributes GetMethodAccessibilityAttribute(KSharpParser.AccessibilityModContext? c)
        {
            if (c is null)
                return MethodAttributes.Public;
            if (c.INTERNAL() != null && c.PROTECTED() != null)
                return MethodAttributes.FamORAssem;
            if (c.PRIVATE() != null && c.PROTECTED() != null)
                return MethodAttributes.FamANDAssem;
            if (c.PROTECTED() != null)
                return MethodAttributes.Family;
            if (c.INTERNAL() != null)
                return MethodAttributes.Assembly;
            if (c.PRIVATE() != null)
                return MethodAttributes.Private;
            if (c.PUBLIC() != null)
                return MethodAttributes.Public;
#if DEBUG
            throw new ArgumentOutOfRangeException();
#else
            return 0;
#endif
        }
    }
}
