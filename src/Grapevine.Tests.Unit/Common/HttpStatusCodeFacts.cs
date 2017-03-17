using Grapevine.Common;
using Shouldly;
using Xunit;

namespace Grapevine.Tests.Unit.Common
{
    public class HttpStatusCodeFacts
    {
        public class ConvertToString
        {
            [Fact]
            public void ConvertsCamelCaseToWords()
            {
                HttpStatusCode.NonAuthoritativeInformation.ConvertToString().ShouldBe("Non Authoritative Information");
            }

            [Fact]
            public void ConvertsSingleWordToWord()
            {
                HttpStatusCode.Ok.ConvertToString().ShouldBe("Ok");
            }
        }

        public class ToInteger
        {
            [Fact]
            public void ConvertsToInteger()
            {
                HttpStatusCode.Ok.ToInteger().ShouldBe(200);
                HttpStatusCode.NotFound.ToInteger().ShouldBe(404);
            }
        }
    }
}
