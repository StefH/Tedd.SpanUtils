﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Tedd.SpanUtils.SourceGenerator
{
    public static class CodeGenStreams
    {

        public static void Generate(string root)
        {
            var le = true;


            {
                var sbRO = new StringBuilder();
                var sbW = new StringBuilder();

                foreach (var ds in CodeGenBodies.DataStructures)
                {
                    GenReadBody(ds, sbRO, false,true);
                    GenReadBody(ds, sbW, false,false);
                    GenWriteBody(ds, sbW, false);
                }

                var strRW = Helper.CreateRefStruct("SpanStream", sbW.ToString(), "");
                var nsRW = Helper.CreateNamespace("Tedd", strRW, CodeGenBodies.usings);
                File.WriteAllText(Path.Combine(root, "SpanStream.generated.cs"), nsRW);
                var strRO = Helper.CreateRefStruct("ReadOnlySpanStream", sbRO.ToString(), "");
                var nsRO = Helper.CreateNamespace("Tedd", strRO, CodeGenBodies.usings);
                File.WriteAllText(Path.Combine(root, "ReadOnlySpanStream.generated.cs"), nsRO);
            }
            {
                var sbRO = new StringBuilder();
                var sbW = new StringBuilder();

                foreach (var ds in CodeGenBodies.DataStructures)
                {
                    GenReadBody(ds, sbRO, true,true);
                    GenReadBody(ds, sbW, true,false);
                    GenWriteBody(ds, sbW, true);
                }
                var strRW = Helper.CreateClass(false, "MemoryStreamer", sbW.ToString(), "");
                var nsRW = Helper.CreateNamespace("Tedd", strRW, CodeGenBodies.usings);
                File.WriteAllText(Path.Combine(root, "MemoryStreamer.generated.cs"), nsRW);
                var strRO = Helper.CreateClass(false, "ReadOnlyMemoryStreamer", sbRO.ToString(), "");
                var nsRO = Helper.CreateNamespace("Tedd", strRO, CodeGenBodies.usings);
                File.WriteAllText(Path.Combine(root, "ReadOnlyMemoryStreamer.generated.cs"), nsRO);
            }
        }

        private static string Sj(List<string> l) => String.Join(", ", l);

        private static void GenReadBody(MethodData ds, StringBuilder sb, bool isMemoryStreamer, bool isReadOnly)
        {
            if (ds.RW == MethodRW.WriteOnly)
                return;

            if (isReadOnly && ds.Name == "Span")
                return;

            var memory = isMemoryStreamer ? "Memory." : "";
            List<string> pDef = new();
            List<string> p = new();

            var checkWriteOk = "";

            // Special case for returning span, then we need to read Span only
            if (ds.Name == "Span")
            {
                if (!isMemoryStreamer)
                    memory = "";
                else
                {
                    memory = "Memory.";
                    checkWriteOk = @"
            if (!CanWrite)
                throw new ReadOnlyException(""Span is read-only, use ReadReadOnlySpan."");";
                }
            }


            if (!string.IsNullOrWhiteSpace(ds.ExtraReadParamsDef))
                pDef.Add(ds.ExtraReadParamsDef);
            if (!string.IsNullOrWhiteSpace(ds.ExtraReadParams))
                p.Add(ds.ExtraReadParams);
            var retType = ds.TypeString;

            if (isMemoryStreamer && ds.TypeString == typeof(byte).Name && ds.Name == typeof(byte).Name)
            {
                retType = "override int";
            }
            if (!ds.NoLengthParam)
            {
                var pC = new List<string>(p);
                pC.Add("out _");
                Helper.Method(sb, false, retType, $"Read{ds.Name}", Sj(pDef), $"Read{ds.Name}({Sj(pC)});", "");
                pDef.Add("out int length");
                p.Add("out length");
            }

            Helper.Method(sb, false, ds.TypeString, $"Read{ds.Name}", Sj(pDef), @$"{checkWriteOk}
            var ret = SpanUtils.Read{ds.Name}({memory}Span.Slice(_position), {Sj(p)});
            Position += {ds.Size};
            return ret;", "");

        }
        private static void GenWriteBody(MethodData ds, StringBuilder sb, bool isMemoryStreamer)
        {
            if (ds.IsAlias || ds.RW == MethodRW.ReadOnly)
                return;


            var name = $"Write{ds.WriteName}";
            var memory = isMemoryStreamer ? "Memory." : "";

            List<string> pDef = new();
            List<string> p = new();

            pDef.Add($"{ds.TypeString} value");
            p.Add("value");


       
            sb.Append($@"
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void {name}({Sj(pDef)}) => {name}({Sj(p)}, out _);
");
            pDef.Add("out int length");
            p.Add("out length");

            sb.Append($@"
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void {name}({Sj(pDef)}) {{
            SpanUtils.{name}({memory}Span.Slice(_position), {Sj(p)});
            Position += {ds.Size};
        }}");
        }
    }
}