/*
 *  Image Processing Utilities for C#2.0 (VC# 2005)
 *  
 *         Copyright junki, Jan, 2006 -
 * 
 *  http://junki.main.jp/
 *  http://code.junki.main.jp/
 * 
 *  how to use : see http://junki.main.jp/csgr/006Library1.htm
 * 
 * This library is free for any non commercial usage. In the case of
 * modifying and/or redistributing the code, it's obligate to retain
 * the original copyright notice.
 * 
 * Moified by Aont 08/14
 * http://d.hatena.ne.jp/aont/
 */

using System;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;

namespace Aont
{
    public unsafe class BmpProc : IDisposable
    {
        bool flagDispose = false;

        protected Bitmap bitmap;
        public readonly int Width, Height;
        protected BitmapData bmpData;
        protected byte* Scan0;
        protected int Stride;
        public readonly int DataLength;

        public readonly PixelFormat Format;
        protected BmpProc(Bitmap bitmap, PixelFormat Format)
        {
            this.bitmap = bitmap;
            Width = bitmap.Width;
            Height = bitmap.Height;
            Rectangle rect = new Rectangle(0, 0, Width, Height);
            bmpData = bitmap.LockBits(rect, ImageLockMode.ReadWrite, Format);
            Scan0 = (byte*)bmpData.Scan0;
            Stride = Math.Abs(bmpData.Stride);

            DataLength = Stride * Height;
        }
        protected virtual void Dispose(bool flag)
        {
            if (!flagDispose)
            {
                if (flag)
                {
                    bitmap.UnlockBits(bmpData);
                }
                this.flagDispose = true;
            }
        }
        public virtual Color GetPixel(int x, int y) { throw new NotImplementedException(); }
        public virtual void SetPixel(int x, int y, Color color) { throw new NotImplementedException(); }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        ~BmpProc()
        {
            Dispose(false);
        }
    }
    public unsafe class BmpProc32 : BmpProc
    {
        public BmpProc32(Bitmap bitmap) : base(bitmap, PixelFormat.Format32bppArgb) { }
        public Color this[int x, int y]
        {
            get
            {
                return Color.FromArgb(*(Int32*)&Scan0[Stride * y + x * 4]);
            }
            set
            {
                *(Int32*)&Scan0[Stride * y + x * 4] = value.ToArgb();
            }
        }
        public override Color GetPixel(int x, int y)
        { return this[x, y]; }
        public override void SetPixel(int x, int y, Color color)
        { this[x, y] = color; }
    }
    public unsafe class BmpProc24 : BmpProc
    {
        public BmpProc24(Bitmap bitmap) : base(bitmap, PixelFormat.Format24bppRgb) { }
        public Color this[int x, int y]
        {
            get
            {
                byte* PixelB = &Scan0[Stride * y + x * 3];
                return Color.FromArgb(PixelB[2], PixelB[1], *PixelB);
            }
            set
            {
                byte* PixelB = &Scan0[Stride * y + x * 3];
                *PixelB = value.B;
                PixelB[1] = value.G;
                PixelB[2] = value.R;
            }
        }
        public override Color GetPixel(int x, int y)
        { return this[x, y]; }
        public override void SetPixel(int x, int y, Color color)
        { this[x, y] = color; }
    }
    public unsafe class BmpProc8 : BmpProc
    {
        public Color[] Palette { get { return bitmap.Palette.Entries; } }
        public BmpProc8(Bitmap bitmap) : base(bitmap, PixelFormat.Format8bppIndexed) { }
        public byte this[int x, int y]
        {
            get { return Scan0[Stride * y + x]; }
            set { Scan0[Stride * y + x] = value; }
        }
        public byte this[int index]
        {
            get { return Scan0[index]; }
            set { Scan0[index] = value; }
        }
        public override Color GetPixel(int x, int y)
        {
            return Palette[this[x, y]];
        }
        public override void SetPixel(int x, int y, Color color)
        {
            int i;
            for (i = 0; i < 256; ++i)
            {
                if (Palette[i] == color)
                    break;
            }
            if (i < 256)
            {
                this[x, y] = (byte)i;
            }
        }
    }
    public unsafe class BmpProc1 : BmpProc
    {
        public BmpProc1(Bitmap bitmap) : base(bitmap, PixelFormat.Format1bppIndexed) { }
        public bool this[int x, int y]
        {
            get
            {
                int xr;
                int ofs = Stride * y + Math.DivRem(x, 8, out xr);
                return ((Scan0[ofs] >> (7 - xr) & 1) == 1);
            }

            set
            {
                int xr;
                int ofs = Stride * y + Math.DivRem(x, 8, out xr);
                if (value)
                    Scan0[ofs] = (byte)(Scan0[ofs] | (1 << (7 - xr)));
                else
                    Scan0[ofs] = (byte)(Scan0[ofs] & (~(1 << (7 - xr))));
            }
        }
        public bool this[int index]
        {
            get { return ((*(Scan0 + index / 8) >> (7 - (index % 8)) & 1) == 1); }
            set
            {
                int xr;
                int ofs = Math.DivRem(index, 8, out xr);
                if (value)
                    Scan0[ofs] = (byte)(Scan0[ofs] | (1 << (7 - xr)));
                else
                    Scan0[ofs] = (byte)(Scan0[ofs] & (~(1 << (7 - xr))));
            }
        }
    }
}