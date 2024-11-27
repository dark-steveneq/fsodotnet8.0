using Microsoft.AspNetCore.Mvc;

namespace FSO.Server.Api.Controllers
{
    public class HomeController : Controller
    {
        [Route("/")]
        [HttpGet]
        public IActionResult Index()
        {
            var api = Api.INSTANCE;
            if (api.Config.SiteEnabled)
            {
                ViewData["name"] = api.Config.SiteName;
                using (var da = Api.INSTANCE.DAFactory.Get())
                {
                    ViewData["sims"] = (int)da.AvatarClaims.GetAllActiveAvatarsCount();
                }
                return View();
            }
            return Redirect("/swagger/");
        }

        [Route("/register")]
        [HttpGet]
        public IActionResult Register()
        {
            var api = Api.INSTANCE;
            if (api.Config.SiteEnabled)
            {
                ViewData["name"] = api.Config.SiteName;
                return View();
            }
            return Redirect("/swagger/");
        }
    }
}
