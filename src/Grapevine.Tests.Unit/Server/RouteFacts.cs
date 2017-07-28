using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text.RegularExpressions;
using Grapevine.Common;
using Grapevine.Core;
using Grapevine.Server;
using NSubstitute;
using NSubstitute.Core;
using Shouldly;
using Xunit;

namespace Grapevine.Tests.Unit.Server
{
    public class RouteFacts
    {
        public class Constructors
        {
            [Fact]
            public void FromAction()
            {
                Action<IHttpContext> action = context => { };
                var name = $"{action.Method.ReflectedType}.{action.Method.Name}";
                const HttpMethod method = HttpMethod.COPY;
                const string pathinfo = "/some/route";

                var route = new Route(action, method, pathinfo);

                route.ShouldSatisfyAllConditions
                    (
                        () => route.Delegate.ShouldBe(action),
                        () => route.Enabled.ShouldBeTrue(),
                        () => route.HttpMethod.ShouldBe(method),
                        () => route.PathInfo.ShouldBe(pathinfo),
                        () => route.Name.ShouldBe(name),
                        () => route.Description.ShouldBe($"{method} {pathinfo} > {name}")
                    );
            }

            [Fact]
            public void FromActionWithEmptyPathInfo()
            {
                Action<IHttpContext> action = context => { };
                var name = $"{action.Method.ReflectedType}.{action.Method.Name}";
                const HttpMethod method = HttpMethod.PATCH;
                const string pathinfo = "";

                var route = new Route(action, method, pathinfo);

                route.ShouldSatisfyAllConditions
                    (
                        () => route.Delegate.ShouldBe(action),
                        () => route.Enabled.ShouldBeTrue(),
                        () => route.HttpMethod.ShouldBe(method),
                        () => route.PathInfo.ShouldBe(pathinfo),
                        () => route.Name.ShouldBe(name),
                        () => route.Description.ShouldBe($"{method} {pathinfo} > {name}")
                    );
            }

            [Fact]
            public void FromMethodInfo()
            {
                var methodInfo = typeof(TestClass).GetMethod("TestRoute");
                var name = $"{methodInfo.ReflectedType}.{methodInfo.Name}";
                const HttpMethod method = HttpMethod.DELETE;
                const string pathinfo = "/some/other/route";

                var route = new Route(methodInfo, method, pathinfo);

                route.ShouldSatisfyAllConditions
                    (
                        () => route.Enabled.ShouldBeTrue(),
                        () => route.HttpMethod.ShouldBe(method),
                        () => route.PathInfo.ShouldBe(pathinfo),
                        () => route.Name.ShouldBe(name),
                        () => route.Description.ShouldBe($"{method} {pathinfo} > {name}")
                    );
            }

            [Fact]
            public void ThrowsExceptionWhenActionIsNull()
            {
                Action<IHttpContext> action = null;

                Should.Throw<ArgumentNullException>(() => { new Route(action, HttpMethod.GET, ""); });
            }

            [Fact]
            public void ThrowsExceptionWhenMethodInfoIsNull()
            {
                var methodInfo = typeof(TestClass).GetMethod("MethodDoesNotExist");

                Should.Throw<ArgumentNullException>(() => { new Route(methodInfo, HttpMethod.GET, ""); });
            }

            public class TestClass
            {
                public void TestRoute(IHttpContext context)
                {
                }
            }
        }

        public class Matches
        {
            private readonly IHttpContext _context;

            public Matches()
            {
                var request = Substitute.For<IHttpRequest>();
                request.HttpMethod.Returns(HttpMethod.POST);
                request.PathInfo.Returns("/user/1234/action/promote");
                request.Headers.Returns(new NameValueCollection
                {
                    {"Accept", "text/plain"},
                    {"Content-Type", "application/x-www-form-urlencoded"}
                });

                _context = Substitute.For<IHttpContext>();
                _context.Request.Returns(request);
                _context.Response.Returns(Substitute.For<IHttpResponse>());
            }

            [Fact]
            public void DoesNotMatchIfMethodsAreNotEquivalent()
            {
                var route = new Route(context => { }, HttpMethod.GET, "/user/[id]/action/[action]");
                route.Matches(_context).ShouldBeFalse();
            }

            [Fact]
            public void DoesNotMatchIfPathInfoDoesNotMatchPattern()
            {
                var route = new Route(context => { }, HttpMethod.POST, "/user/[id]/actions/[action]");
                route.Matches(_context).ShouldBeFalse();
            }

