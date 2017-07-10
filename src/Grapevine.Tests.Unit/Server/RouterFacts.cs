using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using Grapevine.Common;
using Grapevine.Core;
using Grapevine.Core.Exceptions;
using Grapevine.Core.Logging;
using Grapevine.Server;
using Grapevine.Tests.Sample;
using NSubstitute;
using Shouldly;
using Xunit;
using HttpStatusCode = Grapevine.Common.HttpStatusCode;

namespace Grapevine.Tests.Unit.Server
{
    public class RouterFacts
    {
        public class Import
        {
            private readonly Router _router = new Router();

            [Fact]
            public void AppendsRoutesFromImportedRouter()
            {
                var route1 = new Route(context => { }, HttpMethod.GET, "");
                var route2 = new Route(context => { }, HttpMethod.GET, "");
                var route3 = new Route(context => { }, HttpMethod.GET, "");
                var route4 = new Route(context => { }, HttpMethod.GET, "");

                var router = new Router();
                router.Register(route1);
                router.Register(route2);
                router.Register(route3);

                _router.Register(route4);
                _router.Import(router);

                _router.RegisteredRoutes.ShouldSatisfyAllConditions
                (
                    () => _router.RegisteredRoutes.Count.ShouldBe(4),
                    () => _router.RegisteredRoutes[0].ShouldBe(route4),
                    () => _router.RegisteredRoutes[1].ShouldBe(route1),
                    () => _router.RegisteredRoutes[2].ShouldBe(route2),
                    () => _router.RegisteredRoutes[3].ShouldBe(route3)
                );
            }

            [Fact]
            public void AppendsRoutesFromImportedRouterWithoutDuplication()
            {
                var route1 = new Route(context => { }, HttpMethod.GET, "");
                var route2 = new Route(context => { }, HttpMethod.GET, "");
                var route3 = new Route(context => { }, HttpMethod.GET, "");
                var route4 = new Route(context => { }, HttpMethod.GET, "");

                var router = new Router();
                router.Register(route1);
                router.Register(route2);
                router.Register(route3);
                router.Register(route4);

                _router.Register(route4);
                _router.Import(router);

                _router.RegisteredRoutes.ShouldSatisfyAllConditions
                (
                    () => _router.RegisteredRoutes.Count.ShouldBe(4),
                    () => _router.RegisteredRoutes[0].ShouldBe(route4),
                    () => _router.RegisteredRoutes[1].ShouldBe(route1),
                    () => _router.RegisteredRoutes[2].ShouldBe(route2),
                    () => _router.RegisteredRoutes[3].ShouldBe(route3)
                );
            }

            [Fact]
            public void AppendsBeforeEventHandlersWhenImportingRouter()
            {
                var expected = new List<int> { 1, 2, 3, 4 };
                var actual = new List<int>();

                var routerX = new Router();
                routerX.BeforeRouting += ctx => { actual.Add(3); };
                routerX.BeforeRouting += ctx => { actual.Add(4); };

                var routerY = new Router();
                routerY.BeforeRouting += ctx => { actual.Add(1); };
                routerY.BeforeRouting += ctx => { actual.Add(2); };
                routerY.Import(routerX);

                routerY.OnBeforeRouting(Substitute.For<IHttpContext>());

                actual.ShouldBe(expected);
            }

            [Fact]
            public void AppendsAfterEventHandlersWhenImportingRouter()
            {
                var expected = new List<int> { 4, 3, 2, 1 };
                var actual = new List<int>();

                var routerX = new Router();
                routerX.AfterRouting += ctx => { actual.Add(3); };
                routerX.AfterRouting += ctx => { actual.Add(4); };

                var routerY = new Router();
                routerY.AfterRouting += ctx => { actual.Add(1); };
                routerY.AfterRouting += ctx => { actual.Add(2); };
                routerY.Import(routerX);

                routerY.OnAfterRouting(Substitute.For<IHttpContext>());

                actual.ShouldBe(expected);
            }

