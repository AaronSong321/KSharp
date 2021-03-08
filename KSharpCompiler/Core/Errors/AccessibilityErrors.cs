using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;


namespace KSharpCompiler
{
    [ErrorCode(1011, ErrorLevel.Error)]
    public sealed class AccessibilityError : CompilerError
    {
        private AccessibilityError() { }
        public static AccessibilityError NonNestedType()
        {
            return new AccessibilityError() {
                Note = "Non nested type can only have accessibility 'public' or 'internal'."
            };
        }

        public static AccessibilityError MemberExposeType()
        {
            return new AccessibilityError() {
                Note = "Member cannot expose type which is not public."
            };
        }
        
    }
}
