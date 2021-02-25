using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;


namespace KSharp
{
    internal enum LockChainState
    {
        None,
        Failed,
        Acquiring,
        Hold,
        Disposed
    }

    public class LockChain<T> : IDisposable
        where T: class
    {
        private LockChainState state = LockChainState.None;
        private readonly T lockedObject;

        public LockChain(T obj)
        {
            lockedObject = obj ?? throw new ArgumentNullException(nameof(obj));
            bool taken = false;
            Monitor.TryEnter(obj, ref taken);
            if (taken) {
                state = LockChainState.Hold;
            }
            else {
                state = LockChainState.Failed;
                throw new Exception("Get lock failed.");
            }
        }
        
        
        public LockChain(T obj, int milliseconds)
        {
            lockedObject = obj ?? throw new ArgumentNullException(nameof(obj));
            bool taken = false;
            Monitor.TryEnter(obj, milliseconds, ref taken);
            if (taken) {
                state = LockChainState.Hold;
            }
            else {
                state = LockChainState.Failed;
                throw new Exception("Get lock failed.");
            }
        }

        public void Dispose()
        {
            if (state == LockChainState.Hold) {
                state = LockChainState.Disposed;
                Monitor.Exit(lockedObject);
            }
        }
    }

    
    public class LockChain : IDisposable
    {
        private LockChainState state = LockChainState.None;
        private readonly object[] objects;
        private readonly int holdNumber;

        public LockChain(params object[] objects)
        {
            this.objects = objects;
            foreach (var obj in objects) {
                if (obj is null)
                    throw new ArgumentNullException($"object null cannot be locked at index {holdNumber}");
                bool taken = false;
                Monitor.TryEnter(obj, ref taken);
                if (taken) {
                    state = LockChainState.Acquiring;
                    ++holdNumber;
                }
                else {
                    state = LockChainState.Failed;
                    throw new Exception($"Get lock on {obj} failed.");
                }
            }
            if (state != LockChainState.Failed)
                state = LockChainState.Hold;
        }

        public void Dispose()
        {
            if (state == LockChainState.Hold) {
                state = LockChainState.Disposed;
                for (int i = 0; i < holdNumber; ++i) {
                    Monitor.Exit(objects[i]);
                }
            }
        }
    }

    /// <summary>
    /// A matrix representation directed graph.
    /// Class <see cref="Graph"/> is immutable, which means every 'mutating' method returns a new graph.
    /// </summary>
    public class Graph
    {
        public int Dimension { get; }
        private readonly bool[,] matrix;
        public Graph(int dimension)
        {
            Dimension = dimension;
            matrix = new bool[dimension, dimension];
        }
        public Graph Copy()
        {
            Graph g = new Graph(Dimension);
            Array.Copy(matrix, g.matrix, matrix.Length);
            return g;
        }
        public void ForEach(Action<int, int, bool> func)
        {
            for (int i=0;i<Dimension;++i)
                for (int j = 0; j < Dimension; ++j)
                    func(i, j, matrix[i, j]);
        }

        public bool this[int row, int col] {
            get => matrix[row, col];
        }
        public bool Get(int row, int col)
        {
            if (row < 0 || row >= Dimension)
                throw new IndexOutOfRangeException($"row {row} out of range 0..{Dimension}");
            if (col < 0 || col >= Dimension)
                throw new IndexOutOfRangeException($"row {col} out of range 0..{Dimension}");
            return matrix[row, col];
        }
        public Graph Set(int row, int col, bool value)
        {
            if (row < 0 || row >= Dimension)
                throw new IndexOutOfRangeException($"row {row} out of range 0..{Dimension}");
            if (col < 0 || col >= Dimension)
                throw new IndexOutOfRangeException($"row {col} out of range 0..{Dimension}");
            var g = Copy();
            g.matrix[row, col] = value;
            return g;
        }
        public Graph AddEdge(IEnumerable<(int, int)> edges)
        {
            var g = Copy();
            foreach (var (a,b) in edges) {
                g.matrix[a, b] = true;
            }
            return g;
        }
        public Graph TransitiveClosure()
        {
            var g = Copy();
            var m = g.matrix;
            for (int k=0;k<Dimension;++k)
                for (int i=0;i<Dimension;++i)
                    for (int j = 0; j < Dimension; ++j) {
                        m[i, j] = m[i, j] || m[i, k] && m[k, j];
                    }
            return g;
        }
        public unsafe IEnumerable<System.Collections.Generic.List<int>> CalculateCycles()
        {
            int* colour = stackalloc int[Dimension];
            int* parent = stackalloc int[Dimension];
            int* mark = stackalloc int[Dimension];
            int cycleNumber = 0;

            void dfs(int u, int p, int* _colour, int* _parent, int* _mark)
            {
                if (_colour[u] == 2)
                    return;
                if (_colour[u] == 1) {
                    int cur = p;
                    _mark[cur] = ++cycleNumber;
                    while (cur != u) {
                        cur = _parent[cur];
                        _mark[cur] = cycleNumber;
                    }
                    return;
                }
                _parent[u] = p;
                _colour[u] = 1;
                for (int rowIndex = 0; rowIndex < Dimension; ++rowIndex) {
                    if (matrix[u, rowIndex]) {
                        if (rowIndex == _parent[u])
                            continue;
                        dfs(rowIndex, u, _colour, _parent, _mark);
                    }
                }
                _colour[u] = 2;
            }

            System.Collections.Generic.List<int>[] cycles = new System.Collections.Generic.List<int>[Dimension];
            void PartCycles(int* _mark)
            {
                for (int i = 0; i < Dimension; ++i)
                    cycles[i] = new List<int>();
                for (int i=0;i<Dimension;++i)
                    if (_mark[i] != 0)
                        cycles[_mark[i]].Add(i);
            }

            dfs(0, 0, colour, parent, mark);
            PartCycles(mark);
            return cycles.Where(l => l.Count != 0);
        }