            [Fact]
            public void DoesNotImportEventHandlersWhenImportingNonRouter()
            {
                var expected = new List<int> { 1, 2, 3, 4 };
                var actual = new List<int>();

                var routerX = new AlterRouter();
                routerX.BeforeRouting += ctx => { actual.Add(5); };
                routerX.BeforeRouting += ctx => { actual.Add(5); };
                routerX.AfterRouting += ctx => { actual.Add(5); };
                routerX.AfterRouting += ctx => { actual.Add(5); };

                var routerY = new Router();
                routerY.BeforeRouting += ctx => { actual.Add(1); };
                routerY.BeforeRouting += ctx => { actual.Add(2); };
                routerY.AfterRouting += ctx => { actual.Add(4); };
                routerY.AfterRouting += ctx => { actual.Add(3); };
                routerY.Import(routerX);

                routerY.OnBeforeRouting(Substitute.For<IHttpContext>());
                routerY.OnAfterRouting(Substitute.For<IHttpContext>());

                actual.ShouldBe(expected);
            }

            public class AlterRouter : IRouter
            {
                public event RoutingEventHandler AfterRouting;
                public event RoutingEventHandler BeforeRouting;
                public IRouteScanner Scanner { get; set; }
                public IList<IRoute> RoutingTable { get; } = new List<IRoute>();
                public IRouter Import(IRouter router)
                {
                    throw new NotImplementedException();
                }

                public int InsertAt(int index, IRoute route)
                {
                    throw new NotImplementedException();
                }

                public IRouter Register(IRoute route)
                {
                    throw new NotImplementedException();
                }

                public IRouter Scan()
                {
                    throw new NotImplementedException();
                }

                public void Route(object state)
                {
                    throw new NotImplementedException();
                }

                public void Route(IHttpContext context, IList<IRoute> routing)
                {
                    throw new NotImplementedException();
                }

                public IList<IRoute> RoutesFor(IHttpContext context)
                {
                    throw new NotImplementedException();
                }
            }
        }

        public class InsertAt
        {
            private readonly Router _router = new Router();

            private readonly Route _route1 = new Route(context => { }, HttpMethod.GET, "");
            private readonly Route _route2 = new Route(context => { }, HttpMethod.GET, "");
            private readonly Route _route3 = new Route(context => { }, HttpMethod.GET, "");
            private readonly Route _route4 = new Route(context => { }, HttpMethod.GET, "");

            [Theory]
            [InlineData(-1, false)]
            [InlineData(0, true)]
            [InlineData(1, true)]
            [InlineData(2, true)]
            [InlineData(3, true)]
            [InlineData(4, false)]
            public void InsertsRouteAtIndexWhenIndexIsInRange(int position, bool shouldInsert)
            {
                _router.Register(_route1);
                _router.Register(_route2);
                _router.Register(_route3);

                var count = shouldInsert ? 4 : 3;
                var result = shouldInsert ? position + 1 : position;

                var idx = _router.InsertAt(position, _route4);

                idx.ShouldBe(result);
                _router.RegisteredRoutes.Count.ShouldBe(count);

                if (shouldInsert)
                {
                    _router.RegisteredRoutes[position].ShouldBe(_route4);
                }
                else
                {
                    _router.RegisteredRoutes.Contains(_route4).ShouldBeFalse();
                }
            }

            [Fact]
            public void DoesNotInsertDuplicateRoute()
            {
                _router.Register(_route1);
                _router.Register(_route2);
                _router.Register(_route3);
                _router.Register(_route4);
                var position = 2;

                var idx = _router.InsertAt(position, _route4);

                _router.RegisteredRoutes.ShouldSatisfyAllConditions
                (
                    () => _router.RegisteredRoutes.Count.ShouldBe(4),
                    () => _router.RegisteredRoutes[0].ShouldBe(_route1),
                    () => _router.RegisteredRoutes[1].ShouldBe(_route2),
                    () => _router.RegisteredRoutes[2].ShouldBe(_route3),
                    () => _router.RegisteredRoutes[3].ShouldBe(_route4)
                );

                idx.ShouldBe(position);
            }
        }

        public class Register
        {
            private readonly Router _router = new Router();

            private readonly Route _route1 = new Route(context => { }, HttpMethod.GET, "");
            private readonly Route _route2 = new Route(context => { }, HttpMethod.GET, "");
            private readonly Route _route3 = new Route(context => { }, HttpMethod.GET, "");
            private readonly Route _route4 = new Route(context => { }, HttpMethod.GET, "");

