#region

using System;
using System.IO;
using System.Linq;
using System.Net;
using dnlib.DotNet.MD;
using dnlib.PE;

#endregion

namespace KoiVM.WatermarkReader
{
    internal class Program
    {
        private static unsafe void Main(string[] args)
        {
            string file = null;
            if(args.Length == 0)
            {
                Console.Write("File: ");
                file = Console.ReadLine();
            }
            else
            {
                file = args[0];
            }


            var section = ExtractSection(File.ReadAllBytes(file));
            fixed(byte* ptr = section)
            {
                for(var i = 0; i < section.Length; i++)
                {
                    var ptr2 = (uint*) &ptr[i];

                    var a = (uint) IPAddress.NetworkToHostOrder((int) ptr2[0]);
                    var b = (uint) IPAddress.NetworkToHostOrder((int) ptr2[1]);
                    var c = (uint) IPAddress.NetworkToHostOrder((int) ptr2[2]);
                    var d = (uint) IPAddress.NetworkToHostOrder((int) ptr2[3]);
                    if(a == 0 || b == 0 || c == 0)
                        continue;
                    if(a + b + c != d)
                        continue;

                    var _a = a * 0x71b467a9;
                    var _b = b * 0x1edd5797;
                    var _c = c * 0x4fa242cb;
                    if(_a != _b || _b != _c)
                        continue;

                    var id = _a * 0xa7c0b0c7;
                    Console.WriteLine("Watermark found: 0x{0:x8}", id);
                    //Console.ReadKey();
                    //return;
                }
                Console.WriteLine("Finish searching.");
                Console.ReadKey();
            }
        }

        private static byte[] ExtractSection(byte[] file)
        {
            try
            {
                var md = MetaDataCreator.CreateMetaData(new PEImage(file));
                var stream = md.AllStreams.FirstOrDefault(s => s.Name == "#Koi");
                if(stream == null)
                    return file;
                var str = stream.GetClonedImageStream();
                return str.ReadBytes((int) str.Length);
            }
            catch
            {
                return file;
            }
        }
    }
}