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
            ViewData["MachineName"] = machineName;
            return View(directoryItems);
        }

        [HttpGet("remote/{machineName}/file")]
        public async Task<IActionResult> File(string machineName, string path, string filename)
        {
            var cm = _service.ConnectedMachines.FirstOrDefault(m => m.MachineName.Equals(machineName));
            var byteArr = await _service.ReadFileAsync(cm.ConnectionId, path);
            var mimeType = "application/octet-stream";
            return new FileContentResult(byteArr, mimeType)
            {
                FileDownloadName = filename
            };
        }
    }
}
