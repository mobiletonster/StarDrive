using Microsoft.AspNetCore.Mvc;

namespace StarDrive.Server.Controllers
{
    public class RemoteController : Controller
    {
        [HttpGet("remote/{machineName}")]
        public IActionResult Index(string machineName)
        {
            return View();
        }
    }
}
