using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace mage {
    
    public static class BitmapUtils {

        // Returns the regions of the glyphs in the given bitmap.
        public static IEnumerable<Rectangle> GetGlyphRegions(Bitmap bitmap, Func<Color, bool> isMarkerPredicate) {
            if (bitmap == null) {
                throw new NullReferenceException("The given bitmap may not be equal to null.");
            }
            if (isMarkerPredicate == null) {
                throw new NullReferenceException("The given predicate may not be equal to null.");
            }

            using (var bitmapData = new PixelAccessor(bitmap, ImageLockMode.ReadOnly)) {
                for (int y = 1; y < bitmap.Height; ++y) {
                    for (int x = 1; x < bitmap.Width; ++x) {

                        // Look for the top left corner of a character 
                        // (a pixel that is not pink, but was pink immediately to the left and above it)
                        if (!isMarkerPredicate(bitmapData[x, y]) &&
                             isMarkerPredicate(bitmapData[x - 1, y]) &&
                             isMarkerPredicate(bitmapData[x, y - 1])) {

                            // Measure the size of this character.
                            int width = 1;
                            while ((x + width < bitmap.Width) && !isMarkerPredicate(bitmapData[x + width, y])) {
                                ++width;
                            }

                            int height = 1;
                            while ((y + height < bitmap.Height) && !isMarkerPredicate(bitmapData[x, y + height])) {
                                ++height;
                            }

                            yield return new Rectangle(x, y, width, height);
                        }
                    }
                }
            }
        }

        // Returns the region of the given bitmap.
        public static Rectangle GetBitmapRegion(Bitmap bitmap) {
            if (bitmap == null) {
                throw new NullReferenceException("The given bitmap may not be equal to null.");
            }

            return new Rectangle(0, 0, bitmap.Width, bitmap.Height);
        }

        // Copies the content of the given source bitmap in the given region
        // to the content of the given destination bitmap in the given region.
        public static void CopyRegion(Bitmap sourceBitmap, Rectangle region, Bitmap destBitmap) {
            if (sourceBitmap == null) {
                throw new NullReferenceException("The given source bitmap may not be equal to null.");
            }
            if (destBitmap == null) {
                throw new NullReferenceException("The given destination bitmap may not be equal to null.");
            }
            
            using (var sourceData = new PixelAccessor(sourceBitmap, ImageLockMode.ReadOnly, region))
            using (var destData = new PixelAccessor(destBitmap, ImageLockMode.WriteOnly, region)) {
                for (int y = 0; y < region.Height; ++y) {
                    for (int x = 0; x < region.Width; ++x) {
                        destData[x, y] = sourceData[x, y];
                    }
                }
            }
        }

        // Copies the content of the given source bitmap in the given source region
        // to the content of the given destination bitmap in the given destination region.
        public static void CopyRegion(Bitmap sourceBitmap, Rectangle sourceRegion, Bitmap destBitmap, Rectangle destRegion) {
            if (sourceBitmap == null) {
                throw new NullReferenceException("The given source bitmap may not be equal to null.");
            }
            if (destBitmap == null) {
                throw new NullReferenceException("The given destination bitmap may not be equal to null.");
            }
            if (sourceRegion.Width != destRegion.Width) {
                throw new ArgumentException(string.Format("The given regions must have the same width: '{0}' != '{1}'.", sourceRegion.Width, destRegion.Width));
            }
            if (sourceRegion.Height != destRegion.Height) {
                throw new ArgumentException(string.Format("The given regions must have the same height: '{0}' != '{1}'.", sourceRegion.Height, destRegion.Height));
            }

            using (var sourceData = new PixelAccessor(sourceBitmap, ImageLockMode.ReadOnly, sourceRegion))
            using (var destData = new PixelAccessor(destBitmap, ImageLockMode.WriteOnly, destRegion)) {
                for (int y = 0; y < sourceRegion.Height; ++y) {
                    for (int x = 0; x < sourceRegion.Width; ++x) {
                        destData[x, y] = sourceData[x, y];
                    }
                }
            }
        }

        // Checks whether the alpha values of the given bitmap in the given region
        // matches the given alpha value.
        public static bool MatchesAlpha(byte expectedAlpha, Bitmap bitmap, Rectangle? region = null) {
            if (bitmap == null) {
                throw new NullReferenceException("The given bitmap may not be equal to null.");
            }

            using (var bitmapData = new PixelAccessor(bitmap, ImageLockMode.ReadOnly, region)) {
                for (int y = 0; y < bitmapData.Region.Height; ++y) {
                    for (int x = 0; x < bitmapData.Region.Width; ++x) {
                        byte alpha = bitmapData[x, y].A;

                        if (alpha != expectedAlpha) {
                            return false;
                        }
                    }
                }
            }

            return true;
        }

        // Checks whether the RGB values of the given bitmap in the given region
        // matches the given RGB value.
        public static bool MatchesRGB(Color expectedRGB, Bitmap bitmap) {
            if (bitmap == null) {
                throw new NullReferenceException("The given bitmap may not be equal to null.");
            }

            using (var bitmapData = new PixelAccessor(bitmap, ImageLockMode.ReadOnly)) {
                for (int y = 0; y < bitmap.Height; ++y) {
                    for (int x = 0; x < bitmap.Width; ++x) {
                        Color color = bitmapData[x, y];

                        if (color.A == 0)
                            continue;

                        if ((color.R != expectedRGB.R) ||
                            (color.G != expectedRGB.G) ||
                            (color.B != expectedRGB.B)) {
                            return false;
                        }
                    }
                }
            }

            return true;
        }

        // Converts the greyscale luminosity of the given bitmap to alpha data.
        public static void ConvertGreyToAlpha(Bitmap bitmap) {
            if (bitmap == null) {
                throw new NullReferenceException("The given bitmap may not be equal to null.");
            }

            using (var bitmapData = new PixelAccessor(bitmap, ImageLockMode.ReadWrite)) {
                for (int y = 0; y < bitmap.Height; ++y) {
                    for (int x = 0; x < bitmap.Width; ++x) {
                        Color color = bitmapData[x, y];

                        // Average the red, green and blue values to compute brightness.
                        int alpha = (color.R + color.G + color.B) / 3;

                        bitmapData[x, y] = Color.FromArgb(alpha, 255, 255, 255);
                    }
                }
            }
        }

        // Converts the content of the given bitmap to premultiplied alpha format.
        public static void ConvertToPremultipliedAlpha(Bitmap bitmap) {
            if (bitmap == null) {
                throw new NullReferenceException("The given bitmap may not be equal to null.");
            }

            using (var bitmapData = new PixelAccessor(bitmap, ImageLockMode.ReadWrite)) {
                for (int y = 0; y < bitmap.Height; ++y) {
                    for (int x = 0; x < bitmap.Width; ++x) {
                        Color color = bitmapData[x, y];

                        int a = color.A;
                        int r = color.R * a / 255;
                        int g = color.G * a / 255;
                        int b = color.B * a / 255;

                        bitmapData[x, y] = Color.FromArgb(a, r, g, b);
                    }
                }
            }
        }

        // Converts a bitmap to the specified pixel format.
        public static Bitmap ConvertToPixelFormat(Bitmap bitmap, PixelFormat format) {
            return bitmap.Clone(GetBitmapRegion(bitmap), format);
        }

        // To avoid filtering artifacts when scaling or rotating fonts that do not use premultiplied alpha,
        // make sure the one pixel border around each glyph contains the same RGB values as the edge of the
        // glyph itself, but with zero alpha. This processing is an elaborate no-op when using premultiplied
        // alpha, because the premultiply conversion will change the RGB of all such zero alpha pixels to black.
        public static void PadBorderPixels(Bitmap bitmap, Rectangle region) {
            if (bitmap == null) {
                throw new NullReferenceException("The given bitmap may not be equal to null.");
            }

            using (var bitmapData = new PixelAccessor(bitmap, ImageLockMode.ReadWrite)) {
                // Pad the top (inclusive) and bottom (exclusive).
                for (int x = region.Left; x < region.Right; ++x) {
                    CopyBorderPixel(bitmapData, x, region.Top,        x, region.Top - 1);
                    CopyBorderPixel(bitmapData, x, region.Bottom - 1, x, region.Bottom);
                }

                // Pad the left (inclusive) and right (exclusive).
                for (int y = region.Top; y < region.Bottom; ++y) {
                    CopyBorderPixel(bitmapData, region.Left,      y, region.Left - 1, y);
                    CopyBorderPixel(bitmapData, region.Right - 1, y, region.Right,    y);
                }

                // Pad the four corners.
                CopyBorderPixel(bitmapData, region.Left,      region.Top,        region.Left - 1, region.Top - 1);
                CopyBorderPixel(bitmapData, region.Right - 1, region.Top,        region.Right,    region.Top - 1);
                CopyBorderPixel(bitmapData, region.Left,      region.Bottom - 1, region.Left - 1, region.Bottom);
                CopyBorderPixel(bitmapData, region.Right - 1, region.Bottom - 1, region.Right,    region.Bottom);
            }
        }

        // Copies the given source pixel to the given destination pixel in the given bitmap
        // (but forcing alpha to zero).
        static void CopyBorderPixel(PixelAccessor bitmapData, int sourceX, int sourceY, int destX, int destY) {
            Color color = bitmapData[sourceX, sourceY];
            bitmapData[destX, destY] = Color.FromArgb(0, color);
        }

        // Helper for locking a bitmap and efficiently reading or writing its pixels.
        public sealed class PixelAccessor : IDisposable {
            
            public PixelAccessor(Bitmap bitmap, ImageLockMode mode, Rectangle? region = null) {
                if (bitmap == null) {
                    throw new NullReferenceException("The given bitmap may not be equal to null.");
                }

                this.Bitmap = bitmap;
                this.Region = region.GetValueOrDefault(GetBitmapRegion(bitmap));
                this.BitmapData = Bitmap.LockBits(Region, mode, PixelFormat.Format32bppArgb);
            }

            private Bitmap Bitmap { get; set; }
            private BitmapData BitmapData { get; set; }
            public Rectangle Region { get; private set; }

            // Get or set a pixel value.
            public Color this[int x, int y] {
                get {
                    return Color.FromArgb(Marshal.ReadInt32(GetPixelAddress(x, y)));
                }
                set {
                    Marshal.WriteInt32(GetPixelAddress(x, y), value.ToArgb());
                }
            }

            // Dispose (i.e. unlock) the bitmap.
            public void Dispose() {
                if (BitmapData != null) {
                    Bitmap.UnlockBits(BitmapData);
                    BitmapData = null;
                }
            }

            // Gets the address of the given pixel.
            IntPtr GetPixelAddress(int x, int y) {
                return BitmapData.Scan0 + (y * BitmapData.Stride) + (x * sizeof(int));
            }
        }
    }
}
