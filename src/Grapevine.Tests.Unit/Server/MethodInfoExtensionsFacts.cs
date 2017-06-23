using System;
using System.Reflection;
using Grapevine.Core;
using Grapevine.Exceptions;
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
                var methodA = typeof(ConvertToActionHelpers.TestClass).GetMethod("MethodTakesZeroArgs");
                var methodB = typeof(ConvertToActionHelpers.TestClass).GetMethod("MethodTakesTwoArgs");

                Should.Throw<InvalidRouteMethodExceptions>(() => { methodA.ConvertToAction(); });
                Should.Throw<InvalidRouteMethodExceptions>(() => { methodB.ConvertToAction(); });
                Should.Throw<ArgumentNullException>(() => { MethodInfoExtensions.ConvertToAction(null); });
            }

            [Fact]
            public void ReturnsActionForStaticMethod()
            {
                var method = typeof(ConvertToActionHelpers.TestClass).GetMethod("StaticMethod");
                var result = method.ConvertToAction();
                result.ShouldNotBeNull();
            }

            [Fact]
            public void ReturnsActionForInstanceMethod()
            {
                var method = typeof(ConvertToActionHelpers.TestClass).GetMethod("InstanceMethod");
                var result = method.ConvertToAction();
                result.ShouldNotBeNull();
            }
        }

        public class ConvertToActionHelpers
        {
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
                typeof(IsRestRouteEligibleHelpers.TestAbstract).GetMethod("TestAbstractMethod").IsRestRouteEligible().ShouldBeFalse();
            }

            [Fact]
            public void ReturnsFalseWhenMethodInfoReflectedTypeHasNoParameterlessConstructor()
            {
                typeof(IsRestRouteEligibleHelpers.NoParameterlessConstructor).GetMethod("TestMethod").IsRestRouteEligible().ShouldBeFalse();
            }

            [Fact]
            public void ReturnsFalseWhenMethodIsSpecialName()
            {
                typeof(IsRestRouteEligibleHelpers.TestClass).GetMethod("get_TestProperty").IsRestRouteEligible().ShouldBeFalse();
            }

            [Fact]
            public void ReturnsFalseWhenMethodAcceptsMoreOrLessThanOneArgument()
            {
                typeof(IsRestRouteEligibleHelpers.TestClass).GetMethod("TakesZeroArgs").IsRestRouteEligible().ShouldBeFalse();
                typeof(IsRestRouteEligibleHelpers.TestClass).GetMethod("TakesTwoArgs").IsRestRouteEligible().ShouldBeFalse();
            }

            [Fact]
            public void ReturnsFalseWhenFirstArgumentIsNotIHttpContext()
            {
                typeof(IsRestRouteEligibleHelpers.TestClass).GetMethod("TakesWrongArgs").IsRestRouteEligible().ShouldBeFalse();
            }

            [Fact]
            public void ThrowsAggregateExceptionWhenThrowExceptionsIsTrue()
            {
                Should.Throw<InvalidRouteMethodExceptions>(
                    () => typeof(IsRestRouteEligibleHelpers.TestClass).GetMethod("TakesWrongArgs").IsRestRouteEligible(true));
            }

            [Fact]
            public void ReturnsTrueWhenMethodInfoIsEligible()
            {
                typeof(IsRestRouteEligibleHelpers.TestClass).GetMethod("ValidRoute").IsRestRouteEligible().ShouldBeTrue();
            }
        }

        public class IsRestRouteEligibleHelpers
        {
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
                typeof(CanBeInvokedHelpers.ConcreteClass).GetMethod("StaticMethod").CanBeInvoked().ShouldBeTrue();
            }

            [Fact]
            public void ReturnsTrueWhenMethodIsNotStatic()
            {
                typeof(CanBeInvokedHelpers.ConcreteClass).GetMethod("NonStaticMethod").CanBeInvoked().ShouldBeTrue();
            }

            [Fact]
            public void ReturnsFalseWhenMethodIsAbstract()
            {
                typeof(CanBeInvokedHelpers.AbstractClass).GetMethod("AbstractMethod").CanBeInvoked().ShouldBeFalse();
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
                typeof(CanBeInvokedHelpers.ISomeInterface).GetMethod("SomeMethod").CanBeInvoked().ShouldBeFalse();
            }

            [Fact]
            public void ReturnsFalseWhenReflectedTypeIsStruct()
            {
                typeof(CanBeInvokedHelpers.SomeStruct).GetMethod("SomeMethod").CanBeInvoked().ShouldBeFalse();
            }

            [Fact]
            public void ReturnsFalseWhenReflectedTypeIsAbstract()
            {
                typeof(CanBeInvokedHelpers.AbstractClass).GetMethod("RealMethod").CanBeInvoked().ShouldBeFalse();
            }
        }

        public class CanBeInvokedHelpers
        {
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
    }
}