            [Fact]
            public void RegistersRouteToEndOfRoutingTable()
            {
                _router.Register(_route1);
                _router.Register(_route2);
                _router.Register(_route3);
                _router.Register(_route4);

                _router.RegisteredRoutes.ShouldSatisfyAllConditions
                (
                    () => _router.RegisteredRoutes.Count.ShouldBe(4),
                    () => _router.RegisteredRoutes[0].ShouldBe(_route1),
                    () => _router.RegisteredRoutes[1].ShouldBe(_route2),
                    () => _router.RegisteredRoutes[2].ShouldBe(_route3),
                    () => _router.RegisteredRoutes[3].ShouldBe(_route4)
                );
            }

            [Fact]
            public void DoesNotRegisterDuplicateRoute()
            {
                _router.Register(_route1);
                _router.Register(_route2);
                _router.Register(_route3);
                _router.Register(_route4);

                _router.Register(_route1);
                _router.Register(_route2);
                _router.Register(_route3);
                _router.Register(_route4);

                _router.RegisteredRoutes.ShouldSatisfyAllConditions
                (
                    () => _router.RegisteredRoutes.Count.ShouldBe(4),
                    () => _router.RegisteredRoutes[0].ShouldBe(_route1),
                    () => _router.RegisteredRoutes[1].ShouldBe(_route2),
                    () => _router.RegisteredRoutes[2].ShouldBe(_route3),
                    () => _router.RegisteredRoutes[3].ShouldBe(_route4)
                );
            }
        }

        public class Scan
        {
            private readonly Type _type = typeof(TypeScannerA);
            private readonly Assembly _assembly = AppDomain.CurrentDomain.GetAssemblies().First(x => x.GetName().Name == "Grapevine.Tests.Sample");

            private readonly Router _router = new Router();

            [Fact]
            public void AppendsRoutesToRoutingTable()
            {
                var route = new Route(_type.GetMethod("RouteA"), HttpMethod.ALL, "");
                _router.Scan();
                _router.RegisteredRoutes.Count.ShouldBe(3);
            }

            [Fact]
            public void DoesNotAppendDuplicateRoutes()
            {
                var route = new Route(_type.GetMethod("RouteA"), HttpMethod.ALL, "");
                _router.Register(route);

                _router.Scan();

                _router.RegisteredRoutes[0].Name.ShouldBe("Grapevine.Tests.Sample.TypeScannerA.RouteA");
                _router.RegisteredRoutes[1].Name.ShouldBe("Grapevine.Tests.Sample.TypeScannerB.RouteA");
                _router.RegisteredRoutes[2].Name.ShouldBe("Grapevine.Tests.Sample.TypeScannerC.RouteA");

                _router.RegisteredRoutes.Count.ShouldBe(3);
            }
        }

        public class RouteContext
        {
            [Fact]
            public void ThrowsExceptionWhenRoutingTableIsNullOrEmpty()
            {
                var router = new Router();
                var context = Substitute.For<IHttpContext>();

                Should.Throw<RouteNotFoundException>(() => { router.Route(context, null); });
                Should.Throw<RouteNotFoundException>(() => { router.Route(context, new List<IRoute>()); });
            }

            [Fact]
            public void ThrowsExceptionWhenNoRoutesSendResponse()
            {
                var routeExecuted = false;

                var context = Substitute.For<IHttpContext>();

                Should.Throw<RouteNotFoundException>(() =>
                {
                    new Router().Route(context, new List<IRoute>
                    {
                        new Route(ctx => { routeExecuted = true; }, HttpMethod.ALL, "")
                    });
                });

                routeExecuted.ShouldBeTrue();
            }

            [Fact]
            public void OnlyExecutesEnabledRoutes()
            {
                var route1Executed = false;
                var route2Executed = false;

                var context = Substitute.For<IHttpContext>();

                new Router().Route(context, new List<IRoute>
                {
                    new Route(ctx => { route1Executed = true; }, HttpMethod.ALL, "") {Enabled = false},
                    new Route(ctx => { route2Executed = true;  }, HttpMethod.ALL, ""),
                    new Route(ctx => { ctx.WasRespondedTo.Returns(true);  }, HttpMethod.ALL, "")
                });

                route1Executed.ShouldBeFalse();
                route2Executed.ShouldBeTrue();
            }

