using System;
using System.IO;

public class Hexdump {

	public static int Main (string[] arg) {
        if (arg.Length == 1)
        {
            using (Stream s = new FileStream(arg[0], FileMode.Open, FileAccess.Read, FileShare.Read)) {
                byte[] buf = new byte[16];
                int pos = 0;         
                while(true) {
                    int len = s.Read(buf, 0, buf.Length);
                    if (len == 0) return 0;
                    print(buf, len, pos);
                    pos += len;
                }
            }
        } else return 2;        
    }

    static void print(byte[] buf, int len, int pos) {
        System.Console.Write("{0:X4}", pos);
        System.Console.Write("  ");
        for(int i = 0; i < buf.Length; i++)
            if (i < len)
                System.Console.Write("{0:X2} ", buf[i]);
            else
                System.Console.Write(".. ");
        System.Console.Write("  ");
        for(int i = 0; i < buf.Length && i < len; i++)
            if (32 <= buf[i] && buf[i] <= 127)
                System.Console.Write((char)buf[i]);
            else
                System.Console.Write('.');
        System.Console.WriteLine();
    }
}
