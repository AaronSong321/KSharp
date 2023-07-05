namespace KSharp.Compiler;

public static class Enumeration
{
    public static (IEnumerable<T>,IEnumerable<T>) Partition<T>(this IEnumerable<T> col, Predicate<T> func)
    {
        LinkedList<T> a=new(),b=new();
        foreach(var elem in col)
        {
            if(func(elem)) a.AddLast(elem);
            else b.AddLast(elem);
        }
        return (a,b);
    }
}