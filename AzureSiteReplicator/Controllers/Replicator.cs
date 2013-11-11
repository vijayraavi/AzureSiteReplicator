using Microsoft.Web.Deployment;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Xml.Linq;

namespace AzureSiteReplicator.Controllers
{
    public class Replicator
    {
        public void PublishContentToAllSites(string contentPath, string publishSettingsPath)
        {
            PublishContentToAllSitesAsync(contentPath, publishSettingsPath).Wait();
        }

        public async Task PublishContentToAllSitesAsync(string contentPath, string publishSettingsPath)
        {
            string[] publishSettingsFiles = Directory.GetFiles(publishSettingsPath);

            // Publish to all the target sites in parallel
            var allChanges = await Task.WhenAll(publishSettingsFiles.Select(async publishSettingsFile =>
            {
                try
                {
                    return await Task<DeploymentChangeSummary>.Run(() => PublishContentToOneSite(Environment.Instance.ContentPath, publishSettingsFile));
                }
                catch (Exception e)
                {
                    Trace.TraceError("Error processing {0}: {1}", Path.GetFileName(publishSettingsFile), e.Message);
                    return null;
                }
            }));

            // Trace all the results
            for (int i = 0; i < allChanges.Length; i++)
            {
                DeploymentChangeSummary changeSummary = allChanges[i];
                if (changeSummary == null) continue;

                Trace.TraceInformation("Processed {0}", Path.GetFileName(publishSettingsFiles[i]));

                Trace.TraceInformation("BytesCopied: {0}", changeSummary.BytesCopied);
                Trace.TraceInformation("Added: {0}", changeSummary.ObjectsAdded);
                Trace.TraceInformation("Updated: {0}", changeSummary.ObjectsUpdated);
                Trace.TraceInformation("Deleted: {0}", changeSummary.ObjectsDeleted);
                Trace.TraceInformation("Errors: {0}", changeSummary.Errors);
                Trace.TraceInformation("Warnings: {0}", changeSummary.Warnings);
                Trace.TraceInformation("Total changes: {0}", changeSummary.TotalChanges);
            }
        }

        public DeploymentChangeSummary PublishContentToOneSite(string contentPath, string publishSettingsFile)
        {
            var sourceBaseOptions = new DeploymentBaseOptions();
            DeploymentBaseOptions destBaseOptions;
            string siteName = ParsePublishSettings(publishSettingsFile, out destBaseOptions);

            // Publish the content to the remote site
            using (DeploymentObject deploymentObject = DeploymentManager.CreateObject(DeploymentWellKnownProvider.ContentPath, contentPath, sourceBaseOptions))
            {
                // Note: would be nice to have an async flavor of this API...
                return deploymentObject.SyncTo(DeploymentWellKnownProvider.ContentPath, siteName, destBaseOptions, new DeploymentSyncOptions());
            }
        }

        private string ParsePublishSettings(string path, out DeploymentBaseOptions deploymentBaseOptions)
        {
            var document = XDocument.Load(path);
            var profile = document.Descendants("publishProfile").First();

            string siteName = profile.Attribute("msdeploySite").Value;

            deploymentBaseOptions = new DeploymentBaseOptions
            {
                ComputerName = String.Format("https://{0}/msdeploy.axd?site={1}", profile.Attribute("publishUrl").Value, siteName),
                UserName = profile.Attribute("userName").Value,
                Password = profile.Attribute("userPWD").Value,
                AuthenticationType = "Basic"
            };

            return siteName;
        }
    }
}