using System;
using System.Linq;
using Grapevine.Properties;

namespace Grapevine.Core.Exceptions
{
    public class MissingConstructorException : Exception
    {
        public MissingConstructorException(Type type, params Type[] types) : base(string.Format(
            Messages.MissingAppropriateConstructor, type.FullName, types.Length,
            types.Select(x => x.FullName).Aggregate((c, n) => $"{c}, {n}")))
        {
        }
    }
}
