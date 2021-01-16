﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tedd.SpanUtils.SourceGenerator
{
    public static partial class CodeGenBodies
    {

        public static string ReadByte(bool le) => @"
            var ret = span[0];
            [LEN]
            [MOVE]
            return ret;";
        public static string ReadSByte(bool le) => @"
            var ret = (SByte)span[0];
            [LEN]
            [MOVE]
            return ret;";

        public static string ReadUInt16(bool le) => le switch
        {
            true => @"
           var ret = (UInt16)(
                  ((UInt16)span[0] << (8 * 1))
                | ((UInt16)span[1])
                );
            [LEN]
            [MOVE]
            return ret;",
            false => @"
           var ret = (UInt16)(
                  ((UInt16)span[0])
                | ((UInt16)span[1] << (8 * 1))
                );
            [LEN]
            [MOVE]
            return ret;"
        };

        public static string ReadUInt24(bool le) => le switch
        {
            true => @"
            var ret = (UInt24)(Int32)(
                ((UInt32)span[2])
                | ((UInt32)span[1] << (8 * 1))
                | ((UInt32)span[0] << (8 * 2))
                );
            [LEN]
            [MOVE]
            return ret;",
            false => @"
            var ret = (UInt24)(Int32)(
                  ((UInt32)span[2] << (8 * 2))
                | ((UInt32)span[1] << (8 * 1))
                | ((UInt32)span[0])
                );
            [LEN]
            [MOVE]
            return ret;"
        };
        public static string ReadUInt32(bool le) => le switch
        {
            true => @"
            // return MemoryMarshal.Cast<byte, UInt32>(span)[0];
            var ret = (UInt32)(
                  ((UInt32)span[3])
                | ((UInt32)span[2] << (8 * 1))
                | ((UInt32)span[1] << (8 * 2))
                | ((UInt32)span[0] << (8 * 3))
                );
            [LEN]
            [MOVE]
            return ret;",
            false => @"
            // return MemoryMarshal.Cast<byte, UInt32>(span)[0];
            var ret = (UInt32)(
                  ((UInt32)span[3] << (8 * 3))
                | ((UInt32)span[2] << (8 * 2))
                | ((UInt32)span[1] << (8 * 1))
                | ((UInt32)span[0])
                );
            [LEN]
            [MOVE]
            return ret;"
        };
        public static string ReadUInt64(bool le) => le switch
        {
            true => @"
            //return MemoryMarshal.Cast<byte, UInt64>(span)[0];
            // 16% more speed if we read in reverse order due to removal of redundant compiler checks.
            // https://github.com/tedd/Tedd.SpanUtils/issues/3
            var ret = (UInt64)(
                  ((UInt64)span[7])
                | ((UInt64)span[6] << (8 * 1))
                | ((UInt64)span[5] << (8 * 2))
                | ((UInt64)span[4] << (8 * 3))
                | ((UInt64)span[3] << (8 * 4))
                | ((UInt64)span[2] << (8 * 5))
                | ((UInt64)span[1] << (8 * 6))
                | ((UInt64)span[0] << (8 * 7))
                );
            [LEN]
            [MOVE]
            return ret;",
            false => @"
            //return MemoryMarshal.Cast<byte, UInt64>(span)[0];
            // 16% more speed if we read in reverse order due to removal of redundant compiler checks.
            // https://github.com/tedd/Tedd.SpanUtils/issues/3
            var ret = (UInt64)(
                  ((UInt64)span[7] << (8 * 7))
                | ((UInt64)span[6] << (8 * 6))
                | ((UInt64)span[5] << (8 * 5))
                | ((UInt64)span[4] << (8 * 4))
                | ((UInt64)span[3] << (8 * 3))
                | ((UInt64)span[2] << (8 * 2))
                | ((UInt64)span[1] << (8 * 1))
                | ((UInt64)span[0])
                );
            [LEN]
            [MOVE]
            return ret;"
        };

        public static string ReadBoolean(bool le) => @"
            var ret = span[0] != 0;
            [LEN]
            [MOVE]
            return ret;";
        public static string ReadChar(bool le) => @"
            Span<char> a = stackalloc char[1];
            var ab = MemoryMarshal.Cast<char, byte>(a);
            span.Slice(0, sizeof(char)).CopyTo(ab);
            [LEN]
            [MOVE]
            return a[0];"; // TODO: Endianness

        public static string ReadHalf(bool le) => @"
            Span<half> a = stackalloc half[1];
            var ab = MemoryMarshal.Cast<half, byte>(a);
            span.Slice(0, sizeof(half)).CopyTo(ab);
            [LEN]
            [MOVE]
            return a[0];";

        public static string ReadSingle(bool le) => @"
            Span<float> a = stackalloc float[1];
            var ab = MemoryMarshal.Cast<float, byte>(a);
            span.Slice(0, sizeof(float)).CopyTo(ab);
            [LEN]
            [MOVE]
            return a[0];";

        public static string ReadDouble(bool le) => @"
            Span<double> a = stackalloc double[1];
            var ab = MemoryMarshal.Cast<double, byte>(a);
            span.Slice(0, sizeof(double)).CopyTo(ab);
            [LEN]
            [MOVE]
            return a[0];";

        public static string ReadDecimal(bool le) => @"
            Span<decimal> a = stackalloc decimal[1];
            var ab = MemoryMarshal.Cast<decimal, byte>(a);
            span.Slice(0, sizeof(decimal)).CopyTo(ab);
            [LEN]
            [MOVE]
            return a[0];";

        public static string ReadGuid(bool le) => @"
            var ret = new Guid(span.Slice(0, 16).ToArray());
            [LEN]
            [MOVE]
            return ret;"; // TODO: ToArray creates object

        public static string ReadBytes(bool le) => @"
            var ret = span.Slice(0, length).ToArray();
            [LEN]
            [MOVE]
            return ret;";

        public static string ReadSpan(bool le) => @"
            var ret = span.Slice(0, length);
            [LEN]
            [MOVE]
            return ret;";

        public static string ReadVLQInt16(bool le) => @"
            // Lower bound special case
            if (span[0] == 0b0100_0000)
            {
                length = 1;
                [MOVE]
                return Int16.MinValue;
            }

            var i = 0;
            var shift = 0;
            Int32 ret = 0;

            ret |= (Int32)(span[0] & 0b0011_1111) << shift;
            shift += 6;

            while ((span[i++] & 0b1000_0000) != 0)
            {
                if (i == sizeof(Int16) + 2)
                    throw new OverflowException($""VLQ exceeded {sizeof(Int16) + 1} bytes"");

                ret |= (Int32)(span[i] & 0b0111_1111) << shift;
                shift += 7;
            }

            length = i;

            if ((span[0] & 0b0100_0000) != 0)
                ret = (Int16)(ret * -1);

            [MOVE]
            return (Int16)ret;";
        public static string ReadVLQUInt16(bool le) => @"
            var i = 0;
            var shift = 0;
            UInt32 ret = 0;
            do
            {
                if (i == sizeof(UInt16) + 2)
                    throw new OverflowException($""VLQ exceeded {sizeof(UInt16) + 1} bytes"");

                ret |= (UInt32)(span[i] & 0b0111_1111) << shift;
                shift += 7;
            } while ((span[i++] & 0b1000_0000) != 0);

            length = i;
            [MOVE]
            return (UInt16)ret;";
        public static string ReadVLQUInt24(bool le) => @"
            var i = 0;
            var shift = 0;
            UInt32 ret = 0;
            do
            {
                if (i == 3 + 2)
                    throw new OverflowException($""VLQ exceeded {3 + 1} bytes"");

                ret |= (UInt32)(span[i] & 0b0111_1111) << shift;
                shift += 7;
            } while ((span[i++] & 0b1000_0000) != 0);

            length = i;
            [MOVE]
            return ret.ToUInt24();";
        public static string ReadVLQInt32(bool le) => @"
            // Lower bound special case
            if (span[0] == 0b0100_0000)
            {
                length = 1;
                [MOVE]
                return Int32.MinValue;
            }


            var i = 0;
            var shift = 0;
            Int32 ret = 0;

            ret |= (Int32)(span[0] & 0b0011_1111) << shift;
            shift += 6;

            while ((span[i++] & 0b1000_0000) != 0)
            {
                if (i == sizeof(Int32) + 2)
                    throw new OverflowException($""VLQ exceeded {sizeof(Int32) + 1} bytes"");

                ret |= (Int32)(span[i] & 0b0111_1111) << shift;
                shift += 7;
            }

            length = i;

            if ((span[0] & 0b0100_0000) != 0)
                ret = (Int32)(ret * -1);

            [MOVE]
            return (Int32)ret;";
        public static string ReadVLQUInt32(bool le) => @"
            var i = 0;
            var shift = 0;
            UInt32 ret = 0;
            do
            {
                if (i == sizeof(UInt32) + 2)
                    throw new OverflowException($""VLQ exceeded {sizeof(UInt32) + 1} bytes"");

                ret |= (UInt32)(span[i] & 0b0111_1111) << shift;
                shift += 7;
            } while ((span[i++] & 0b1000_0000) != 0);

            length = i;
            [MOVE]
            return ret;";
        public static string ReadVLQInt64(bool le) => @"
            // Lower bound special case
            if (span[0] == 0b0100_0000)
            {
                length = 1;
                [MOVE]
                return Int64.MinValue;
            }

            var i = 0;
            var shift = 0;
            Int64 ret = 0;

            ret |= (Int64)(span[0] & 0b0011_1111) << shift;
            shift += 6;

            while ((span[i++] & 0b1000_0000) != 0)
            {
                if (i == sizeof(Int64) + 3)
                    throw new OverflowException($""VLQ exceeded {sizeof(Int64) + 2} bytes"");

                ret |= (Int64)(span[i] & 0b0111_1111) << shift;
                shift += 7;
            }

            length = i;

            if ((span[0] & 0b0100_0000) != 0)
                ret = (Int64)(ret * -1);

            [MOVE]
            return (Int64)ret;";
        public static string ReadVLQUInt64(bool le) => @"
            var i = 0;
            var shift = 0;
            UInt64 ret = 0;
            do
            {
                if (i == sizeof(UInt64) + 3)
                    throw new OverflowException($""VLQ exceeded {sizeof(UInt64) + 2} bytes"");

                ret |= ((UInt64)(span[i] & 0b0111_1111) << shift);
                shift += 7;
            } while ((span[i++] & 0b1000_0000) != 0);

            length = i;
            [MOVE]
            return ret;";


        public static string ReadSize(bool le) => @"
            var b1 = span[0];
            var s = b1 >> 6;

            length = s + 1;
            var ret = s switch
            {
                0b00 => (UInt32)b1 & 0b00111111,
                0b01 => (UInt32)SpanUtils.ReadUInt16(span) & 0b00111111_11111111,
                0b10 => (UInt32)SpanUtils.ReadUInt24(span) & 0b00111111_11111111_11111111,
                //case 0b11:
                _ => (UInt32)SpanUtils.ReadUInt32(span) & 0b00111111_11111111_11111111_11111111
            };
            [MOVE]
            return ret;";

        public static string ReadSizedBytes(bool le) => @"
            var size = SpanUtils.ReadSize(span, out var len);
            length = len + (int)size;
            var ret = span.Slice(len, (int)size).ToArray();
            [MOVE]
            return ret;";


        public static string ReadSizedString(bool le) => @"
            var size = SpanUtils.ReadSize(span, out var len);
            length = len + (int)size;
#if NETCOREAPP || NETSTANDARD21
            var ros = (ReadOnlySpan<byte>)span.Slice(len, (int)size);
            var ret = Encoding.UTF8.GetString(ros);
#else
            var bytes = span.Slice(len, (int)size).ToArray();
            var ret = Encoding.UTF8.GetString(bytes);
#endif
            [MOVE]
            return ret;";




        public static string ReadInt16(bool le) => ReadUInt16(le).Replace("return ", "return (Int16)");
        public static string ReadInt32(bool le) => ReadUInt32(le).Replace("return ", "return (Int32)");
        public static string ReadInt64(bool le) => ReadUInt64(le).Replace("return ", "return (Int64)");

    }
}
