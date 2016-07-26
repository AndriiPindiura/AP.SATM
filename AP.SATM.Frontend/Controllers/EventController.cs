using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Mvc;
using AP.SATM.Frontend.Models;
// For more information on enabling MVC for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace AP.SATM.Frontend.Controllers
{
    public class EventController : Controller
    {
        // GET: /<controller>/
        public IActionResult Index()
        {
            LayoutModel lm = new LayoutModel();
            lm.ocxWidth = 1024;
            lm.ocxHeight = 480;
            return View(lm);
        }
    }
}
