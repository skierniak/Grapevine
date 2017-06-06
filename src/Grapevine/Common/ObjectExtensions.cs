using System;

namespace Grapevine.Common
{
    public static class ObjectExtensions
    {
        internal static bool TryDisposing(this object obj)
        {
            if (!obj.GetType().Implements<IDisposable>()) return true;
            ((IDisposable)obj).Dispose();
            return true;
        }
    }
}
