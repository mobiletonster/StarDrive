using Microsoft.AspNetCore.Mvc;
using StarDrive.Server.Services;
using System.Diagnostics;
using System.IO;

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
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            var cm = _service.ConnectedMachines.FirstOrDefault(m => m.MachineName.Equals(machineName));
            var byteArr = await _service.ReadFileAsync(cm.ConnectionId, path);
            var mimeType = "application/octet-stream";
            var fileContentResult = new FileContentResult(byteArr, mimeType)
            {
                FileDownloadName = filename
            };
            stopwatch.Stop();
            Console.WriteLine($"content result received in {stopwatch.ElapsedMilliseconds} ms");
            Console.WriteLine("completing content result");
            return fileContentResult;
        }

        [HttpGet("remote/{machineName}/filestream")]
        public async Task<IActionResult> FileStream(string machineName, string path, string filename, int bytesize=1048)
        {
            var cm = _service.ConnectedMachines.FirstOrDefault(m => m.MachineName.Equals(machineName));
            await _service.ReadFileStreamAsync(cm.ConnectionId, path, bytesize);
            return Ok();
            //var mimeType = "application/octet-stream";
            //var memoryStream = new MemoryStream();
            //await foreach (var item in fileStream)
            //{
            //    memoryStream.WriteByte(item);
            //}
            //memoryStream.Close();

            //return new FileStreamResult(memoryStream, mimeType)
            //{
            //    FileDownloadName = filename
            //};

            //return new FileContentResult(byteArr, mimeType)
            //{
            //    FileDownloadName = filename
            //};
        }

        [HttpGet("remote/{machineName}/filechannel")]
        public async Task<IActionResult> FileChannel(string machineName,string path, string filename, int bytesize=1048)
        {
            var cm = _service.ConnectedMachines.FirstOrDefault(m => m.MachineName.Equals(machineName));
            await _service.ReadFileChannelAsync(cm.ConnectionId, path, bytesize);
            return Ok();
        }
    }
}
