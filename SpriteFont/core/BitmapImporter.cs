using System;
using System.Linq;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;

namespace mage {
    
    // Extracts font glyphs from a specially marked 2D bitmap. Characters should be
    // arranged in a grid ordered from top left to bottom right. Monochrome characters
    // should use white for solid areas and black for transparent areas. To include
    // multicolored characters, add an alpha channel to the bitmap and use that to
    // control which parts of the character are solid. The spaces between characters
    // and around the edges of the grid should be filled with bright magenta (red=255,
    // green=0, blue=255). It doesn't matter if your grid includes lots of wasted space,
    // because the converter will rearrange characters, packing as tightly as possible.
    public sealed class BitmapImporter : IFontImporter {

        public BitmapImporter() {}

        // Properties hold the imported font data.
        public IEnumerable<Glyph> Glyphs { get; private set; }
        public float LineSpacing { get; private set; }

        // Imports the source font associated to the given command line options.
        public void Import(CommandLineOptions options) {
            if (options == null) {
                throw new NullReferenceException("The given command line options may not be equal to null.");
            }

            // Load the source bitmap.
            Bitmap bitmap;
            try {
                bitmap = new Bitmap(options.SourceFont);
            }
            catch {
                throw new Exception(string.Format("Unable to load '{0}'.", options.SourceFont));
            }

            // Convert to our desired pixel format.
            bitmap = BitmapUtils.ConvertToPixelFormat(bitmap, PixelFormat.Format32bppArgb);

            // What characters are included in this font?
            var characters = CharacterRegion.Flatten(options.CharacterRegions).ToArray();
            int characterIndex = 0;
            char currentCharacter = '\0';

            // Split the source image into a list of individual glyphs.
            var glyphList = new List<Glyph>();

            Glyphs = glyphList;
            LineSpacing = 0;


            foreach (Rectangle region in BitmapUtils.GetGlyphRegions(bitmap, IsMarkerColor)) {
                if (characterIndex < characters.Length) {
                    currentCharacter = characters[characterIndex];
                }
                ++currentCharacter;

                glyphList.Add(new Glyph(currentCharacter, bitmap, region));
                LineSpacing = Math.Max(LineSpacing, region.Height);
            }

            // If the bitmap doesn't already have an alpha channel, create one now.
            if (BitmapUtils.MatchesAlpha(255, bitmap)) {
                BitmapUtils.ConvertGreyToAlpha(bitmap);
            }
        }

        // Checks whether the given color is the magic magenta marker value.
        public static bool IsMarkerColor(Color color) {
            return color.ToArgb() == Color.Magenta.ToArgb();
        }
    }
}
