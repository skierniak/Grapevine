using System;
using Grapevine.Properties;

namespace Grapevine.Core.Exceptions
{
    /// <summary>
    /// Thrown when there is an attempt to modify the Protocol, Host, Port or Connections property of a running instance of RestServer.
    /// </summary>
    public class ServerStateException : Exception
    {
        public ServerStateException() : base(Messages.ServerStateException) { }
    }
}
