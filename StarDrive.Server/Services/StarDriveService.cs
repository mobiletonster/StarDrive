using Microsoft.AspNetCore.SignalR;
using StarDrive.Server.Models;

namespace StarDrive.Server.Services
{
    public class StarDriveService
    {
        private readonly IHubContext<DriveHub> _driveHub;
        public List<ConnectedMachine> ConnectedMachines { get; set; }= new List<ConnectedMachine>();
        public StarDriveService(IHubContext<DriveHub> driveHub)
        {
            _driveHub = driveHub;
        }

        public void ConnectMachine(string connectionId, string machineName)
        {
            var cm = ConnectedMachines.FirstOrDefault(m=>m.MachineName == machineName); 
            if(cm != null)
            {
                cm.ConnectionId = connectionId;
                cm.IsConnected = true;
            }
            else
            {
                ConnectedMachines.Add(new ConnectedMachine() { MachineName=machineName, ConnectionId = connectionId, IsConnected = true });
            }
        }
        public void DisconnectMachine(string connectionId)
        {
            var cm = ConnectedMachines.FirstOrDefault(m=>m.ConnectionId==connectionId);
            cm.IsConnected = false;
            cm.ConnectionId = string.Empty;
        }

        public async Task ReadDirAsync(string connectionId, string path)
        {
            await _driveHub.Clients.Client(connectionId).SendAsync("ReadDir", path);
        }
    }
}
