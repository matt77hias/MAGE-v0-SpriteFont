using System.Drawing;

namespace mage {
    
    // Crops unused space from around the edge of a glyph bitmap.
    public static class GlyphCropper {

        public static void Crop(Glyph glyph) {
            
            // Crop the top.
            while ((glyph.Subrectangle.Height > 1) && BitmapUtils.IsEntirelyAlpha(0, glyph.Bitmap, new Rectangle(glyph.Subrectangle.X, glyph.Subrectangle.Y, glyph.Subrectangle.Width, 1))) {
                glyph.Subrectangle.Y++;
                glyph.Subrectangle.Height--;
                glyph.OffsetY++;
            }

            // Crop the bottom.
            while ((glyph.Subrectangle.Height > 1) && BitmapUtils.IsEntirelyAlpha(0, glyph.Bitmap, new Rectangle(glyph.Subrectangle.X, glyph.Subrectangle.Bottom - 1, glyph.Subrectangle.Width, 1))) {
                glyph.Subrectangle.Height--;
            }

            // Crop the left.
            while ((glyph.Subrectangle.Width > 1) && BitmapUtils.IsEntirelyAlpha(0, glyph.Bitmap, new Rectangle(glyph.Subrectangle.X, glyph.Subrectangle.Y, 1, glyph.Subrectangle.Height))) {
                glyph.Subrectangle.X++;
                glyph.Subrectangle.Width--;
                glyph.OffsetX++;
            }

            // Crop the right.
            while ((glyph.Subrectangle.Width > 1) && BitmapUtils.IsEntirelyAlpha(0, glyph.Bitmap, new Rectangle(glyph.Subrectangle.Right - 1, glyph.Subrectangle.Y, 1, glyph.Subrectangle.Height))) {
                glyph.Subrectangle.Width--;
                glyph.AdvanceX++;
            }
        }
    }
}
