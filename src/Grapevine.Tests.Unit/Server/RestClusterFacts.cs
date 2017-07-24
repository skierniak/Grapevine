using System.Collections.Generic;
using Grapevine.Core;
using Grapevine.Server;
using NSubstitute;
using Shouldly;
using Xunit;

namespace Grapevine.Tests.Unit.Server
{
    public class RestClusterFacts
    {
        protected const string GlobalPrefix = "GlobalPrefix";
        protected IRestServer Server = Substitute.For<IRestServer>();
        protected RestCluster Cluster = new RestCluster();
        protected List<Handlers> Raised = new List<Handlers>();

        public RestClusterFacts()
        {
            Cluster.BeforeStartingAll += () => { Raised.Add(Handlers.BeforeStartAll); };
            Cluster.BeforeStartingEach += _ => { Raised.Add(Handlers.BeforeStartEach); };
            Cluster.AfterStartingEach += _ => { Raised.Add(Handlers.AfterStartEach); };
            Cluster.AfterStartingAll += () => { Raised.Add(Handlers.AfterStartAll); };

            Cluster.BeforeStoppingAll += () => { Raised.Add(Handlers.BeforeStopAll); };
            Cluster.BeforeStoppingEach += _ => { Raised.Add(Handlers.BeforeStopEach); };
            Cluster.AfterStoppingEach += _ => { Raised.Add(Handlers.AfterStopEach); };
            Cluster.AfterStoppingAll += () => { Raised.Add(Handlers.AfterStopAll); };

            Server.When(x => x.Start()).Do(_ => { Raised.Add(Handlers.ServerStart); });
            Server.When(x => x.Stop()).Do(_ => { Raised.Add(Handlers.ServerStop); });
            Server.ListenerPrefix.Returns(GlobalPrefix);
        }

        [Fact]
        public void AddGetRemoveServers()
        {
            var a = new StubServer("servera");
            var b = new StubServer("serverb");
            var c = new StubServer("serverc");
            var d = new StubServer("serverd");

            var cluster = new RestCluster();
            cluster.ShouldNotBeNull();
            cluster.Count.ShouldBe(0);

            cluster.Add("servera", a);
            cluster.Add("serverb", b);

            cluster.Count.ShouldBe(2);
            cluster.Get("servera").ShouldBe(a);
            cluster.Get("serverb").ShouldBe(b);

            cluster["serverc"] = c;
            cluster.Count.ShouldBe(3);
            cluster.Get("serverc").ShouldBe(c);

            cluster.Remove("serverd").ShouldBeTrue();
            cluster.Count.ShouldBe(3);

            cluster.Remove("servera").ShouldBeTrue();
            cluster.Count.ShouldBe(2);

            cluster.Add(d);
            cluster.Count.ShouldBe(3);
            cluster[d.ListenerPrefix].ShouldBe(d);
            cluster.Get(d.ListenerPrefix).ShouldBe(d);
        }

        [Fact]
        public void GetReturnsNullWhenItDoesNotExist()
        {
            Cluster.Add(Server);

            Cluster.Get(GlobalPrefix).ShouldBe(Server);
            Cluster.Get("NotAvailable").ShouldBeNull();
        }

        [Fact]
        public void ServersAddAfterClusterStartsShouldBeStarted()
        {
            var expected = new List<Handlers> {Handlers.BeforeStartEach, Handlers.ServerStart, Handlers.AfterStartEach};

            Cluster.Started = true;
            Cluster.Add(Server);

            Server.Received().Start();
            Raised.ShouldBe(expected);
        }

        [Fact]
        public void ServersAddBeforeClusterStartsShouldNotBeStarted()
        {
            Cluster.Add(Server);

            Server.DidNotReceive().Start();
            Raised.Count.ShouldBe(0);
        }

        [Fact]
        public void ServersRemovedAfterStartedShouldBeStopped()
        {
            var expected = new List<Handlers> {Handlers.BeforeStopEach, Handlers.ServerStop, Handlers.AfterStopEach};

            Cluster.Add(Server);
            Cluster.Started = true;

            Cluster.Remove(Server.ListenerPrefix);

            Server.Received().Stop();
            Raised.ShouldBe(expected);
        }

        [Fact]
        public void ServersRemovedBeforeStartedShouldNotBeStopped()
        {
            Cluster.Add(Server);

            Cluster.Remove(Server.ListenerPrefix);

            Server.DidNotReceive().Stop();
            Raised.Count.ShouldBe(0);
        }

