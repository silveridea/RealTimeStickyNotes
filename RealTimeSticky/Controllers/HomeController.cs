using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace RealTimeSticky.Controllers
{
    public class HomeController : BootstrapBaseController
    {
        //
        // GET: /Home/
        public ActionResult Index()
        {
            if (string.IsNullOrEmpty(Common.GetConnectionString()))
                return Redirect("~/Install/Index/");

            return View();
        }
    }
}
