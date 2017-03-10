using System;
using System.Diagnostics;
using System.Reflection;

namespace Grapevine.Logging
{
    /// <summary>
    /// Manages logging for Grapevine, used to set the loggging provider.
    /// </summary>
    public static class GrapevineLogManager
    {
        private static IGrapevineLoggingProvider _provider;
        private static bool _providerRetrieved;

        static GrapevineLogManager()
        {
            var assembly = Assembly.GetEntryAssembly();
            Provider = (assembly != null) ? (IGrapevineLoggingProvider)new NoOpLoggingProvider() : (IGrapevineLoggingProvider)new InMemoryLoggingProvider();
        }

        /// <summary>
        /// The logging provider used for logging in Npgsql.
        /// </summary>
        public static IGrapevineLoggingProvider Provider
        {
            get
            {
                _providerRetrieved = true;
                return _provider;
            }
            set
            {
                if (_providerRetrieved)
                    throw new InvalidOperationException(
                        "The logging provider must be set before any Grapevine action is taken");
                _provider = value;
            }
        }

        internal static GrapevineLogger CreateLogger(string name)
        {
            return Provider.CreateLogger(name);
        }

        internal static GrapevineLogger CreateLogger(Type type)
        {
            return Provider.CreateLogger(type.Name);
        }

        internal static GrapevineLogger CreateLogger<T>()
        {
            return Provider.CreateLogger(typeof(T).Name);
        }

        internal static GrapevineLogger GetCurrentClassLogger()
        {
            return CreateLogger(GetClassFullName());
        }

        private static string GetClassFullName()
        {
            string className;
            var framesToSkip = 2;

            Type declaringType;
            do
            {
                var frame = new StackTrace().GetFrame(framesToSkip);
                var method = frame.GetMethod();

                declaringType = method.DeclaringType;
                if (declaringType == null)
                {
                    className = method.Name;
                    break;
                }

                framesToSkip++;
                className = declaringType.FullName;
            } while (declaringType.Module.Name.Equals("mscorlib.dll", StringComparison.OrdinalIgnoreCase));

            return className;
        }
    }
}
