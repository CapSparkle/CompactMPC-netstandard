﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompactMPC
{
    public abstract class PackedArray<T> : IReadOnlyList<T>
    {
        private byte[] _buffer;
        private int _length;

        protected PackedArray(int numberOfBytes, int numberOfElements)
        {
            if (numberOfElements < 0)
                throw new ArgumentOutOfRangeException(nameof(numberOfElements));

            _buffer = new byte[numberOfBytes];
            _length = numberOfElements;
        }

        protected PackedArray(byte[] bytes, int numberOfBytes, int numberOfElements)
        {
            if (numberOfElements < 0)
                throw new ArgumentOutOfRangeException(nameof(numberOfElements));

            if (bytes.Length < numberOfBytes)
                throw new ArgumentException("Not enough data provided.", nameof(bytes));

            _buffer = new byte[numberOfBytes];
            _length = numberOfElements;

            Array.Copy(bytes, _buffer, numberOfBytes);
        }

        protected PackedArray(int numberOfBytes, T[] elements)
            : this(numberOfBytes, elements.Length)
        {
            for (int i = 0; i < _length; ++i)
                WriteElement(elements[i], i);
        }

        public byte[] ToBytes()
        {
            byte[] bytes = new byte[_buffer.Length];
            Array.Copy(_buffer, bytes, _buffer.Length);
            return bytes;
        }

        public T[] ToArray()
        {
            T[] result = new T[Length];
            for (int i = 0; i < result.Length; ++i)
                result[i] = this[i];

            return result;
        }

        public IEnumerator<T> GetEnumerator()
        {
            return Enumerable.Range(0, _length).Select(i => this[i]).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        protected static int RequiredBytes(int numberOfElements, int elementsPerByte)
        {
            if (numberOfElements > 0)
                return (numberOfElements - 1) / elementsPerByte + 1;

            return 0;
        }

        protected byte ReadBit(int index)
        {
            return ReadBits(index, 8, 0x1);
        }

        protected void WriteBit(byte bit, int index)
        {
            WriteBits(bit, index, 8, 0x1);
        }

        protected byte ReadBits(int index, int elementsPerByte, int bitMask)
        {
            int byteIndex = index / elementsPerByte;
            int bitsPerElement = 8 / elementsPerByte;
            int shift = bitsPerElement * (index % elementsPerByte);
            return (byte)((_buffer[byteIndex] >> shift) & bitMask);
        }

        protected void WriteBits(byte bits, int index, int elementsPerByte, int bitMask)
        {
            int byteIndex = index / elementsPerByte;
            int bitsPerElement = 8 / elementsPerByte;
            int shift = bitsPerElement * (index % elementsPerByte);
            _buffer[byteIndex] = (byte)((_buffer[byteIndex] & ~(bitMask << shift)) | ((bits & bitMask) << shift));
        }

        protected abstract T ReadElement(int index);
        protected abstract void WriteElement(T value, int index);

        public T this[int index]
        {
            get
            {
                if (index < 0 || index >= _length)
                    throw new ArgumentOutOfRangeException(nameof(index));

                return ReadElement(index);
            }
            set
            {
                if (index < 0 || index >= _length)
                    throw new ArgumentOutOfRangeException(nameof(index));

                WriteElement(value, index);
            }
        }

        int IReadOnlyCollection<T>.Count
        {
            get
            {
                return _length;
            }
        }

        public int Length
        {
            get
            {
                return _length;
            }
        }

        protected byte[] Buffer
        {
            get
            {
                return _buffer;
            }
        }
    }
}