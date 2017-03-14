using System.Collections.Generic;
using System.Drawing;

namespace mage {

    // Available output texture formats.
    public enum TextureFormat {
        Auto,
        Rgba32,
        Bgra4444,
        CompressedMono,
    }

    // Feature levels
    public enum FeatureLevel {
        FL9_1,
        FL9_2,
        FL9_3,
        FL10_0,
        FL10_1,
        FL11_0,
        FL11_1,
        FL12_0,
        FL12_1,
    }

    public sealed class CommandLineOptions {
        
        // Input can be either a system (TrueType) font or a specially marked bitmap file.
        [CommandLineParser.Required]
        public string SourceFont;

        // Output spritefont binary.
        [CommandLineParser.Required]
        public string OutputFile;

        // Characters to include in the font (e.g. "/CharacterRegion:0x20-0x7F /CharacterRegion:0x123")
        [CommandLineParser.Name("CharacterRegion")]
        public readonly List<CharacterRegion> CharacterRegions = new List<CharacterRegion>();

        // Fallback character used when asked to render a codepoint that is not
        // included in the font. If zero, missing characters throw exceptions.
        public readonly int DefaultCharacter = 0;

        // Size and style for TrueType fonts (ignored when converting a bitmap font).
        public float FontSize = 23;
        public FontStyle FontStyle = FontStyle.Regular;

        // Spacing overrides. Zero is default spacing, negative closer together, positive further apart.
        public float LineSpacing = 0;
        public float CharacterSpacing = 0;

        // Flag indicating whether sharp antialiasing mode should be used
        // for TrueType font rasterization (versus smooth antialiasing mode).
        public bool Sharp = false;

        // What format should the output texture be?
        public TextureFormat TextureFormat = TextureFormat.Auto;

        // Controls texture-size based warnings
        public FeatureLevel FeatureLevel = FeatureLevel.FL9_1;

        // Flag indicating whether interpolative alpha should be used
        // (versus premultiplied alpha).
        public bool NoPremultiply = false;

        // Flag indicating whether fast packing should be used.
        // For large fonts, the default tightest packing is too slow.
        public bool FastPack = false;

        // Dumps the generated sprite texture to a bitmap file (useful for debugging).
        public string DebugOutputSpriteSheet = null;
    }
}
