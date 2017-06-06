using System;
using Grapevine.Common;
using Shouldly;
using Xunit;

namespace Grapevine.Tests.Unit.Common
{
    public class ObjectExtensionsFacts
    {
        public class TryDisposing
        {
            [Fact]
            public void DoesNotDisposeWhenDoesNotImplementIDisposable()
            {
                var obj = new IsNotDisposable();

                obj.TryDisposing();

                obj.WasDisposed.ShouldBeFalse();
            }

            [Fact]
            public void DisposesWhenImplementsIDisposable()
            {
                var obj = new IsDisposable();

                obj.TryDisposing();

                obj.WasDisposed.ShouldBeTrue();
            }

            [Fact]
            public void DisposesWhenSecretlyImplementsIDisposable()
            {
                var obj = new IsSecretlyDisposable();

                obj.TryDisposing();

                obj.WasDisposed.ShouldBeTrue();
            }
        }
    }

    public class IsNotDisposable
    {
        public bool WasDisposed { get; private set; }

        public void Dispose()
        {
            WasDisposed = true;
        }
    }

    public class IsDisposable : IDisposable
    {
        public bool WasDisposed { get; private set; }

        public void Dispose()
        {
            WasDisposed = true;
        }
    }

    public class IsSecretlyDisposable : IDisposable
    {
        public bool WasDisposed { get; private set; }

        void IDisposable.Dispose()
        {
            WasDisposed = true;
        }
    }
}
