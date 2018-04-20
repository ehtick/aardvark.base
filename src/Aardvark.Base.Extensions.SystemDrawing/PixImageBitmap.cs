﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;

namespace Aardvark.Base
{
    //public class PixImageBitmap<T> : PixImageBitmap
    //{
    //    public PixImageBitmap(System.Drawing.Bitmap bitmap)
    //    {
    //        var loadImage = Create(bitmap);
    //        var loadPixImage = loadImage as PixImage<T>;
    //        if (loadPixImage == null)
    //            loadPixImage = new PixImage<T>(loadImage);
    //        Format = loadPixImage.Format;
    //        Volume = loadPixImage.Volume;
    //    }
    //}

    /// <summary>
    /// </summary>
    public abstract class PixImageBitmap : PixImage
    {
        /// <summary>
        /// </summary>
        protected static Dictionary<PixelFormat, (PixFormat, int)> s_pixFormatAndCountOfPixelFormatBitmap =
            new Dictionary<PixelFormat, (PixFormat, int)>()
        {
            { PixelFormat.Format1bppIndexed, (PixFormat.ByteBW, 1) },
            { PixelFormat.Format8bppIndexed, (PixFormat.ByteBW, 1) },

            { PixelFormat.Format16bppGrayScale, (PixFormat.UShortGray, 1) },

            { PixelFormat.Format24bppRgb, (PixFormat.ByteBGR, 3) },
            { PixelFormat.Format32bppRgb, (PixFormat.ByteBGR, 4) },
            { PixelFormat.Format32bppArgb, (PixFormat.ByteBGRA, 4) },
            { PixelFormat.Format32bppPArgb, (PixFormat.ByteBGRP, 4) },

            { PixelFormat.Format48bppRgb, (PixFormat.UShortBGR, 3) },
            { PixelFormat.Format64bppArgb, (PixFormat.UShortBGRA, 4) },
            { PixelFormat.Format64bppPArgb, (PixFormat.UShortBGRP, 4) },

        };

        /// <summary></summary>
        protected static Dictionary<PixelFormat, PixelFormat> s_bitmapLockFormats = new Dictionary<PixelFormat, PixelFormat>() 
        { 
            { PixelFormat.Format16bppArgb1555, PixelFormat.Format32bppArgb },
            { PixelFormat.Format16bppRgb555, PixelFormat.Format24bppRgb },
            { PixelFormat.Format16bppRgb565, PixelFormat.Format24bppRgb },
            { PixelFormat.Format4bppIndexed, PixelFormat.Format8bppIndexed }
        };

        /// <summary></summary>
        protected static Dictionary<PixFormat, PixelFormat> s_pixelFormats =
            new Dictionary<PixFormat, PixelFormat>()
        {
            { PixFormat.ByteBGR, PixelFormat.Format24bppRgb },
            { PixFormat.ByteBGRA, PixelFormat.Format32bppArgb },
            { PixFormat.ByteBW, PixelFormat.Format8bppIndexed },
            { PixFormat.ByteRGBP, PixelFormat.Format32bppPArgb },
            { PixFormat.UShortGray, PixelFormat.Format16bppGrayScale },
            { PixFormat.UShortBGR, PixelFormat.Format48bppRgb },
            { PixFormat.UShortBGRA, PixelFormat.Format64bppArgb },
            { PixFormat.UShortBGRP, PixelFormat.Format64bppPArgb },
        };

        /// <summary></summary>
        protected static Dictionary<PixFileFormat, ImageFormat> s_imageFormats =
            new Dictionary<PixFileFormat, ImageFormat>()
        {
            {PixFileFormat.Bmp, ImageFormat.Bmp},
            {PixFileFormat.Gif, ImageFormat.Gif},
            {PixFileFormat.Jpeg, ImageFormat.Jpeg},
            {PixFileFormat.Png, ImageFormat.Png},
            {PixFileFormat.Tiff, ImageFormat.Tiff},
            {PixFileFormat.Wmp, ImageFormat.Wmf},
        };

        /// <summary></summary>
        protected static Dictionary<Guid, ImageCodecInfo> s_imageCodecInfos =
            ImageCodecInfo.GetImageEncoders().ToDictionary(c => c.FormatID);
        
