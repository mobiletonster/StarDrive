namespace StarDrive.Shared;

public class FileInfoResult
{
    public string? FileName { get; set; }
    public string? FileVersion { get; set; }
    public string? FilePath { get; set; }
    public string? RelativePath { get; set; }
    public long Length { get; set; }
    public DateTime CreationTime { get; set; }
    public DateTime LastWriteTime { get; set; }
    public DateTime LastAccessTime { get; set; }
    public override string ToString()
    {
        return $"{FileName} - {FileVersion}  --  {RelativePath}";
    }
}