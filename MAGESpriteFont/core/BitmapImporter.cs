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
    // and around the edges of the grid should be filled with bright pink (red=255,
    // green=0, blue=255). It doesn't matter if your grid includes lots of wasted space,
    // because the converter will rearrange characters, packing as tightly as possible.
    public class BitmapImporter : IFontImporter {

        public IEnumerable< Glyph > Glyphs { get; private set; }
        public float LineSpacing { get; private set; }

        public void Import(CommandLineOptions options) {
            // Imports the bitmap.
            Bitmap bitmap;
            try {
                bitmap = new Bitmap(options.SourceFont);
            }
            catch {
                throw new Exception(string.Format("Unable to load '{0}'.", options.SourceFont));
            }

            // Convert to the desired pixel format.
            bitmap = BitmapUtils.ChangePixelFormat(bitmap, PixelFormat.Format32bppArgb);

            // What characters are included in this font?
            var characters = CharacterRegion.Flatten(options.CharacterRegions).ToArray();
            int character_index = 0;
            char current_character = '\0';

            // Split the source image into a list of individual glyphs.
            var glyphList = new List< Glyph >();

            Glyphs = glyphList;
            LineSpacing = 0;

            foreach (Rectangle rectangle in FindGlyphs(bitmap))
            {
                if (character_index < characters.Length)
                    current_character = characters[character_index++];
                else
                    current_character++;

                glyphList.Add(new Glyph(current_character, bitmap, rectangle));

                LineSpacing = Math.Max(LineSpacing, rectangle.Height);
            }

            // If the bitmap doesn't already have an alpha channel, create one now.
            if (BitmapUtils.IsEntirelyAlpha(255, bitmap))
            {
                BitmapUtils.ConvertGreyToAlpha(bitmap);
            }
        }


        // Searches a 2D bitmap for characters that are surrounded by a marker pink color.
        static IEnumerable<Rectangle> FindGlyphs(Bitmap bitmap)
        {
            using (var bitmapData = new BitmapUtils.PixelAccessor(bitmap, ImageLockMode.ReadOnly))
            {
                for (int y = 1; y < bitmap.Height; y++)
                {
                    for (int x = 1; x < bitmap.Width; x++)
                    {
                        // Look for the top left corner of a character (a pixel that is not pink, but was pink immediately to the left and above it)
                        if (!IsMarkerColor(bitmapData[x, y]) &&
                             IsMarkerColor(bitmapData[x - 1, y]) &&
                             IsMarkerColor(bitmapData[x, y - 1]))
                        {
                            // Measure the size of this character.
                            int w = 1, h = 1;

                            while ((x + w < bitmap.Width) && !IsMarkerColor(bitmapData[x + w, y]))
                            {
                                w++;
                            }

                            while ((y + h < bitmap.Height) && !IsMarkerColor(bitmapData[x, y + h]))
                            {
                                h++;
                            }

                            yield return new Rectangle(x, y, w, h);
                        }
                    }
                }
            }
        }


        // Checks whether a color is the magic magenta marker value.
        static bool IsMarkerColor(Color color)
        {
            return color.ToArgb() == Color.Magenta.ToArgb();
        }
    }
}