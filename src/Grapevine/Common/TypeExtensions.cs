using System;
using System.Linq;
using System.Reflection;

namespace Grapevine.Common
{
    public static class TypeExtensions
    {
        /// <summary>
        /// Returns a value indication whether the generic type implements the type parameter specified
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="type"></param>
        /// <returns></returns>
        internal static bool Implements<T>(this Type type)
        {
            return type.GetInterfaces().Contains(typeof(T));
        }

        /// <summary>
        /// Returns a value indicating whether the type has a constructor that takes no parameters
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        internal static bool HasParameterlessConstructor(this Type type)
        {
            return type.GetConstructor(Type.EmptyTypes) != null || type.GetConstructors().Any(ctor => ctor.GetParameters()[0].IsOptional);
        }
    }
}
