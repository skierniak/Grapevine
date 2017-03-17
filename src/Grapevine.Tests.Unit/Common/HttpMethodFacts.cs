using Grapevine.Common;
using Shouldly;
using Xunit;

namespace Grapevine.Tests.Unit.Common
{
    public class HttpMethodFacts
    {
        public class IsEquivalentTo
        {
            [Fact]
            public void AllIsEquivalentToAll()
            {
                HttpMethod.ALL.IsEquivalentTo(HttpMethod.ALL).ShouldBeTrue();
            }

            [Fact]
            public void AllIsEquivalentToEverything()
            {
                HttpMethod.ALL.IsEquivalentTo(HttpMethod.POST).ShouldBeTrue();
                HttpMethod.ALL.IsEquivalentTo(HttpMethod.GET).ShouldBeTrue();
            }

            [Fact]
            public void EverythingIsEquivalentToAll()
            {
                HttpMethod.POST.IsEquivalentTo(HttpMethod.ALL).ShouldBeTrue();
                HttpMethod.GET.IsEquivalentTo(HttpMethod.ALL).ShouldBeTrue();
            }

            [Fact]
            public void NotAllIsNotEqivalentToNotAll()
            {
                HttpMethod.POST.IsEquivalentTo(HttpMethod.GET).ShouldBeFalse();
            }
        }

        public class FromString
        {
            private const HttpMethod Default = 0;

            [Fact]
            public static void ReturnsDefaultWhenUnknownMethod()
            {
                HttpMethods.FromString(null).ShouldBe(Default);
                HttpMethods.FromString("").ShouldBe(Default);
                HttpMethods.FromString("SMEAR").ShouldBe(Default);
            }

            [Fact]
            public static void ReturnsHttpMethod()
            {
                HttpMethods.FromString("post").ShouldBe(HttpMethod.POST);
                HttpMethods.FromString("PATCH").ShouldBe(HttpMethod.PATCH);
                HttpMethods.FromString("gEt").ShouldBe(HttpMethod.GET);
            }
        }
    }
}
