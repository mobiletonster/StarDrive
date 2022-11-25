﻿
using Microsoft.AspNetCore.SignalR;
using StarDrive.Server.Models;
using StarDrive.Server.Services;

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
    }
}