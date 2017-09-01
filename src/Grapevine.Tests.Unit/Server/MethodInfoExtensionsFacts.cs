using System;
using System.Linq;
using System.Reflection;
using Grapevine.Common;
using Grapevine.Core;
using Grapevine.Core.Exceptions;
using Grapevine.Server;
using NSubstitute;
using Shouldly;
using Xunit;

namespace Grapevine.Tests.Unit.Server
{
    public class MethodInfoExtensionsFacts
    {
        public class ConvertToAction
        {
            [Fact]
            public void ThrowsExceptionWhenMethodIsNotRestRouteEligible()
            {
                var methodA = typeof(TestClass).GetMethod("MethodTakesZeroArgs");
                var methodB = typeof(TestClass).GetMethod("MethodTakesTwoArgs");

                Should.Throw<InvalidRouteMethodExceptions>(() => { methodA.ConvertToAction(); });
                Should.Throw<InvalidRouteMethodExceptions>(() => { methodB.ConvertToAction(); });
                Should.Throw<ArgumentNullException>(() => { MethodInfoExtensions.ConvertToAction(null); });
            }

            [Fact]
            public void ReturnsActionForStaticMethod()
            {
                var method = typeof(TestClass).GetMethod("StaticMethod");
                var result = method.ConvertToAction();
                result.ShouldNotBeNull();
            }

            [Fact]
            public void ReturnsActionForInstanceMethod()
            {
                var method = typeof(TestClass).GetMethod("InstanceMethod");
                var result = method.ConvertToAction();
                result.ShouldNotBeNull();
            }

            public class TestClass
            {
                public void MethodTakesZeroArgs()
                {
                }

                public void MethodTakesTwoArgs(IHttpContext context, string name)
                {
                }

                public static void StaticMethod(IHttpContext context)
                {
                }

                public void InstanceMethod(IHttpContext context)
                {
                }
            }
        }

        public class IsRestRouteEligible
        {
            [Fact]
            public void ThrowsExceptionWhenMethodInfoIsNull()
            {
                MethodInfo method = null;
                Action<MethodInfo> action = info => info.IsRestRouteEligible();
                Should.Throw<ArgumentNullException>(() => action(method));
            }

            [Fact]
            public void ReturnsFalseWhenMethodInfoIsNotInvokable()
            {
                typeof(TestAbstract).GetMethod("TestAbstractMethod").IsRestRouteEligible().ShouldBeFalse();
            }

            [Fact]
            public void ReturnsFalseWhenMethodInfoReflectedTypeHasNoParameterlessConstructor()
            {
                typeof(NoParameterlessConstructor).GetMethod("TestMethod").IsRestRouteEligible().ShouldBeFalse();
            }

            [Fact]
            public void ReturnsFalseWhenMethodIsSpecialName()
            {
                typeof(TestClass).GetMethod("get_TestProperty").IsRestRouteEligible().ShouldBeFalse();
            }

            [Fact]
            public void ReturnsFalseWhenMethodAcceptsMoreOrLessThanOneArgument()
            {
                typeof(TestClass).GetMethod("TakesZeroArgs").IsRestRouteEligible().ShouldBeFalse();
                typeof(TestClass).GetMethod("TakesTwoArgs").IsRestRouteEligible().ShouldBeFalse();
            }

            [Fact]
            public void ReturnsFalseWhenFirstArgumentIsNotIHttpContext()
            {
                typeof(TestClass).GetMethod("TakesWrongArgs").IsRestRouteEligible().ShouldBeFalse();
            }

            [Fact]
            public void ThrowsAggregateExceptionWhenThrowExceptionsIsTrue()
            {
                Should.Throw<InvalidRouteMethodExceptions>(
                    () => typeof(TestClass).GetMethod("TakesWrongArgs").IsRestRouteEligible(true));
            }

            [Fact]
            public void ReturnsTrueWhenMethodInfoIsEligible()
            {
                typeof(TestClass).GetMethod("ValidRoute").IsRestRouteEligible().ShouldBeTrue();
            }

            public abstract class TestAbstract
            {
                public abstract void TestAbstractMethod();

                //public virtual void TestVirtualMethod() { /* intentionally left blank */ }
            }

            public class TestClass
            {
                public string TestProperty { get; set; }

                public void TakesZeroArgs()
                {
                }

                public void TakesTwoArgs(IHttpContext context, string name)
                {
                }

                public void TakesWrongArgs(string name)
                {
                }

                public void ValidRoute(IHttpContext context)
                {
                }
            }

            public class NoParameterlessConstructor
            {
                public NoParameterlessConstructor(string name)
                {
                }

                public void TestMethod(IHttpContext context)
                {
                }
            }
        }

