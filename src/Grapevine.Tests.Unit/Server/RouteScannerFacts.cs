using System;
using System.Linq;
using System.Reflection;
using Grapevine.Common;
using Grapevine.Core;
using Grapevine.Server;
using Grapevine.Tests.Sample;
using Shouldly;
using Xunit;

namespace Grapevine.Tests.Unit.Server
{
    public class RouteScannerFacts
    {
        [Fact]
        public void PreLoadsAssemblies()
        {
            var assemblies = RouteScanner.Assemblies;
            assemblies.ShouldSatisfyAllConditions
            (
                () => assemblies.Count.ShouldBeGreaterThan(0),
                () => assemblies.Any(a => a.GlobalAssemblyCache).ShouldBeFalse(),
                () => assemblies.Any(a => a.GetName().Name == "Grapevine").ShouldBeFalse(),
                () => assemblies.Any(a => a.GetName().Name.StartsWith("vshost")).ShouldBeFalse()
            );
        }

        public class Scope
        {
            [Fact]
            public void GetsAndSetsScope()
            {
                const string scope = "scope";
                var scanner = new RouteScanner {Scope = scope};
                scanner.Scope.ShouldBe(scope);
            }
        }

        public class Exclude
        {
            private readonly RouteScanner _scanner = new RouteScanner();
            private readonly Type _type = typeof(SampleRoutes);
            private readonly Assembly _assembly = AppDomain.CurrentDomain.GetAssemblies().First(x => x.GetName().Name == "Grapevine.Tests.Sample");

            [Fact]
            public void AddsTypeToExcludesList()
            {
                _scanner.Exclude(_type);

                _scanner.ShouldSatisfyAllConditions
                (
                    () => _scanner.ExcludedTypes.Count.ShouldBe(1),
                    () => _scanner.ExcludedTypes[0].ShouldBe(_type)
                );
            }

            [Fact]
            public void AddsAssemblyToExcludesList()
            {
                _scanner.Exclude(_assembly);

                _scanner.ShouldSatisfyAllConditions
                (
                    () => _scanner.ExcludedAssemblies.Count.ShouldBe(1),
                    () => _scanner.ExcludedAssemblies[0].ShouldBe(_assembly)
                );
            }

            [Fact]
            public void DoesNotAddDuplicateTypeToExcludesList()
            {
                _scanner.Exclude(_type);
                _scanner.Exclude(_type);

                _scanner.ShouldSatisfyAllConditions
                (
                    () => _scanner.ExcludedTypes.Count.ShouldBe(1),
                    () => _scanner.ExcludedTypes[0].ShouldBe(_type)
                );
            }

            [Fact]
            public void DoesNotAddDuplicateAssemblyToExcludesList()
            {
                _scanner.Exclude(_assembly);
                _scanner.Exclude(_assembly);

                _scanner.ShouldSatisfyAllConditions
                (
                    () => _scanner.ExcludedAssemblies.Count.ShouldBe(1),
                    () => _scanner.ExcludedAssemblies[0].ShouldBe(_assembly)
                );
            }
        }

        public class Include
        {
            private readonly RouteScanner _scanner = new RouteScanner();
            private readonly Type _type = typeof(SampleRoutes);
            private readonly Assembly _assembly = AppDomain.CurrentDomain.GetAssemblies().First(x => x.GetName().Name == "Grapevine.Tests.Sample");

            [Fact]
            public void AddsTypeToIncludesList()
            {
                _scanner.Include(_type);

                _scanner.ShouldSatisfyAllConditions
                (
                    () => _scanner.IncludedTypes.Count.ShouldBe(1),
                    () => _scanner.IncludedTypes[0].ShouldBe(_type)
                );
            }

            [Fact]
            public void AddsAssemblyToIncludesList()
            {
                _scanner.Include(_assembly);

                _scanner.ShouldSatisfyAllConditions
                (
                    () => _scanner.IncludedAssemblies.Count.ShouldBe(1),
                    () => _scanner.IncludedAssemblies[0].ShouldBe(_assembly)
                );
            }

            [Fact]
            public void DoesNotAddDuplicateTypeToIncludesList()
            {
                _scanner.Include(_type);
                _scanner.Include(_type);

                _scanner.ShouldSatisfyAllConditions
                (
                    () => _scanner.IncludedTypes.Count.ShouldBe(1),
                    () => _scanner.IncludedTypes[0].ShouldBe(_type)
                );
            }

