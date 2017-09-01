using System;
using System.Collections.Generic;
using System.Net;
using System.Reflection;
using Grapevine.Common;
using Grapevine.Core;
using Grapevine.Core.Logging;
using Grapevine.Properties;
using Grapevine.Server;
using Grapevine.Tests.Sample;
using Grapevine.Tests.Unit.Stubs;
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
            private readonly Assembly _assembly = typeof(TypeScannerA).Assembly;
            private readonly Router _router = new Router();

            [Fact]
            public void AppendsRoutesToRoutingTable()
            {
                _router.Scan();
                _router.RegisteredRoutes.Count.ShouldBe(3);
            }

            [Fact]
            public void DoesNotAppendDuplicateRoutes()
            {
                var route = new Route(_type.GetMethod("RouteA"), HttpMethod.ALL, "");
                _router.Register(route);

                _router.Scan();

                _router.ShouldSatisfyAllConditions(
                    () => _router.RegisteredRoutes.Count.ShouldBe(3),
                    () => _router.RegisteredRoutes[0].Name.ShouldBe("Grapevine.Tests.Sample.TypeScannerA.RouteA"),
                    () => _router.RegisteredRoutes[1].Name.ShouldBe("Grapevine.Tests.Sample.TypeScannerB.RouteA"),
                    () => _router.RegisteredRoutes[2].Name.ShouldBe("Grapevine.Tests.Sample.TypeScannerC.RouteA")
                );

                _router.RegisteredRoutes.Count.ShouldBe(3);
            }
        }

        public class RouteContext
        {
            [Fact]
            public void ReturnsWithNotFoundStatusCodeWhenRoutingListIsNull()
            {
                var executed = false;
                var context = new StubContext();

                var router = new Router();
                router.BeforeRouting += ctx => { executed = true; };

                router.Route(context, null);

                executed.ShouldBeFalse();
                context.Response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
            }

            [Fact]
            public void ReturnsWithNotFoundStatusCodeWhenRoutingListIsEmpty()
            {
                var executed = false;
                var context = new StubContext();

                var router = new Router();
                router.BeforeRouting += ctx => { executed = true; };

                router.Route(context, new List<IRoute>());

                executed.ShouldBeFalse();
                context.Response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
            }

            [Fact]
            public void ExecutesBeforeAndAfterEventHandlers()
            {
                var results = new List<string>();
                var expected = new List<string>{ "B.1", "B.2", "R.1", "A.2", "A.1" };

                var context = new StubContext();

                var router = new Router();
                router.BeforeRouting += ctx => { results.Add("B.1"); };
                router.BeforeRouting += ctx => { results.Add("B.2"); };
                router.AfterRouting += ctx => { results.Add("A.1"); };
                router.AfterRouting += ctx => { results.Add("A.2"); };

                router.Route(context, new List<IRoute> { new Route(ctx => { results.Add("R.1"); }, HttpMethod.ALL, string.Empty) });

                results.ShouldBe(expected);
            }

            [Fact]
            public void ExecutesBeforeAndAfterEventHandlersWhenExecptionThrown()
            {
                var results = new List<string>();
                var expected = new List<string> { "B.1", "B.2", "A.2", "A.1" };

                var context = new StubContext();

                var router = new Router();
                router.BeforeRouting += ctx => { results.Add("B.1"); };
                router.BeforeRouting += ctx => { results.Add("B.2"); };
                router.AfterRouting += ctx => { results.Add("A.1"); };
                router.AfterRouting += ctx => { results.Add("A.2"); };

                Should.Throw<Exception>(() => router.Route(context, new List<IRoute> { new Route(ctx => { throw new Exception(); }, HttpMethod.ALL, string.Empty) }));

                results.ShouldBe(expected);
            }

            [Fact]
            public void OnlyExecutesEnabledRoutes()
            {
                var route1Executed = false;
                var route2Executed = false;

                var context = new StubContext();

                new Router().Route(context, new List<IRoute>
                {
                    new Route(ctx => { route1Executed = true; }, HttpMethod.ALL, "") {Enabled = false},
                    new Route(ctx => { route2Executed = true;  }, HttpMethod.ALL, ""),
                    new Route(ctx => { ctx.Response.ResponseSent = true;  }, HttpMethod.ALL, "")
                });

                route1Executed.ShouldBeFalse();
                route2Executed.ShouldBeTrue();
            }

            [Fact]
            public void StopsRoutingWhenResponseIsSent()
            {
                var route1Executed = false;
                var route2Executed = false;

                var context = new StubContext();

                new Router().Route(context, new List<IRoute>
                {
                    new Route(ctx => { route1Executed = true;
                        ctx.Response.ResponseSent = true;
                    }, HttpMethod.ALL, ""),
                    new Route(ctx => { route2Executed = true;  }, HttpMethod.ALL, "")
                });

                route1Executed.ShouldBeTrue();
                route2Executed.ShouldBeFalse();
            }

            [Fact]
            public void StopsRoutingWhenStatusCodeChanges()
            {
                var route1Executed = false;
                var route2Executed = false;

                var context = new StubContext();

                new Router().Route(context, new List<IRoute>
                {
                    new Route(ctx => { route1Executed = true;
                        ctx.Response.StatusCode = HttpStatusCode.NotAcceptable;
                    }, HttpMethod.ALL, ""),
                    new Route(ctx => { route2Executed = true;  }, HttpMethod.ALL, "")
                });

                route1Executed.ShouldBeTrue();
                route2Executed.ShouldBeFalse();
            }

            [Fact]
            public void HasCorrectExceptionMessageWhenHttpListenerExceptionIsThrownBecauseConnectionIsAborted()
            {
                var logger = InMemoryLogger.GetLogger(Guid.NewGuid().ToString());
                var router = new Router { Logger = logger };

                router.Route(new StubContext(),
                    new List<IRoute>
                    {
                        new Route(ctx => { throw new HttpListenerException(64); }, HttpMethod.ALL, "")
                    });

                logger.Logs.ShouldSatisfyAllConditions(
                    () => logger.Logs.Count.ShouldBe(3),
                    () => logger.Logs[1].Message.ShouldBe(Messages.ConnectionAborted)
                );
            }

            [Fact]
            public void HasCorrectExceptionMessageWhenHttpListenerExceptionIsThrownForOtherReasons()
            {
                var logger = InMemoryLogger.GetLogger(Guid.NewGuid().ToString());
                var router = new Router { Logger = logger };

                router.Route(new StubContext(),
                    new List<IRoute>
                    {
                        new Route(ctx => { throw new HttpListenerException(32); }, HttpMethod.ALL, "")
                    });

                logger.Logs.ShouldSatisfyAllConditions(
                    () => logger.Logs.Count.ShouldBe(3),
                    () => logger.Logs[1].Message.ShouldBe(Messages.UnknownListenerException)
                );
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
                var response = new StubResponse();

                var request = new StubRequest
                {
                    HttpMethod = HttpMethod.POST,
                    PathInfo = "/some/url"
                };

                var context = new StubContext {Request = request, Response = response};

                var router = new Router().Register(new Route(ctx =>
                {
                    response.SendResponse(null);
                }, HttpMethod.ALL, ""));

                router.Route(context);
                response.StatusCode.ShouldBe(HttpStatusCode.Ok);
            }

            //[Fact]
            //public void DoesNotThrowExceptionWhenResponseHasBeenSent()
            //{
            //    var response = Substitute.For<IHttpResponse>();

            //    var context = Substitute.For<IHttpContext>();
            //    context.Response.Returns(response);
            //    context.Request.HttpMethod.Returns(HttpMethod.POST);
            //    context.Request.PathInfo.Returns("/some/url");

            //    var router = new Router().Register(new Route(ctx =>
            //    {
            //        response.StatusCode.Returns(HttpStatusCode.Ok);
            //        context.WasRespondedTo.Returns(true);
            //        throw new Exception();
            //    }, HttpMethod.ALL, ""));

            //    router.Route(context);
            //    response.StatusCode.ShouldBe(HttpStatusCode.Ok);
            //}

            //[Fact]
            //public void SendsNotFoundResponse()
            //{
            //    var called = false;

            //    var response = Substitute.For<IHttpResponse>();
            //    response.ContentEncoding.Returns(Encoding.ASCII);
            //    response.When(x => x.SendResponse(Arg.Any<byte[]>())).Do(info => { called = true; });

            //    var context = Substitute.For<IHttpContext>();
            //    context.Response.Returns(response);
            //    context.Request.HttpMethod.Returns(HttpMethod.POST);
            //    context.Request.PathInfo.Returns("/some/url");

            //    var router = new Router().Register(new Route(ctx => { }, HttpMethod.ALL, ""));
            //    router.Route(context);

            //    response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
            //    called.ShouldBeTrue();
            //}

            //[Fact]
            //public void SendsNotImplementedResponse()
            //{
            //    var called = false;

            //    var response = Substitute.For<IHttpResponse>();
            //    response.ContentEncoding.Returns(Encoding.ASCII);
            //    response.When(x => x.SendResponse(Arg.Any<byte[]>())).Do(info => { called = true; });

            //    var context = Substitute.For<IHttpContext>();
            //    context.Response.Returns(response);
            //    context.Request.HttpMethod.Returns(HttpMethod.POST);
            //    context.Request.PathInfo.Returns("/some/url");

            //    var router = new Router().Register(new Route(ctx => { throw new NotImplementedException(); }, HttpMethod.ALL, ""));
            //    router.Route(context);

            //    response.StatusCode.ShouldBe(HttpStatusCode.NotImplemented);
            //    called.ShouldBeTrue();
            //}

            //[Fact]
            //public void SendsInternalServerError()
            //{
            //    var called = false;

            //    var response = Substitute.For<IHttpResponse>();
            //    response.ContentEncoding.Returns(Encoding.ASCII);
            //    response.When(x => x.SendResponse(Arg.Any<byte[]>())).Do(info => { called = true; });

            //    var context = Substitute.For<IHttpContext>();
            //    context.Response.Returns(response);
            //    context.Request.HttpMethod.Returns(HttpMethod.POST);
            //    context.Request.PathInfo.Returns("/some/url");

            //    var router = new Router().Register(new Route(ctx => { throw new Exception(); }, HttpMethod.ALL, ""));
            //    router.Route(context);

            //    response.StatusCode.ShouldBe(HttpStatusCode.InternalServerError);
            //    called.ShouldBeTrue();
            //}
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

        public class ErrorHandling
        {
            [Fact]
            public void RespondsWithInternalServerErrorByDefault()
            {
                var response = Substitute.For<IHttpResponse>();
                response.StatusCode.Returns(HttpStatusCode.Ok);

                var context = Substitute.For<IHttpContext>();
                context.Response.Returns(response);

                var router = new Router();

                router.ErrorHandling(context);

                response.Received().SendResponse(HttpStatusCode.InternalServerError);
            }

            [Fact]
            public void InvokesGlobalErrorHandler()
            {
                const HttpStatusCode status = HttpStatusCode.NotAcceptable;
                var result = false;

                var response = Substitute.For<IHttpResponse>();
                response.StatusCode.Returns(status);

                var context = Substitute.For<IHttpContext>();
                context.Response.Returns(response);

                Router.GlobalErrorHandlers[status] = ctx => { result = true; };

                var router = new Router();
                router.ErrorHandling(context);

                result.ShouldBeTrue();
            }

            [Fact]
            public void InvokesLocalErrorHandler()
            {
                const HttpStatusCode status = HttpStatusCode.ImATeapot;
                var result = false;

                var response = Substitute.For<IHttpResponse>();
                response.StatusCode.Returns(status);

                var context = Substitute.For<IHttpContext>();
                context.Response.Returns(response);

                var router = new Router {LocalErrorHandlers = {[status] = ctx => { result = true; }}};

                router.ErrorHandling(context);

                result.ShouldBeTrue();
            }

            [Fact]
            public void InvokesLocalOverGlobalErrorHandler()
            {
                const HttpStatusCode status = HttpStatusCode.AlreadyReported;
                var local = false;
                var global = false;

                var response = Substitute.For<IHttpResponse>();
                response.StatusCode.Returns(status);

                var context = Substitute.For<IHttpContext>();
                context.Response.Returns(response);

                Router.GlobalErrorHandlers.Add(status, ctx => { global = true; });

                var router = new Router();
                router.LocalErrorHandlers.Add(status, ctx => { local = true; });

                router.ErrorHandling(context);

                local.ShouldBeTrue();
                global.ShouldBeFalse();
            }

            [Fact]
            public void DoesNotRespondWhenAlreadyRespondedTo()
            {
                const HttpStatusCode status = HttpStatusCode.BadGateway;

                var response = Substitute.For<IHttpResponse>();
                response.StatusCode.Returns(status);

                var context = Substitute.For<IHttpContext>();
                context.Response.Returns(response);

                var router = new Router();
                router.LocalErrorHandlers.Add(status, ctx => { context.WasRespondedTo.Returns(true); });

                router.ErrorHandling(context);

                response.DidNotReceive().SendResponse(status);
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
