using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;


namespace KSharpCompiler
{
    [ErrorCode(1007, ErrorLevel.Error)]
    public class MemberResolveError: CompilerError
    {
        private MemberResolveError() { }
        public static MemberResolveError WrongAccessType()
        {
            return new MemberResolveError();
        }

        public static MemberResolveError ValueAccess()
        {
            return new MemberResolveError() { Note = $"Cannot access 'value' in non-property getter/setter definition." };
        }
        public static MemberResolveError BackupAccess()
        {
            return new MemberResolveError() { Note = "Cannot access 'field' in non-property getter/setter definition." };
        }
        public static MemberResolveError ThisAccess()
        {
            return new MemberResolveError() { Note = $"Cannot access 'this' in non-instance method definition." };
        }
        public static MemberResolveError BaseAccess()
        {
            return new MemberResolveError() { Note = "Cannot access 'base' in non-instance method definition." };
        }
    }
}
