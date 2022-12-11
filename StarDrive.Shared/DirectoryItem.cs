using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarDrive.Shared;

public class DirectoryItem
{
    public bool IsDirectory { get; set; }
    public string Name { get; set; }
    public string Path { get; set; }
    public DateTime Modified { get; set; }
    public long Size { get; set; }
}
