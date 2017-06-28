using Grapevine.Common;
using Grapevine.Server;
using NSubstitute;
using Shouldly;
using Xunit;

namespace Grapevine.Tests.Unit.Common
{
    public class TypeExtensionsFacts
    {
        public class Implements
        {
            [Fact]
            public void ReturnsTrueWhenImplements()
            {
                typeof(TestImplementation).Implements<ITestInterface>().ShouldBeTrue();
            }

            [Fact]
            public void ReturnsTrueWhenSuperImplements()
            {
                typeof(IntermediateImplementation).Implements<ITestInterface>().ShouldBeTrue();
            }

            [Fact]
            public void ReturnsFalseWhenDoesNotImplement()
            {
                typeof(NotATestImplementation).Implements<ITestInterface>().ShouldBeFalse();
            }

            [Fact]
            public void ReturnsFalseWhenInterfaceIsChild()
            {
                typeof(TestImplementation).Implements<ITestIntermediate>().ShouldBeFalse();
            }

            public interface ITestInterface
            {
                void TestMethod(string testParameter);
            }

            public interface ITestIntermediate : ITestInterface
            {
                void AnotherTestMethod();
            }

            public class TestImplementation : ITestInterface
            {
                public void TestMethod(string testParameter)
                {
                    throw new System.NotImplementedException();
                }
            }

            public class NotATestImplementation
            {
                public void TestMethod(string testParameter)
                {
                    throw new System.NotImplementedException();
                }
            }

            public class IntermediateImplementation : ITestIntermediate
            {
                public void TestMethod(string testParameter)
                {
                    throw new System.NotImplementedException();
                }

                public void AnotherTestMethod()
                {
                    throw new System.NotImplementedException();
                }
            }
        }

        public class HasParameterlessConstructor
        {
            [Fact]
            public void ReturnsTrueWhenClassHasImplicitParameterlessConstructor()
            {
                typeof(ImplicitParameterless).HasParameterlessConstructor().ShouldBeTrue();
            }

            [Fact]
            public void ReturnsTrueWhenClassHasOptionalParameterConstructor()
            {
                typeof(OptionalParameters).HasParameterlessConstructor().ShouldBeTrue();
            }

            [Fact]
            public void ReturnsTrueWhenClassHasExplicitParameterlessConstructor()
            {
                typeof(ExplicitParameterless).HasParameterlessConstructor().ShouldBeTrue();
            }

            [Fact]
            public void ReturnsTrueWhenClassHasMultipleConstructorsAndExplicitParameterlessConstructor()
            {
                typeof(MultipleWithExplicit).HasParameterlessConstructor().ShouldBeTrue();
            }

            [Fact]
            public void ReturnsFalseWhenClassHasNoParameterlessConstructor()
            {
                typeof(NoParameterless).HasParameterlessConstructor().ShouldBeFalse();
            }

            [Fact]
            public void ReturnsFalseWhenClassHasPrivateParameterlessConstructor()
            {
                typeof(PrivateConstructor).HasParameterlessConstructor().ShouldBeFalse();
            }

            public class ImplicitParameterless
            {
                public string SomeProperty { get; set; }
            }

            public class OptionalParameters
            {
                public OptionalParameters(string someProperty = null)
                {
                    SomeProperty = someProperty;
                }

                public string SomeProperty { get; set; }
            }

            public class ExplicitParameterless
            {
                public ExplicitParameterless()
                {
                    SomeProperty = "SomeProperty";
                }

                public string SomeProperty { get; set; }
            }

            public class MultipleWithExplicit
            {
                public MultipleWithExplicit()
                {
                    Property1 = "Property1";
                    Property2 = false;
                }

                public MultipleWithExplicit(bool property2)
                {
                    Property1 = "Property1";
                    Property2 = property2;
                }

                public MultipleWithExplicit(string property1)
                {
                    Property1 = property1;
                    Property2 = false;
                }

                public MultipleWithExplicit(string property1, bool property2)
                {
                    Property1 = property1;
                    Property2 = property2;
                }

                public string Property1 { get; set; }
                public bool Property2 { get; set; }
            }

            public class NoParameterless
            {
                public NoParameterless(string someProperty)
                {
                    SomeProperty = someProperty;
                }

                public string SomeProperty { get; set; }
            }

            public class PrivateConstructor
            {
                private PrivateConstructor() { }

                public string SomeProperty { get; set; }

                public PrivateConstructor Generate()
                {
                    return new PrivateConstructor();
                }
            }
        }

        public class IsRestResource
        {
            [Fact]
            public void AbstractClassesAreNotRestResources()
            {
                typeof(AbstractClass).IsRestResource().ShouldBeFalse();
            }

            [Fact]
            public void NonClassesAreNotRestResources()
            {
                typeof(NotAClass).IsRestResource().ShouldBeFalse();
            }

            [Fact]
            public void ClassesWithoutAttributeAreNotRestResources()
            {
                typeof(IsNotARestResource).IsRestResource().ShouldBeFalse();
            }

            [Fact]
            public void ClassesWithAttributeAreRestResources()
            {
                typeof(IsARestResource).IsRestResource().ShouldBeTrue();
            }

            [RestResource]
            public abstract class AbstractClass{}

            public struct NotAClass{}

            public class IsNotARestResource{}

            [RestResource]
            public class IsARestResource{}
        }

        public class GetRestResource
        {
            [Fact]
            public void ReturnsNullIfIsNotRestResource()
            {
                typeof(NotARestResource).GetRestResource().ShouldBeNull();
            }

            [Fact]
            public void ReturnsRestResourceAttribute()
            {
                var result = typeof(ValidRestResource).GetRestResource();

                result.ShouldSatisfyAllConditions
                (
                    () => result.ShouldNotBeNull(),
                    () => result.BasePath.ShouldBe("something"),
                    () => result.Scope.ShouldBe("otherthing")
                );
            }

            public class NotARestResource{}

            [RestResource(BasePath = "something", Scope = "otherthing")]
            public class ValidRestResource{}
        }
    }
}
