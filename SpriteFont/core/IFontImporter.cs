using System.Collections.Generic;

namespace mage {
    
    // Importer interface to support multiple source font formats.
    public interface IFontImporter {

        void Import(CommandLineOptions options);

        IEnumerable<Glyph> Glyphs { get; }
        float LineSpacing { get; }
    }
}
