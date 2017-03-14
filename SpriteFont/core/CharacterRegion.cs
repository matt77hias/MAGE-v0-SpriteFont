using System;
using System.Linq;
using System.ComponentModel;
using System.Globalization;
using System.Collections.Generic;

namespace mage {
    
    // Describes a range of consecutive characters.
    [TypeConverter(typeof(CharacterRegionTypeConverter))]
    public class CharacterRegion {

        public CharacterRegion(char start, char end) {
            if (start > end) {
                throw new ArgumentException(string.Format("The start character '{0}' may not be larger than the end character '{1}'.", start, end));
            }

            Start = start;
            End = end;
        }

        public readonly char Start;
        public readonly char End;

        // Enumerates all individual characters within this character region.
        public IEnumerable<char> Characters {
            get {
                for (char c = Start; c <= End; ++c) {
                    yield return c;
                }
            }
        }

        // Flattens a list of character regions into a combined list of individual characters.
        public static IEnumerable<char> Flatten(IEnumerable<CharacterRegion> regions) {
            if (regions == null) {
                throw new NullReferenceException("The given regions may not be equal to null.");
            }
            if (regions.Any()) {
                return regions.SelectMany(region => region.Characters).Distinct();
            }
            return defaultRegion.Characters;
        }

        // The base ASCII character set.
        private static readonly CharacterRegion defaultRegion = new CharacterRegion(' ', '~');
    }

    // Custom type converter enables CommandLineParser to parse CharacterRegion command line options.
    public class CharacterRegionTypeConverter : TypeConverter {

        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType) {
            return sourceType == typeof(string);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value) {
            string source = value as string;
            if (string.IsNullOrEmpty(source)) {
                throw new ArgumentException("The given string value may not be null or empty.");
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
                    return new CharacterRegion(split[0], split[0]);
                case 2:
                    return new CharacterRegion(split[0], split[1]);
                default:
                    throw new ArgumentException(string.Format("Unsupported string format: '{0}'.", source));
            }
        }

        protected static char ConvertCharacter(string value) {
            if (value.Length == 1) {
                return value[0];
            }
            
            // Otherwise it must be an integer (eg. "32" or "0x20").
            return (char)(int)intConverter.ConvertFromInvariantString(value);
        }

        protected static readonly TypeConverter intConverter = TypeDescriptor.GetConverter(typeof(int));
    }
}
