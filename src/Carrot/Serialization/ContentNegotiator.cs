using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Carrot.Serialization
{
    public interface IContentNegotiator
    {
        SortedSet<ContentNegotiator.MediaTypeHeader> Negotiate(String contentType);
    }

    public class ContentNegotiator : IContentNegotiator
    {
        internal ContentNegotiator()
        {
        }

        public SortedSet<MediaTypeHeader> Negotiate(String contentType)
        {
            Guard.AgainstNull(contentType, nameof(contentType));

            return new SortedSet<MediaTypeHeader>(contentType
                    .Split(new[] {','}, StringSplitOptions.RemoveEmptyEntries)
                    .Select(_ => MediaTypeHeader.Parse(_.Trim()))
                    .OrderByDescending(_ => _.Quality),
                MediaTypeHeader.MediaTypeHeaderQualityComparer.Instance);
        }

        public class MediaTypeHeader : IEquatable<MediaTypeHeader>
        {
            public readonly MediaType MediaType;
            public readonly Single Quality;

            private const Single DefaultQuality = 1.0f;

            internal MediaTypeHeader(MediaType type, Single quality)
            {
                MediaType = type;
                Quality = quality;
            }

            public static MediaTypeHeader Parse(String source)
            {
                var type = default(MediaType);
                var quality = DefaultQuality;

                foreach (var s in source.Split(new[] {';'}, StringSplitOptions.RemoveEmptyEntries)
                                              .Select(_ => _.Trim()))
                    if (s.StartsWith("q", StringComparison.Ordinal))
                        quality = Single.Parse(s[(s.IndexOf('=') + 1)..].TrimStart(), CultureInfo.InvariantCulture);
                    else if (s.IndexOf('=') == -1)
                        type = MediaType.Parse(s);

                return new MediaTypeHeader(type, quality);
            }

            public static Boolean operator ==(MediaTypeHeader left, MediaTypeHeader right) => Equals(left, right);

            public static Boolean operator !=(MediaTypeHeader left, MediaTypeHeader right) => !Equals(left, right);

            public Boolean Equals(MediaTypeHeader other)
            => other is not null && (ReferenceEquals(this, other) || MediaType.Equals(other.MediaType));

            public override Boolean Equals(Object obj)
            => obj is not null &&
               (ReferenceEquals(this, obj) || obj is MediaTypeHeader other && Equals(other));

            public override Int32 GetHashCode() => MediaType.GetHashCode();

            public override String ToString() => $"{MediaType};q={Quality.ToString(CultureInfo.InvariantCulture)}";

            internal class MediaTypeHeaderQualityComparer : IComparer<MediaTypeHeader>
            {
                internal static readonly MediaTypeHeaderQualityComparer Instance = new MediaTypeHeaderQualityComparer();

                private MediaTypeHeaderQualityComparer()
                {
                }

                public Int32 Compare(MediaTypeHeader x, MediaTypeHeader y) => x.Quality.CompareTo(y.Quality) * -1;
            }
        }

        public abstract class RegistrationTree : IEquatable<RegistrationTree>
        {
            public readonly String Name;
            public readonly String Suffix;
            public readonly String Prefix;

            internal RegistrationTree(String name, String suffix, String prefix)
            {
                Name = name;
                Suffix = suffix;
                Prefix = prefix;
            }

            public static Boolean operator ==(RegistrationTree left, RegistrationTree right) => Equals(left, right);

            public static Boolean operator !=(RegistrationTree left, RegistrationTree right) => !Equals(left, right);

            public Boolean Equals(RegistrationTree other)
            => other is not null &&
               (ReferenceEquals(this, other) ||
                String.Equals(Prefix, other.Prefix) &&
                String.Equals(Name, other.Name) &&
                String.Equals(Suffix, other.Suffix));

            public override Boolean Equals(Object obj)
            => obj is not null &&
               (ReferenceEquals(this, obj) || obj is RegistrationTree other && Equals(other));

            public override Int32 GetHashCode() => HashCode.Combine(Name, Prefix, Suffix);


            public override String ToString()
            => Suffix == null
                ? $"{Prefix}{Name}"
                : $"{Prefix}{Name}+{Suffix}";

            internal static RegistrationTree Parse(String source)
            => source.StartsWith("vnd.", StringComparison.Ordinal)
                ? new VendorTree(ParseName(source, "vnd."), ParseSuffix(source))
                : (RegistrationTree) new StandardTree(ParseName(source), ParseSuffix(source));

            protected static String ParseSuffix(String source)
            => source.IndexOf("+", StringComparison.Ordinal) == -1
                ? null
                : source[(source.IndexOf("+", StringComparison.Ordinal) + 1)..];

            protected static String ParseName(String source, String key = null)
            {
                var index = key == null ? 0 : source.IndexOf(key, StringComparison.Ordinal);

                if (index == -1)
                    return null;

                var start = index + (key?.Length ?? 0);
                var end = source.IndexOf("+", StringComparison.Ordinal);

                return end == -1 ? source[start..] : source[start..end];
            }
        }

        public class VendorTree : RegistrationTree
        {
            private const String PrefixKey = "vnd.";

            internal VendorTree(String name, String suffix)
                : base(name, suffix, PrefixKey)
            {
            }
        }

        public class StandardTree : RegistrationTree
        {
            private const String PrefixKey = "";

            internal StandardTree(String name, String suffix)
                : base(name, suffix, PrefixKey)
            {
            }
        }

        public class MediaType : IEquatable<MediaType>
        {
            public readonly String Type;
            public readonly RegistrationTree RegistrationTree;

            internal MediaType(String type, RegistrationTree registrationTree = null)
            {
                Type = type;
                RegistrationTree = registrationTree;
            }

            public static Boolean operator ==(MediaType left, MediaType right) => Equals(left, right);

            public static Boolean operator !=(MediaType left, MediaType right) => !Equals(left, right);

            public static implicit operator MediaType(String value) => Parse(value);

            public Boolean Equals(MediaType other)
            => other is not null && (ReferenceEquals(this, other) ||
                                         String.Equals(Type, other.Type) &&
                                         RegistrationTree.Equals(other.RegistrationTree));

            public override Boolean Equals(Object obj)
            => obj is not null && (ReferenceEquals(this, obj) || obj is MediaType other && Equals(other));

            public override Int32 GetHashCode() => HashCode.Combine(Type, RegistrationTree);

            public override String ToString()
            => RegistrationTree == null
                ? Type
                : $"{Type}/{RegistrationTree}";

            internal static MediaType Parse(String source)
            {
                Guard.AgainstNull(source, nameof(source));

                var strings = source.Split(new[] {'/'}, StringSplitOptions.RemoveEmptyEntries)
                                           .Select(_ => _.Trim())
                                           .ToArray();

                return strings.Length <= 1
                    ? new MediaType(source)
                    : new MediaType(strings[0], RegistrationTree.Parse(strings[1].Trim()));
            }
        }
    }
}