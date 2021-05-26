namespace System.Web
{
    using Common.ImageManipulation;
    using System;
    using System.Drawing;
    using System.Drawing.Drawing2D;
    using System.Drawing.Imaging;
    using System.IO;
    using System.Security;
    using System.Web;

    public class ImageTools
    {
        private ImageTools()
        {
        }

        private static Size GetAspectRatioSize(int maxWidth, int maxHeight, int actualWidth, int actualHeight)
        {
            Size size = new Size(maxWidth, maxHeight);
            float num = ((float)maxWidth) / ((float)actualWidth);
            float num2 = ((float)maxHeight) / ((float)actualHeight);
            if ((num != 1f) || (num2 != 1f))
            {
                if (num < num2)
                {
                    size.Height = (int)Math.Round((double)(actualHeight * num));
                }
                else if (num > num2)
                {
                    size.Width = (int)Math.Round((double)(actualWidth * num2));
                }
            }
            if (size.Height <= 0)
            {
                size.Height = 1;
            }
            if (size.Width <= 0)
            {
                size.Width = 1;
            }
            return size;
        }

        private static ImageCodecInfo GetJpgCodec()
        {
            ImageCodecInfo[] imageEncoders = ImageCodecInfo.GetImageEncoders();
            for (int i = 0; i < imageEncoders.Length; i++)
            {
                if (imageEncoders[i].MimeType.Equals("image/jpeg"))
                {
                    return imageEncoders[i];
                }
            }
            return null;
        }

        public static bool IsImageExtension(string extension)
        {
            string str = extension.ToLower();
            return ((str != null) && (((str == "jpg") || (str == "jpeg")) || (((str == "gif") || (str == "png")) || (str == "bmp"))));
        }

        public static bool ResizeImage(string sourceFile, string targetFile, int maxWidth, int maxHeight, bool preserverAspectRatio, int quality)
        {
            Image image;
            Size size;
            Image image2;
            Rectangle rectangle;
            try
            {
                image = Image.FromFile(sourceFile);
            }
            catch (OutOfMemoryException)
            {
                return false;
            }
            maxWidth = (maxWidth == 0) ? image.Width : maxWidth;
            maxHeight = (maxHeight == 0) ? image.Height : maxHeight;
            if ((image.Width <= maxWidth) && (image.Height <= maxHeight))
            {
                image.Dispose();
                if (sourceFile != targetFile)
                {
                    File.Copy(sourceFile, targetFile);
                }
                return true;
            }
            if (preserverAspectRatio)
            {
                size = GetAspectRatioSize(maxWidth, maxHeight, image.Width, image.Height);
            }
            else
            {
                size = new Size(maxWidth, maxHeight);
            }
            if ((((image.PixelFormat == PixelFormat.Indexed) || (image.PixelFormat == PixelFormat.Format1bppIndexed)) || (image.PixelFormat == PixelFormat.Format4bppIndexed)) || (image.PixelFormat == PixelFormat.Format8bppIndexed))
            {
                image2 = new Bitmap(size.Width, size.Height, PixelFormat.Format24bppRgb);
            }
            else
            {
                image2 = new Bitmap(size.Width, size.Height, image.PixelFormat);
            }
            Graphics graphics = Graphics.FromImage(image2);
            if (quality > 80)
            {
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                rectangle = new Rectangle(-1, -1, size.Width + 1, size.Height + 1);
            }
            else
            {
                rectangle = new Rectangle(0, 0, size.Width, size.Height);
            }
            graphics.FillRectangle(new SolidBrush(Color.White), rectangle);
            graphics.DrawImage(image, rectangle);
            image.Dispose();
            string str = Path.GetExtension(targetFile).ToLower();
            string str3 = str;
            if ((str3 != null) && ((str3 == ".jpg") || (str3 == ".jpeg")))
            {
                ImageCodecInfo jpgCodec = GetJpgCodec();
                if (jpgCodec != null)
                {
                    EncoderParameters encoderParams = new EncoderParameters(1);
                    encoderParams.Param[0] = new EncoderParameter(Encoder.Quality, (long)quality);
                    image2.Save(targetFile, jpgCodec, encoderParams);
                }
                else
                {
                    image2.Save(targetFile);
                }
            }
            else
            {
                switch (str)
                {
                    case ".gif":
                        try
                        {
                            OctreeQuantizer quantizer = new OctreeQuantizer(0xff, 8);
                            using (Bitmap bitmap = quantizer.Quantize(image2))
                            {
                                bitmap.Save(targetFile, ImageFormat.Gif);
                            }
                        }
                        catch (SecurityException)
                        {
                            image2.Save(targetFile, ImageFormat.Png);
                        }
                        break;

                    case ".png":
                        image2.Save(targetFile, ImageFormat.Png);
                        break;

                    case ".bmp":
                        image2.Save(targetFile, ImageFormat.Bmp);
                        break;
                }
            }
            graphics.Dispose();
            image2.Dispose();
            return true;
        }
        public static ImageFormat GetImageFormat(string filename)
        {
            string str3 = Path.GetExtension(filename).ToLower();
            if ((str3 != null) && ((str3 == ".jpg") || (str3 == ".jpeg")))
            {
                return ImageFormat.Jpeg;
            }
            else
            {
                switch (str3)
                {
                    case ".gif":

                        return ImageFormat.Gif;
                    case ".png":
                        return ImageFormat.Png;

                    case ".bmp":
                        return ImageFormat.Bmp;
                }
            }
            return ImageFormat.Jpeg;
        }
        public static bool ValidateImage(string filePath)
        {
            try
            {
                Image.FromFile(filePath).Dispose();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static bool ValidateUriImage(string uri)
        {
            try
            {
                Image.FromFile(HttpContext.Current.Server.MapPath(uri)).Dispose();
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}

