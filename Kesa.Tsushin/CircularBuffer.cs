using System;
using System.CodeDom;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kesa.Tsushin
{
    public class CircularByteBufferStream : Stream
    {
        private CircularBuffer<byte> _buffer;

        public override bool CanRead => true;
        public override bool CanSeek => false;
        public override bool CanWrite => true;
        public override long Length { get; }
        public override long Position { get; set; }

        public CircularByteBufferStream()
        {
            _buffer = new CircularBuffer<byte>(1024);
        }

        public override void Flush()
        {
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException();
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            int read = 0;

            while (count > 0 && _buffer.TryPop(out var item))
            {
                count--;

                buffer[read + offset] = item;

                read++;
            }

            return read;
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            _buffer.Push(new ArraySegment<byte>(buffer, offset, count));
        }
    }

    public class CircularBuffer<T>
    {
        private int _capacity;
        private int _size;
        private int _head;
        private int _tail;
        private T[] _array;
        private bool _canGrow;

        private bool IsFull => _size == _capacity;


        public CircularBuffer(int capacity)
            : this(16, true)
        {
        }

        public CircularBuffer(int capacity, bool canGrow)
        {
            _capacity = capacity;
            _array = new T[_capacity];
            _canGrow = canGrow;
        }

        public void Clear()
        {
            _size = 0;
            _head = 0;
            _tail = 0;
        }

        public bool Push(T item)
        {
            if (IsFull)
            {
                if (_canGrow)
                {
                    Grow();
                    return Push(item);
                }

                return false;
            }

            var index = _tail;
            Increment(ref _tail);
            _size++;

            _array[index] = item;
            return true;
        }

        public bool Push(IEnumerable<T> items)
        {
            var arr = items is T[] castedArray 
                ? castedArray 
                : items is List<T> castedList 
                    ? castedList.ToArray() 
                    : items.ToArray();

            if (_size + arr.Length > _capacity && _canGrow)
            {
                Grow();
            }
            else
            {
                return false;
            }

            var index = _head;

            for (int i = 0; i < arr.Length; i++)
            {
                _array[index] = arr[i];
                Increment(ref index);
            }

            return true;
        }

        private void Grow()
        {
            var existing = ToArray();
            _array = new T[_capacity*=2];
            existing.CopyTo(_array, 0);
            _head = 0;
            _tail = _size;
        }

        public bool TryPop(out T item)
        {
            item = default;

            if (_size == 0)
            {
                return false;
            }

            item = _array[_head];
            Increment(ref _head);
            _size--;
            return true;
        }

        private void Increment(ref int value)
        {
            if (value + 1 == _capacity)
            {
                value = 0;
            }
            else
            {
                value++;
            }
        }

        private IEnumerable<T> GetItems()
        {
            if (_size == 0)
            {
                yield break;
            }

            var needsFlag = IsFull;
            var flag = true;

            for (int i = _head; ; Increment(ref i))
            {
                if (i == _tail)
                {
                    if (needsFlag && flag)
                    {
                        flag = false;
                    }
                    else
                    {
                        yield break;
                    }
                }

                yield return _array[i];
            }
        }

        public void CopyTo(T[] array, int index, int count)
        {
            using (var enumerator = GetItems().GetEnumerator())
            {
                while (count > 0 && enumerator.MoveNext())
                {
                    count--;
                    array[index] = enumerator.Current;
                    index++;
                }
            }
        }

        public T[] ToArray()
        {
            return GetItems().ToArray();
        }
    }
}
