using System.Reflection;
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
        }

        public class ConvertToActionHelpers
        {
        }

        public class IsRestRouteEligible
        {

        }

        public class IsRestRouteEligibleHelpers
        {

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
