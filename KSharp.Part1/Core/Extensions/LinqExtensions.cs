using System;
using System.Collections.Generic;
using System.Linq;

namespace KSharp
{
    public static class LinqExtensions
    {
        public static void ForEach<T>(this IEnumerable<T> collection, Action<T> func)
        {
            if (func is null)
                throw new ArgumentNullException(nameof(func));
            if (collection is null)
                throw new ArgumentNullException(nameof(collection));
            foreach (var elem in collection) {
                func(elem);
            }
        }
        public static void ForEach<T1, T2>(IEnumerable<T1> col1, IEnumerable<T2> col2, Action<T1, T2> func)
        {
            if (col1 == null) throw new ArgumentNullException(nameof(col1));
            if (col2 == null) throw new ArgumentNullException(nameof(col2));
            if (func == null) throw new ArgumentNullException(nameof(func));
            if (col1.Count() != col2.Count())
                throw new ArgumentException("Collections have different counts.");
            using var it1 = col1.GetEnumerator();
            using var it2 = col2.GetEnumerator();
            while (it1.MoveNext()) {
                it2.MoveNext();
                func(it1.Current, it2.Current);
            }
        }
        public static void ForEachIndex<T>(this IEnumerable<T> collection, Action<T, int> func)
        {
            if (func is null)
                throw new ArgumentNullException(nameof(func));
            if (collection is null)
                throw new ArgumentNullException(nameof(collection));
            int index = -1;
            foreach (var elem in collection) {
                func(elem, ++index);
            }
        }

        public static IEnumerable<T> WhereNot<T>(this IEnumerable<T> collection, Func<T, bool> func)
        {
            foreach (var elem in collection)
                if (!func(elem))
                    yield return elem;
        }
    }

    public static class ArrayLinqExtensions
    {
        public static U[] Map<T, U>(this T[] collection, Func<T, U> func)
        {
            var g = new U[collection.Length];
            for (int i = 0; i < collection.Length; ++i) {
                g[i] = func(collection[i]);
            }
            return g;
        }
        public static T[] Filter<T>(this T[] collection, Predicate<T> predicate)
        {
            T[] g = new T[collection.Length];
            int num = 0;
            for (int i = 0; i < collection.Length; ++i)
                if (predicate(collection[i]))
                    g[num++] = collection[i];
            T[] res = new T[num];
            Array.Copy(g, res, num);
            return res;
        }
        public static T[] NonNull<T>(this T?[] collection)
            where T : class
        {
            T[] g = new T[collection.Length];
            int num = 0;
            for (int i = 0; i < collection.Length; ++i)
                if (collection[i] != null)
                    g[num++] = collection[i]!;
            T[] res = new T[num];
            Array.Copy(g, res, num);
            return res;
        }
    }

    public static class ListLinqExtensions
    {
        public static List<T> Filter<T>(this List<T> collection, Predicate<T> predicate)
        {
            List<T> g = new List<T>(collection.Count);
            foreach (var elem in collection)
                if (predicate(elem))
                    g.Add(elem);
            return g;
        }
        // public static System.Collections.Generic.List<T> Filter<T>(this System.Collections.Generic.List<T> collection, delegate*<T, bool> predicate)
        // {
        //     System.Collections.Generic.List<T> g = new List<T>(collection.Count);
        //     foreach (var elem in collection)
        //         if (predicate(elem))
        //             g.Add(elem);
        //     return g;
        // }
        public static List<T> FilterIndex<T>(this List<T> collection, Func<T, int, bool> predicate)
        {
            List<T> g = new List<T>(collection.Count);
            int index = -1;
            foreach (var elem in collection) {
                if (predicate(elem, ++index))
                    g.Add(elem);
            }
            return g;
        }
        public static List<(T, T)> Filter2<T>(IList<T> c1, IList<T> c2, Func<T, T, bool> predicate)
        {
            List<(T, T)> g = new List<(T, T)>(c1.Count);
            for (int i = 0; i < c1.Count; ++i)
                if (predicate(c1[i], c2[i]))
                    g.Add((c1[i], c2[i]));
            return g;
        }

        public static List<U> Map<T, U>(this List<T> collection, Func<T, U> mapMethod)
        {
            List<U> g = new List<U>(collection.Count);
            foreach (var elem in collection)
                g.Add(mapMethod(elem));
            return g;
        }
        // public static System.Collections.Generic.List<U> Map<T, U>(this System.Collections.Generic.List<T> collection, delegate*<T, U> mapMethod)
        // {
        //     System.Collections.Generic.List<U> g = new(collection.Count);
        //     foreach (var elem in collection)
        //         g.Add(mapMethod(elem));
        //     return g;
        // }
        public static List<TResult> MapIndex<T, TResult>(this List<T> collection, Func<T, int, TResult> mapMethod)
        {
            List<TResult> g = new List<TResult>(collection.Count);
            int i = -1;
            foreach (var elem in collection) {
                g.Add(mapMethod(elem, ++i));
            }
            return g;
        }
        public static List<TResult> Map2<T1, T2, TResult>(IList<T1> c1, IList<T2> c2, Func<T1, T2, TResult> met)
        {
            if (c1.Count != c2.Count)
                throw new ArgumentException("System.Collections.Generic.List count mismatch.");
            List<TResult> g = new List<TResult>(c1.Count);
            for (int i = 0; i < c1.Count; ++i) {
                g.Add(met(c1[i], c2[i]));
            }
            return g;
        }
        public static List<T> NonNull<T>(this List<T> collection)
            where T : class
        {
            List<T> g = new List<T>(collection.Count);
            foreach (var elem in collection) {
                if (elem != null)
                    g.Add(elem!);
            }
            return g;
        }
        public static List<T> Flatten<T>(this IList<List<T>> collection)
        {
            List<T> g = new List<T>();
            foreach (var p in collection) {
                foreach (var elem in p)
                    g.Add(elem);
            }
            return g;
        }
        public static void RemoveAt<T>(this IList<T> collection, Index index)
        {
            collection.RemoveAt(index.IsFromEnd ? collection.Count - index.Value : index.Value);
        }
    }

    public static class DictionaryLinqExtensions
    {
        public static U FindOrAddNew<T, U>(this IDictionary<T, U> dic, T key)
            where U: new() where T: notnull
        {
            if (dic.TryGetValue(key, out var value))
                return value;
            else {
                var b = new U();
                dic[key] = b;
                return b;
            }
        }
    }

    public interface ILinqProvider<out T>: IEnumerable<T>
    {
        void ForEach(Action<T> func)
        {
            if (func is null)
                throw new ArgumentNullException(nameof(func));
            foreach (var elem in this) {
                func(elem);
            }
        }
        void ForEachIndex(Action<T, int> func)
        {
            if (func is null)
                throw new ArgumentNullException(nameof(func));
            int index = -1;
            foreach (var elem in this) {
                func(elem, ++index);
            }
        }
        IEnumerable<T> Filter(Func<T, bool> func)
        {
            return this.Where(func);
        }
        IEnumerable<T> FilterIndex(Func<T, int, bool> func)
        {
            return this.Where(func);
        }
        IEnumerable<U> Map<U>(Func<T, U> func)
        {
            return this.Select(func);
        }
        IEnumerable<U> Map<U>(Func<T, int, U> func)
        {
            return this.Select(func);
        }

    }
}