            [Fact]
            public void DoesNotAddDuplicateAssemblyToIncludesList()
            {
                _scanner.Include(_assembly);
                _scanner.Include(_assembly);

                _scanner.ShouldSatisfyAllConditions
                (
                    () => _scanner.IncludedAssemblies.Count.ShouldBe(1),
                    () => _scanner.IncludedAssemblies[0].ShouldBe(_assembly)
                );
            }
        }

        public class Scan
        {
            private readonly RouteScanner _scanner = new RouteScanner();
            private readonly Type _type = typeof(TypeScannerC);
            private readonly Assembly _assembly = AppDomain.CurrentDomain.GetAssemblies().First(x => x.GetName().Name == "Grapevine.Tests.Sample");

            [Fact]
            public void ReturnsRoutes()
            {
                var routes = _scanner.Scan();
                routes.Count.ShouldBe(3);
            }

            [Fact]
            public void ReturnsRoutesWithExclusions()
            {
                _scanner.Exclude(_assembly);
                var routes = _scanner.Scan();
                routes.Count.ShouldBe(0);
            }

            [Fact]
            public void ReturnsRoutesWithInclusions()
            {
                _scanner.Include(Assembly.GetExecutingAssembly());
                var routes = _scanner.Scan();
                routes.Count.ShouldBe(0);

                _scanner.Include(_assembly);
                routes = _scanner.Scan();
                routes.Count.ShouldBe(3);
            }
        }

        public class ScanAssembly
        {
            private readonly RouteScanner _scanner = new RouteScanner();
            private readonly Type _type = typeof(TypeScannerC);
            private readonly Assembly _assembly = AppDomain.CurrentDomain.GetAssemblies().First(x => x.GetName().Name == "Grapevine.Tests.Sample");

            [Fact]
            public void ReturnsRoutesForAssembly()
            {
                _scanner.ScanAssembly(_assembly).Count.ShouldBe(3);
            }

            [Fact]
            public void ReturnsRoutesForAssemblyWithoutExcludedTypes()
            {
                _scanner.Exclude(_type);
                _scanner.ScanAssembly(_assembly).Count.ShouldBe(2);
            }

            [Fact]
            public void ReturnsRoutesForAssemblyUsingIncludedTypes()
            {
                _scanner.Include(_type);
                _scanner.ScanAssembly(_assembly).Count.ShouldBe(1);
            }

            [Theory]
            [InlineData("")]
            public void ReturnsRoutesWithAssemblyBasePath(string basepath)
            {
                var expected = string.IsNullOrEmpty(basepath) ? "/typeb/routea" : $"/{basepath}/typeb/routea";
                _scanner.Include<TypeScannerB>();
                var routes = _scanner.ScanAssembly(_assembly);

                routes.ShouldSatisfyAllConditions
                (
                    () => routes.Count.ShouldBe(1),
                    () => routes[0].PathInfo.ShouldBe(expected)
                );
            }

            [Theory]
            [InlineData("")]
            public void ReturnsRoutesWithAssemblyBasePathWithoutTypeBasePath(string basepath)
            {
                var expected = string.IsNullOrEmpty(basepath) ? "/routea" : $"/{basepath}/routea";
                _scanner.Include<TypeScannerA>();
                var routes = _scanner.ScanAssembly(_assembly);

                routes.ShouldSatisfyAllConditions
                (
                    () => routes.Count.ShouldBe(1),
                    () => routes[0].PathInfo.ShouldBe(expected)
                );
            }
        }

        public class ScanType
        {
            private readonly RouteScanner _scanner = new RouteScanner();
            private readonly Type _type = typeof(SampleRoutes);

            [Fact]
            public void ReturnsEmptyListWhenTypeIsAbstract()
            {
                _scanner.ScanType(typeof(AbstractTestClass)).Count.ShouldBe(0);
            }

            [Fact]
            public void ReturnsEmptyListWhenTypeIsNotClass()
            {
                _scanner.ScanType(typeof(NotAClass)).Count.ShouldBe(0);
            }

