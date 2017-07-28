using System;
using System.Collections.Generic;
using System.Net;
using Grapevine.Common;
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
        [Fact]
        public void DoesNotStopNonListeningServerWhenDisposing()
        {
            var listener = Substitute.For<IHttpListener>();
            var stopped = false;

            var server = new RestServer(listener) {TestingMode = true};
            server.BeforeStopping += restServer => { stopped = true; };

            server.Dispose();
            listener.Received().Close();
            stopped.ShouldBeFalse();
        }

        [Fact]
        public void DoesNotThrowExceptionWhileDisposingWhenListenerIsNull()
        {
            var stopped = false;

            var server = new RestServer { TestingMode = true };
            server.BeforeStopping += restServer => { stopped = true; };
            server.Listener = null;

            server.Dispose();
            stopped.ShouldBeFalse();
        }

        [Fact]
        public void StopsServerWhenDisposing()
        {
            var listener = Substitute.For<IHttpListener>();
            listener.IsListening.Returns(true);
            var stopped = false;

            var server = new RestServer(listener) { TestingMode = true };
            server.BeforeStopping += restServer => { stopped = true; };

            server.Dispose();
            listener.Received().Close();
            stopped.ShouldBeTrue();
        }

        [Fact]
        public void StartsAndStops()
        {
            using (var server = new RestServer { TestingMode = true })
            {
                server.Port = PortFinder.FindNextLocalOpenPort(10234);
                server.Start();
            }
        }

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

        public class Start
        {
            [Fact]
            public void DoesNotStartWhenAlreadyListening()
            {
                var started = false;

                var listener = Substitute.For<IHttpListener>();
                listener.IsListening.Returns(true);

                var server = new RestServer(listener) { TestingMode = true };
                server.BeforeStarting += restServer => { started = true; };

                server.Start();
                started.ShouldBeFalse();
            }

            [Fact]
            public void DoesNotStartWhenAlreadyStarting()
            {
                var started = false;

                var server = new RestServer() { TestingMode = true };
                server.IsStarting = true;
                server.BeforeStarting += restServer => { started = true; };

                server.Start();
                started.ShouldBeFalse();
            }

            [Fact]
            public void ThrowsExceptionWhenStartIsAttepmtedWhileStopping()
            {
                var server = new RestServer();
                server.IsStopping = true;

                Should.Throw<UnableToStartHostException>(() => { server.Start(); });
            }

            [Fact]
            public void ScansForRoutesIfRoutingTableIsEmpty()
            {
                var listener = Substitute.For<IHttpListener>();

                var router = Substitute.For<IRouter>();
                router.RoutingTable.Returns(new List<IRoute>());

                var server = new RestServer(listener) {Router = router, TestingMode = true};

                server.Start();

                router.Received().Scan();
            }

            [Fact]
            public void DoesNotScanForRoutesIfRoutingTableIsNotEmpty()
            {
                var listener = Substitute.For<IHttpListener>();

                var router = Substitute.For<IRouter>();
                router.RoutingTable.Returns(new List<IRoute> { new Route(context => { }, HttpMethod.ALL, string.Empty) });

                var server = new RestServer(listener) {Router = router, TestingMode = true};

                server.Start();

                router.DidNotReceive().Scan();
            }

            [Fact]
            public void ExecutesAfterEventsWhenListenerIsListening()
            {
                var executed = false;

                var listener = Substitute.For<IHttpListener>();
                listener.When(x => x.Start()).Do(x => { listener.IsListening.Returns(true); });

                var server = new RestServer(listener) { TestingMode = true };
                server.AfterStarting += restServer => { executed = true; };
                server.Start();

                executed.ShouldBeTrue();
            }

            [Fact]
            public void ThrowsUnableToStartHostExceptionWhenExceptionsAreThrown()
            {
                var executed = false;

                var listener = Substitute.For<IHttpListener>();
                listener.When(x => x.Start()).Do(x => { throw new Exception(); });

                var server = new RestServer(listener);
                server.AfterStarting += restServer => { executed = true; };

                Should.Throw<UnableToStartHostException>(() => server.Start());

                executed.ShouldBeFalse();
                server.IsStarting.ShouldBeFalse();
            }

            [Fact(Skip = "Local only when running ArangoDB")]
            public void ThrowsBetterExceptionMessageWhenPortIsInUse()
            {
                const string port = "8529";

                using (var testServer = new RestServer { Port = port })
                {
                    var ushe = Should.Throw<UnableToStartHostException>(() => { testServer.Start(); });
                    ushe.Message.ShouldBe($"Grapevine is unable to start because another process is already running on port {port}");
                }
            }
        }

        public class Stop
        {
            [Fact]
            public void DoesNotStopWhenNotAlreadyListening()
            {
                var stopped = false;

                var server = new RestServer() { TestingMode = true };
                server.BeforeStopping += restServer => { stopped = true; };

                server.Stop();
                stopped.ShouldBeFalse();
            }

            [Fact]
            public void DoesNotStopWhenAlreadyStopping()
            {
                var stopped = false;

                var listener = Substitute.For<IHttpListener>();
                listener.IsListening.Returns(true);

                var server = new RestServer(listener) { TestingMode = true };
                server.IsStopping = true;
                server.BeforeStopping += restServer => { stopped = true; };

                server.Stop();
                stopped.ShouldBeFalse();
            }

            [Fact]
            public void ThrowsExceptionWhenStopIsAttepmtedWhileStarting()
            {
                var listener = Substitute.For<IHttpListener>();
                listener.IsListening.Returns(true);

                var server = new RestServer(listener) { TestingMode = true };
                server.IsStarting = true;

                Should.Throw<UnableToStopHostException>(() => { server.Stop(); });
            }

            [Fact]
            public void AfterStoppingEventsOnlyFiredIfListenerStops()
            {
                var stopped = false;

                var listener = Substitute.For<IHttpListener>();
                listener.IsListening.Returns(true);

                var server = new RestServer(listener) { TestingMode = true };
                server.AfterStopping += restServer => stopped = true;

                server.Stop();

                stopped.ShouldBeFalse();
            }

            [Fact]
            public void ExecutionOrder()
            {
                const string before = "OnBeforeStopping";
                const string during = "Listener.Stop";
                const string after = "OnAfterStopping";
                var events = new List<string>();

                var listener = Substitute.For<IHttpListener>();
                listener.IsListening.Returns(true);
                listener.When(x => x.Stop()).Do(x => { events.Add(during);
                    listener.IsListening.Returns(false);
                });

                var server = new RestServer(listener) { TestingMode = true };
                server.BeforeStopping += restServer => events.Add(before);
                server.AfterStopping += restServer => events.Add(after);

                server.Stop();

                events.Count.ShouldBe(3);
                events[0].ShouldBe(before);
                events[1].ShouldBe(during);
                events[2].ShouldBe(after);
            }

            [Fact]
            public void ThrowsUnableToStopHostExceptionWhenExceptionsAreThrown()
            {
                var executed = false;

                var listener = Substitute.For<IHttpListener>();
                listener.IsListening.Returns(true);
                listener.When(x => x.Stop()).Do(x => { throw new Exception(); });

                var server = new RestServer(listener) { TestingMode = true };
                server.AfterStopping += restServer => { executed = true; };

                Should.Throw<UnableToStopHostException>(() => server.Stop());

                executed.ShouldBeFalse();
                server.IsStopping.ShouldBeFalse();
            }

        }

        public class EventHandlers
        {
            public RestServer Server = new RestServer();

            [Fact]
            public void BeforeStartingExecutionOrder()
            {
                var results = new List<int>();

                Server.BeforeStarting += server => { results.Add(1); };
                Server.BeforeStarting += server => { results.Add(2); };

                Server.OnBeforeStarting();

                results.Count.ShouldBe(2);
                results[0].ShouldBe(1);
                results[1].ShouldBe(2);
            }

            [Fact]
            public void BeforeStartingContinuesWhenIsNull()
            {
                Should.NotThrow(() => { Server.OnBeforeStarting(); });
            }

            [Fact]
            public void BeforeStartingThrowsAggregateExceptionWhenExceptionsAreEncountered()
            {
                Server.BeforeStarting += server => { throw new Exception("Exception 1"); };
                Server.BeforeStarting += server => { throw new Exception("Exception 2"); };

                Should.Throw<AggregateException>(() => { Server.OnBeforeStarting(); });
            }

            [Fact]
            public void AfterStartingExecutionOrder()
            {
                var results = new List<int>();

                Server.AfterStarting += server => { results.Add(1); };
                Server.AfterStarting += server => { results.Add(2); };

                Server.OnAfterStarting();

                results.Count.ShouldBe(2);
                results[0].ShouldBe(2);
                results[1].ShouldBe(1);
            }

            [Fact]
            public void AfterStartingContinuesWhenIsNull()
            {
                Should.NotThrow(() => { Server.OnAfterStarting(); });
            }

            [Fact]
            public void AfterStartingThrowsAggregateExceptionWhenExceptionsAreEncountered()
            {
                Server.AfterStarting += server => { throw new Exception("Exception 1"); };
                Server.AfterStarting += server => { throw new Exception("Exception 2"); };

                Should.Throw<AggregateException>(() => { Server.OnAfterStarting(); });
            }

            [Fact]
            public void BeforeStoppingExecutionOrder()
            {
                var results = new List<int>();

                Server.BeforeStopping += server => { results.Add(1); };
                Server.BeforeStopping += server => { results.Add(2); };

                Server.OnBeforeStopping();

                results.Count.ShouldBe(2);
                results[0].ShouldBe(1);
                results[1].ShouldBe(2);
            }

            [Fact]
            public void BeforeStopptingContinuesWhenIsNull()
            {
                Should.NotThrow(() => { Server.OnBeforeStopping(); });
            }

            [Fact]
            public void BeforeStoppingThrowsAggregateExceptionWhenExceptionsAreEncountered()
            {
                Server.BeforeStopping += server => { throw new Exception("Exception 1"); };
                Server.BeforeStopping += server => { throw new Exception("Exception 2"); };

                Should.Throw<AggregateException>(() => { Server.OnBeforeStopping(); });
            }

            [Fact]
            public void AfterStoppingExecutionOrder()
            {
                var results = new List<int>();

                Server.AfterStopping += server => { results.Add(1); };
                Server.AfterStopping += server => { results.Add(2); };

                Server.OnAfterStopping();

                results.Count.ShouldBe(2);
                results[0].ShouldBe(2);
                results[1].ShouldBe(1);
            }

            [Fact]
            public void AfterStoppingContinuesWhenIsNull()
            {
                Should.NotThrow(() => { Server.OnAfterStopping(); });
            }

            [Fact]
            public void AfterStoppingThrowsAggregateExceptionWhenExceptionsAreEncountered()
            {
                Server.AfterStopping += server => { throw new Exception("Exception 1"); };
                Server.AfterStopping += server => { throw new Exception("Exception 2"); };

                Should.Throw<AggregateException>(() => { Server.OnAfterStopping(); });
            }
        }
    }
}
