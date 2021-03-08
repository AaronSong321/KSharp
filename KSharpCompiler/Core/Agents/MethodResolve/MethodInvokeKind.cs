namespace KSharpCompiler
{
    public enum MethodInvokeKind
    {
        Method,
        Delegate,
        PropertyGet,
        PropertySet,
        IndexerGet,
        IndexerSet,
        Operator
    }

    public static class MethodInvokeKindExtensions
    {
        public static bool IsSetter(this MethodInvokeKind invokeKind)
        {
            return invokeKind == MethodInvokeKind.PropertySet || invokeKind == MethodInvokeKind.IndexerSet;
        }
    }
}