            [Theory]
            [InlineData("")]
            [InlineData("api")]
            public void ReturnsRoutesWithoutAdditionalBasepathFromType(string basepath)
            {
                var expected = string.IsNullOrEmpty(basepath) ? "/routea" : $"/{basepath}/routea";
                var routes = _scanner.ScanType(typeof(TypeScannerA), basepath);
                routes.ShouldSatisfyAllConditions
                (
                    () => routes.Count.ShouldBe(1),
                    () => routes[0].PathInfo.ShouldBe(expected)
                );
            }

            [Theory]
            [InlineData("")]
            [InlineData("api")]
            public void ReturnsRoutesWithAdditionalBasepathFromType(string basepath)
            {
                var expected = string.IsNullOrEmpty(basepath) ? "/typeb/routea" : $"/{basepath}/typeb/routea";
                var routes = _scanner.ScanType(typeof(TypeScannerB), basepath);
                routes.ShouldSatisfyAllConditions
                (
                    () => routes.Count.ShouldBe(1),
                    () => routes[0].PathInfo.ShouldBe(expected)
                );
            }

            [Fact]
            public void ReturnsRoutesEvenWhenTypeIsExcluded()
            {
                _scanner.Exclude<TypeScannerA>();
                var routes = _scanner.ScanType(typeof(TypeScannerA));
                routes.ShouldSatisfyAllConditions
                (
                    () => routes.Count.ShouldBe(1),
                    () => routes[0].PathInfo.ShouldBe("/routea")
                );
            }

            public abstract class AbstractTestClass
            {
                [RestRoute]
                public void RouteA(IHttpContext context) { }
            }

            public struct NotAClass { }
        }

        public class ScanMethod
        {
            private readonly RouteScanner _scanner = new RouteScanner();
            private readonly Type _type = typeof(SampleRoutes);

            [Fact]
            public void BasePathIsOptional()
            {
                _scanner.ScanMethod(_type.GetMethod("RouteB")).Count.ShouldBe(1);
            }

            [Theory]
            [InlineData("")]
            [InlineData("api")]
            public void ReturnsEmptyListWhenMethodHasNoAttributes(string basepath)
            {
                _scanner.ScanMethod(_type.GetMethod("RouteA"), basepath).Count.ShouldBe(0);
            }

            [Theory]
            [InlineData("")]
            [InlineData("api")]
            public void ReturnsRouteWithDefaultOptions(string basepath)
            {
                var expected = string.IsNullOrEmpty(basepath) ? "" : $"/{basepath}";
                var routes = _scanner.ScanMethod(_type.GetMethod("RouteB"), basepath);

                routes.ShouldSatisfyAllConditions
                (
                    () => routes.Count.ShouldBe(1),
                    () => routes[0].HttpMethod.ShouldBe(HttpMethod.ALL),
                    () => routes[0].PathInfo.ShouldBe(expected)
                );
            }

            [Theory]
            [InlineData("")]
            [InlineData("api")]
            public void ReturnsRouteWithExplicitOptions(string basepath)
            {
                var expected = string.IsNullOrEmpty(basepath) ? "/some/path" : $"/{basepath}/some/path";
                var routes = _scanner.ScanMethod(_type.GetMethod("RouteC"), basepath);

                routes.ShouldSatisfyAllConditions
                (
                    () => routes.Count.ShouldBe(1),
                    () => routes[0].HttpMethod.ShouldBe(HttpMethod.POST),
                    () => routes[0].PathInfo.ShouldBe(expected)
                );
            }

            [Theory]
            [InlineData("")]
            [InlineData("api")]
            public void ReturnsRouteWithMultipleExplicitOptions(string basepath)
            {
                var expectedA = string.IsNullOrEmpty(basepath) ? "/patch/this" : $"/{basepath}/patch/this";
                var expectedB = string.IsNullOrEmpty(basepath) ? "/delete/this" : $"/{basepath}/delete/this";
                var routes = _scanner.ScanMethod(_type.GetMethod("RouteD"), basepath);

                routes.ShouldSatisfyAllConditions
                (
                    () => routes.Count.ShouldBe(2),
                    () => routes[0].HttpMethod.ShouldBe(HttpMethod.PATCH),
                    () => routes[0].PathInfo.ShouldBe(expectedA),
                    () => routes[1].HttpMethod.ShouldBe(HttpMethod.DELETE),
                    () => routes[1].PathInfo.ShouldBe(expectedB)
                );
            }
        }
    }
}