        private static PixelFormat GetLockFormat(PixelFormat format)
            => s_pixFormatAndCountOfPixelFormatBitmap.ContainsKey(format) ? format : s_bitmapLockFormats[format];

        /// <summary></summary>
        protected static PixImage CreateRawBitmap(Bitmap bitmap)
        {
            var sdipf = GetLockFormat(bitmap.PixelFormat);
            var pfc = s_pixFormatAndCountOfPixelFormatBitmap[sdipf];

            var sx = bitmap.Width;
            var sy = bitmap.Height;
            var ch = pfc.Item2;

            var pixImage = Create(pfc.Item1, sx, sy, ch);
            var array = pixImage.Array;

            if (pfc.Item1.Format == Col.Format.BW)
            {
                var bitImage = new PixImage<byte>(Col.Format.BW, 1 + (sx - 1) / 8, sy, 1);

                BitmapData bdata = bitmap.LockBits(
                    new Rectangle(0, 0, sx, sy),
                    ImageLockMode.ReadOnly, sdipf);

                bdata.Scan0.CopyTo(bitImage.Volume.Data);

                bitmap.UnlockBits(bdata);
                ExpandPixels(bitImage, pixImage.ToPixImage<byte>());
            }
            else
            {
                BitmapData bdata = bitmap.LockBits(
                    new Rectangle(0, 0, sx, sy),
                    ImageLockMode.ReadOnly, sdipf);

                bdata.Scan0.CopyTo(array);
                bitmap.UnlockBits(bdata);
            }
            return pixImage;
        }
        
        /// <summary>
        /// Load image from stream via System.Drawing.
        /// </summary>
        /// <returns>If file could not be read, returns null, otherwise a Piximage.</returns>
        private static PixImage CreateRawBitmap(
                Stream stream,
                PixLoadOptions loadFlags = PixLoadOptions.Default)
        {
            try
            {
                using (var bmp = (Bitmap)Bitmap.FromStream(stream))
                {
                    var result = CreateRawBitmap(bmp);
                    return result;
                }
            }
            catch
            {
                return null;
            }
        }

        /// <summary></summary>
        public static PixImage Create(Bitmap bitmap)
        {
            var loadImage = CreateRawBitmap(bitmap);
            return loadImage.ToPixImage(loadImage.Format);
        }

        /// <summary>
        /// Save image to stream via System.Drawing.
        /// </summary>
        /// <returns>True if the file was successfully saved.</returns>
        private bool SaveAsImageBitmap(
                Stream stream, PixFileFormat format,
                PixSaveOptions options, int qualityLevel)
        {
            try
            {
                var self = this.ToCanonicalDenseLayout();
                var size = self.Size;
                var pixelFormat = s_pixelFormats[self.PixFormat];
                var imageFormat = s_imageFormats[format];
                
                using (var bmp = new Bitmap(size.X, size.Y, pixelFormat))
                {
                    var bdata = bmp.LockBits(new Rectangle(0, 0, size.X, size.Y), ImageLockMode.ReadOnly, pixelFormat);
                    self.Data.CopyTo(bdata.Scan0);
                    bmp.UnlockBits(bdata);

                    if(qualityLevel >= 0)
                    {
                        var codec = s_imageCodecInfos[imageFormat.Guid];
                        var parameters = new EncoderParameters(1);
                        parameters.Param[0] = new EncoderParameter(Encoder.Quality, qualityLevel);
                        bmp.Save(stream, codec, parameters);
                        parameters.Dispose();
                    }
                    else
                    {
                        bmp.Save(stream, imageFormat);
                    }

                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary></summary>
        public Bitmap ToBitmap() => new Bitmap(ToMemoryStream(PixFileFormat.Png), false);

        /// <summary>
        /// Gets info about a PixImage without loading the entire image into memory.
        /// </summary>
        /// <returns>null if the file info could not be loaded.</returns>
        public static PixImageInfo InfoFromFileNameBitmap(
                string fileName, PixLoadOptions options)
        {
            // TODO: implement
            return null;
        }
    }
}