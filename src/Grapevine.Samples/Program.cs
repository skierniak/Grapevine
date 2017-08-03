using Grapevine.Core.Logging;
using Grapevine.Server;

namespace Grapevine.Samples
{
    public class Program
    {
        public static void Main(string[] args)
        {
            //GrapevineLogManager.Provider = new NLogLoggingProvider();
            GrapevineLogManager.LogToConsole();

            var server = new RestServer();
            //using (var server = new RestServer())
            //{
                server.BeforeStarting += _ => { (_ as RestServer)?.Logger.Info("Starting Server"); };
                server.AfterStarting += _ => { (_ as RestServer)?.Logger.Info("Server Started"); };
                server.BeforeStopping += _ => { (_ as RestServer)?.Logger.Info("Stopping Server"); };
                server.AfterStopping += _ => { (_ as RestServer)?.Logger.Info("Server Stopped"); };

                // register routes here

                server.Start();
                System.Console.ReadLine();
                //server.Stop();
            //}
        }
    }
}
