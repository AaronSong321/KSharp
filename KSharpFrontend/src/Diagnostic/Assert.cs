using System.Diagnostics;
using System.Numerics;

namespace KSharp.Compiler;

static class KDebug
{
    [Conditional("DEBUG")]
    public static void Assert(bool condition)
    {
        Debug.Assert(condition);
    }
}