using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;


namespace KSharp
{
    public interface IDataHolder<out T>
    {
        T Data { get; }
    }

    public class ListNode<T> : IDataHolder<T>
    {
        public T Data { get; }
        public ListNode<T> Next { get; internal set; }
        public ListNode<T> Prev { get; internal set; }
        
        public ListNode(T elem) => (Data, Next, Prev) = (elem, null!, null!);
    }

    public interface IImmutableCollection<out TElement>: IEnumerable<TElement>
    {
        int Count { get; }
        IImmutableCollection<TElement> Copy();
        
    }

    public interface ILinearImmutableCollection<out TElement> : IImmutableCollection<TElement>
    {
        TElement this[Index index] { get; }
        new ILinearImmutableCollection<TElement> Copy();
    }
    
    public class LinkList<T>: ILinearImmutableCollection<T>
    {
        private ListNode<T>? Root { get; }
        private ListNode<T>? End { get; }
        private bool IsOriginal {
            get => Count == 0 || Next(End!) == Root!;
        }
        private bool IsClockwise { get; }
        public int Count { get; }
        
        public T this[Index index] {
            get {
                if (index.IsFromEnd) {
                    if (index.Value == 0 || index.Value > Count)
                        throw new IndexOutOfRangeException($"index {index} out of range {^1..^(Count+1)}");
                    var node = IterateEnd()!;
                    for (int i = 1; i < index.Value; ++i)
                        node = Prev(node);
                    return node.Data;
                }
                else {
                    if (index.Value >= Count)
                        throw new IndexOutOfRangeException($"index {index} out of range {..Count}");
                    var node = IterateBegin()!;
                    for (int i = 0; i < index.Value; ++i)
                        node = Next(node);
                    return node.Data;
                }
            }
        }

        private LinkList(ListNode<T>? root, ListNode<T>? end, bool isClockwise, int count) => (Root, End, IsClockwise, Count) = (root, end, isClockwise, count);
        private ListNode<T> Next(ListNode<T> node)
        {
            return IsClockwise ? node.Next : node.Prev;
        }
        private void Next(ListNode<T> node, ListNode<T> value)
        {
            if (IsClockwise)
                node.Next = value;
            else
                node.Prev = value;
        }
        private ListNode<T> Prev(ListNode<T> node)
        {
            return IsClockwise ? node.Prev : node.Next;
        }
        private void Prev(ListNode<T> node, ListNode<T> value)
        {
            if (IsClockwise)
                node.Prev = value;
            else {
                node.Next = value;
            }
        }
        private ListNode<T>? IterateBegin()
        {
            return IsClockwise ? Root : End;
        }
        private ListNode<T>? IterateEnd()
        {
            return IsClockwise ? End : Root;
        }
        
        private ListNode<T> LinkAfter(ListNode<T>? original, ListNode<T> node)
        {
            if (original is null) {
                Next(node, node);
                Prev(node, node);
            }
            else {
                Prev(Next(original!), node);
                Next(node, Next(original));
                Next(original, node);
                Prev(node, original);
            }
            return node;
        }
        private ListNode<T> LinkBefore(ListNode<T>? original, ListNode<T> node)
        {
            if (original is null) {
                node.Next = node;
                node.Prev = node;
            }
            else {
                LinkAfter(node, original!);
            }
            return node;
        }

        public LinkList() : this(null, null, true, 0)
        {
            
        }

        public IEnumerator<T> GetEnumerator()
        {
            if (Root is null) yield break;
            var node = Prev(Root);
            for (int i = 0; i < Count; ++i) {
                node = Next(node);
                yield return node.Data;
            }
        }
        public IEnumerable<ListNode<T>> Traverse()
        {
            if (Root is null)
                yield break;
            var node = Prev(IterateBegin()!);
            for (int i = 0; i < Count; ++i) {
                node = Next(node);
                yield return node;
            }
        }
        
        public LinkList<T> Copy()
        {
            if (Root is null)
                return new LinkList<T>();
            var root = IterateBegin()!;
            var end = root;
            foreach (var g in Traverse().Skip(1)) {
                end = LinkAfter(end, g);
            }
            
            return new LinkList<T>(root, end, IsClockwise, Count);
        }

        IImmutableCollection<T> IImmutableCollection<T>.Copy()
        {
            return Copy();
        }
        ILinearImmutableCollection<T> ILinearImmutableCollection<T>.Copy() => Copy(); 

        public LinkList<T> Concatenate(LinkList<T> r)
        {
            if (Count == 0) {
                return r.Count == 0 ? new LinkList<T>() : r;
            }
            if (r.Count == 0)
                return this;
            if (IsClockwise == r.IsClockwise && IsOriginal && r.IsOriginal) {
                Next(IterateEnd()!, r.IterateBegin()!);
                Prev(IterateBegin()!, r.IterateEnd()!);
                Prev(r.IterateBegin()!, IterateEnd()!);
                Next(r.IterateEnd()!, IterateBegin()!);
                return new LinkList<T>(IterateBegin(), r.IterateEnd(), IsClockwise, Count + r.Count);
            }
            return Copy().Concatenate(r.Copy());
        }

        public LinkList<T> AppendAfter(T elem)
        {
            ListNode<T> node = new ListNode<T>(elem);
            LinkAfter(IterateEnd(), node);
            return new LinkList<T>(IterateBegin(), node, IsClockwise, Count + 1);
        }

        public LinkList<T> AppendBefore(T elem)
        {
            ListNode<T> node = new ListNode<T>(elem);
            LinkBefore(IterateBegin(), node);
            return new LinkList<T>(node, IterateEnd(), IsClockwise, Count + 1);
        }

        public LinkList<T> Add(T elem)
        {
            return AppendAfter(elem);
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
