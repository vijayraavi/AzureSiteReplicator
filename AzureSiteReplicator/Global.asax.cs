using AzureSiteReplicator.Controllers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace AzureSiteReplicator
{
    public class MvcApplication : System.Web.HttpApplication
    {
        private int _inUseCount;
        private DateTime _lastChangeTime;
        private DateTime _publishStartTime;

        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            StartContentFolderWatcher();
        }

        private void StartContentFolderWatcher()
        {
            var fileSystemWatcher = new FileSystemWatcher(Environment.Instance.ContentPath);
            fileSystemWatcher.Created += OnChanged;
            fileSystemWatcher.Changed += OnChanged;
            fileSystemWatcher.Deleted += OnChanged;
            fileSystemWatcher.Renamed += OnChanged;
            fileSystemWatcher.Error += OnError;
            fileSystemWatcher.IncludeSubdirectories = true;
            fileSystemWatcher.EnableRaisingEvents = true;
        }

        private void OnChanged(object sender, FileSystemEventArgs e)
        {
            Trace.TraceInformation("{0} OnChanged {1} {2}", DateTime.Now, e.FullPath, e.ChangeType);
            _lastChangeTime = DateTime.Now;

            // Start the publishing async, but don't wait for it. Just fire and forget.
            var task = Publish();
        }

        private async Task Publish()
        {
            if (Interlocked.Increment(ref _inUseCount) == 1)
            {
                _publishStartTime = DateTime.MinValue;

                // Keep publishing as long as some changes happened after we started the previous publish
                while (_publishStartTime < _lastChangeTime)
                {
                    // Wait till there are no change notifications for a while, so we don't start deploying while
                    // files are still being copied to the source folder
                    while (DateTime.Now - _lastChangeTime < TimeSpan.FromMilliseconds(250))
                    {
                        await Task.Delay(100);
                    }

                    _publishStartTime = DateTime.Now;
                    var replicator = new Replicator();
                    try
                    {
                        await replicator.PublishContentToAllSites(Environment.Instance.ContentPath, Environment.Instance.PublishSettingsPath);
                    }
                    catch (Exception e)
                    {
                        Trace.TraceError("Publishing failed: {0}", e.ToString());
                    }
                }
            }
            Interlocked.Decrement(ref _inUseCount);
        }

        private void OnError(object sender, ErrorEventArgs e)
        {
            Trace.TraceError(e.GetException().ToString());
        }
    }
}
