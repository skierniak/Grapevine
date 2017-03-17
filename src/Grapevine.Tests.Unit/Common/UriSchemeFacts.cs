using Grapevine.Common;
using Shouldly;
using Xunit;

namespace Grapevine.Tests.Unit.Common
{
    public class UriSchemeFacts
    {
        public class ToScheme
        {
            [Fact]
            public void ReturnsLowerCaseValue()
            {
                UriScheme.Http.ToScheme().ShouldBe("http");
            }
        }
    }
}
