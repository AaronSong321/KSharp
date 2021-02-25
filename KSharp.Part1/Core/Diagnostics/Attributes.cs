using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;


namespace KSharp
{
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class ThreadSafeAttribute : Attribute
    {
        
    }
    
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class UnionAttribute : Attribute
    {

    }

    [AttributeUsage(AttributeTargets.Struct)]
    public sealed class UnionCaseAttribute: Attribute
    {
        
    }

    [AttributeUsage(AttributeTargets.Method)]
    public class GlobalFunctionAttribute : Attribute {
        
    }

    [AttributeUsage(AttributeTargets.Method)]
    public sealed class InfixFunctionAttribute : GlobalFunctionAttribute {
        
    }
}
