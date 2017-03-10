using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Grapevine.Interfaces
{
    public interface IDynamicProperties
    {
        /// <summary>
        /// Returns an IDictionary object available for adding application-specific properties at run-time
        /// </summary>
        IDictionary<string, object> Properties { get; }
    }

    public abstract class DynamicProperties
    {
        private ConcurrentDictionary<string, object> _properties;

        public IDictionary<string, object> Properties
        {
            get
            {
                if (_properties != null) return _properties;
                _properties = new ConcurrentDictionary<string, object>();
                return _properties;
            }
        }

    }

    public static class DynamicPropertiesExtensions
    {
        public static T GetPropertyValueAs<T>(this IDynamicProperties props, string key)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));
            if (string.IsNullOrWhiteSpace(key)) throw new ArgumentException(nameof(key));

            if (!props.Properties.ContainsKey(key)) throw new DynamicValueNotFoundException(key);

            var property = props.Properties[key];
            if (property is T) return (T)property;

            var result = Convert.ChangeType(property, typeof(T));
            return (T)result;
        }

        public static bool ContainsProperty(this IDynamicProperties props, string key)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));
            if (string.IsNullOrWhiteSpace(key)) throw new ArgumentException(nameof(key));
            return props.Properties.ContainsKey(key);
        }
    }

    public class DynamicValueNotFoundException : Exception
    {
        public DynamicValueNotFoundException(string key) : base($"Propery {key} not found") { }
    }
}
