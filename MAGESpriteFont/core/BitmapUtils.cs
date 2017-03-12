using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace mage {
    
    public static class BitmapUtils {

        // Copies a rectangular area from one bitmap to another.
        public static void CopyRegion(Bitmap source, Rectangle source_region, Bitmap output, Rectangle output_region) {
            if (source_region.Width  != output_region.Width ||
                source_region.Height != output_region.Height) {
                throw new ArgumentException();
            }

            using (var sourceData = new PixelAccessor(source, ImageLockMode.ReadOnly, source_region))
            using (var outputData = new PixelAccessor(output, ImageLockMode.WriteOnly, output_region)) {
                for (int y = 0; y < source_region.Height; ++y) {
                    for (int x = 0; x < source_region.Width; ++x) {
                        outputData[x, y] = sourceData[x, y];
                    }
                }
            }
        }

        // Checks whether an area of a bitmap contains entirely the specified alpha value.
        public static bool IsEntirelyAlpha(byte expected_alpha, Bitmap bitmap, Rectangle? region = null) {
            using (var bitmap_data = new PixelAccessor(bitmap, ImageLockMode.ReadOnly, region)) {
                for (int y = 0; y < bitmap_data.Region.Height; ++y) {
                    for (int x = 0; x < bitmap_data.Region.Width; ++x) {
                        byte alpha = bitmap_data[x, y].A;
                        if (alpha != expected_alpha) {
                            return false;
                        }
                    }
                }
            }

            return true;
        }

        // Checks whether a bitmap contains entirely the specified RGB value.
        public static bool IsEntirelyRGB(Color expected_rgb, Bitmap bitmap) {
            using (var bitmap_data = new PixelAccessor(bitmap, ImageLockMode.ReadOnly)) {
                for (int y = 0; y < bitmap.Height; ++y) {
                    for (int x = 0; x < bitmap.Width; ++x) {
                        Color color = bitmap_data[x, y];

                        if (color.A == 0) {
                            continue;
                        }

                        if ((color.R != expected_rgb.R) ||
                            (color.G != expected_rgb.G) ||
                            (color.B != expected_rgb.B)) {
                            return false;
                        }
                    }
                }
            }
            
            return true;
        }

        // Converts greyscale luminosity to alpha data.
        public static void ConvertGreyToAlpha(Bitmap bitmap) {
            using (var bitmap_data = new PixelAccessor(bitmap, ImageLockMode.ReadWrite)) {
                for (int y = 0; y < bitmap.Height; ++y) {
                    for (int x = 0; x < bitmap.Width; ++x) {
                        Color color = bitmap_data[x, y];

                        int alpha = (color.R + color.G + color.B) / 3;
                        bitmap_data[x, y] = Color.FromArgb(alpha, 255, 255, 255);
                    }
                }
            }
        }

        // Converts a bitmap to premultiplied alpha format.
        public static void PremultiplyAlpha(Bitmap bitmap) {
            using (var bitmap_data = new PixelAccessor(bitmap, ImageLockMode.ReadWrite)) {
                for (int y = 0; y < bitmap.Height; ++y) {
                    for (int x = 0; x < bitmap.Width; ++x) {
                        Color color = bitmap_data[x, y];

                        int a = color.A;
                        int r = color.R * a / 255;
                        int g = color.G * a / 255;
                        int b = color.B * a / 255;
                        bitmap_data[x, y] = Color.FromArgb(a, r, g, b);
                    }
                }
            }
        }

        // To avoid filtering artifacts when scaling or rotating fonts that do not use premultiplied alpha,
        // make sure the one pixel border around each glyph contains the same RGB values as the edge of the
        // glyph itself, but with zero alpha. This processing is an elaborate no-op when using premultiplied
        // alpha, because the premultiply conversion will change the RGB of all such zero alpha pixels to black.
        public static void PadBorderPixels(Bitmap bitmap, Rectangle region) {
            using (var bitmap_data = new PixelAccessor(bitmap, ImageLockMode.ReadWrite)) {
                
                // Pad the top and bottom.
                for (int x = region.Left; x < region.Right; ++x) {
                    CopyBorderPixel(bitmap_data, x, region.Top, x, region.Top - 1);
                    CopyBorderPixel(bitmap_data, x, region.Bottom - 1, x, region.Bottom);
                }

                // Pad the left and right.
                for (int y = region.Top; y < region.Bottom; ++y) {
                    CopyBorderPixel(bitmap_data, region.Left, y, region.Left - 1, y);
                    CopyBorderPixel(bitmap_data, region.Right - 1, y, region.Right, y);
                }

                // Pad the four corners.
                CopyBorderPixel(bitmap_data, region.Left, region.Top, region.Left - 1, region.Top - 1);
                CopyBorderPixel(bitmap_data, region.Right - 1, region.Top, region.Right, region.Top - 1);
                CopyBorderPixel(bitmap_data, region.Left, region.Bottom - 1, region.Left - 1, region.Bottom);
                CopyBorderPixel(bitmap_data, region.Right - 1, region.Bottom - 1, region.Right, region.Bottom);
            }
        }

        // Copies a single pixel within a bitmap, preserving RGB but forcing alpha to zero.
        static void CopyBorderPixel(PixelAccessor bitmap_data, int source_x, int source_y, int dest_x, int dest_y) {
            Color color = bitmap_data[source_x, source_y];
            bitmap_data[dest_x, dest_y] = Color.FromArgb(0, color);
        }

        // Converts a bitmap to the specified pixel format.
        public static Bitmap ChangePixelFormat(Bitmap bitmap, PixelFormat format) {
            Rectangle region = new Rectangle(0, 0, bitmap.Width, bitmap.Height);
            return bitmap.Clone(region, format);
        }

        // Helper for locking a bitmap and efficiently reading or writing its pixels.
        public sealed class PixelAccessor : IDisposable {

            private static Rectangle CreateDefaultRegion(Bitmap bitmap) {
                return new Rectangle(0, 0, bitmap.Width, bitmap.Height);
            }

            public PixelAccessor(Bitmap bitmap, ImageLockMode mode, Rectangle? region = null) {
                this.bitmap = bitmap;
                this.Region = region.GetValueOrDefault(CreateDefaultRegion(bitmap));
                this.bitmap_data = bitmap.LockBits(Region, mode, PixelFormat.Format32bppArgb);
            }

            private Bitmap bitmap;
            private BitmapData bitmap_data;
            public Rectangle Region { get; private set; }

            public void Dispose()  {
                if (bitmap_data != null) {
                    bitmap.UnlockBits(bitmap_data);
                    bitmap_data = null;
                }
            }

            // Get or set a pixel value.
            public Color this[int x, int y] {
                get {
                    return Color.FromArgb(Marshal.ReadInt32(PixelAddress(x, y)));
                }
                set {
                    Marshal.WriteInt32(PixelAddress(x, y), value.ToArgb()); 
                }
            }

            // Computes the address of the specified pixel.
            IntPtr PixelAddress(int x, int y) {
                return bitmap_data.Scan0 + (y * bitmap_data.Stride) + (x * sizeof(int));
            }
        }
    }
}
