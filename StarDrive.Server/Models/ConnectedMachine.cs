using StarDrive.Shared;

namespace StarDrive.Server.Models
{
    public class ConnectedMachine
    {
        public string ConnectionId { get; set; }
        public string MachineName { get; set; } = string.Empty;
        public bool IsConnected { get; set; }
        public List<DirectoryItem> DirectoryItems { get; set; }
    }
}
