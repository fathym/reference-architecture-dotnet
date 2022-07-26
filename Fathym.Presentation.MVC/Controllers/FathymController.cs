using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Fathym.Presentation.MVC.Controllers
{
    public class FathymController : Controller
    {
        #region Fields
        protected string currentAction;

        protected string currentController;
        #endregion

        #region Constructors
        public FathymController()
        { }
        #endregion

        #region API Methods
        public virtual async Task<IActionResult> Index()
        {
            return View();
        }
        #endregion

        #region Life Cycle
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            ViewBag.CurrentAction = currentAction = context.RouteData.Values["action"].ToString();

            ViewBag.CurrentController = currentController = context.RouteData.Values["controller"].ToString();

            ViewBag.UXRelease = (Assembly.GetEntryAssembly() ?? GetType().Assembly)
                        .GetCustomAttribute<AssemblyInformationalVersionAttribute>()
                        .InformationalVersion;

            base.OnActionExecuting(context);
        }
        #endregion
    }
}
