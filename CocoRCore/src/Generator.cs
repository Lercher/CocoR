using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace CocoRCore.CSharp // was at.jku.ssw.Coco for .Net V2
{
    public enum GW
    {
        StartLine, Append, EndLine, Line, LineIndent1, Break
    }

    //-----------------------------------------------------------------------------
    //  Generator
    //-----------------------------------------------------------------------------
    public class Generator : IDisposable
    {
        private const int EOF = -1;

        private string frameFile;
        private StreamReader frameReader;
        private StreamWriter gen;
        private readonly Tab Tab;
        public int Indentation = 0;

        public Generator(Tab tab) => Tab = tab;

        public void Dispose()
        {
            frameReader?.Dispose();
            gen?.Dispose();
        }


        public FileInfo OpenFrame(string frame)
        {
            if (Tab.frameDir != null)
                frameFile = Path.Combine(Tab.frameDir, frame);
            if (frameFile == null || !File.Exists(frameFile))
                frameFile = Path.Combine(Tab.srcDir, frame);
            if (frameFile == null || !File.Exists(frameFile))
            {
                var ass = this.GetType().GetTypeInfo().Assembly;
                var n = ass.GetManifestResourceNames().FirstOrDefault(r => r.EndsWith(frame, StringComparison.OrdinalIgnoreCase));
                if (n == null) 
                    throw new FatalError($"Can't find frame file {frame}");
                frameReader = new StreamReader(ass.GetManifestResourceStream(n));
                return new FileInfo(frame);
            }


            try
            {
                frameReader = File.OpenText(frameFile);
                return new FileInfo(frameFile);
            }
            catch (IOException ex)
            {
                throw new FatalError($"Can't open frame file {frameFile}: {ex.Message}", ex);
            }
        }

        public void Write(GW mode,  string fmt, params object[] args)
        {
            if (mode == GW.Break)
            {
                gen.WriteLine();
                gen.WriteLine();
                return;
            }
            if (mode == GW.StartLine || mode == GW.Line || mode == GW.LineIndent1)
                WriteStart();
            if (mode == GW.LineIndent1)
                Indent1();
            
            if (args.Length == 0)
                gen.Write(fmt);
            else
                gen.Write(fmt, args); // note to vb implementors: use WriteLine(string format, params object[] arg) instead of WriteLine(string format, object arg0) here

            if (mode == GW.EndLine || mode == GW.Line || mode == GW.LineIndent1)
                gen.WriteLine();
        }

        private void WriteStart()
        {
            for (var i = 0; i < Indentation; i++)
                Indent1();
        }

        private void Indent1() => gen.Write("    ");

        public int PushIndentation(int n)
        {
            var i = Indentation;
            Indentation = n;
            return i;
        }

        public FileInfo OpenGen(string target)
        {
            var fn = Path.Combine(Tab.outDir, target);
            try
            {
                if (Tab.createOld && File.Exists(fn))
                    File.Copy(fn, $"{fn}.old", true);
                gen = new StreamWriter(new FileStream(fn, FileMode.Create)); /* pdt */
                return new FileInfo(fn);
            }
            catch (IOException ex)
            {
                throw new FatalError($"Can't generate file {fn}: {ex.Message}", ex);
            }
        }


        public void SkipFramePart(string stop) => CopyFramePart(stop, generateOutput: false);
        public void CopyFramePart(string stop) => CopyFramePart(stop, generateOutput: true);

        // if stop == null, copies until end of file
        public void CopyFramePart(string stop, bool generateOutput)
        {
            try
            {
                for (;;)
                {
                    var line = frameReader.ReadLine();
                    if (line == null)
                    {
                        if (stop == null) return;
                        throw new FatalError($"Incomplete or corrupt frame file {frameFile}, expected {stop}");
                    }
                    if (line.Trim() == stop)
                        return;
                    if (generateOutput)
                        gen.WriteLine(line);
                }
            }
            catch (Exception ex)
            {
                throw new FatalError($"Error reading frame file {frameFile}: {ex.Message}");
            }
        }
    }

} // end namespace
