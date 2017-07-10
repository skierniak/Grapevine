using System;
namespace Grapevine.Core.Exceptions
{
    public class MissingConstructorException : Exception
    {
        public MissingConstructorException(string msg) : base(msg) { }
    }
}
