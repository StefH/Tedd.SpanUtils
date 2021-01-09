﻿using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace Tedd
{
    public static class SpanWrite
    {
        #region Primitives
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte Write(this Span<byte> span, byte value)
        {
            span[0] = value;
            return 1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte Write(this Span<byte> span, sbyte value)
        {
            span[0] = (byte)value;
            return 1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte Write(this Span<byte> span, Int16 value) => span.Write((UInt16)value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte Write(this Span<byte> span, UInt16 value)
        {
            //MemoryMarshal.Cast<byte, UInt16>(span)[0] = value;
            span[1] = (byte)(value & 0xFF);
            span[0] = (byte)(value >> (8 * 1));

            return 2;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte Write(this Span<byte> span, UInt24 value)
        {
            //MemoryMarshal.Cast<byte, UInt32>(span)[0] = value;
            span[2] = (byte)((Int32)value & 0xFF);
            span[0] = (byte)(((Int32)value >> (8 * 2)) & 0xFF);
            span[1] = (byte)(((Int32)value >> (8 * 1)) & 0xFF);

            return 3;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte Write(this Span<byte> span, Int32 value) => span.Write((UInt32)value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte Write(this Span<byte> span, UInt32 value)
        {
            //MemoryMarshal.Cast<byte, UInt32>(span)[0] = value;
            span[3] = (byte)(value & 0xFF);
            span[0] = (byte)(value >> (8 * 3));
            span[1] = (byte)((value >> (8 * 2)) & 0xFF);
            span[2] = (byte)((value >> (8 * 1)) & 0xFF);

            return 4;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte Write(this Span<byte> span, Int64 value) => span.Write((UInt64)value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte Write(this Span<byte> span, UInt64 value)
        {
            // 13% more speed if we write last first, then rest in sequence.
            // https://github.com/tedd/Tedd.SpanUtils/issues/3
            span[7] = (byte)(value & 0xFF);
            span[0] = (byte)(value >> (8 * 7));
            span[1] = (byte)((value >> (8 * 6)) & 0xFF);
            span[2] = (byte)((value >> (8 * 5)) & 0xFF);
            span[3] = (byte)((value >> (8 * 4)) & 0xFF);
            span[4] = (byte)((value >> (8 * 3)) & 0xFF);
            span[5] = (byte)((value >> (8 * 2)) & 0xFF);
            span[6] = (byte)((value >> (8 * 1)) & 0xFF);

            return 8;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte Write(this Span<byte> span, bool value)
        {
            span[0] = (byte)(value ? 1 : 0);
            return 1;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe byte Write(this Span<byte> span, char value)
        {
            Span<char> a = stackalloc char[1] { value };
            var ab = MemoryMarshal.Cast<char, byte>(a);
            ab.CopyTo(span);
            return sizeof(char);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte Write(this Span<byte> span, float value)
        {
            Span<float> a = stackalloc float[1] { value };
            var ab = MemoryMarshal.Cast<float, byte>(a);
            ab.CopyTo(span);
            return sizeof(float);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte Write(this Span<byte> span, double value)
        {
            Span<double> a = stackalloc double[1] { value };
            var ab = MemoryMarshal.Cast<double, byte>(a);
            ab.CopyTo(span);
            return sizeof(double);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte Write(this Span<byte> span, decimal value)
        {
            Span<decimal> a = stackalloc decimal[1] { value };
            var ab = MemoryMarshal.Cast<decimal, byte>(a);
            ab.CopyTo(span);
            return sizeof(decimal);
        }

        #endregion

        #region Other
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte Write(this Span<byte> span, Guid value)
        {
            var array = new Span<byte>(value.ToByteArray());
            array.CopyTo(span);

            return 16;
        }
        #endregion

        #region Arrays
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Write(this Span<byte> span, byte[] value) => span.Write(new Span<byte>(value));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Write(this Span<byte> span, Span<byte> value)
        {
            value.CopyTo(span);
            return value.Length;
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Write(this Span<byte> span, ReadOnlySpan<byte> value)
        {
            value.CopyTo(span);
            return value.Length;
        }



        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int SizedWrite(this Span<byte> span, byte[] value) => span.SizedWrite(new Span<byte>(value));

        public static int SizedWrite(this Span<byte> span, string value)
        {
#if NETCOREAPP || NETSTANDARD21
            // We use GetByteCount followed by direct copy to avoid creating a byte array (avoid GC).
            // For larger strings this could cause 
            var strSize = (UInt32)Encoding.UTF8.GetByteCount(value);
            var mbs = span.MeasureWriteSize((UInt32)strSize);
            var len = (int)mbs + (int)strSize;
            if (len > span.Length)
                throw new ArgumentException("Data length too big for target span.", nameof(value));

            _ = span.WriteSize(strSize);

            Encoding.UTF8.GetBytes(value, span.Slice(mbs));
#else
            var bytes = Encoding.UTF8.GetBytes(value);
            if (bytes.Length > span.Length)
                throw new ArgumentException("String is too long.", nameof(value));

            var mbs = span.WriteSize((UInt32)bytes.Length);
            var len = (int)mbs + bytes.Length;
            var s = span.Slice(mbs, bytes.Length);
            s.Write(bytes);
#endif

            return len;
        }

        public static int SizedWrite(this Span<byte> span, Span<byte> value)
        {
            var mbs = span.MeasureWriteSize((UInt32)value.Length);
            var len = mbs + value.Length;
            if (len > span.Length)
                throw new ArgumentException("Data length too big for target span.", nameof(value));

            _ = span.WriteSize((UInt32)value.Length);
            value.CopyTo(span.Slice(mbs));

            return len;
        }


        public static int SizedWrite(this Span<byte> span, ReadOnlySpan<byte> value)
        {
            var mbs = span.MeasureWriteSize((UInt32)value.Length);
            var len = mbs + value.Length;
            if (len > span.Length)
                throw new ArgumentException("Data length too big for target span.", nameof(value));
            _ = span.WriteSize((UInt32)value.Length);
            value.CopyTo(span.Slice(mbs));

            return len;
        }
        #endregion

        #region WriteSize
        /// <summary>
        /// Writes a UInt32 up to 29-bit using 1 to 4 bytes.
        /// </summary>
        /// <param name="span"></param>
        /// <param name="value"></param>
        public static byte WriteSize(this Span<byte> span, UInt32 value)
        {
            var bs = span.MeasureWriteSize(value);
            if (bs == 1)
                span.Write((Byte)value);
            else if (bs == 2)
                span.Write((UInt16)((UInt16)value | (0b01 << 14)));
            // Even larger (up to 4,2M) we store length as 3 bytes
            else if (bs == 3)
                span.Write((UInt24)((UInt32)value | (0b10 << 22)));
            else if (bs == 4)
                span.Write((UInt32)((UInt32)value | (0b11 << 30)));

            return bs;
        }

        /// <summary>
        /// Counts how many bytes WriteSize will use for a given value.
        /// </summary>
        /// <param name="span"></param>
        /// <param name="value"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte MeasureWriteSize(this Span<byte> span, UInt32 value) => value.MeasureWriteSize();
        #endregion

        #region VLQ
        public static byte WriteVLQ(this Span<byte> span, Int16 value)
        {
            byte i = 0;
            if (value < 0)
            {
                span[0] = 0b0100_0000;
                // Special case for lower bound, no more processing required
                if (value == Int16.MinValue)
                    return 1;
                value *= -1;

            }
            else span[0] = 0b0000_0000;

            // Write first byte, special case as we only have room for 6 bits in this one
            span[0] |= (byte)(value & 0b0011_1111);
            value >>= 6;

            // Still got some left? Go at it with 7 bit increments
            while (value > 0)
            {
                // We need one more byte, set current bit flag for that
                span[i++] |= 0b1000_0000;
                // Then the next 7 bits. Note that we don't need to remove 8th bit as overflow always would set that anyway.
                span[i] = (byte)value;
                value >>= 7;
            }

            return ++i;
        }

        public static byte WriteVLQ(this Span<byte> span, UInt16 value)
        {
            byte i = 0;
            while (value >= 0x80)
            {
                span[i++] = ((byte)(value | 0x80));
                value >>= 7;
            }
            span[i++] = (byte)value;
            return i;
        }

        public static byte WriteVLQ(this Span<byte> span, UInt24 value)
        {
            value = (UInt24)((UInt32)value & 0b11111111_11111111_11111111);
            byte i = 0;
            while ((UInt32)value >= 0x80)
            {
                span[i++] = ((byte)((UInt32)value | 0x80));
                value = (UInt24)((UInt32)value >> 7);
            }
            span[i++] = (byte)value;
            return i;
        }

        public static byte WriteVLQ(this Span<byte> span, Int32 value)
        {
            byte i = 0;
            if (value < 0)
            {
                span[0] = 0b0100_0000;
                // Special case for lower bound, no more processing required
                if (value == Int32.MinValue)
                    return 1;
                value *= -1;

            }
            else span[0] = 0b0000_0000;

            // Write first byte, special case as we only have room for 6 bits in this one
            span[0] |= (byte)(value & 0b0011_1111);
            value >>= 6;

            // Still got some left? Go at it with 7 bit increments
            while (value > 0)
            {
                // We need one more byte, set current bit flag for that
                span[i++] |= 0b1000_0000;
                // Then the next 7 bits. Note that we don't need to remove 8th bit as overflow always would set that anyway.
                span[i] = (byte)value;
                value >>= 7;
            }

            return ++i;
        }

        public static byte WriteVLQ(this Span<byte> span, UInt32 value)
        {
            byte i = 0;
            while (value >= 0x80)
            {
                span[i++] = ((byte)(value | 0x80));
                value >>= 7;
            }
            span[i++] = (byte)value;
            return i;
        }

        public static byte WriteVLQ(this Span<byte> span, Int64 value)
        {
            byte i = 0;
            if (value < 0)
            {
                span[0] = 0b0100_0000;
                // Special case for lower bound, no more processing required
                if (value == Int64.MinValue)
                    return 1;
                value *= -1;

            }
            else span[0] = 0b0000_0000;

            // Write first byte, special case as we only have room for 6 bits in this one
            span[0] |= (byte)(value & 0b0011_1111);
            value >>= 6;

            // Still got some left? Go at it with 7 bit increments
            while (value > 0)
            {
                // We need one more byte, set current bit flag for that
                span[i++] |= 0b1000_0000;
                // Then the next 7 bits. Note that we don't need to remove 8th bit as overflow always would set that anyway.
                span[i] = (byte)value;
                value >>= 7;
            }

            return ++i;
        }

        public static byte WriteVLQ(this Span<byte> span, UInt64 value)
        {
            byte i = 0;
            while (value >= 0x80)
            {
                span[i++] = ((byte)(value | 0x80));
                value >>= 7;
            }
            span[i++] = (byte)value;
            return i;
        }
        #endregion VLQ

        #region VInt
        /// <summary>
		/// Writes a VInt (EBML Variable Length Integer) to the specified span.
		/// </summary>
		/// <returns>The number of bytes written.</returns>
		public static int WriteVInt(this Span<byte> span, ulong value)
        {
            int position = 0;
            int size = VInt.GetSize(value);

            value |= 1UL << (7 * size);
            for (int i = size - 1; i >= 0; --i)
            {
                span[position++] = (byte)(value >> (8 * i));
            }

            return position;
        }
        #endregion
    }
}
