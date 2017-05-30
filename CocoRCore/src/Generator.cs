using System;
using System.IO;

namespace CocoRCore.CSharp // was at.jku.ssw.Coco for .Net V2
{
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

        public Generator(Tab tab) => Tab = tab;

        public void Dispose()
        {
            frameReader?.Dispose();
            gen?.Dispose();
        }

        public void OpenFrame(string frame)
        {
            if (Tab.frameDir != null)
                frameFile = Path.Combine(Tab.frameDir, frame);
            if (frameFile == null || !File.Exists(frameFile))
                frameFile = Path.Combine(Tab.srcDir, frame);
            if (frameFile == null || !File.Exists(frameFile))
                frameFile = frame;
            if (frameFile == null || !File.Exists(frameFile))
                throw new FatalError($"Can't find frame file {frame}");

            try
            {
                frameReader = File.OpenText(frameFile);
            }
            catch (IOException ex)
            {
                throw new FatalError($"Can't open frame file {frameFile}: {ex.Message}", ex);
            }
        }



        public TextWriter OpenGen(string target)
        {
            var fn = Path.Combine(Tab.outDir, target);
            try
            {
                if (Tab.createOld && File.Exists(fn))
                    File.Copy(fn, $"{fn}.old", true);
                gen = new StreamWriter(new FileStream(fn, FileMode.Create)); /* pdt */
            }
            catch (IOException ex)
            {
                throw new FatalError($"Can't generate file {fn}: {ex.Message}", ex);
            }
            return gen;
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
                        throw new FatalError($"Incomplete or corrupt frame file {frameFile}");
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
