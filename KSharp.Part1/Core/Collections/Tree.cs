// using System;
// using System.Collections.Generic;
// using System.Linq;
// using System.Threading;
// using System.Threading.Tasks;
//
//
// namespace KSharp.Collections
// {
//     public class Tree<T>
//     {
//         public class Node
//         {
//             private static bool? isValueType;
//             internal Node? Child { get; set; }
//             internal Node? Parent { get; set; }
//             internal Node Left { get; set; }
//             internal Node Right { get; set; }
//             public T Value { get; }
//             internal Node(T value)
//             {
//                 Value = value;
//                 if (isValueType is null) {
//                     T c = value;
//                     isValueType = ReferenceEquals(c, value);
//                 }
//                 Left = null!;
//                 Right = null!;
//             }
//
//             public IEnumerable<Node> GetChildren()
//             {
//                 var first = Child;
//                 if (first != null)
//                     yield return first;
//                 else {
//                     yield break;
//                 }
//                 while (first.Right != Child) {
//                     first = first.Right!;
//                     yield return first;
//                 }
//             }
//
//             public Node? FindChildNode(Node b)
//             {
//                 if (b.Parent != this) {
//                     throw new ArgumentException(nameof(b));
//                 }
//                 return GetChildren().FirstOrDefault(k => k == b);
//             }
//
//             public T? FindChild(Predicate<T> func)
//             {
//                 var b = GetChildren().FirstOrDefault(t => func(t.Value));
//                 return (T?) (b is null ? default : b!.Value);
//             }
//             
//             public void AddChild(Node child)
//             {
//                 if (Child is null) {
//                     Child = child;
//                     child.Right = child;
//                     child.Left = child;
//                 }
//                 else {
//                     Child.Left.Right = child;
//                     child.Left = Child.Left;
//                     Child.Left = child;
//                     child.Right = Child;
//                 }
//                 child.Parent = this;
//             }
//
//             public void RemoveChild(Node child)
//             {
//                 if (child.Parent != this) {
//                     throw new ArgumentException(nameof(child));
//                 }
//                 if (child == Child) {
//                     if (child.Right == child) {
//                         Child = null;
//                     }
//                     else {
//                         Child = Child.Right;
//                     }
//                 }
//                 child.Left.Right = child.Right;
//                 child.Right.Left = child.Left;
//                 child.Parent = null;
//             }
//         }
//
//         public Node? Root { get; set; }
//         public Tree() { }
//         public Tree(T value)
//         {
//             Root = new(value);
//         }
//     }
// }
