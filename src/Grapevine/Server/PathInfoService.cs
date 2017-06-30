using System;
using Grapevine.Common;

namespace Grapevine.Server
{
    public static class PathInfoService
    {
        internal static string GeneratePathInfo(string pathInfo, string basePath)
        {
            var pathinfo = pathInfo ?? string.Empty;
            var prefix = string.Empty;

            if (pathinfo.StartsWith("^"))
            {
                prefix = "^";
                pathinfo = pathinfo.TrimStart('^');
            }

            if (!string.IsNullOrEmpty(pathinfo) && !pathinfo.StartsWith("/")) pathinfo = $"/{pathinfo}";
            if (!string.IsNullOrEmpty(basePath) && !basePath.StartsWith("/")) basePath = $"/{basePath}";

            return $"{prefix}{basePath}{pathinfo}";
        }

        internal static string GenerateBasePath(string basePath, Type type)
        {
            var bpArgument = basePath ?? string.Empty;
            var bpOnResource = type != null && type.IsRestResource() ? type.GetRestResource().BasePath : string.Empty;

            if (string.IsNullOrWhiteSpace(bpArgument)) return bpOnResource;
            if (string.IsNullOrWhiteSpace(bpOnResource)) return bpArgument;

            bpArgument = bpArgument.TrimEnd('/');
            bpOnResource = bpOnResource.TrimStart('/');

            return $"{bpArgument}/{bpOnResource}";
        }

        internal static string SanitizeBasePath(string basePath)
        {
            var basepath = basePath?.Trim().TrimEnd('/').TrimStart('/') ?? string.Empty;
            return string.IsNullOrWhiteSpace(basepath) ? basepath : $"/{basepath}";
        }
    }
}
