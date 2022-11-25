using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using StarDrive.Server.Models;
using StarDrive.Server.Services;
using System.Diagnostics;

namespace StarDrive.Server.Controllers
{
    public class HomeController : Controller
    {
        private readonly StarDriveService _starDriveService;
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger, StarDriveService starDriveService)
        {
            _logger = logger;
            _starDriveService = starDriveService;
        }

        public IActionResult Index()
        {
            var connectedMachines = _starDriveService.ConnectedMachines;
            return View(connectedMachines);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}