using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;


namespace KSharp {
    /// <summary>
    /// Create a range of integers. The <see cref="End"/> is NOT included in the range when calling <see cref="To"/> or <see cref="IntRange"/>.
    /// If <see cref="End"/> is intended to be included, use <see cref="Until"/>
    /// </summary>
    public class IntRange: IEnumerable<int> {
        public int Start { get; }
        public int End { get; }

        public IntRange(int start, int end) {
            Start = start;
            End = end;
        }

        public IntRange(int end) {
            Start = 0;
            End = end;
        }

        public IEnumerator<int> GetEnumerator() {
            for (int a = Start; a < End; ++a)
                yield return a;
        }

        /// <summary>
        /// <paramref name="end"/> is excluded
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        [InfixFunction]
        public static IntRange To(int start, int end) {
            return new IntRange(start, end);
        }

        /// <summary>
        /// <paramref name="until"/> is included
        /// </summary>
        /// <param name="start"></param>
        /// <param name="until"></param>
        /// <returns></returns>
        [InfixFunction]
        public static IntRange Until(int start, int until) {
            return new IntRange(start, until + 1);
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }

        public void ForEach(Action action) {
            foreach (var _ in this)
                action();
        }

        public void ForEachIndex(Action<int> action) {
            foreach (var a in this)
                action(a);
        }
        public bool Contains(int number)
        {
            return number >= Start && number < End;
        }

        public override string ToString()
        {
            return $"{Start}..{End - 1}";
        }
    }
}
