using Grapevine.Common;
using Grapevine.Core;
using Grapevine.Server;

namespace Grapevine.Tests.Sample
{
    public class SampleRoutes
    {
        public void RouteA(IHttpContext context) { }

        [RestRoute]
        public void RouteB(IHttpContext context) { }

        [RestRoute(HttpMethod = HttpMethod.POST, PathInfo = "some/path")]
        public void RouteC(IHttpContext context) { }

        [RestRoute(HttpMethod = HttpMethod.PATCH, PathInfo = "patch/this")]
        [RestRoute(HttpMethod = HttpMethod.DELETE, PathInfo = "delete/this")]
        public void RouteD(IHttpContext context) { }
    }

    [RestResource]
    public class TypeScannerA
    {
        [RestRoute(PathInfo = "/routea")]
        public void RouteA(IHttpContext context) { }
    }

    [RestResource(BasePath = "typeb")]
    public class TypeScannerB
    {
        [RestRoute(PathInfo = "/routea")]
        public void RouteA(IHttpContext context) { }
    }

    [RestResource]
    public class TypeScannerC
    {
        [RestRoute]
        public void RouteA(IHttpContext context) { }
    }
}
