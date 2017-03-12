using System.Collections.Generic;

namespace mage {
   
    public interface IFontImporter {

        void Import(CommandLineOptions options);

        IEnumerable< Glyph > Glyphs { get; }
        float LineSpacing { get; }
    }
}
