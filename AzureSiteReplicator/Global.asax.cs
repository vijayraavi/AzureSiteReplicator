using AzureSiteReplicator.Controllers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace AzureSiteReplicator
{
    public class MvcApplication : System.Web.HttpApplication
    {
        private bool _deploymentPending = false;
        private bool _deploymentNeeded = false;

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
            _deploymentNeeded = true;

            // Don't do anything if we're already deploying
            if (_deploymentPending) return;

            while (_deploymentNeeded)
            {
                _deploymentNeeded = false;
                _deploymentPending = true;
                OnChangedAsync().Wait();
                _deploymentPending = false;
            }
        }

        private async Task OnChangedAsync()
        {
            var replicator = new Replicator();
            await replicator.PublishContentToAllSites(Environment.Instance.ContentPath, Environment.Instance.PublishSettingsPath);
        }

        private void OnError(object sender, ErrorEventArgs e)
        {
            System.Diagnostics.Trace.TraceError(e.GetException().ToString());
        }
    }
}
