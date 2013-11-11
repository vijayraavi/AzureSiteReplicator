using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Hosting;

namespace AzureSiteReplicator
{
    public class Environment
    {
        public static Environment Instance = new Environment();

        public Environment()
        {
            string homePath = System.Environment.ExpandEnvironmentVariables(@"%SystemDrive%\home");

            if (Directory.Exists(homePath))
            {
                // Running on Azure

                // Publish the wwwroot folder
                ContentPath = Path.Combine(homePath, "site", "wwwroot");

                PublishSettingsPath = Path.Combine(homePath, "data", "SiteReplicator");
            }
            else
            {
                // Local case: run from App_Data for testing purpose

                string appData = HostingEnvironment.MapPath("~/App_Data");

                ContentPath = Path.Combine(appData, "source");

                PublishSettingsPath = Path.Combine(appData, "PublishSettingsFiles");
            }

            Directory.CreateDirectory(ContentPath);
            Directory.CreateDirectory(PublishSettingsPath);
        }

        // Path to the web content we want to replicate
        public string ContentPath { get; set; }

        // Path to the publish settings files that drive where we publish to
        public string PublishSettingsPath { get; set; }
    }
}