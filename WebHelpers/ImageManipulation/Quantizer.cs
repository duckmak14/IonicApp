namespace Common.ImageManipulation
{
    using System;
    using System.Drawing;
    using System.Drawing.Imaging;
    using System.Runtime.InteropServices;

    public abstract class Quantizer
    {
        private int _pixelSize;
        private bool _singlePass;

        public Quantizer(bool singlePass)
        {
            this._singlePass = singlePass;
            this._pixelSize = Marshal.SizeOf(typeof(Color32));
        }

        protected virtual void FirstPass(BitmapData sourceData, int width, int height)
        {
            IntPtr ptr = sourceData.Scan0;
            for (int i = 0; i < height; i++)
            {
                IntPtr pSourcePixel = ptr;
                for (int j = 0; j < width; j++)
                {
                    this.InitialQuantizePixel(new Color32(pSourcePixel));
                    pSourcePixel = (IntPtr) (pSourcePixel.ToInt64() + this._pixelSize);
                }
                ptr = (IntPtr) (ptr.ToInt64() + sourceData.Stride);
            }
        }

        protected abstract ColorPalette GetPalette(ColorPalette original);
        protected virtual void InitialQuantizePixel(Color32 pixel)
        {
        }

        public Bitmap Quantize(Image source)
        {
            int height = source.Height;
            int width = source.Width;
            Rectangle rect = new Rectangle(0, 0, width, height);
            Bitmap image = new Bitmap(width, height, PixelFormat.Format32bppArgb);
            Bitmap output = new Bitmap(width, height, PixelFormat.Format8bppIndexed);
            using (Graphics graphics = Graphics.FromImage(image))
            {
                graphics.PageUnit = GraphicsUnit.Pixel;
                graphics.DrawImage(source, rect);
            }
            BitmapData sourceData = null;
            try
            {
                sourceData = image.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
                if (!this._singlePass)
                {
                    this.FirstPass(sourceData, width, height);
                }
                output.Palette = this.GetPalette(output.Palette);
                this.SecondPass(sourceData, output, width, height, rect);
            }
            finally
            {
                image.UnlockBits(sourceData);
            }
            return output;
        }

        protected abstract byte QuantizePixel(Color32 pixel);
        protected virtual void SecondPass(BitmapData sourceData, Bitmap output, int width, int height, Rectangle bounds)
        {
            BitmapData bitmapdata = null;
            try
            {
                bitmapdata = output.LockBits(bounds, ImageLockMode.WriteOnly, PixelFormat.Format8bppIndexed);
                IntPtr ptr = sourceData.Scan0;
                IntPtr pSourcePixel = ptr;
                IntPtr ptr3 = pSourcePixel;
                IntPtr ptr4 = bitmapdata.Scan0;
                IntPtr ptr5 = ptr4;
                byte val = this.QuantizePixel(new Color32(pSourcePixel));
                Marshal.WriteByte(ptr5, val);
                for (int i = 0; i < height; i++)
                {
                    pSourcePixel = ptr;
                    ptr5 = ptr4;
                    for (int j = 0; j < width; j++)
                    {
                        if (Marshal.ReadInt32(ptr3) != Marshal.ReadInt32(pSourcePixel))
                        {
                            val = this.QuantizePixel(new Color32(pSourcePixel));
                            ptr3 = pSourcePixel;
                        }
                        Marshal.WriteByte(ptr5, val);
                        pSourcePixel = (IntPtr) (pSourcePixel.ToInt64() + this._pixelSize);
                        ptr5 = (IntPtr) (ptr5.ToInt64() + 1L);
                    }
                    ptr = (IntPtr) (ptr.ToInt64() + sourceData.Stride);
                    ptr4 = (IntPtr) (ptr4.ToInt64() + bitmapdata.Stride);
                }
            }
            finally
            {
                output.UnlockBits(bitmapdata);
            }
        }

        [StructLayout(LayoutKind.Explicit)]
        public struct Color32
        {
            [FieldOffset(3)]
            public byte Alpha;
            [FieldOffset(0)]
            public int ARGB;
            [FieldOffset(0)]
            public byte Blue;
            [FieldOffset(1)]
            public byte Green;
            [FieldOffset(2)]
            public byte Red;

            public Color32(IntPtr pSourcePixel)
            {
                this = (Quantizer.Color32) Marshal.PtrToStructure(pSourcePixel, typeof(Quantizer.Color32));
            }

            public System.Drawing.Color Color
            {
                get
                {
                    return System.Drawing.Color.FromArgb(this.Alpha, this.Red, this.Green, this.Blue);
                }
            }
        }
    }
}

