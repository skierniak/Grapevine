using Grapevine.Core;
using NSubstitute;
using Shouldly;
using Xunit;

namespace Grapevine.Tests.Unit.Core
{
    public class HttpContextFacts
    {
        [Fact]
        public void CanCreateMock()
        {
            var request = Substitute.For<IHttpRequest>();
            var response = Substitute.For<IHttpResponse>();
            var context = Substitute.For<IHttpContext>();

            context.ShouldNotBeNull();
            request.ShouldNotBeNull();
            response.ShouldNotBeNull();

            context.Request.Returns(request);
            context.Response.Returns(response);

            context.Request.ShouldBe(request);
            context.Response.ShouldBe(response);
        }
    }
}
