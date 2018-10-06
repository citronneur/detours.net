using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DemoConsole
{
    public class Program
    {
        public static void Main(string[] args)
        {
            using (FileStream fs = File.Create("c:\\dev\\foo"))
            {
                Byte[] info = new UTF8Encoding(true).GetBytes("This is some text in the file.");
                // Add some information to the file.
                fs.Write(info, 0, info.Length);
            }
        }
    }
}
