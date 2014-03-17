using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Microsoft.Web.Deployment;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;

namespace AzureSiteReplicator
{
    public class WebDeployHelper
    {
        public DeploymentChangeSummary DeployContentToOneSite(string contentPath, string publishSettingsFile)
        {
            var sourceBaseOptions = new DeploymentBaseOptions();
            DeploymentBaseOptions destBaseOptions;
            string siteName = SetBaseOptions(publishSettingsFile, out destBaseOptions);

            Trace.TraceInformation("Starting WebDeploy for {0}", Path.GetFileName(publishSettingsFile));

            // Publish the content to the remote site
            using (var deploymentObject = DeploymentManager.CreateObject(DeploymentWellKnownProvider.ContentPath, contentPath, sourceBaseOptions))
            {
                // Note: would be nice to have an async flavor of this API...
                return deploymentObject.SyncTo(DeploymentWellKnownProvider.ContentPath, siteName, destBaseOptions, new DeploymentSyncOptions());
            }
        }

        private string SetBaseOptions(string publishSettingsPath, out DeploymentBaseOptions deploymentBaseOptions)
        {
            PublishSettings publishSettings = new PublishSettings(publishSettingsPath);
            deploymentBaseOptions = new DeploymentBaseOptions
            {
                ComputerName = publishSettings.ComputerName,
                UserName = publishSettings.Username,
                Password = publishSettings.Password,
                AuthenticationType = publishSettings.AuthenticationType
            };

            if (publishSettings.AllowUntrusted)
            {
                ServicePointManager.ServerCertificateValidationCallback = AllowCertificateCallback;
            }

            return publishSettings.SiteName;
        }

        private static bool AllowCertificateCallback(object sender, X509Certificate cert, X509Chain chain, SslPolicyErrors errors)
        {
            return true;
        }
    }
}
