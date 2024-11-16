using Microsoft.AspNetCore.Mvc;

namespace FSO.Server.Api.Controllers
{
    [Route("/")]
    public class HomeController : ControllerBase
    {
        [HttpGet]
        public ActionResult Index()
        {
            return Redirect("/swagger");
        }
    }
}