            [Fact]
            public void HasCorrectExceptionMessageWhenHttpListenerExceptionIsThrownBecauseConnectionIsAborted()
            {
                var logger = InMemoryLogger.GetLogger("chump1");
                var router = new Router() { Logger = logger };

                Should.Throw<RouteNotFoundException>(() =>
                {
                    router.Route(Substitute.For<IHttpContext>(),
                        new List<IRoute>
                        {
                            new Route(ctx => { throw new HttpListenerException(64); }, HttpMethod.ALL, "")
                        });
                });

                logger.Logs.Count.ShouldBe(3);
                logger.Logs[1].Message.ShouldBe(Router.ConnectionAbortedMsg);
            }

            [Fact]
            public void HasCorrectExceptionMessageWhenHttpListenerExceptionIsThrownForOtherReasons()
            {
                var logger = InMemoryLogger.GetLogger("chump2");
                var router = new Router() { Logger = logger };

                Should.Throw<RouteNotFoundException>(() =>
                {
                    router.Route(Substitute.For<IHttpContext>(),
                        new List<IRoute>
                        {
                            new Route(ctx => { throw new HttpListenerException(32); }, HttpMethod.ALL, "")
                        });
                });

                logger.Logs.Count.ShouldBe(3);
                logger.Logs[1].Message.ShouldBe(Router.UnknownListenerExceptionMsg);
            }
        }

        public class RouteObject
        {
            [Fact]
            public void DoesNotThrowExceptionWhenObjectDoesNotImplementIHttpContext()
            {
                var router = new Router();
                router.Route(new object());
            }

            [Fact]
            public void RoutesContext()
            {
                var response = Substitute.For<IHttpResponse>();

                var context = Substitute.For<IHttpContext>();
                context.Response.Returns(response);
                context.Request.HttpMethod.Returns(HttpMethod.POST);
                context.Request.PathInfo.Returns("/some/url");

                var router = new Router().Register(new Route(ctx =>
                {
                    response.StatusCode = HttpStatusCode.Ok;
                    context.WasRespondedTo.Returns(true);
                }, HttpMethod.ALL, ""));

                router.Route(context);
                response.StatusCode.ShouldBe(HttpStatusCode.Ok);
            }

            [Fact]
            public void DoesNotThrowExceptionWhenResponseHasBeenSent()
            {
                var response = Substitute.For<IHttpResponse>();

                var context = Substitute.For<IHttpContext>();
                context.Response.Returns(response);
                context.Request.HttpMethod.Returns(HttpMethod.POST);
                context.Request.PathInfo.Returns("/some/url");

                var router = new Router().Register(new Route(ctx =>
                {
                    response.StatusCode = HttpStatusCode.Ok;
                    context.WasRespondedTo.Returns(true);
                    throw new Exception();
                }, HttpMethod.ALL, ""));

                router.Route(context);
                response.StatusCode.ShouldBe(HttpStatusCode.Ok);
            }

            [Fact]
            public void SendsNotFoundResponse()
            {
                var called = false;

                var response = Substitute.For<IHttpResponse>();
                response.ContentEncoding.Returns(Encoding.ASCII);
                response.When(x => x.SendResponse(Arg.Any<byte[]>())).Do(info => { called = true; });

                var context = Substitute.For<IHttpContext>();
                context.Response.Returns(response);
                context.Request.HttpMethod.Returns(HttpMethod.POST);
                context.Request.PathInfo.Returns("/some/url");

                var router = new Router().Register(new Route(ctx => { }, HttpMethod.ALL, ""));
                router.Route(context);

                response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
                called.ShouldBeTrue();
            }

            [Fact]
            public void SendsNotImplementedResponse()
            {
                var called = false;

                var response = Substitute.For<IHttpResponse>();
                response.ContentEncoding.Returns(Encoding.ASCII);
                response.When(x => x.SendResponse(Arg.Any<byte[]>())).Do(info => { called = true; });

                var context = Substitute.For<IHttpContext>();
                context.Response.Returns(response);
                context.Request.HttpMethod.Returns(HttpMethod.POST);
                context.Request.PathInfo.Returns("/some/url");

                var router = new Router().Register(new Route(ctx => { throw new NotImplementedException(); }, HttpMethod.ALL, ""));
                router.Route(context);

                response.StatusCode.ShouldBe(HttpStatusCode.NotImplemented);
                called.ShouldBeTrue();
            }

