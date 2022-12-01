using Microsoft.AspNetCore.Mvc;
using StarDrive.Server.Services;

namespace StarDrive.Server.Controllers
{
    public class RemoteController : Controller
    {
        private readonly StarDriveService _service;
        public RemoteController(StarDriveService service)
        {
            _service = service;
        }

        [HttpGet("remote/{machineName}")]
        public async Task<IActionResult> Index(string machineName, string path=@"C:\")
        {
            var cm = _service.ConnectedMachines.FirstOrDefault(m => m.MachineName.Equals(machineName));
            var directoryItems = await _service.ReadDirAsync(cm.ConnectionId,path);
            cm.DirectoryItems = directoryItems;
            return View(directoryItems);
        }
    }
}
