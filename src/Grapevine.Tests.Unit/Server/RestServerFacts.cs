using Grapevine.Core;
using Grapevine.Core.Exceptions;
using Grapevine.Server;
using NSubstitute;
using Shouldly;
using Xunit;

namespace Grapevine.Tests.Unit.Server
{
    public class RestServerFacts
    {
        public class Constructors
        {
            [Fact]
            public void Parameterless()
            {
                var server = new RestServer();
                server.ShouldNotBeNull();
            }

            [Fact]
            public void SingleParameter()
            {
                var listener = Substitute.For<IHttpListener>();
                var server = new RestServer(listener);
                server.ShouldNotBeNull();
                server.Listener.ShouldBe(listener);
            }
        }

        public class Prefix
        {
            [Fact]
            public void CanChangeDefaultValue()
            {
                var server = new RestServer();
                server.ListenerPrefix.ShouldBe("http://localhost:1234/");

                server.Port = "3456";
                server.Host = "0.0.0.0";
                server.ListenerPrefix.ShouldBe("http://+:3456/");
            }

            [Fact]
            public void GetSetHost()
            {
                var server = new RestServer();
                server.Host.ShouldBe("localhost");

                server.Host = "*";
                server.Host.ShouldBe("*");

                server.Host = "+";
                server.Host.ShouldBe("+");

                server.Host = "0.0.0.0";
                server.Host.ShouldBe("+");

                server.Host = "LOCALHOST";
                server.Host.ShouldBe("localhost");
            }

            [Fact]
            public void SetHostThrowsExceptionIfListenerIsListening()
            {
                var listener = Substitute.For<IHttpListener>();
                listener.IsListening.Returns(true);

                var server = new RestServer(listener);

                Should.Throw<ServerStateException>(() => { server.Host = "*"; });
            }

            [Fact]
            public void GetSetPort()
            {
                var server = new RestServer();
                server.Port.ShouldBe("1234");
                server.Port = "4321";
                server.Port.ShouldBe("4321");
            }

            [Fact]
            public void SetPortThrowsExceptionIfListenerIsListening()
            {
                var listener = Substitute.For<IHttpListener>();
                listener.IsListening.Returns(true);

                var server = new RestServer(listener);

                Should.Throw<ServerStateException>(() => { server.Port = "4321"; });
            }

            [Fact]
            public void GetSetProtocol()
            {
                var server = new RestServer();
                server.UseHttps.ShouldBeFalse();
                server.ListenerPrefix.StartsWith("http:").ShouldBeTrue();

                server.UseHttps = true;
                server.UseHttps.ShouldBeTrue();
                server.ListenerPrefix.StartsWith("https:").ShouldBeTrue();

                server.UseHttps = false;
                server.UseHttps.ShouldBeFalse();
                server.ListenerPrefix.StartsWith("http:").ShouldBeTrue();
            }

            [Fact]
            public void SetProtocolThrowsExceptionIfListenerIsListening()
            {
                var listener = Substitute.For<IHttpListener>();
                listener.IsListening.Returns(true);

                var server = new RestServer(listener);

                Should.Throw<ServerStateException>(() => { server.UseHttps = true; });
            }
        }
    }
}