            [Fact]
            public void MatchesWhenNoAdditionalHeadersAreReferenced()
            {
                var route = new Route(context => { }, HttpMethod.POST, "/user/[id]/action/[action]");
                route.Matches(_context).ShouldBeTrue();
            }

            [Fact]
            public void MatchesWhenHttpMethodsAreEquivalent()
            {
                var route = new Route(context => { }, HttpMethod.ALL, "/user/[id]/action/[action]");
                route.Matches(_context).ShouldBeTrue();
            }

            [Fact]
            public void MatchesWhenPathInfoIsWildcard()
            {
                var route = new Route(context => { }, HttpMethod.POST, string.Empty);
                route.Matches(_context).ShouldBeTrue();
            }

            [Fact]
            public void DoesNotMatchWhenHeaderDoesNotMatchExpression()
            {
                var route = new Route(context => { }, HttpMethod.POST, string.Empty);
                route.MatchOn("Accept", new Regex(@"text/html"));
                route.Matches(_context).ShouldBeFalse();
            }

            [Fact]
            public void MatchesWhenHeaderMatchesExpression()
            {
                var route = new Route(context => { }, HttpMethod.POST, string.Empty);
                route.MatchOn("Accept", new Regex(@"text/plain"));
                route.Matches(_context).ShouldBeTrue();
            }

            [Fact]
            public void DoesNotMatchWhenHeaderDoesNotExist()
            {
                var route = new Route(context => { }, HttpMethod.POST, string.Empty);
                route.MatchOn("MissingHeader", new Regex(@"header-value"));
                route.Matches(_context).ShouldBeFalse();
            }

            [Fact]
            public void MatchesWhenMissingHeaderWithInverseMatching()
            {
                var route = new Route(context => { }, HttpMethod.POST, string.Empty);
                route.MatchOn("MissingHeader", new Regex(@"^((?!hede).)*$"));
                route.Matches(_context).ShouldBeTrue();
            }
        }

        public class MatchOn
        {
            [Fact]
            public void AddsHeaderToCollection()
            {
                const string header = "SomeHeader";
                var regexA = new Regex(@"\d+");
                var regexB = new Regex(@"\w+");

                var route = new Route(context => { }, HttpMethod.ALL, "");
                route.MatchesOn.Count.ShouldBe(0);

                route.MatchOn(header, regexA);
                route.MatchesOn.Count.ShouldBe(1);
                route.MatchesOn[header].ShouldBe(regexA);

                route.MatchOn(header, regexB);
                route.MatchesOn.Count.ShouldBe(1);
                route.MatchesOn[header].ShouldBe(regexB);
            }
        }

        public class Invoke
        {
            [Fact]
            public void DoesNotExecuteDelegateWhenRouteIsDisabled()
            {
                var executed = false;
                var route = new Route(context => { executed = true; }, HttpMethod.GET, string.Empty) {Enabled = false};

                route.Invoke(Substitute.For<IHttpContext>());

                executed.ShouldBeFalse();
            }

            [Fact]
            public void ExecuteDelegateWhenRouteIsEnabled()
            {
                var executed = false;
                var route = new Route(context => { executed = true; }, HttpMethod.GET, string.Empty);

                route.Invoke(Substitute.For<IHttpContext>());

                executed.ShouldBeTrue();
            }

            [Fact]
            public void AssignsRequestPathParametersPriorToExecution()
            {
                var request = Substitute.For<IHttpRequest>();
                request.PathInfo.Returns("/user/1234/action/promote");
                request.PathParameters.Returns(new Dictionary<string, string>());

                var context = Substitute.For<IHttpContext>();
                context.Request.Returns(request);

                var routeA = new Route(ctx => { }, HttpMethod.GET, "/user/[id]/action/[action]");
                var routeB = new Route(ctx => { }, HttpMethod.GET, "/user/[num]/action/[rule]");

                routeA.Invoke(context);

                context.Request.PathParameters.ShouldSatisfyAllConditions
                (
                    () => context.Request.PathParameters.Count.ShouldBe(2),
                    () => context.Request.PathParameters["id"].ShouldNotBeNull(),
                    () => context.Request.PathParameters["id"].ShouldBe("1234"),
                    () => context.Request.PathParameters["action"].ShouldNotBeNull(),
                    () => context.Request.PathParameters["action"].ShouldBe("promote")
                );

                routeB.Invoke(context);

                context.Request.PathParameters.ShouldSatisfyAllConditions
                (
                    () => context.Request.PathParameters.Count.ShouldBe(2),
                    () => context.Request.PathParameters["num"].ShouldNotBeNull(),
                    () => context.Request.PathParameters["num"].ShouldBe("1234"),
                    () => context.Request.PathParameters["rule"].ShouldNotBeNull(),
                    () => context.Request.PathParameters["rule"].ShouldBe("promote")
                );
            }
        }

