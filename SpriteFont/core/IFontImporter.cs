using System.Collections.Generic;

namespace mage {
    
    // Importer interface to support multiple source font formats.
    public interface IFontImporter {

        IEnumerable<Glyph> Glyphs { get; }
        float LineSpacing { get; }

        // Imports the source font associated to the given command line options.
        void Import(CommandLineOptions options);
    }
}
