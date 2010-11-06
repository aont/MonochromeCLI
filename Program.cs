using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;

namespace Aont
{
    class Program
    {
        static Random rand = new Random();
        static void Main(string[] args)
        {

            var OriginalPath = args[0];

            var Original = new Bitmap(OriginalPath);

            var Converted = new Bitmap(Original.Width, Original.Height, PixelFormat.Format1bppIndexed);

            double average = 0.5;
            double sigma = 0.1;
            if (args.Length > 2)
            {
                average = double.Parse(args[1]);
                if (args.Length == 3)
                    sigma = double.Parse(args[2]);
                else
                    throw new ArgumentException("Too Many Options!");
            }


            Stopwatch sw = new Stopwatch();
            sw.Start();

            int Width = Original.Width, Height = Original.Height;
            using (var proc = new BmpProc1(Converted))
            {
                double alpha, beta;

                double b;
                using (var proc_original = new BmpProc32(Original))
                    for (int y = 0; y < Height; ++y)
                    {
                        for (int x = 0; x < Width; ++x)
                        {
                            b = proc_original[x, y].GetBrightness();
                            alpha = rand.NextDouble();
                            beta = rand.NextDouble();
                            b += sigma * Math.Sqrt(-2 * Math.Log(alpha)) * Math.Cos(2 * Math.PI * beta);
                            if (b > average)
                                proc[x, y] = true;
                            else
                                proc[x, y] = false;
                        }
                    }
                Original.Dispose();
            }
            sw.Stop();
            Console.WriteLine("{0} Seconds.", sw.Elapsed.TotalSeconds);
            var SaveFileName = Path.Combine(Path.GetDirectoryName(OriginalPath), Path.GetFileNameWithoutExtension(OriginalPath) + "_mono.bmp");
            Converted.Save(SaveFileName, ImageFormat.Bmp);
            Converted.Dispose();
        }
    }
}
