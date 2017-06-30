using System;
using Grapevine.Server;
using Shouldly;
using Xunit;

namespace Grapevine.Tests.Unit.Server
{
    public class PathInfoServiceFacts
    {
        [Theory]
        [InlineData("", "", "")]
        [InlineData(null, null, "")]
        [InlineData("", null, "")]
        [InlineData(null, "", "")]
        [InlineData("^/user/thing", "api", "^/api/user/thing")]
        [InlineData("/user/thing", "api", "/api/user/thing")]
        [InlineData("user/thing", "api", "/api/user/thing")]
        public void GeneratePathInfo(string pathInfo, string basePath, string expected)
        {
            PathInfoService.GeneratePathInfo(pathInfo, basePath).ShouldBe(expected);
        }

        [Theory]
        [InlineData("basepatharg", typeof(RestResourceA), "basepatharg/restresource")]
        [InlineData(null, typeof(RestResourceA), "restresource")]
        [InlineData("basepatharg", typeof(RestResourceB), "basepatharg")]
        [InlineData("basepatharg", typeof(NotRestResource), "basepatharg")]
        [InlineData("basepatharg", null, "basepatharg")]
        [InlineData("basepatharg", typeof(RestResourceC), "basepatharg/restresource")]
        [InlineData("basepatharg/", typeof(RestResourceA), "basepatharg/restresource")]
        public void GenerateBasePath(string basePath, Type type, string expected)
        {
            PathInfoService.GenerateBasePath(basePath, type).ShouldBe(expected);
        }

        [Theory]
        [InlineData("", "")]
        [InlineData(null, "")]
        [InlineData("/", "")]
        [InlineData(" path", "/path")]
        [InlineData("path ", "/path")]
        [InlineData(" path ", "/path")]
        [InlineData("/path ", "/path")]
        [InlineData(" /path", "/path")]
        [InlineData("path/ ", "/path")]
        [InlineData(" path/", "/path")]
        [InlineData("/path/ ", "/path")]
        [InlineData(" /path/", "/path")]
        public void SanitizeBasePath(string basePath, string expected)
        {
            PathInfoService.SanitizeBasePath(basePath).ShouldBe(expected);
        }

        [RestResource(BasePath = "restresource")]
        public class RestResourceA {}

        [RestResource]
        public class RestResourceB { }

        [RestResource(BasePath = "/restresource")]
        public class RestResourceC { }

        public class NotRestResource { }
    }
}
