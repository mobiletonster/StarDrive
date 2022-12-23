
using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.AspNetCore.Routing.Constraints;
using Microsoft.AspNetCore.SignalR;
using StarDrive.Server.Models;
using StarDrive.Server.Services;
using StarDrive.Shared;
using System.Diagnostics;
using System.Threading.Channels;

namespace StarDrive.Server
{
    public class DriveHub : Hub
    {
        private readonly ILogger<DriveHub> _logger;
        private readonly StarDriveService _starDriveService;
        public DriveHub(ILogger<DriveHub> logger, StarDriveService starDriveService)
        {
            _logger = logger;   
            _starDriveService = starDriveService;
        }

        public override async Task OnConnectedAsync()
        {
            var httpContext = Context.GetHttpContext();
            var machineName = httpContext.Request.Query["MachineName"].FirstOrDefault();
            _starDriveService.ConnectMachine(Context.ConnectionId, machineName);

            _logger.LogInformation($"{Context.ConnectionId} connected to the server {httpContext.Request.QueryString}");
            await base.OnConnectedAsync();
        }

        public async override Task OnDisconnectedAsync(Exception? exception)
        {
            _starDriveService.DisconnectMachine(Context.ConnectionId);
            await base.OnDisconnectedAsync(exception);
        }

        public async Task DirectoryResult(List<DirectoryItem> directoryItems)
        {
            var connectionId = Context.ConnectionId;

            var dirItems = directoryItems;
            var cm = _starDriveService.ConnectedMachines.FirstOrDefault(m => m.ConnectionId == connectionId);
            cm.DirectoryItems = directoryItems;
            await Task.CompletedTask;
        }

        public async Task UploadStream(IAsyncEnumerable<byte[]> stream, string path, int bytesize)
        {
            var filename = Path.GetFileName(path);
            int totalBytes = 0;
            int count = 0;
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            await foreach (var chunk in stream)
            {
                count++;
                totalBytes += chunk.Length;
            }
            stopwatch.Stop();
            Console.WriteLine($"stream,{count},{stopwatch.ElapsedMilliseconds},{bytesize},{totalBytes},{filename}");
            _logger.LogInformation($"stream,{count},{stopwatch.ElapsedMilliseconds},{bytesize},{totalBytes},{filename}");
        }

        public async Task UploadChannel(ChannelReader<byte[]> chunk, string path, int bytesize)
        {
            var filename = Path.GetFileName(path);
            int count = 0;
            int totalBytes = 0;
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            while (await chunk.WaitToReadAsync())
            {
                count++;
                while (chunk.TryRead(out var item))
                {
                    totalBytes+=item.Length;
                }

            }
            stopwatch.Stop();
            Console.WriteLine($"channel,{count},{stopwatch.ElapsedMilliseconds},{bytesize},{totalBytes},{filename}");
            _logger.LogInformation($"channel,{count},{stopwatch.ElapsedMilliseconds},{bytesize},{totalBytes},{filename}");
        }
    }
}
