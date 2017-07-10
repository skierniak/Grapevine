using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Grapevine.Common;
using Grapevine.Core;
using Grapevine.Server;
using Grapevine.Tests.Sample;
using NSubstitute;
using Shouldly;
using Xunit;

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
                var route1 = new Grapevine.Server.Route(context => { }, HttpMethod.GET, "");
                var route2 = new Grapevine.Server.Route(context => { }, HttpMethod.GET, "");
                var route3 = new Grapevine.Server.Route(context => { }, HttpMethod.GET, "");
                var route4 = new Grapevine.Server.Route(context => { }, HttpMethod.GET, "");

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
                var route1 = new Grapevine.Server.Route(context => { }, HttpMethod.GET, "");
                var route2 = new Grapevine.Server.Route(context => { }, HttpMethod.GET, "");
                var route3 = new Grapevine.Server.Route(context => { }, HttpMethod.GET, "");
                var route4 = new Grapevine.Server.Route(context => { }, HttpMethod.GET, "");

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
        }

        public class InsertAt
        {
            private readonly Router _router = new Router();

            private readonly Grapevine.Server.Route _route1 = new Grapevine.Server.Route(context => { }, HttpMethod.GET, "");
            private readonly Grapevine.Server.Route _route2 = new Grapevine.Server.Route(context => { }, HttpMethod.GET, "");
            private readonly Grapevine.Server.Route _route3 = new Grapevine.Server.Route(context => { }, HttpMethod.GET, "");
            private readonly Grapevine.Server.Route _route4 = new Grapevine.Server.Route(context => { }, HttpMethod.GET, "");

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

            private readonly Grapevine.Server.Route _route1 = new Grapevine.Server.Route(context => { }, HttpMethod.GET, "");
            private readonly Grapevine.Server.Route _route2 = new Grapevine.Server.Route(context => { }, HttpMethod.GET, "");
            private readonly Grapevine.Server.Route _route3 = new Grapevine.Server.Route(context => { }, HttpMethod.GET, "");
            private readonly Grapevine.Server.Route _route4 = new Grapevine.Server.Route(context => { }, HttpMethod.GET, "");

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
                var route = new Grapevine.Server.Route(_type.GetMethod("RouteA"), HttpMethod.ALL, "");
                _router.Scan();
                _router.RegisteredRoutes.Count.ShouldBe(3);
            }

            [Fact]
            public void DoesNotAppendDuplicateRoutes()
            {
                var route = new Grapevine.Server.Route(_type.GetMethod("RouteA"), HttpMethod.ALL, "");
                _router.Register(route);

                _router.Scan();

                _router.RegisteredRoutes[0].Name.ShouldBe("Grapevine.Tests.Sample.TypeScannerA.RouteA");
                _router.RegisteredRoutes[1].Name.ShouldBe("Grapevine.Tests.Sample.TypeScannerB.RouteA");
                _router.RegisteredRoutes[2].Name.ShouldBe("Grapevine.Tests.Sample.TypeScannerC.RouteA");

                _router.RegisteredRoutes.Count.ShouldBe(3);
            }
        }

        public class Route
        {
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