        public class EqualsOverride
        {
            [Fact]
            public void IsFalseWhenObjectIsNotRoute()
            {
                (new Route(context => { }, HttpMethod.ALL, string.Empty)).Equals(new object()).ShouldBeFalse();
            }

            [Fact]
            public void IsFalseWhenMethodsAreDifferent()
            {
                Action<IHttpContext> func1 = ctx => { ctx.Response.SendResponse(HttpStatusCode.Ok); };
                Action<IHttpContext> func2 = ctx => { ctx.Response.SendResponse(HttpStatusCode.Ok); };

                var route1 = new Route(func1, HttpMethod.ALL, string.Empty);
                var route2 = new Route(func2, HttpMethod.ALL, string.Empty);

                route1.Equals(route2).ShouldBeFalse();
            }

            [Fact]
            public void IsFalseWhenHttpMethodsAreNotEquivalent()
            {
                Action<IHttpContext> func = ctx => { ctx.Response.SendResponse(HttpStatusCode.Ok); };

                var route1 = new Route(func, HttpMethod.GET, string.Empty);
                var route2 = new Route(func, HttpMethod.POST, string.Empty);

                route1.Equals(route2).ShouldBeFalse();
            }

            [Fact]
            public void IsFalseWhenPathInfoPatternsDoNotMatch()
            {
                Action<IHttpContext> func = ctx => { ctx.Response.SendResponse(HttpStatusCode.Ok); };

                var route1 = new Route(func, HttpMethod.ALL, "/user/[id]");
                var route2 = new Route(func, HttpMethod.ALL, "/customer/[id]");

                route1.Equals(route2).ShouldBeFalse();
            }

            [Fact]
            public void IsTrueWhenHttpMethodNameAndPathInfoPatternsMatch()
            {
                Action<IHttpContext> func = ctx => { ctx.Response.SendResponse(HttpStatusCode.Ok); };

                var route1 = new Route(func, HttpMethod.ALL, "/[action]/[id]");
                var route2 = new Route(func, HttpMethod.ALL, "/customer/[id]");

                route1.Equals(route2).ShouldBeTrue();
            }

            [Fact]
            public void IsTrueWhenParentTypesAreDifferent()
            {
                Action<IHttpContext> func = ctx => { ctx.Response.SendResponse(HttpStatusCode.Ok); };

                var route1 = new Route(func, HttpMethod.ALL, "/user/[id]");
                var route2 = new QuasiRoute {Name = route1.Name, HttpMethod = route1.HttpMethod, PathInfo = route1.PathInfo, PathInfoPattern = route1.PathInfoPattern};

                route1.Equals(route2).ShouldBeTrue();
            }

            public class QuasiRoute : IRoute
            {
                public Action<IHttpContext> Delegate { get; }
                public string Description { get; set; }
                public bool Enabled { get; set; }
                public HttpMethod HttpMethod { get; set; }
                public string Name { get; set; }
                public string PathInfo { get; set; }
                public Regex PathInfoPattern { get; set; }

                public bool Matches(IHttpContext context)
                {
                    throw new NotImplementedException();
                }

                public IRoute MatchOn(string header, Regex pattern)
                {
                    throw new NotImplementedException();
                }

                public void Invoke(IHttpContext context)
                {
                    throw new NotImplementedException();
                }
            }
        }

        public class ToStringOverride
        {
            [Fact]
            public void OverridesToString()
            {
                Action<IHttpContext> func = ctx => { ctx.Response.SendResponse(HttpStatusCode.Ok); };
                const HttpMethod method = HttpMethod.ALL;
                const string pathinfo = "/user/id";

                var expected = $"{method} {pathinfo} > {func.Method.ReflectedType}.{func.Method.Name}";

                var route1 = new Route(func, method, pathinfo);

                route1.ToString().ShouldBe(expected);
            }

            [Fact]
            public void OverridesGetHashCode()
            {
                Action<IHttpContext> func = ctx => { ctx.Response.SendResponse(HttpStatusCode.Ok); };
                const HttpMethod method = HttpMethod.ALL;
                const string pathinfo = "/user/id";

                var expected = $"{method} {pathinfo} > {func.Method.ReflectedType}.{func.Method.Name}".GetHashCode();

                var route1 = new Route(func, method, pathinfo);

                route1.GetHashCode().ShouldBe(expected);
            }
        }
    }
}