            [Fact]
            public void SendsInternalServerError()
            {
                var called = false;

                var response = Substitute.For<IHttpResponse>();
                response.ContentEncoding.Returns(Encoding.ASCII);
                response.When(x => x.SendResponse(Arg.Any<byte[]>())).Do(info => { called = true; });

                var context = Substitute.For<IHttpContext>();
                context.Response.Returns(response);
                context.Request.HttpMethod.Returns(HttpMethod.POST);
                context.Request.PathInfo.Returns("/some/url");

                var router = new Router().Register(new Route(ctx => { throw new Exception(); }, HttpMethod.ALL, ""));
                router.Route(context);

                response.StatusCode.ShouldBe(HttpStatusCode.InternalServerError);
                called.ShouldBeTrue();
            }
        }

        public class RoutesFor
        {
            private readonly Router _router = new Router();
            private readonly IRoute _route = Substitute.For<IRoute>();

            [Fact]
            private void ReturnsEmptyListWhenNoMatchingRoutesFound()
            {
                _router.RoutesFor(Substitute.For<IHttpContext>()).Count.ShouldBe(0);

                _route.Matches(Arg.Any<IHttpContext>()).Returns(false);
                _route.Enabled.Returns(true);

                _router.Register(_route);

                _router.RoutesFor(Substitute.For<IHttpContext>()).Count.ShouldBe(0);
            }

            [Fact]
            private void ReturnsOnlyEnabledMatchingRoutes()
            {
                _route.Matches(Arg.Any<IHttpContext>()).Returns(true);
                _route.Enabled.Returns(true);

                _router.Register(_route);

                var list = _router.RoutesFor(Substitute.For<IHttpContext>());

                list.Count.ShouldBe(1);
                list[0].ShouldBe(_route);
            }

            [Fact]
            private void DoesNotReturnsDisabledMatchingRoutes()
            {
                _route.Matches(Arg.Any<IHttpContext>()).Returns(true);
                _route.Enabled.Returns(false);

                _router.Register(_route);

                _router.RoutesFor(Substitute.For<IHttpContext>()).Count.ShouldBe(0);
            }
        }

        public class OnBeforeRouting
        {
            private readonly Router _router = new Router();

            [Fact]
            public void DoesNotThrowExceptionWhenNoEventsHaveBeenAdded()
            {
                _router.OnBeforeRouting(Substitute.For<IHttpContext>());
            }

            [Fact]
            public void InvokesEventsInOrderAdded()
            {
                var results = new List<string>();

                _router.BeforeRouting += context => { results.Add("First"); };
                _router.BeforeRouting += context => { results.Add("Second"); };
                _router.BeforeRouting += context => { results.Add("Third"); };

                _router.OnBeforeRouting(Substitute.For<IHttpContext>());

                results.ShouldSatisfyAllConditions
                (
                    () => results.Count.ShouldBe(3),
                    () => results[0].ShouldBe("First"),
                    () => results[1].ShouldBe("Second"),
                    () => results[2].ShouldBe("Third")
                );
            }
        }

        public class OnAfterRouting
        {
            private readonly Router _router = new Router();

            [Fact]
            public void DoesNotThrowExceptionWhenNoEventsHaveBeenAdded()
            {
                _router.OnAfterRouting(Substitute.For<IHttpContext>());
            }

            [Fact]
            public void InvokesEventsInReverseOrderAdded()
            {
                var results = new List<string>();

                _router.AfterRouting += context => { results.Add("First"); };
                _router.AfterRouting += context => { results.Add("Second"); };
                _router.AfterRouting += context => { results.Add("Third"); };

                _router.OnAfterRouting(Substitute.For<IHttpContext>());

                results.ShouldSatisfyAllConditions
                (
                    () => results.Count.ShouldBe(3),
                    () => results[0].ShouldBe("Third"),
                    () => results[1].ShouldBe("Second"),
                    () => results[2].ShouldBe("First")
                );
            }
        }
    }
}
