using System;
using Grapevine.Common;
using Shouldly;
using Xunit;

namespace Grapevine.Tests.Unit.Common
{
    public class EnumExtensionFacts
    {
        public class GetEnumAttribute
        {
            [Fact]
            public void ReturnsDefaultWhenNoAttributes()
            {
                var result = HttpMethod.ALL.GetEnumAttribute<ContentTypeMetadata>();
                result.ShouldNotBeNull();
                result.IsBinary.ShouldBeFalse();
                result.IsText.ShouldBeTrue();
                result.Value.ShouldBe("text/plain");
            }

            [Fact]
            public void ReturnsValueWhenFound()
            {
                var result = ContentType.PNG.GetEnumAttribute<ContentTypeMetadata>();
                result.ShouldNotBeNull();
                result.IsBinary.ShouldBeTrue();
                result.IsText.ShouldBeFalse();
                result.Value.ShouldBe("image/png");
            }
        }

        public class GetEnumAttributes
        {
            [Fact]
            public void ReturnsListWithMultipleAttributes()
            {
                var list = Test.TestEnum1.GetEnumAttributes<TestMetadata>();
                list.Length.ShouldBe(2);
            }

            [Fact]
            public void ReturnsListWithOnlyOneAttribute()
            {
                var list = Test.TestEnum2.GetEnumAttributes<TestMetadata>();
                list.Length.ShouldBe(1);
            }

            [Fact]
            public void ReturnsEmptyAttributeList()
            {
                var list = Test.TestEnum3.GetEnumAttributes<TestMetadata>();
                list.Length.ShouldBe(0);
            }
        }

        public class FromString
        {
            [Fact]
            public void ThrowsExceptionOnNonEnum()
            {
                Should.Throw<ArgumentException>(() => EnumExtensions.FromString<NotAnEnum>("Thingy"));
            }

            [Fact]
            public void ReturnsDefaultWhenValueNotFound()
            {
                EnumExtensions.FromString<HttpMethod>("NotAMethod").ShouldBe((HttpMethod) 0);
            }

            [Fact]
            public void ReturnsEnumWhenValueFound()
            {
                EnumExtensions.FromString<HttpMethod>("Get").ShouldBe(HttpMethod.GET);
            }
        }
    }

    public struct NotAnEnum : IComparable, IFormattable, IConvertible
    {
        public int CompareTo(object obj)
        {
            throw new NotImplementedException();
        }

        public string ToString(string format, IFormatProvider formatProvider)
        {
            throw new NotImplementedException();
        }

        public TypeCode GetTypeCode()
        {
            throw new NotImplementedException();
        }

        public bool ToBoolean(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public char ToChar(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public sbyte ToSByte(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public byte ToByte(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public short ToInt16(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public ushort ToUInt16(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public int ToInt32(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public uint ToUInt32(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public long ToInt64(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public ulong ToUInt64(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public float ToSingle(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public double ToDouble(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public decimal ToDecimal(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public DateTime ToDateTime(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public string ToString(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public object ToType(Type conversionType, IFormatProvider provider)
        {
            throw new NotImplementedException();
        }
    }

    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
    internal class TestMetadata : Attribute
    {
        public string Value { get; set; }
    }

    internal enum Test
    {
        [TestMetadata(Value = "Value1")]
        [TestMetadata(Value = "Value2")]
        TestEnum1,
        [TestMetadata(Value = "Value3")]
        TestEnum2,
        TestEnum3
    }
}
