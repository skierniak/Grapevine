using System;
using Grapevine.Common;
using Xunit;
using Shouldly;

namespace Grapevine.Tests.Unit.Common
{
    public class PatternParserFacts
    {
        public class GeneratePatternKeys
        {
            [Fact]
            public void ReturnsEmptyListWhenNoKeys()
            {
                const string pathinfo = "/pattern/has/no/keys";
                var result = PatternParser.GeneratePatternKeys(pathinfo);

                result.ShouldSatisfyAllConditions
                (
                    () => result.ShouldNotBeNull(),
                    () => result.ShouldBeEmpty()
                );
            }

            [Fact]
            public void ReturnsListOfOneKey()
            {
                const string pathinfo = "/[key]";
                var result = PatternParser.GeneratePatternKeys(pathinfo);

                result.ShouldSatisfyAllConditions
                (
                    () => result.ShouldNotBeNull(),
                    () => result.Count.ShouldBe(1),
                    () => result[0].ShouldBe("key")
                );
            }

            [Fact]
            public void ReturnsListOfOneKeyWhenKeyIsInFirstPosition()
            {
                const string pathinfo = "/[pattern]/has/one/key";
                var result = PatternParser.GeneratePatternKeys(pathinfo);

                result.ShouldSatisfyAllConditions
                (
                    () => result.ShouldNotBeNull(),
                    () => result.Count.ShouldBe(1),
                    () => result[0].ShouldBe("pattern")
                );
            }

            [Fact]
            public void ReturnsListOfOneKeyWhenKeyIsInLastPosition()
            {
                const string pathinfo = "/pattern/has/one/[key]";
                var result = PatternParser.GeneratePatternKeys(pathinfo);

                result.ShouldSatisfyAllConditions
                (
                    () => result.ShouldNotBeNull(),
                    () => result.Count.ShouldBe(1),
                    () => result[0].ShouldBe("key")
                );
            }

            [Fact]
            public void ReturnsListOfOneKeyWhenKeyIsInMiddle()
            {
                const string pathinfo = "/pattern/has/[one]/key";
                var result = PatternParser.GeneratePatternKeys(pathinfo);

                result.ShouldSatisfyAllConditions
                (
                    () => result.ShouldNotBeNull(),
                    () => result.Count.ShouldBe(1),
                    () => result[0].ShouldBe("one")
                );
            }

            [Fact]
            public void ReturnsListOfMultipleKeys()
            {
                const string pathinfo = "/pattern/[has]/[two]/keys";
                var result = PatternParser.GeneratePatternKeys(pathinfo);

                result.ShouldSatisfyAllConditions
                (
                    () => result.ShouldNotBeNull(),
                    () => result.Count.ShouldBe(2),
                    () => result[0].ShouldBe("has"),
                    () => result[1].ShouldBe("two")
                );
            }

            [Fact]
            public void ReturnsListOfMultipleNonConsecutiveKeys()
            {
                const string pathinfo = "/pattern/[has]/two/[keys]";
                var result = PatternParser.GeneratePatternKeys(pathinfo);

                result.ShouldSatisfyAllConditions
                (
                    () => result.ShouldNotBeNull(),
                    () => result.Count.ShouldBe(2),
                    () => result[0].ShouldBe("has"),
                    () => result[1].ShouldBe("keys")
                );
            }

            [Fact]
            public void ThrowsExceptionWhenKeysAreDuplicated()
            {
                const string pathinfo = "/pattern/[has]/[duplicated]/[has]/keys";
                Should.Throw<ArgumentException>(() => PatternParser.GeneratePatternKeys(pathinfo));
            }

            [Fact]
            public void ExceptionMessageShouldContainPattern()
            {
                const string pathinfo = "/pattern/[has]/[duplicated]/[has]/keys";

                try
                {
                    PatternParser.GeneratePatternKeys(pathinfo);
                    throw new Exception("Did not throw expected exception");
                }
                catch (Exception e)
                {
                    e.Message.EndsWith(pathinfo).ShouldBeTrue();
                }
            }
        }

        public class GenerateRegEx
        {
            [Theory]
            [InlineData(null)]
            [InlineData("")]
            [InlineData(" ")]
            public void ReturnsDefaultRegExWhenPathInfoIsNullOrEmpty(string val)
            {
                var regex = PatternParser.GenerateRegEx(val);
                regex.ToString().ShouldBe(@"^.*$");
            }

            [Theory]
            [InlineData("/path/without/params")]
            [InlineData("/path/without/params$")]
            public void CreatesRegExWithoutPathInfoParams(string variation)
            {
                var regex = PatternParser.GenerateRegEx(variation);
                regex.ToString().ShouldBe(@"^/path/without/params$");
            }

            [Theory]
            [InlineData("/resource/[id]", @"^/resource/([^/]+)$")]
            [InlineData("/resource/[id]/action", @"^/resource/([^/]+)/action$")]
            public void CreatesRegExWithPathInfoParams(string pattern, string result)
            {
                PatternParser.GenerateRegEx(pattern).ToString().ShouldBe(result);
            }

            [Fact]
            public void CreatesRegExFromRegEx()
            {
                const string pattern = @"^/is/[already]/regex";
                var regex = PatternParser.GenerateRegEx(pattern);
                regex.ToString().ShouldBe(pattern);
            }

            [Fact]
            public void DoesNotMatchExpandedPathInfo()
            {
                const string pattern = "/controllers/[id]";
                const string pathinfo = "/controllers/2/request";

                var regex = PatternParser.GenerateRegEx(pattern);

                regex.IsMatch(pathinfo).ShouldBeFalse();
            }
        }
    }
}
