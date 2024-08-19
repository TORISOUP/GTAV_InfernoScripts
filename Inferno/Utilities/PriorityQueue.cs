// this code is borrowed from RxOfficial(rx.codeplex.com) and modified

using System;
using System.Collections.Generic;
using System.Threading;

namespace Inferno.Utilities
{
    internal class PriorityQueue<T> where T : IComparable<T>
    {
        private static long _count = long.MinValue;

        private IndexedItem[] _items;

        public PriorityQueue()
            : this(16)
        {
        }

        public PriorityQueue(int capacity)
        {
            _items = new IndexedItem[capacity];
            Count = 0;
        }

        public int Count { get; private set; }

        private bool IsHigherPriority(int left, int right)
        {
            return _items[left].CompareTo(_items[right]) < 0;
        }

        private void Percolate(int index)
        {
            if (index >= Count || index < 0)
                return;
            var parent = (index - 1) / 2;
            if (parent < 0 || parent == index)
                return;

            if (IsHigherPriority(index, parent))
            {
                var temp = _items[index];
                _items[index] = _items[parent];
                _items[parent] = temp;
                Percolate(parent);
            }
        }

        private void Heapify()
        {
            Heapify(0);
        }

        private void Heapify(int index)
        {
            if (index >= Count || index < 0)
                return;

            var left = 2 * index + 1;
            var right = 2 * index + 2;
            var first = index;

            if (left < Count && IsHigherPriority(left, first))
                first = left;
            if (right < Count && IsHigherPriority(right, first))
                first = right;
            if (first != index)
            {
                var temp = _items[index];
                _items[index] = _items[first];
                _items[first] = temp;
                Heapify(first);
            }
        }

        public T Peek()
        {
            if (Count == 0)
                throw new InvalidOperationException("HEAP is Empty");

            return _items[0].Value;
        }

        private void RemoveAt(int index)
        {
            _items[index] = _items[--Count];
            _items[Count] = default;
            Heapify();
            if (Count < _items.Length / 4)
            {
                var temp = _items;
                _items = new IndexedItem[_items.Length / 2];
                Array.Copy(temp, 0, _items, 0, Count);
            }
        }

        public T Dequeue()
        {
            var result = Peek();
            RemoveAt(0);
            return result;
        }

        public void Enqueue(T item)
        {
            if (Count >= _items.Length)
            {
                var temp = _items;
                _items = new IndexedItem[_items.Length * 2];
                Array.Copy(temp, _items, temp.Length);
            }

            var index = Count++;
            _items[index] = new IndexedItem { Value = item, Id = Interlocked.Increment(ref _count) };
            Percolate(index);
        }

        public bool Remove(T item)
        {
            for (var i = 0; i < Count; ++i)
                if (EqualityComparer<T>.Default.Equals(_items[i].Value, item))
                {
                    RemoveAt(i);
                    return true;
                }

            return false;
        }

        private struct IndexedItem : IComparable<IndexedItem>
        {
            public T Value;
            public long Id;

            public int CompareTo(IndexedItem other)
            {
                var c = Value.CompareTo(other.Value);
                if (c == 0)
                    c = Id.CompareTo(other.Id);
                return c;
            }
        }
    }
}