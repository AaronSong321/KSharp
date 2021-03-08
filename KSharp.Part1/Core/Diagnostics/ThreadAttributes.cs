
using System;

namespace KSharp
{
    /// <summary>
    /// Indicates this type or method is guaranteed to be thread safe
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class | AttributeTargets.Struct, Inherited = false)]
    public sealed class ThreadSafeAttribute : Attribute { }

    /// <summary>
    /// Indicates this type or method shall not be accessed by multiple threads
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class | AttributeTargets.Struct)]
    public sealed class NoThreadingAttribute : Attribute { }

    /// <summary>
    /// Indicates this method performs only readonly operations on shared resources
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Field)]
    public sealed class ThreadReadOnlyAttribute : Attribute
    {
    }

    /// <summary>
    /// Indicates this method doesn't access any shared resource and therefore is thread-safe
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class ThreadDontCareAttribute : Attribute { }
}