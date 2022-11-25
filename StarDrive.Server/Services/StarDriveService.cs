using StarDrive.Server.Models;

namespace StarDrive.Server.Services
{
    public class StarDriveService
    {
        public List<ConnectedMachine> ConnectedMachines { get; set; }= new List<ConnectedMachine>();
        public StarDriveService()
        {

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
    }
}
