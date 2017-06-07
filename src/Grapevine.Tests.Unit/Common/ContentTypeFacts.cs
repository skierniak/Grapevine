using Grapevine.Common;
using Shouldly;
using Xunit;

namespace Grapevine.Tests.Unit.Common
{
    public class ContentTypeFacts
    {
        public class ExtensionMethods
        {
            public class IsText
            {
                [Fact]
                public void ReturnsTrue()
                {
                    ContentType.TXT.IsText().ShouldBeTrue();
                }

                [Fact]
                public void ReturnsFalse()
                {
                    ContentType.JPG.IsText().ShouldBeFalse();
                }
            }

            public class IsBinary
            {
                [Fact]
                public void ReturnsTrue()
                {
                    ContentType.TXT.IsBinary().ShouldBeFalse();
                }

                [Fact]
                public void ReturnsFalse()
                {
                    ContentType.JPG.IsBinary().ShouldBeTrue();
                }
            }

            public class Value
            {
                [Fact]
                public void ReturnsValue()
                {
                    ContentType.JPG.Value().ShouldBe("image/jpeg");
                }
            }

            public class FromString
            {
                [Fact]
                public void ReturnsDefaultWhenParameterIsNullOrEmpty()
                {
                    ContentTypes.FromString(null).Equals(ContentType.Text).ShouldBeTrue();
                    ContentTypes.FromString(string.Empty).Equals(ContentType.Text).ShouldBeTrue();
                }

                [Fact]
                public void ReturnsDefaultWhenParameterIsNotInEnum()
                {
                    ContentTypes.FromString("does not exist").Equals(ContentType.Text).ShouldBeTrue();
                }

                [Fact]
                public void ReturnsContentTypeFromString()
                {
                    ContentTypes.FromString("application/json").Equals(ContentType.JSON).ShouldBeTrue();
                }

                [Fact]
                public void ReturnsContentTypeFromStringWithParameters()
                {
                    ContentTypes.FromString("text/html; charset=UTF-8").Equals(ContentType.HTML).ShouldBeTrue();
                }

                [Fact]
                public void ReturnsContentTypeFromStringWithMultipleValues()
                {
                    ContentTypes.FromString("text/html,text/plain").Equals(ContentType.HTML).ShouldBeTrue();
                }

                [Fact]
                public void ReturnsContentTypeFromStringWithMultipleValuesWithParameters()
                {
                    ContentTypes.FromString("text/html,text/plain; charset=windows-1251").Equals(ContentType.HTML).ShouldBeTrue();
                    ContentTypes.FromString("text/html; charset=UTF-8,text/plain").Equals(ContentType.HTML).ShouldBeTrue();
                }
            }

            public class FromExtension
            {
                private const ContentType Default = 0;

                [Fact]
                public void ReturnsDefaultWhenExtensionIsNull()
                {
                    ContentTypes.FromExtension(null).ShouldBe(Default);
                }

                [Fact]
                public void ReturnsDefaultWhenExtensionIsEmpty()
                {
                    ContentTypes.FromExtension("filename").ShouldBe(Default);
                }

                [Fact]
                public void ReturnsDefaultWhenExtensionIsNotInCache()
                {
                    ContentTypes.FromExtension("filename.filextension").ShouldBe(Default);
                }

                [Fact]
                public void ReturnsContentTypeFromExtension()
                {
                    ContentTypes.FromExtension("filename.acu").ShouldBe(ContentType.ACU);
                }
            }
        }

        public class InternalMethods
        {
            public class IsCached
            {
                [Fact]
                public void ReturnsTrueWhenValueIsCached()
                {
                    ContentTypes.FromString("text/html,text/plain").Equals(ContentType.HTML).ShouldBeTrue();
                    ContentTypes.IsCached("text/html,text/plain").ShouldBeTrue();
                }

                [Fact]
                public void ReturnsFalseWhenValueIsNotCached()
                {
                    const string contenttype = "text/html; charset=UTF-21";
                    ContentTypes.IsCached(contenttype).ShouldBeFalse();
                    ContentTypes.FromString(contenttype).Equals(ContentType.HTML).ShouldBeTrue();
                    ContentTypes.IsCached(contenttype).ShouldBeTrue();
                    ContentTypes.FromString(contenttype).Equals(ContentType.HTML).ShouldBeTrue();
                    ContentTypes.IsCached(contenttype).ShouldBeTrue();
                }

                [Fact]
                public void ReturnsFalseWhenValueShouldNotBeCached()
                {
                    const string contenttype = "text/html";
                    ContentTypes.IsCached(contenttype).ShouldBeFalse();
                    ContentTypes.FromString(contenttype).Equals(ContentType.HTML).ShouldBeTrue();
                    ContentTypes.IsCached(contenttype).ShouldBeFalse();
                }
            }
        }
    }
}
