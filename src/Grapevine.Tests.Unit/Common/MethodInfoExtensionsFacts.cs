using System.Reflection;
using Grapevine.Common;
using NSubstitute;
using Shouldly;
using Xunit;

namespace Grapevine.Tests.Unit.Common
{
    public class MethodInfoExtensionsFacts
    {
        public class ConvertToAction
        {
        }

        public class IsRestRouteEligible
        {
        }

        public class CanInvoke
        {
            [Fact]
            public void ReturnsTrueWhenMethodIsStatic()
            {
                typeof(ConcreteClass).GetMethod("StaticMethod").CanInvoke().ShouldBeTrue();
            }

            [Fact]
            public void ReturnsTrueWhenMethodIsNotStatic()
            {
                typeof(ConcreteClass).GetMethod("NonStaticMethod").CanInvoke().ShouldBeTrue();
            }

            [Fact]
            public void ReturnsFalseWhenMethodIsAbstract()
            {
                typeof(AbstractClass).GetMethod("AbstractMethod").CanInvoke().ShouldBeFalse();
            }

            [Fact]
            public void ReturnsFalseWhenReflectedTypeIsNull()
            {
                var methodInfo = Substitute.For<MethodInfo>();
                methodInfo.ReflectedType.ShouldBeNull();
                methodInfo.CanInvoke().ShouldBeFalse();
            }

            [Fact]
            public void ReturnsFalseWhenReflectedTypeIsInterface()
            {
                typeof(ISomeInterface).GetMethod("SomeMethod").CanInvoke().ShouldBeFalse();
            }

            [Fact]
            public void ReturnsFalseWhenReflectedTypeIsStruct()
            {
                typeof(SomeStruct).GetMethod("SomeMethod").CanInvoke().ShouldBeFalse();
            }

            [Fact]
            public void ReturnsFalseWhenReflectedTypeIsAbstract()
            {
                typeof(AbstractClass).GetMethod("RealMethod").CanInvoke().ShouldBeFalse();
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
    }
}
