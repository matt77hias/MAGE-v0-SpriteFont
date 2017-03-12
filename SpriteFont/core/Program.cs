//-----------------------------------------------------------------------------
// Includes
//-----------------------------------------------------------------------------

using System;
using System.IO;
using System.Linq;
using System.Drawing;

//-----------------------------------------------------------------------------
// Declarations and Definitions
//-----------------------------------------------------------------------------
namespace mage {

    public class Program {

        public static int Main(string[] args) {
           
            // Parse the commandline options.
            var options = new CommandLineOptions();
            var parser = new CommandLineParser(options);

            if (!parser.ParseCommandLine(args)) {
                return 1;
            }

            try {
                // Convert the font.
                MakeSpriteFont(options);
                return 0;
            }
            catch (Exception e) {
                Console.WriteLine();
                Console.Error.WriteLine("Error: {0}", e.Message);
                return 1;
            }
        }

        static void MakeSpriteFont(CommandLineOptions options) {
            //-----------------------------------------------------------------
            // Import the glyph data.
            //-----------------------------------------------------------------
            Console.WriteLine("Importing {0}", options.SourceFont);

            float line_spacing;
            Glyph[] glyphs = ImportFont(options, out line_spacing);

            Console.WriteLine("Imported {0} glyphs", glyphs.Length);

            //-----------------------------------------------------------------
            // Optimize the glyph data.
            //-----------------------------------------------------------------
            Console.WriteLine("Cropping glyph borders");

            foreach (Glyph glyph in glyphs) {
                GlyphCropper.Crop(glyph);
            }

            //-----------------------------------------------------------------
            // Pack the glyph data.
            //-----------------------------------------------------------------
            Console.WriteLine("Packing glyphs into sprite sheet");

            Bitmap bitmap;
            if (options.FastPack) {
                bitmap = GlyphPacker.ArrangeGlyphsFast(glyphs);
            }
            else {
                bitmap = GlyphPacker.ArrangeGlyphs(glyphs);
            }

            //-----------------------------------------------------------------
            // Warnings
            //-----------------------------------------------------------------
            CheckTextureSize(options, bitmap);

            //-----------------------------------------------------------------
            // Adjusting
            //-----------------------------------------------------------------
            line_spacing += options.LineSpacing;
            foreach (Glyph glyph in glyphs) {
                glyph.AdvanceX += options.CharacterSpacing;
            }

            // Automatically detect whether this is a monochromatic or color font?
            if (options.TextureFormat == TextureFormat.Auto) {
                bool is_mono = BitmapUtils.IsEntirelyRGB(Color.White, bitmap);
                options.TextureFormat = is_mono ? TextureFormat.CompressedMono :
                                                  TextureFormat.Rgba32;
            }

            // Convert to premultiplied alpha format.
            if (!options.NoPremultiply) {
                Console.WriteLine("Premultiplying alpha");

                BitmapUtils.PremultiplyAlpha(bitmap);
            }

            // Save output files.
            if (!string.IsNullOrEmpty(options.DebugOutputSpriteSheet)) {
                Console.WriteLine("Saving debug output spritesheet {0}", options.DebugOutputSpriteSheet);

                bitmap.Save(options.DebugOutputSpriteSheet);
            }

            Console.WriteLine("Writing {0} ({1} format)", options.OutputFile, options.TextureFormat);

            SpriteFontWriter.WriteSpriteFont(options, glyphs, line_spacing, bitmap);
        }

        static Glyph[] ImportFont(CommandLineOptions options, out float line_spacing) {
            string file_extension = Path.GetExtension(options.SourceFont).ToLowerInvariant();
            string[] bitmap_file_extensions = { ".bmp", ".png", ".gif" };

            // Create the font importer.
            IFontImporter importer;
            if (bitmap_file_extensions.Contains(file_extension)) {
                importer = new BitmapImporter();
            }
            else {
                importer = new TrueTypeImporter();
            }

            // Import the font data.
            importer.Import(options);
            line_spacing = importer.LineSpacing;
            var glyphs = importer.Glyphs
                                 .OrderBy(glyph => glyph.Character)
                                 .ToArray();

            // Validate the font data.
            if (glyphs.Length == 0) {
                throw new Exception("Font does not contain any glyphs.");
            }
            if ((options.DefaultCharacter != 0) && !glyphs.Any(glyph => glyph.Character == options.DefaultCharacter)) {
                throw new Exception("The specified default character is not part of this font.");
            }

            return glyphs;
        }

        static void CheckTextureSize(CommandLineOptions options, Bitmap bitmap) {
            if (bitmap.Width > 16384 || bitmap.Height > 16384) {
                Console.WriteLine("WARNING: Resulting texture is too large for all known Feature Levels (9.1 - 12.1)");
            }
            else if (bitmap.Width > 8192 || bitmap.Height > 8192) {
                if (options.FeatureLevel < FeatureLevel.FL11_0) {
                    Console.WriteLine("WARNING: Resulting texture requires a Feature Level 11.0 or later device.");
                }
            }
            else if (bitmap.Width > 4096 || bitmap.Height > 4096) {
                if (options.FeatureLevel < FeatureLevel.FL10_0) {
                    Console.WriteLine("WARNING: Resulting texture requires a Feature Level 10.0 or later device.");
                }
            }
            else if (bitmap.Width > 2048 || bitmap.Height > 2048) {
                if (options.FeatureLevel < FeatureLevel.FL9_3) {
                    Console.WriteLine("WARNING: Resulting texture requires a Feature Level 9.3 or later device.");
                }
            }
        }
    }
}
