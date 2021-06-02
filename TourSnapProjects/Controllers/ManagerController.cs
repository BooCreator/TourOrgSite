using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace TourSnapProjects.Controllers
{
    public class ManagerController : Controller
    {
        // GET: Manager
        public ActionResult Requests()
        {
            this.LoadUserData();
            this.LoadMainMenu();
            return this.View();
        }
    }
}