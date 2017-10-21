using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sharp86
{
    class HistoryBuffer<T>
    {
        public HistoryBuffer(int length)
        {
            _buffer = new T[length];
        }

        T[] _buffer;
        long _pos;

        public int Count
        {
            get
            {
                if (_pos > _buffer.Length)
                    return _buffer.Length;
                else
                    return (int)_pos;
            }
        }

        public void Write(T val)
        {
            _buffer[_pos % _buffer.Length] = val;
            _pos++;
        }

        public T this[int index]
        {
            get
            {
                if (index >= Count)
                    throw new ArgumentException("Index out of range");
                if (_pos < _buffer.Length)
                    return _buffer[index];

                return _buffer[(_pos + index) % _buffer.Length];
            }
        }

        public int Capacity
        {
            get
            {
                return _buffer.Length;
            }
            set
            {
                var h = new HistoryBuffer<T>(value);
                for (int i = 0; i < Count; i++)
                {
                    h.Write(this[i]);
                }

                _pos = h._pos;
                _buffer = h._buffer;
            }
        }

        public void Clear()
        {
            _pos = 0;
        }
    }
}
