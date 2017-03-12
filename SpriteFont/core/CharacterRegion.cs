using System;
using System.Linq;
using System.ComponentModel;
using System.Globalization;
using System.Collections.Generic;

namespace mage {
    
    // Describes a range of consecutive characters that should be included in the font.
    [TypeConverter(typeof(CharacterRegionTypeConverter))]
    public class CharacterRegion {

        private char Start;
        private char End;

        public CharacterRegion(char start, char end) {
            if (start > end) {
                throw new ArgumentException();
            }

            this.Start = start;
            this.End = end;
        }

        public IEnumerable< Char > Characters {
            get {
                for (char c = Start; c <= End; c++) {
                    yield return c;
                }
            }
        }

        // The base ASCII character set.
        public static readonly CharacterRegion DEFAULT_CHARACTER_REGION = new CharacterRegion(' ', '~');

        public static IEnumerable< Char > Flatten(IEnumerable< CharacterRegion > regions) {
            if (regions.Any()) {
                return regions.SelectMany(region => region.Characters).Distinct();
            }
            else {
                return DEFAULT_CHARACTER_REGION.Characters;
            }
        }
    }

    // Custom type converter enables CommandLineParser to parse CharacterRegion command line options.
    public class CharacterRegionTypeConverter : TypeConverter {

        public override bool CanConvertFrom(ITypeDescriptorContext context, Type source_type) {
            return source_type == typeof(string);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value) {
            string source = value as string;
            if (string.IsNullOrEmpty(source)) {
                throw new ArgumentException();
            }

            // Supported input formats:
            //  A
            //  A-Z
            //  32-127
            //  0x20-0x7F

            char[] split = source.Split('-')
                                 .Select(ConvertCharacter)
                                 .ToArray();

            switch (split.Length) {
                case 1:
                    // Only a single character (eg. "a").
                    return new CharacterRegion(split[0], split[0]);
                case 2:
                    // Range of characters (eg. "a-z").
                    return new CharacterRegion(split[0], split[1]);
                default:
                    throw new ArgumentException();
            }
        }

        protected static readonly TypeConverter INTEGER_CONVERTER = TypeDescriptor.GetConverter(typeof(int));

        protected static char ConvertCharacter(string value) {
            if (value.Length == 1) {
                return value[0];
            }
            else {
                // An integer (eg. "32" or "0x20").
                return (char)(int)INTEGER_CONVERTER.ConvertFromInvariantString(value);
            }
        }
    }
}
