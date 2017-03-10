using Grapevine.Core;
using NSubstitute;
using Shouldly;
using Xunit;

namespace Grapevine.Tests.Unit.Interfaces
{
    public class HttpContextFacts
    {
        [Fact]
        public void CanCreateMock()
        {
            var request = Substitute.For<IHttpRequest<object>>();
            var response = Substitute.For<IHttpResponse<object>>();
            var context = Substitute.For<IHttpContext<object, object, object>>();

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
