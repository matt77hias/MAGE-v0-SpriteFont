using System.Drawing;

namespace mage {
    
    public sealed class Glyph {

        private static Rectangle CreateDefaultRegion(Bitmap bitmap) {
            return new Rectangle(0, 0, bitmap.Width, bitmap.Height);
        }

        public Glyph(char character, Bitmap bitmap, Rectangle? subrect = null) {
            this.Character = character;
            this.Bitmap = bitmap;
            this.Subrectangle = subrect.GetValueOrDefault(CreateDefaultRegion(bitmap));
        }

        // Glyph Unicode character.
        public char Character { get; set; }

        // Glyph image data.
        public Bitmap Bitmap;
        public Rectangle Subrectangle;

        // Glyph layout data.
        public float OffsetX { get; set; }
        public float OffsetY { get; set; }
        public float AdvanceX { get; set; }
    }
}
