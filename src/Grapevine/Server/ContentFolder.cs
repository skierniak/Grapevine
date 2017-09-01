using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Grapevine.Core;
using Grapevine.Common;

namespace Grapevine.Server
{
    public interface IContentFolder : IDisposable
    {
        /// <summary>
        /// Gets or sets the default file to return when a directory is requested
        /// </summary>
        string IndexFileName { get; set; }

        /// <summary>
        /// Gets or sets the optional prefix for specifying when static content should be returned
        /// </summary>
        string Prefix { get; set; }

        /// <summary>
        /// Gets the folder used when scanning for static content requests
        /// </summary>
        string FolderPath { get; }

        /// <summary>
        /// Send file specified by the inbound http context (if exists)
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        void SendFile(IHttpContext context);
    }

    public class ContentFolder : IContentFolder
    {
        protected ConcurrentDictionary<string, string> DirectoryList { get; set; }

        public static string DefaultFolderName { get; } = "public";
        public static string DefaultIndexFileName { get; } = "index.html";

        private FileSystemWatcher _watcher;
        private string _indexFileName = DefaultIndexFileName;
        private string _prefix;
        private string _path;

        public ContentFolder() : this(Path.Combine(Directory.GetCurrentDirectory(), DefaultFolderName), string.Empty) { }

        public ContentFolder(string path) : this(path, string.Empty) { }

        public ContentFolder(string path, string prefix)
        {
            DirectoryList = new ConcurrentDictionary<string, string>();
            FolderPath = Path.GetFullPath(path);
            Prefix = prefix;

            Watcher = new FileSystemWatcher
            {
                Path = FolderPath,
                Filter = "*",
                EnableRaisingEvents = true,
                IncludeSubdirectories = true,
                NotifyFilter = NotifyFilters.FileName
            };

            Watcher.Created += (sender, args) => { AddToDirectoryList(args.FullPath); };
            Watcher.Deleted += (sender, args) => { RemoveFromDirectoryList(args.FullPath); };
            Watcher.Renamed += (sender, args) => { RenameInDirectoryList(args.OldFullPath, args.FullPath); };

            PopulateDirectoryList();
        }

        public string IndexFileName
        {
            get => _indexFileName;
            set
            {
                if (string.IsNullOrWhiteSpace(value) || value == _indexFileName) return;
                _indexFileName = value;
                PopulateDirectoryList();
            }
        }

        public string Prefix
        {
            get => _prefix;
            set
            {
                var prefix = string.IsNullOrWhiteSpace(value) ? string.Empty : $"/{value.Trim().TrimStart('/').TrimEnd('/').Trim()}";
                if (prefix.Equals(_prefix)) return;

                _prefix = prefix;
                PopulateDirectoryList();
            }
        }

        public string FolderPath
        {
            get => _path;
            protected internal set
            {
                var path = Path.GetFullPath(value);
                if (!Directory.Exists(path)) path = Directory.CreateDirectory(path).FullName;
                _path = path;
            }
        }

        public FileSystemWatcher Watcher
        {
            get => _watcher;
            protected internal set
            {
                if (value == null || value == _watcher) return;
                var tmpwatcher = _watcher;
                _watcher = value;
                tmpwatcher?.Dispose();
            }
        }

        public IDictionary<string, string> DirectoryListing => DirectoryList;

        public void SendFile(IHttpContext context)
        {
            if (DirectoryList.ContainsKey(context.Request.PathInfo))
            {
                var filepath = DirectoryList[context.Request.PathInfo];

                var lastModified = File.GetLastWriteTimeUtc(filepath).ToString("R");
                context.Response.AddHeader("Last-Modified", lastModified);

                if (context.Request.Headers.AllKeys.Contains("If-Modified-Since"))
                {
                    if (context.Request.Headers["If-Modified-Since"].Equals(lastModified))
                    {
                        context.Response.SendResponse(HttpStatusCode.NotModified);
                        return;
                    }
                }

                context.Response.StatusCode = HttpStatusCode.Ok;
                context.Response.ContentType = ContentTypes.FromExtension(filepath);
                context.Response.SendResponse(File.ReadAllBytes(filepath));
            }

            if (!string.IsNullOrWhiteSpace(Prefix) && context.Request.PathInfo.StartsWith(Prefix) && !context.WasRespondedTo)
            {
                context.Response.StatusCode = HttpStatusCode.NotFound;
            }
        }

        protected void PopulateDirectoryList()
        {
            DirectoryList.Clear();
            foreach (var item in Directory.GetFiles(FolderPath, "*", SearchOption.AllDirectories).ToList())
            {
                AddToDirectoryList(item);
            }
        }

        protected void AddToDirectoryList(string fullPath)
        {
            DirectoryList[CreateDirectoryListKey(fullPath)] = fullPath;
            if (fullPath.EndsWith($"\\{_indexFileName}"))
                DirectoryList[CreateDirectoryListKey(fullPath.Replace($"\\{_indexFileName}", ""))] = fullPath;
        }

        protected void RemoveFromDirectoryList(string fullPath)
        {
            DirectoryList.Where(x => x.Value == fullPath).ToList().ForEach(pair =>
            {
                string key;
                DirectoryList.TryRemove(pair.Key, out key);
            });
        }

        protected void RenameInDirectoryList(string oldFullPath, string newFullPath)
        {
            RemoveFromDirectoryList(oldFullPath);
            AddToDirectoryList(newFullPath);
        }

        protected string CreateDirectoryListKey(string item)
        {
            return $"{Prefix}{item.Replace(FolderPath, string.Empty).Replace(@"\", "/")}";
        }

        public void Dispose()
        {
            _watcher.Dispose();
        }
    }
}
