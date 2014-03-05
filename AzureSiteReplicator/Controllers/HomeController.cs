using System;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AzureSiteReplicator.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            var publishSettingsFiles = Directory.GetFiles(Environment.Instance.PublishSettingsPath)
                .Select(path=> Path.GetFileName(path).Split('.').First());

            return View(publishSettingsFiles);
        }

        [HttpPost]
        public ActionResult Index(HttpPostedFileBase file)
        {
            if (file.ContentLength > 0)
            {
                var fileName = Path.GetFileName(file.FileName);
                var path = Path.Combine(Environment.Instance.PublishSettingsPath, fileName);
                file.SaveAs(path);

                // Trigger a deployment since we just added a new target site
                Replicator.Instance.TriggerDeployment();
            }

            return RedirectToAction("Index");
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}