        [Fact]
        public void StartAll()
        {
            var expected = new List<Handlers>
            {
                Handlers.BeforeStartAll,
                Handlers.BeforeStartEach,
                Handlers.AfterStartEach,
                Handlers.BeforeStartEach,
                Handlers.AfterStartEach,
                Handlers.BeforeStartEach,
                Handlers.AfterStartEach,
                Handlers.AfterStartAll
            };

            var a = new StubServer("servera");
            var b = new StubServer("serverb");
            var c = new StubServer("serverc");
            var d = new StubServer("serverd") {IsListening = true};

            Cluster.Add(a);
            Cluster.Add(b);
            Cluster.Add(c);
            Cluster.Add(d);

            Cluster.StartAll();

            a.IsListening.ShouldBeTrue();
            b.IsListening.ShouldBeTrue();
            c.IsListening.ShouldBeTrue();
            d.IsListening.ShouldBeTrue();

            Raised.ShouldBe(expected);
        }

        [Fact]
        public void StartAllDoesNotRunIfStartedIsTrue()
        {
            Cluster.Add(Server);
            Cluster.Started = true;

            Cluster.StartAll();

            Server.DidNotReceive().Start();
            Raised.Count.ShouldBe(0);
        }

        [Fact]
        public void StopAll()
        {
            var expected = new List<Handlers>
            {
                Handlers.BeforeStopAll,
                Handlers.BeforeStopEach,
                Handlers.AfterStopEach,
                Handlers.BeforeStopEach,
                Handlers.AfterStopEach,
                Handlers.BeforeStopEach,
                Handlers.AfterStopEach,
                Handlers.AfterStopAll
            };

            var a = new StubServer("servera");
            var b = new StubServer("serverb") { IsListening = true };
            var c = new StubServer("serverc") { IsListening = true };
            var d = new StubServer("serverd") { IsListening = true };

            Cluster.Add(a);
            Cluster.Add(b);
            Cluster.Add(c);
            Cluster.Add(d);

            Cluster.Started = true;
            Cluster.StopAll();

            a.IsListening.ShouldBeFalse();
            b.IsListening.ShouldBeFalse();
            c.IsListening.ShouldBeFalse();
            d.IsListening.ShouldBeFalse();

            Raised.ShouldBe(expected);
        }

        [Fact]
        public void StopAllDoesNotRunIfStartedIsFalse()
        {
            Cluster.Add(Server);

            Cluster.StopAll();

            Server.DidNotReceive().Stop();
            Raised.Count.ShouldBe(0);
        }

        [Fact]
        public void StartAndStopDoesNotErrorWhenNoHandlersAreDefined()
        {
            var servera = new StubServer("servera");
            var serverb = new StubServer("serverb");
            var serverc = new StubServer("serverc");
            var serverd = new StubServer("serverd");

            var cluster = new RestCluster();
            cluster.Add(servera);
            cluster.Add(serverb);
            cluster.Add(serverc);
            cluster.Add(serverd);

            cluster.StartAll();

            servera.IsListening.ShouldBeTrue();
            serverb.IsListening.ShouldBeTrue();
            serverc.IsListening.ShouldBeTrue();
            serverd.IsListening.ShouldBeTrue();

            cluster.StopAll();

            servera.IsListening.ShouldBeFalse();
            serverb.IsListening.ShouldBeFalse();
            serverc.IsListening.ShouldBeFalse();
            serverd.IsListening.ShouldBeFalse();
        }
    }

    public enum Handlers
    {
        BeforeStartAll,
        BeforeStartEach,
        AfterStartEach,
        AfterStartAll,
        BeforeStopAll,
        BeforeStopEach,
        AfterStopEach,
        AfterStopAll,
        ServerStart,
        ServerStop
    }

    public class StubServer : IRestServer
    {
        public IDictionary<string, object> Properties { get; }
        public void Dispose()
        {
            throw new System.NotImplementedException();
        }

        public event ServerEventHandler AfterStarting;
        public event ServerEventHandler AfterStopping;
        public event ServerEventHandler BeforeStarting;
        public event ServerEventHandler BeforeStopping;
        public IList<IContentFolder> ContentFolders { get; }
        public string Host { get; set; }
        public bool IsListening { get; set; }
        public IHttpListener Listener { get; }
        public string ListenerPrefix { get; }
        public string Port { get; set; }
        public IRouter Router { get; set; }
        public bool UseHttps { get; set; }
        public void Start()
        {
            IsListening = true;
        }

        public void Stop()
        {
            IsListening = false;
        }

        public StubServer(string prefix)
        {
            ListenerPrefix = prefix;
        }
    }
}