        public class CanBeInvoked
        {
            [Fact]
            public void ReturnsTrueWhenMethodIsStatic()
            {
                typeof(ConcreteClass).GetMethod("StaticMethod").CanBeInvoked().ShouldBeTrue();
            }

            [Fact]
            public void ReturnsTrueWhenMethodIsNotStatic()
            {
                typeof(ConcreteClass).GetMethod("NonStaticMethod").CanBeInvoked().ShouldBeTrue();
            }

            [Fact]
            public void ReturnsFalseWhenMethodIsAbstract()
            {
                typeof(AbstractClass).GetMethod("AbstractMethod").CanBeInvoked().ShouldBeFalse();
            }

            [Fact]
            public void ReturnsFalseWhenReflectedTypeIsNull()
            {
                var methodInfo = Substitute.For<MethodInfo>();
                methodInfo.ReflectedType.ShouldBeNull();
                methodInfo.CanBeInvoked().ShouldBeFalse();
            }

            [Fact]
            public void ReturnsFalseWhenReflectedTypeIsInterface()
            {
                typeof(ISomeInterface).GetMethod("SomeMethod").CanBeInvoked().ShouldBeFalse();
            }

            [Fact]
            public void ReturnsFalseWhenReflectedTypeIsStruct()
            {
                typeof(SomeStruct).GetMethod("SomeMethod").CanBeInvoked().ShouldBeFalse();
            }

            [Fact]
            public void ReturnsFalseWhenReflectedTypeIsAbstract()
            {
                typeof(AbstractClass).GetMethod("RealMethod").CanBeInvoked().ShouldBeFalse();
            }

            public abstract class AbstractClass
            {
                public abstract void AbstractMethod();

                public void RealMethod()
                {
                }
            }

            public class ConcreteClass
            {
                public static void StaticMethod()
                {
                }

                public void NonStaticMethod()
                {
                }
            }

            public struct SomeStruct
            {
                public void SomeMethod()
                {
                }
            }

            public interface ISomeInterface
            {
                void SomeMethod();
            }

        }

        public class GetRouteAttributes
        {
            [Fact]
            public void ReturnsEmptyListWhenNoAttriubtes()
            {
                var list = typeof(TestClass).GetMethod("NoAttributes").GetRouteAttributes();

                list.ShouldSatisfyAllConditions
                (
                    () => list.ShouldNotBeNull(),
                    () => list.ShouldBeEmpty()
                );
            }

            [Fact]
            public void ReturnsListWhenSingleAttribute()
            {
                var list = typeof(TestClass).GetMethod("SingleAttribute").GetRouteAttributes();

                list.ShouldSatisfyAllConditions
                (
                    () => list.ShouldNotBeNull(),
                    () => list.Count().ShouldBe(1)
                );
            }

            [Fact]
            public void ReturnsListWhenMultipleAttributes()
            {
                var list = typeof(TestClass).GetMethod("MultipleAttributes").GetRouteAttributes();

                list.ShouldSatisfyAllConditions
                (
                    () => list.ShouldNotBeNull(),
                    () => list.Count().ShouldBe(2)
                );
            }

            public class TestClass
            {
                public void NoAttributes(IHttpContext context)
                {
                }

                [RestRoute(HttpMethod = HttpMethod.CONNECT, PathInfo = "Attribute1")]
                public void SingleAttribute(IHttpContext context)
                {
                }

                [RestRoute(HttpMethod = HttpMethod.DELETE, PathInfo = "AttributeA")]
                [RestRoute(HttpMethod = HttpMethod.OPTIONS, PathInfo = "AttributeB")]
                public void MultipleAttributes(IHttpContext context)
                {
                }
            }
        }

        public class IsRestRoute
        {
            [Fact]
            public void ReturnsTrueWhenAttributeExistsAndMethodIsEligible()
            {
                typeof(TestClass).GetMethod("EligibleRoute").IsRestRoute().ShouldBeTrue();
            }

            [Fact]
            public void ReturnsFalseWhenAttributeDoesNotExist()
            {
                typeof(TestClass).GetMethod("MissingAttribute").IsRestRoute().ShouldBeFalse();
            }

            [Fact]
            public void ReturnsFaleWhenAttributeExistsButMethodIsNotEligible()
            {
                typeof(TestClass).GetMethod("InEligibleRoute").IsRestRoute().ShouldBeFalse();
            }

            [Fact]
            public void ThrowsExceptionWhenFlagIsTrue()
            {
                Should.Throw<InvalidRouteMethodExceptions>(() => typeof(TestClass).GetMethod("MissingAttribute").IsRestRoute(true));
            }

            public class TestClass
            {
                [RestRoute]
                public void EligibleRoute(IHttpContext context) { }

                [RestRoute]
                public void InEligibleRoute(IHttpContext context, bool flag) { }

                public void MissingAttribute(IHttpContext context) { }
            }
        }
    }
}
