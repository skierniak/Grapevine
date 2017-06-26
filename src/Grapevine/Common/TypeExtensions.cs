using System;
using System.Linq;
using Grapevine.Server;

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

        /// <summary>
        /// Returns true if the type is a valid RestResource that can be scanned.
        /// </summary>
        internal static bool IsRestResource(this Type type)
        {
            if (type.IsAbstract || !type.IsClass) return false;
            return type.GetCustomAttributes(true).Any(a => a is RestResource);
        }

        /// <summary>
        /// Returns the value of the RestResource attribute; returns null if the type does not have a RestResource attribute.
        /// </summary>
        internal static RestResource GetRestResource(this Type type)
        {
            if (!type.IsRestResource()) return null;
            return (RestResource)type.GetCustomAttributes(true).First(a => a is RestResource);
        }
    }
}
