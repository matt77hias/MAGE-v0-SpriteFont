using System;
using System.Drawing;

namespace mage {
   
    // A class of single characters within a font.
    public sealed class Glyph {
        
        public Glyph(char character, Bitmap bitmap, Rectangle? region = null) {
            if (bitmap == null) {
                throw new NullReferenceException("The given bitmap may not be equal to null.");
            }

            Character = character;
            Bitmap = bitmap;
            Region = region.GetValueOrDefault(BitmapUtils.GetRegion(bitmap));
        }

        // Glyph character.
        public char Character { get; set; }

        // Glyph image data.
        public Bitmap Bitmap { get; set; }
        public Rectangle Region;

        // Glyph layout information.
        public float OffsetX { get; set; }
        public float OffsetY { get; set; }
        public float AdvanceX { get; set; }
    }
}