        public IEnumerable<int> OutDegree(int vertex)
        {
            for (int i=0;i<Dimension;++i)
                if (matrix[vertex, i])
                    yield return i;
        }
        public IEnumerable<int> InDegree(int vertex)
        {
            for (int i=0;i<Dimension;++i)
                if (matrix[i, vertex])
                    yield return i;
        }
    }

    // public class Graph2
    // {
    //     public int Dimension { get; }
    //     private readonly List<int>[] edge;
    //     public Graph2(int dimension)
    //     {
    //         Dimension = dimension;
    //         edge = new List<int>[dimension];
    //         for (int i = 0; i < dimension; ++i)
    //             edge[i] = new();
    //     }
    // }

    public class LockChainManager
    {
        private readonly System.Collections.Generic.List<object> sortedObjects = new List<object>();
        private Graph partialSort = new Graph(0);
        private readonly Mutex partialSortLock = new Mutex();
        
        public LockChainManager() { }
        public void Add(params object[] objects)
        {
            if (objects.Length < 2)
                throw new ArgumentException("Provide at least two objects for partial sort.");
            if (objects.Distinct().Count() != objects.Length)
                throw new ArgumentException("Arguments must be distinct.");
            lock (sortedObjects) {
                int[] index = objects.Select(AddIfNotExists).ToArray();
                var indexGroup = new (int, int)[index.Length - 1];
                for (int i = 0; i < objects.Length - 1; ++i) {
                    indexGroup[i] = (index[i], index[i + 1]);
                }
                if (partialSortLock.WaitOne()) {
                    if (partialSort.Dimension != sortedObjects.Count)
                        partialSort = new Graph(sortedObjects.Count);
                    partialSort = partialSort.AddEdge(indexGroup);
                    partialSortLock.ReleaseMutex();
                }
            }
        }
        private int AddIfNotExists(object a)
        {
            int b;
            if ((b = sortedObjects.IndexOf(a)) != -1) {
                return b;
            }
            sortedObjects.Add(a);
            return sortedObjects.Count - 1;
        }
        
        public void Refresh()
        {
            Graph v;
            lock(sortedObjects)
                if (partialSortLock.WaitOne()) {
                    partialSort = partialSort.TransitiveClosure();
                    v = partialSort;
                    partialSortLock.ReleaseMutex();
                }
                else {
                    throw new Exception();
                }
            var cycles = v.CalculateCycles().ToArray();
            if (cycles.Length != 0)
                throw new Exception($"cycle detected in lock chain: {cycles}");
        }

        public LockChain Lock(params object[] o)
        {
            LinkList<object> h = new LinkList<object>();
            void AddIfNotContains(object c)
            {
                if (!h.Contains(c))
                    h.Add(c);
            }
            lock (sortedObjects) {
                foreach (var i in o) {
                    int index = sortedObjects.IndexOf(i);
                    if (index == -1) {
                        AddIfNotContains(i);
                    }
                    else {
                        var p = partialSort.InDegree(index).Select(t => sortedObjects[t]);
                        p.Append(i).ForEach(AddIfNotContains);
                    }
                }
            }
            return new LockChain(h.ToArray());
        }
    }
}
