using System;
using System.IO;

namespace CocoRCore.CSharp // was at.jku.ssw.Coco for .Net V2
{
    //-----------------------------------------------------------------------------
    //  Generator
    //-----------------------------------------------------------------------------
    public class Generator
    {
        private const int EOF = -1;

        private FileStream fram;
        private StreamWriter gen;
        private readonly Tab tab;
        private string frameFile;

        public Generator(Tab tab)
        {
            this.tab = tab;
        }

        public FileStream OpenFrame(String frame)
        {
            if (tab.frameDir != null) frameFile = Path.Combine(tab.frameDir, frame);
            if (frameFile == null || !File.Exists(frameFile)) frameFile = Path.Combine(tab.srcDir, frame);
            if (frameFile == null || !File.Exists(frameFile)) throw new FatalError("Cannot find : " + frame);

            try
            {
                fram = new FileStream(frameFile, FileMode.Open, FileAccess.Read, FileShare.Read);
            }
            catch (FileNotFoundException)
            {
                throw new FatalError("Cannot open frame file: " + frameFile);
            }
            return fram;
        }



        public StreamWriter OpenGen(string target)
        {
            string fn = Path.Combine(tab.outDir, target);
            try
            {
                if (File.Exists(fn)) File.Copy(fn, fn + ".old", true);
                gen = new StreamWriter(new FileStream(fn, FileMode.Create)); /* pdt */
            }
            catch (IOException)
            {
                throw new FatalError("Cannot generate file: " + fn);
            }
            return gen;
        }


        public void GenCopyright()
        {
            string copyFr = null;
            if (tab.frameDir != null) copyFr = Path.Combine(tab.frameDir, "Copyright.frame");
            if (copyFr == null || !File.Exists(copyFr)) copyFr = Path.Combine(tab.srcDir, "Copyright.frame");
            if (copyFr == null || !File.Exists(copyFr)) return;

            try
            {
                FileStream scannerFram = fram;
                fram = new FileStream(copyFr, FileMode.Open, FileAccess.Read, FileShare.Read);
                CopyFramePart(null);
                fram = scannerFram;
            }
            catch (FileNotFoundException)
            {
                throw new FatalError("Cannot open Copyright.frame");
            }
        }

        public void SkipFramePart(String stop)
        {
            CopyFramePart(stop, false);
        }


        public void CopyFramePart(String stop)
        {
            CopyFramePart(stop, true);
        }

        // if stop == null, copies until end of file
        public void CopyFramePart(string stop, bool generateOutput)
        {
            char startCh = (char)0;
            int endOfStopString = 0;

            if (stop != null)
            {
                startCh = stop[0];
                endOfStopString = stop.Length - 1;
            }

            int ch = framRead();
            while (ch != EOF)
            {
                if (stop != null && ch == startCh)
                {
                    int i = 0;
                    do
                    {
                        if (i == endOfStopString) return; // stop[0..i] found
                        ch = framRead(); i++;
                    } while (ch == stop[i]);
                    // stop[0..i-1] found; continue with last read character
                    if (generateOutput) gen.Write(stop.Substring(0, i));
                }
                else
                {
                    if (generateOutput) gen.Write((char)ch);
                    ch = framRead();
                }
            }

            if (stop != null) throw new FatalError("Incomplete or corrupt frame file: " + frameFile);
        }

        private int framRead()
        {
            try
            {
                return fram.ReadByte();
            }
            catch (Exception)
            {
                throw new FatalError("Error reading frame file: " + frameFile);
            }
        }
    }

} // end namespace
